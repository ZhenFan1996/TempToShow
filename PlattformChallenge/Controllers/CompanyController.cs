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
            if (Id == null || Id == "")
            {
                ErrorViewModel errorViewModel = new ErrorViewModel();
                errorViewModel.RequestId = "invalid challenge id value for looking up all solutions";
                return View("Error", errorViewModel);
            }
            var challenge = await _cRepository.GetAll()
                .Include(c => c.Company)
                .Include(c => c.LanguageChallenges)
                .FirstOrDefaultAsync(c => c.C_Id == Id);
            if (challenge == null)
            {
                ErrorViewModel errorViewModel = new ErrorViewModel();
                errorViewModel.RequestId = "there's no challenge with this id, please check again";
                return View("Error", errorViewModel);
            }

            if (_currUser.Id != challenge.Com_ID)
            {
                ErrorViewModel errorViewModel = new ErrorViewModel();
                errorViewModel.RequestId = "You can't see solutions of challenge from other companies";
                return View("Error", errorViewModel);
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

            var model = new AllSolutionsViewModel()
            {
                Solutions = solutionList
        };
            return View(model);
        }

    }
}
