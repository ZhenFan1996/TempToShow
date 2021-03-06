using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PlattformChallenge.Core.Interfaces;
using PlattformChallenge.Core.Model;
using PlattformChallenge.Models;
using PlattformChallenge.Services;
using PlattformChallenge.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PlattformChallenge.Controllers
{
    [Authorize(Roles = "Company")]
    public class CompanyController : Controller
    {
        private readonly UserManager<PlatformUser> _userManger;
        private PlatformUser _currUser => _userManger.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)).Result;
        private readonly IRepository<Challenge> _cRepository;
        private readonly IRepository<Participation> _pRepository;
        private readonly IRepository<Solution> _sRepository;
        private readonly ILogger<CompanyController> logger;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailSender _sender;
        private readonly IStringLocalizer<CompanyController> localizer;

        public CompanyController(UserManager<PlatformUser> userManager, IRepository<Challenge> cRepository,
            IRepository<Participation> pRepository, IRepository<Solution> sRepository,ILogger<CompanyController> logger,
            IWebHostEnvironment webHostEnvironment, IEmailSender sender, IStringLocalizer<CompanyController> localizer)
        {
            this._userManger = userManager;
            this._cRepository = cRepository;
            this._pRepository = pRepository;
            this._sRepository = sRepository;
            this.logger = logger;
            this._env = webHostEnvironment;
            this._sender = sender;
            this.localizer = localizer;
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
                Company = _currUser,
                LogoPath = "/images/" + (_currUser.Logo ?? "default.png")
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
                    ViewBag.Message = localizer["NoRateAfterClosing"];
                    return View("Index");
                }


                var toUpdate = await _sRepository.FirstOrDefaultAsync(s => s.S_Id == vm.CurrSolutionId);
                toUpdate.Point = vm.Point;
                var pro = await _userManger.FindByIdAsync((await _pRepository.FirstOrDefaultAsync(p => p.S_Id == vm.CurrSolutionId)).P_Id);
                string subject = localizer["NotedTitle", challenge.Title]; 
                string body = localizer["Noted", pro.Name, challenge.Title, vm.Point];

                await _sender.SendEmailAsync(pro.Email, subject, body);
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
           
            var toUpdate = await _cRepository.GetAll().Include(c=> c.Participations).FirstOrDefaultAsync(c => c.C_Id == Id);
            if (toUpdate.IsClose)
            //This shouldn't suppose to happen in normal situation, because the close button will be deactived
            {
                ViewBag.Message = localizer["AlreadyClosed"];
                return View("Index");
            }

            else if (toUpdate.Deadline >= DateTime.UtcNow)
            {
                ViewBag.Message = localizer["NoCloseBeforeDeadline"];
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
                        ViewBag.Message = localizer["MustRateAllSolutions"];
                        return View("Index");
                    }
                }
                if (allSolutions.Count > 0)
                {
                    var bestSolution = allSolutions.OrderByDescending(s => s.s.Point).First();
                    List<Solution> solList = new List<Solution>();
                    foreach(var s in allSolutions)
                    {
                        solList.Add(s.s);
                    }
                    int bestScore = (int)bestSolution.s.Point;
                    if(!checkIfOnlyBest(solList, bestScore))
                    {
                        ViewBag.Message = localizer["TwoSolutionSameScore"]; 
                        return View("Index");
                    }
                    
                    var participation = await _pRepository.FirstOrDefaultAsync(w => w.S_Id == bestSolution.s.S_Id);
                    var winnerId = participation.P_Id;
                    toUpdate.Winner_Id = winnerId;
                    toUpdate.Best_Solution_Id = bestSolution.s.S_Id;
                 }
                else
                {
                    toUpdate.Winner_Id = "NoWinner";
                }
                toUpdate.IsClose = true;

                
                try
                {
                    await _cRepository.UpdateAsync(toUpdate);
                    foreach (var p in toUpdate.Participations) {
                        var user = await _userManger.FindByIdAsync(p.P_Id);
                        string subject = localizer["ClosedTitle", toUpdate.Title]; 
                        string body = localizer["Closed",user.Name,toUpdate.Title];
                        await _sender.SendEmailAsync(user.Email, subject, body);
                    }

                    logger.LogInformation($"The challenge with id {toUpdate.C_Id} is closed.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }
               
            return RedirectToAction("Index");
        }
        /// <summary>
        /// Show a company profile
        /// </summary>
        /// <param name="c_id">Id of a company user</param>
        /// <returns>Requested company profile view if succeed; 
        /// NotFound view if the given id parameter is not correct</returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Profile(string c_id)
        {
            var company = await _userManger.FindByIdAsync(c_id);
            if (company == null)
            {
                Response.StatusCode = 400;
                return View("NotFound");
            }

            var challenges = await (from c
                             in _cRepository.GetAll().Where(c => c.Com_ID == c_id).Include(c => c.Company)
                                    select c).ToListAsync();
            
            var num = challenges.Count();

            var model = new ProfileViewModel()
            {
                Email = company.Email,
                Name = company.Name,
                Address = company.Address ?? "",
                Bio = company.Bio,
                Phone = company.PhoneNumber ?? "",
                Hobby = company.Hobby ?? "",
                Birthday = company.Birthday,
                InvolvedChallengeNumber = num, 
                LogoPath = "/images/" + (company.Logo ?? "default.png")
            };
            return View(model);
        }

        /// <summary>
        /// HttpGeT - Load information setting site for profile
        /// </summary>
        /// <returns>HttpPost Formular</returns>
        [HttpGet]
        public IActionResult ProfileSetting()
        {
            DateTime defaultTime = _currUser.Birthday.Equals(DateTime.MinValue) ? DateTime.UtcNow.Date.AddYears(-10) : _currUser.Birthday;
            ProfileSettingViewModel model = new ProfileSettingViewModel()
            {
                Name = _currUser.Name,
                Address = _currUser.Address,
                Bio = _currUser.Bio,
                Phone = _currUser.PhoneNumber,
                Hobby = _currUser.Hobby,
                Birthday = defaultTime,
                LogoPath = "/images/" + (_currUser.Logo ?? "default.png")
            };
            return View(model);
        }

        /// <summary>
        /// HttpPost - Set received profile update information to database
        /// </summary>
        /// <param name="model">ProfileSettingViewModel with given parameters from user input</param>
        /// <returns>The same site if succeed or Error info site if failed</returns>
        [HttpPost]
        public async Task<IActionResult> ProfileSetting(ProfileSettingViewModel model)
        {
            string dir = Path.Combine(_env.WebRootPath, "images");
            if (model.Logo != null)
            {
                string logoName = Guid.NewGuid().ToString() + Path.GetExtension(model.Logo.FileName);
                string path = await Upload(model.Logo, logoName, dir);
                _currUser.Logo = logoName;
            }          
            _currUser.Name = model.Name;
            _currUser.Address = model.Address;
            _currUser.Bio = model.Bio;
            _currUser.PhoneNumber = model.Phone;
            _currUser.Birthday = model.Birthday;
            _currUser.Hobby = model.Hobby;

            var result = await _userManger.UpdateAsync(_currUser);
            if (result.Succeeded)
            {
                logger.LogInformation($"{_currUser.Id} edited profile.");
                return RedirectToAction();
            }
            else
            {
                throw new Exception("The setting failed");
            }
        }


        private bool checkIfOnlyBest(List<Solution> solList, int bestScore)
        {
            int count = 0;
            foreach(Solution s in solList)
            {
                if (s.Point == bestScore)
                {
                    count++;
                }
            }
            if (count > 1)
            {
                return false;
            }
            return true;
        }

        private ViewResult IllegalOpCheck(String Id)
        {
            if (Id == null || Id == "")
            {
                Response.StatusCode = 400;
                @ViewBag.ErrorMessage = localizer["EmptyCID"]; 
                return View("NotFound");
            }
           Challenge challenge =  _cRepository.GetAll()
                .Include(c => c.Company)
                .Include(c => c.LanguageChallenges)
                .FirstOrDefault(c => c.C_Id == Id);
            if (challenge == null)
            {
                Response.StatusCode = 404;
                @ViewBag.ErrorMessage = localizer["CannotFoundCID", Id]; 
                return View("NotFound");
            }
           
            if (_currUser.Id != challenge.Com_ID)
            {
                Response.StatusCode = 403;
                @ViewBag.ErrorMessage = localizer["NoAccessOtherCom"]; 
                return View("Error");
            }

            return null;
        }
        /// <summary>
        /// Download submitted Solution
        /// </summary>
        /// <param name="s_id">Solution Id</param>
        /// <returns></returns>
        [AllowAnonymous]
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

        private async Task<string> Upload(IFormFile file, string fileName, string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string filePath = Path.Combine(dir, fileName);
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return filePath;
        }
    }
}
