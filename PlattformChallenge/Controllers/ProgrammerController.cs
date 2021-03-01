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
using PlattformChallenge.Services;
using Microsoft.Extensions.Localization;

namespace PlattformChallenge.Controllers
{
    [Authorize(Roles = "Programmer")]
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
        private readonly IEmailSender _sender;
        private readonly IStringLocalizer<ProgrammerController> localizer;

        public ProgrammerController(UserManager<PlatformUser> userManager, IRepository<Challenge> cRepository, IRepository<Participation> pRepository,IRepository<Solution> sRepository, ConfigProviderService appcfg,
            ILogger<ProgrammerController> logger, IWebHostEnvironment webHostEnvironment, IEmailSender sender, IStringLocalizer<ProgrammerController> localizer
            )
        {
            this._userManger = userManager;
            this._cRepository = cRepository;
            this._pRepository = pRepository;
            this._sRepository = sRepository;
            this._env = webHostEnvironment;
            _appcfg = appcfg;
            this.logger = logger;
            this._sender = sender;
            this.localizer = localizer;
        }
        /// <summary>
        /// Get current user information and challenges participated in and return to the page
        /// </summary>
        public async Task<IActionResult> Index() {

            var cp = await (from c in _cRepository.GetAll().Include(c => c.Company).Include(c => c.LanguageChallenges)
                             join p in _pRepository.GetAll().Include(p => p.Challenge).Include(p => p.Solution)
                             on c.C_Id equals p.C_Id
                             where p.P_Id == _currUser.Id
                             select new { c,p}).ToListAsync();
            var challenges = new List<Challenge>();
            var participations = new List<Participation>();
            foreach (var e in cp) {
                challenges.Add(e.c);
                participations.Add(e.p);
            }
            int inProgress = challenges.Where(c => !c.IsClose).Count();

            var model = new ProgrammerIndexViewModel() {
                Challenges = challenges,
                Participations = participations,
                Programmer = _currUser,
                Phone = _currUser.PhoneNumber ?? "***",
                LogoPath = "/images/" + (_currUser.Logo ?? "default.png"),
                InProgress = inProgress,
                Completet = challenges.Count() - inProgress                
            };
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Profile(string p_id) {

            var programmer = await _userManger.FindByIdAsync(p_id);
            if (programmer == null) {
                Response.StatusCode = 400;
                return View("NotFound");
            }

            var num = _pRepository.GetAll().Where(p => p.P_Id == p_id).Count();

            var model = new ProfileViewModel()
            {
                Email = programmer.Email,
                Name = programmer.Name,
                Address = programmer.Address ??"*****",
                Bio = programmer.Bio,
                Phone = programmer.PhoneNumber ??"*****",
                Hobby = programmer.Hobby ??"*****",
                Birthday = programmer.Birthday,
                InvolvedChallengeNumber = num,
                LogoPath = "/images/" + (programmer.Logo ?? "default.png")
            };

            return View(model);
        }

        public async Task<IActionResult> WonChallenges()
        {
            var challenges = await  _cRepository.GetAll().Include(c => c.Company)
                            .Where(c => c.Winner_Id == _currUser.Id).ToListAsync();
            int sumBonus = 0;
            foreach(var c in challenges)
            {
                sumBonus += c.Bonus;
            }
            var model = new WonChallengeViewModel()
            {
                Challenges = challenges,
                SumBonus = sumBonus
            };
            return View(model);
        }

            /// <summary>
            /// Exit the currently selected challenge
            /// </summary>
            /// <param name="id"></param> the challenge id
            /// <returns>View index</returns>

            public async Task<IActionResult> Cancel(string id) {
            var p = await (from pc in _pRepository.GetAll().Include(p =>p.Solution).Include(p => p.Challenge)
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
                string subject = $"Successfuly Cancel for {par.Challenge.Title} ";
                string body = localizer["Cancel", _currUser.Name, par.Challenge.Title];
                await _sender.SendEmailAsync(_currUser.Email, subject, body);
                if (par.Solution != null)
                {
                    await _sRepository.DeleteAsync(par.Solution);                   
                }
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
                IsVaild = c.Deadline >= DateTime.UtcNow

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
                    model.IsVaild = c.Deadline >= DateTime.UtcNow;
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
                    Submit_Date = DateTime.UtcNow,
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
                model.IsVaild = c.Deadline >= DateTime.UtcNow;
                string subject = $"Your Solution for {c.Title} has successfully uploaded";
                string body = localizer["Upload", _currUser.Name, c.Title, par.S_Id];
                await _sender.SendEmailAsync(_currUser.Email, subject, body);
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
                LogoPath = "/images/" + (_currUser.Logo ?? "default.png")
            };
            
            return View(model);
        }

        
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
