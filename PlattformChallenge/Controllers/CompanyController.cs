using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlattformChallenge.Core.Interfaces;
using PlattformChallenge.Core.Model;
using PlattformChallenge.Models;
using PlattformChallenge.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PlattformChallenge.Controllers
{

    public class CompanyController : Controller
    {
        private readonly UserManager<PlatformUser> _userManger;
        private PlatformUser _currUser => _userManger.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)).Result;
        private readonly IRepository<Challenge> _cRepository;
        private readonly IRepository<Participation> _pRepository;
        private readonly IRepository<Solution> _sRepository;
        private readonly ILogger<CompanyController> logger;

        public CompanyController(UserManager<PlatformUser> userManager, IRepository<Challenge> cRepository, IRepository<Participation> pRepository, IRepository<Solution> sRepository,ILogger<CompanyController> logger)
        {
            this._userManger = userManager;
            this._cRepository = cRepository;
            this._pRepository = pRepository;
            this._sRepository = sRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Get challenges which are published from current company user
        /// </summary>
        /// <returns>A view with all published challenges</returns>
        public async Task<IActionResult> Index()
        {

            var challenges = await (from c
                             in _cRepository.GetAll().Where(c => c.Com_ID==_currUser.Id).Include(c => c.Company)
                                                       select c).ToListAsync();
                 
            var model = new CompanyIndexViewModel()
            {
                Challenges = challenges,
                Company = _currUser
            };
            return View(model);
        }

        /// <summary>
        /// See all solutions from programmer users for a selected own challenge
        /// </summary>
        /// <param name="Id">Challenge Id</param>
        /// <returns>A View with all solution list</returns>
        public async Task<IActionResult> AllSolutions(string Id)
        {
            if (IllegalOpCheck(Id) != null)
            {
                return View("Error");
            }
            var solutions = await (from p in _pRepository.GetAll()
                                   join s in _sRepository.GetAll()
                                   on p.S_Id equals s.S_Id
                                   where p.C_Id == Id
                                   select new { s,p }
                                  ).ToListAsync();

            var solutionList = new List<Solution>();
            foreach (var c in solutions)
            {
                solutionList.Add(c.s);
            }

            var pNameList = new List<String>();
            foreach (var c in solutions)
            {
                PlatformUser currProgrammer = _userManger.FindByIdAsync(c.p.P_Id).Result;
                string pName = currProgrammer.Name;
                pNameList.Add(pName);
            }

            var currCha = _cRepository.FirstOrDefault(c => c.C_Id == Id);
            var model = new AllSolutionsViewModel()
            {
                Solutions = solutionList,
                ProgrammerNameList = pNameList,
                CurrChallenge = currCha,
                CurrChallengeId = Id
            };
            return View(model);
        }
        /// <summary>
        /// Rate a selected solution by updating point and status
        /// </summary>
        /// <param name="vm">AllSolutionsViewModel</param>
        /// <returns>View with all solutions with updated point</returns>
        [HttpPost]
        public async Task<IActionResult> RateSolution(AllSolutionsViewModel vm)
        {
   
            if (ModelState.IsValid)
            {
                var solItem = await (from p in _pRepository.GetAll()
                                       join s in _sRepository.GetAll()
                                       on p.S_Id equals s.S_Id
                                       where p.C_Id == vm.CurrChallengeId
                                       select new { s, p }
                                 ).ToListAsync();
               
                var challengeId = solItem.First().p.C_Id;
                var challenge = await _cRepository.FirstOrDefaultAsync(c => c.C_Id == challengeId);
                if (challenge.IsClose)
                //This shouldn't suppose to happen in normal situation, because the rate button will be deactived
                {
                    ViewBag.Message = string.Format("You can't rate solution anymore because this challenge is already closed");
                    return View("Index");
                }


                var toUpdate = await _sRepository.FirstOrDefaultAsync(s => s.S_Id == vm.CurrSolutionId);
                toUpdate.Point = vm.Point;
                toUpdate.Status = StatusEnum.Rated;
                    try
                    {
                        await _sRepository.UpdateAsync(toUpdate);
                    logger.LogInformation($"The Solution with id {toUpdate.S_Id} is rated with the point {vm.Point}");
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        throw;                     
                    }
                
            }
            return RedirectToAction("AllSolutions", new { id = vm.CurrChallengeId });
        }
        /// <summary>
        /// Close a challenge if all solutions are rated
        /// </summary>
        /// <param name="Id">Id of the challenge which should be closed</param>
        /// <returns>A View back to portal index</returns>
        public async Task<IActionResult> CloseChallenge(String Id)
        {
            if (IllegalOpCheck(Id) != null)
            {
                return View("Error");
            }
           
            var toUpdate = await _cRepository.FirstOrDefaultAsync(c => c.C_Id == Id);
            if (toUpdate.IsClose)
            //This shouldn't suppose to happen in normal situation, because the close button will be deactived
            {
                ViewBag.Message = string.Format("You already closed this challenge");
                return View("Index");
            }

            else if (toUpdate.Deadline >= DateTime.Now)
            {
                ViewBag.Message = string.Format("You can not close the challenge before the deadline");
                return View("Index");

            }
            else
            {
                var allSolutions = await (from p in _pRepository.GetAll()
                                          join s in _sRepository.GetAll()
                                          on p.S_Id equals s.S_Id
                                          where p.C_Id == Id
                                          select new { s, p }
                                     ).ToListAsync();

                foreach (var s in allSolutions)
                {
                    if (s.s.Point == null || s.s.Point == 0)
                    {
                        ViewBag.Message = "There's at least one challenge you didn't rate, therefore you can't close this challenge yet. " +
                            "Please rate all solutions before closing a challenge";
                        return View("Index");
                    }
                }
                if (allSolutions.Count > 0)
                {
                    var bestSolution = allSolutions.OrderByDescending(s => s.s.Point).First();
                    var participation = await _pRepository.FirstOrDefaultAsync(w => w.S_Id == bestSolution.s.S_Id);
                    var winnerId = participation.P_Id;
                    toUpdate.Winner_Id = winnerId;
                    toUpdate.Best_Solution_Id = bestSolution.s.S_Id;
                    toUpdate.IsClose = true;
                }
                else
                {
                    toUpdate.Winner_Id = "NoWinner";
                    toUpdate.IsClose = true;
                }
                try
                {
                    await _cRepository.UpdateAsync(toUpdate);
                    logger.LogInformation($"The challenge with id {toUpdate.C_Id} is closed.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }
               
            return RedirectToAction("Index");
        }


            private ViewResult IllegalOpCheck(String Id)
        {
            if (Id == null || Id == "")
            {
                Response.StatusCode = 400;
                @ViewBag.ErrorMessage = "Invalid empty challenge id value";
                return View("NotFound");
            }
           Challenge challenge =  _cRepository.GetAll()
                .Include(c => c.Company)
                .Include(c => c.LanguageChallenges)
                .FirstOrDefault(c => c.C_Id == Id);
            if (challenge == null)
            {
                Response.StatusCode = 404;
                @ViewBag.ErrorMessage = $"The Challenge with id {Id} can not be found";
                return View("NotFound");
            }
           
            if (_currUser.Id != challenge.Com_ID)
            {
                Response.StatusCode = 403;
                @ViewBag.ErrorMessage = "You don't have access to challenges from other companies";
                return View("Error");
            }

            return null;
        }
        public async Task<FileResult> DownloadSolution(string s_id)
        {
            Solution solution = await _sRepository.FirstOrDefaultAsync(s => s.S_Id == s_id);
            var memory = new MemoryStream();
            using (var stream = new FileStream(solution.URL, FileMode.Open))
            {
                  await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            var ext = Path.GetExtension(solution.URL).ToLowerInvariant();
            return File(memory, "application/zip", Path.GetFileName(solution.URL));
        }
    }
}
