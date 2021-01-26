using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList;
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
        public CompanyController(UserManager<PlatformUser> userManager, IRepository<Challenge> cRepository, IRepository<Participation> pRepository, IRepository<Solution> sRepository)
        {
            this._userManger = userManager;
            this._cRepository = cRepository;
            this._pRepository = pRepository;
            this._sRepository = sRepository;
        }

        /// <summary>
        /// Get challenges which are published from current company user
        /// </summary>
        /// <returns>A view with all published challenges</returns>
        public async Task<IActionResult> Index()
        {

            var publishedChallenges = from c
                             in _cRepository.GetAll().Where(c => c.Com_ID==_currUser.Id).Include(c => c.Company)
                                                       select c;
            var challenges = new List<Challenge>();
            foreach(var c in publishedChallenges)
            {
                challenges.Add(c);
            }
           
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
            IllegalOpCheck(Id);
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

            var model = new AllSolutionsViewModel()
            {
                Solutions = solutionList,
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
                var challenge = _cRepository.FirstOrDefault(c => c.C_Id == challengeId);
                if (challenge.Winner_Id != null)
                {
                    ErrorViewModel errorViewModel = new ErrorViewModel();
                    errorViewModel.RequestId = "You can't rate solution anymore because this challenge is already closed";
                    return View("Error", errorViewModel);
                }


                var toUpdate = await _sRepository.GetAllListAsync(s => s.S_Id == vm.CurrSolutionId);
                if (toUpdate.Count != 1)
                {
                    ErrorViewModel errorViewModel = new ErrorViewModel();
                    errorViewModel.RequestId = "There's something wrong with data. Please contact us";
                    return View("Error", errorViewModel);
                }
                toUpdate.First().Point = vm.Point;
                toUpdate.First().Status = StatusEnum.Rated;
                    try
                    {
                        await _sRepository.UpdateAsync(toUpdate.First());
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

            var toUpdate = await _cRepository.GetAllListAsync(c => c.C_Id == Id);
            if (toUpdate.First().Winner_Id != null)
            {
                ErrorViewModel errorViewModel = new ErrorViewModel();
                errorViewModel.RequestId = "You already closed this challenge";
                return View("Error", errorViewModel);
            }

            var allSolutions = await (from p in _pRepository.GetAll()
                                 join s in _sRepository.GetAll()
                                 on p.S_Id equals s.S_Id
                                 where p.C_Id == Id
                                 select new { s, p }
                                 ).ToListAsync();

            foreach(var s in allSolutions)
            {
                if(s.s.Point == null || s.s.Point == 0)
                {
                    ErrorViewModel errorViewModel = new ErrorViewModel();
                    errorViewModel.RequestId = "There's at least one challenge you didn't rate, therefore you can't close this challenge yet. " +
                        "Please rate all solutions before closing a challenge";
                    return View("Error", errorViewModel);
                }
            }

            var bestSolution = allSolutions.OrderByDescending(s => s.s.Point).First();
            var participation = await _pRepository.FirstOrDefaultAsync(w => w.S_Id == bestSolution.s.S_Id);
            var winnerId = participation.P_Id;
            toUpdate.First().Winner_Id = winnerId;
            toUpdate.First().Best_Solution_Id = bestSolution.s.S_Id;
            try
            {
                await _cRepository.UpdateAsync(toUpdate.First());
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }


            return RedirectToAction("Index");
        }


            private ViewResult IllegalOpCheck(String Id)
        {
            ErrorViewModel errorViewModel = new ErrorViewModel();
            if (Id == null || Id == "")
            {
                errorViewModel.RequestId = "invalid empty challenge id value";
                return View("Error", errorViewModel);
            }
           Challenge challenge =  _cRepository.GetAll()
                .Include(c => c.Company)
                .Include(c => c.LanguageChallenges)
                .FirstOrDefault(c => c.C_Id == Id);
            if (challenge == null)
            {
                errorViewModel.RequestId = "there's no challenge with this id, please check again";
                return View("Error", errorViewModel);
            }
            if (challenge.Deadline <= DateTime.Now) {

                errorViewModel.RequestId = "Man can not close the challenge bevor the deadline";
                return View("Error", errorViewModel);
            }
            if (_currUser.Id != challenge.Com_ID)
            {
                errorViewModel.RequestId = "You don't have access to challenges from other companies";
                return View("Error", errorViewModel);
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
