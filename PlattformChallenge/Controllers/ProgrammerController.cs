using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlattformChallenge.Core.Interfaces;
using PlattformChallenge.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlattformChallenge.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace PlattformChallenge.Controllers
{
    [Authorize(Roles="Programmer")]
    public class ProgrammerController :Controller
    {

        private readonly UserManager<PlatformUser> _userManger;
        private PlatformUser _currUser => _userManger.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)).Result;
        private readonly IRepository<Challenge> _cRepository;
        private readonly IRepository<Participation> _pRepository;

        public ProgrammerController(UserManager<PlatformUser> userManager, IRepository<Challenge> cRepository, IRepository<Participation> pRepository)
        {
            this._userManger = userManager;
            this._cRepository = cRepository;
            this._pRepository = pRepository;
        }
        /// <summary>
        /// Get current user information and challenges participated in and return to the page
        /// </summary>
        public async Task<IActionResult> Index() {

            var cp = await (from c in _cRepository.GetAll().Include(c => c.Company).Include(c => c.LanguageChallenges)
                             join p in _pRepository.GetAll()
                             on c.C_Id equals p.C_Id
                             where p.P_Id == _currUser.Id
                             select new { c,p}).ToListAsync();
            var challenges = new List<Challenge>();
            var participations = new List<Participation>();
            foreach (var e in cp) {
                challenges.Add(e.c);
                participations.Add(e.p);
            }
            var model = new ProgrammerIndexViewModel() {
                Challenges = challenges,
                Participations = participations,
                Programmer = _currUser
            };
            return View(model);
        }
        /// <summary>
        /// Exit the currently selected challenge
        /// </summary>
        /// <param name="id"></param> the challenge id
        /// <returns>View index</returns>
        public async Task<IActionResult> Cancel(string id) {
            var p = await (from pc in _pRepository.GetAll()
                    where pc.C_Id == id && pc.P_Id == _currUser.Id
                    select pc).ToListAsync();
            if (p.Count !=1)
            {
                return View();
            }
            else 
            {
                await _pRepository.DeleteAsync(p => p.C_Id == id);
                return RedirectToAction("Index");
            }
        }
        [HttpGet]
        public  async Task<IActionResult> UploadSolution(string c_id) {

            var par =(await _pRepository.GetAllListAsync(p => p.C_Id == c_id && p.P_Id == _currUser.Id)).FirstOrDefault();
            var c = await _cRepository.FirstOrDefaultAsync(x => x.C_Id == c_id);

            var model = new UploadSolutionViewModel()
            {
                Participation = par,
                Challenge = c,
                Programmer =_currUser
                
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadSolution(UploadSolutionViewModel model) {
            var par = (await _pRepository.GetAllListAsync(p => p.C_Id == model.C_Id && p.P_Id == _currUser.Id)).FirstOrDefault();
            var c = await _cRepository.FirstOrDefaultAsync(x => x.C_Id == model.C_Id);


            return View();
        }

    }
}
