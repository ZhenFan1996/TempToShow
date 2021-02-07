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
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

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
        private readonly IWebHostEnvironment _env;
        private readonly ConfigProviderService _appcfg;
        private readonly ILogger<ProgrammerController> logger;

        public ProgrammerController(UserManager<PlatformUser> userManager, IRepository<Challenge> cRepository, IRepository<Participation> pRepository,IRepository<Solution> sRepository, ConfigProviderService appcfg,
            ILogger<ProgrammerController> logger, IWebHostEnvironment webHostEnvironment
            )
        {
            this._userManger = userManager;
            this._cRepository = cRepository;
            this._pRepository = pRepository;
            this._sRepository = sRepository;
            this._env = webHostEnvironment;
            _appcfg = appcfg;
            this.logger = logger;
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
                Programmer = _currUser,
                LogoPath = "/images/" + (_currUser.Logo ?? "user.jpg")

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
                logger.LogInformation($"The Programmer with id {par.P_Id} cancel the challenge{par.C_Id}" );
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
                string dir = Path.Combine(_appcfg.AppCfg.SolutionPath, "Solutions");
                string path = await Upload(model.SolutionFile,fileName,dir);
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
                logger.LogInformation($"The Programmer with id{par.P_Id} update the solution with id{par.S_Id} for the challenge with id {par.C_Id}");
                model.Challenge = c;
                par.Solution = s;
                model.Participation = par;
                model.Programmer = _currUser;
                model.IsVaild = c.Deadline >= DateTime.Now;
                return View(model);
            }
            else {
                return View(model);
            }
        }



        [HttpGet]
        public  IActionResult ProfileSetting() {

            ProfileSettingViewModel model = new ProfileSettingViewModel()
            {
                Name = _currUser.Name,
                Address = _currUser.Address,
                Bio = _currUser.Bio,
                Phone = _currUser.PhoneNumber,
                Hobby = _currUser.Hobby,
                Birthday = _currUser.Birthday,
                LogoPath = "/images/" + (_currUser.Logo ?? "user.jpg")
            };
            
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ProfileSetting(ProfileSettingViewModel model)
        {
            string dir = Path.Combine(_env.WebRootPath, "images");
            string logoName = Guid.NewGuid().ToString() + Path.GetExtension(model.Logo.FileName);
            string path = await Upload(model.Logo, logoName, dir);
            _currUser.Name = model.Name;
            _currUser.Address = model.Address;
            _currUser.Bio = model.Bio;
            _currUser.PhoneNumber = model.Phone;
            _currUser.Birthday = model.Birthday;
            _currUser.Hobby = model.Hobby;
            _currUser.Logo = logoName;
            var result = await _userManger.UpdateAsync(_currUser);
            if (result.Succeeded)
            {
                return RedirectToAction();
            }
            else {

                throw new Exception("The setting failed");
            }
        }

        private async Task<string> Upload(IFormFile file,string fileName, string dir) {
           

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
