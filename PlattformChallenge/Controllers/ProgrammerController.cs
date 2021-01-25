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
using System.IO;
using Microsoft.AspNetCore.Http;
using PlattformChallenge.Infrastructure;

namespace PlattformChallenge.Controllers
{
    [Authorize(Roles="Programmer")]
    public class ProgrammerController :Controller
    {
        private readonly UserManager<PlatformUser> _userManger;
        private PlatformUser _currUser => _userManger.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)).Result;
        private readonly IRepository<Challenge> _cRepository;
        private readonly IRepository<Participation> _pRepository;
        private readonly IRepository<Solution> _sRepository;
        private readonly ConfigProviderService _appcfg;

        public ProgrammerController(UserManager<PlatformUser> userManager, IRepository<Challenge> cRepository, IRepository<Participation> pRepository,IRepository<Solution> sRepository, ConfigProviderService appcfg)
        {
            this._userManger = userManager;
            this._cRepository = cRepository;
            this._pRepository = pRepository;
            this._sRepository = sRepository;
            _appcfg = appcfg;
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
            var p = await (from pc in _pRepository.GetAll().Include(p =>p.Solution)
                    where pc.C_Id == id && pc.P_Id == _currUser.Id
                    select pc).ToListAsync();
            if (p.Count !=1)
            {
                return View();
            }
            else 
            {
                var par = p.FirstOrDefault();
                await _pRepository.DeleteAsync(par);
                await _sRepository.DeleteAsync(par.Solution);
                return RedirectToAction("Index");
            }
        }
        [HttpGet]
        public  async Task<IActionResult> UploadSolution(string c_id) {
            var par = await _pRepository.GetAll()
                .Include(p => p.Solution)
                .Include(p =>p.Challenge)
                .Include(p => p.Programmer)
                .FirstOrDefaultAsync(p => p.C_Id == c_id && p.P_Id == _currUser.Id);

            var c = await _cRepository.FirstOrDefaultAsync(x => x.C_Id == c_id);


            var model = new UploadSolutionViewModel()
            {
                Participation = par,
                Challenge = c,
                Programmer =_currUser,
                IsVaild = c.Deadline >= DateTime.Now

            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadSolution(UploadSolutionViewModel model) {
            if (ModelState.IsValid)
            {
                
          
                var par = (await _pRepository.GetAll().Where(p => p.C_Id == model.C_Id && p.P_Id == _currUser.Id).Include(p => p.Solution).AsNoTracking().ToListAsync()).FirstOrDefault();
                var c = await _cRepository.FirstOrDefaultAsync(x => x.C_Id == model.C_Id);
                string fileName = c.Title + "_" + _currUser.Name + ".zip";


                if (model.SolutionFile==null||(!model.SolutionFile.ContentType.Equals("application/zip")&&!model.SolutionFile.ContentType.Equals("application/x-zip-compressed")))
                {
                    ModelState.AddModelError("","The type is false");
                    model.IsVaild = c.Deadline >= DateTime.Now;
                    model.Challenge = c;
                    model.Programmer = _currUser;
                    model.Participation = par;
                    return View(model);
                }
                string path = await Upload(model.SolutionFile,fileName);
                Solution s = new Solution()
                {
                    S_Id = Guid.NewGuid().ToString(),
                    URL = path,
                    Status = StatusEnum.Receive,
                    Submit_Date = DateTime.Now,
                    FileName = fileName
                };
                if (par.S_Id == null)
                {

                    await _sRepository.InsertAsync(s);
                    par.S_Id = s.S_Id;
                    await _pRepository.UpdateAsync(par);
                }
                else
                {
                    par.S_Id = null;
                    await _pRepository.UpdateAsync(par);
                    await _sRepository.DeleteAsync(x => x.S_Id == par.S_Id);
                    await _sRepository.InsertAsync(s);
                    par.S_Id = s.S_Id;
                    await _pRepository.UpdateAsync(par);

                }
                model.Challenge = c;
                par.Solution = s;
                model.Participation = par;
                model.Programmer = _currUser;
                return View(model);
            }
            else {
                return View(model);
            }
        }

        private async Task<string> Upload(IFormFile file,string fileName) {

            string dir = Path.Combine(_appcfg.AppCfg.SolutionPath, "Solutions");

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
