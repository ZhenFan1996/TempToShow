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
    }
}
