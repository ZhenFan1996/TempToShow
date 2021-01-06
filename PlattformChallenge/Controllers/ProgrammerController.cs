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

namespace PlattformChallenge.Controllers
{
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

        public async Task<IActionResult> Index() {

            var challenge = await (from c in _cRepository.GetAll().Include(c => c.Company).Include(c => c.LanguageChallenges)
                             join p in _pRepository.GetAll()
                             on c.C_Id equals p.C_Id
                             where p.P_Id == _currUser.Id
                             select c).ToListAsync();
            var model = new ProgrammerIndexViewModel() {
                Challenges = challenge,
                Programmer = _currUser
            };

            return View(model);
        }

        public async Task<IActionResult> Cancel(string id) {
            await _pRepository.DeleteAsync(p => p.C_Id == id);
            return RedirectToAction("Index");
        }
    }
}
