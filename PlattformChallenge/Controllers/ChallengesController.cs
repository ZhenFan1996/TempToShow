using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PlattformChallenge.Core.Interfaces;
using PlattformChallenge.Core.Model;
using PlattformChallenge.Infrastructure;
using PlattformChallenge.Models;
using PlattformChallenge.Services;
using PlattformChallenge.ViewModels;
using TimeZoneConverter;

namespace PlattformChallenge.Controllers
{
    public class ChallengesController : Controller
    {
        private readonly IRepository<Challenge> _repository;
        private readonly IRepository<PlatformUser> _pRepository;
        private readonly IRepository<Language> _lRepository;
        private readonly IRepository<LanguageChallenge> _lcRepository;
        private readonly IRepository<Participation> _particiRepository;
        private readonly ILogger<ChallengesController> logger;
        private readonly IEmailSender _sender;
        private readonly IStringLocalizer<ChallengesController> localizer;

        public ChallengesController(IRepository<Challenge> repository, IRepository<PlatformUser> pRepository,
            IRepository<Language> lRepository, IRepository<LanguageChallenge> lcRepository, IRepository<Participation> particiRepository,
            ILogger<ChallengesController> looger,IEmailSender sender, IStringLocalizer<ChallengesController> localizer
            )
        {
            _repository = repository;
            _pRepository = pRepository;
            _lRepository = lRepository;
            _lcRepository = lcRepository;
            _particiRepository = particiRepository;
            this.logger = looger;
            this._sender = sender;
            this.localizer = localizer;
        }
        #region list

        /// <summary>
        ///  Get the list of current active challenges with paging function
        //    default situation is return the challenges, which descending sorted by date
        /// </summary>
        /// <param name="pageNumber">which page it should be shown</param>
        /// <param name="sortOrder">which sort criteria</param>
        /// <param name="searchString">which search keyword</param>
        /// <param name="status">called challenge status. 
        /// Default 0: Current Challenges; 1: Past Challenges; 2: Future Challenges</param>
        /// <returns>A view with list of current active challenges</returns>
        [HttpGet]
        public async Task<IActionResult> Index(int? pageNumber, string sortOrder, string searchString,bool[] isSelected, int? status)
        {
            ViewData["DateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "Date" : "";
            ViewData["DeadlineSortParm"] = sortOrder == "Deadline" ? "deadline_desc" : "Deadline";
            ViewData["BonusSortParm"] = sortOrder == "Bonus" ? "bonus_desc" : "Bonus";
            ViewData["QuotaSortParm"] = sortOrder == "Quota" ? "quota_desc" : "Quota";
            ViewData["CurrentFilter"] = searchString;
            ViewData["Languages"] = await _lRepository.GetAllListAsync();
            ViewData["LanguagesFilter"] = isSelected;
            ViewData["Status"] = status;
            ViewData["SortOrder"] = sortOrder;
            IQueryable<Challenge> challenges = null;
            switch (status)
            {
                case 1://Challenges in the past
                    challenges = from c
                             in _repository.GetAll().Where(c => c.Deadline <= DateTime.UtcNow)
                             .Include(c => c.Company)
                                 select c;
                    break;
                case 2://Challenges in the future
                    challenges = from c
                             in _repository.GetAll().Where(c => c.Release_Date > DateTime.UtcNow)
                             .Include(c => c.Company)
                                 select c;
                    break;
                default: //Current Active Challenges
                    challenges = from c
                             in _repository.GetAll().Where(c=>c.Release_Date <= DateTime.UtcNow && c.Deadline > DateTime.UtcNow)
                             .Include(c => c.Company)
                                 select c;
                    break;
            }
            if (!String.IsNullOrEmpty(searchString))
            {
                challenges = challenges.Where(c => c.Title.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "Bonus":
                    challenges = challenges.OrderBy(c => c.Bonus);
                    break;
                case "bonus_desc":
                    challenges = challenges.OrderByDescending(c => c.Bonus);
                    break;
                case "Date":
                    challenges = challenges.OrderBy(c => c.Release_Date);
                    break;
                case "Quota":
                    challenges = challenges.OrderBy(c => c.Max_Participant);
                    break;
                case "quota_desc":
                    challenges = challenges.OrderByDescending(c => c.Max_Participant);
                    break;
                case "Deadline":
                    challenges = challenges.OrderBy(c => c.Deadline);
                    break;
                case "deadline_desc":
                    challenges = challenges.OrderByDescending(c => c.Deadline);
                    break;
                default:
                    challenges = challenges.OrderByDescending(c => c.Release_Date);
                    break;
            }
            if (isSelected.Count() != 0)
            {
                IEnumerable<string> lId = isSelected.Select((sel, idx) => (sel, (idx + 1).ToString())).Where(l => l.ToTuple().Item1).Select(l => l.Item2);
                challenges = from ch in(from c in challenges
                             join lc in _lcRepository.GetAll()
                             on c.C_Id equals lc.C_Id
                             where lId.Contains(lc.Language_Id)
                             select c).Distinct()
                             orderby ch.Release_Date descending
                             select ch;               
            }
            int pageSize = 10;
            int currStatus = 0;
            if (status.HasValue)
            {
                currStatus = (int)status;
            }
            var model = new ChallengeIndexViewModel()
            {
                Challenges = await PaginatedList<Challenge>.CreateAsync(challenges.AsNoTracking(), pageNumber ?? 1, pageSize),
                Languages = await _lRepository.GetAllListAsync(),
                Status = currStatus
            };
            return View(model);
        }

        #endregion
        #region details
        /// <summary>
        ///  Get detail information of a certain challenge which is assigned by challenge Id
        /// </summary>
        /// <param name="id"> The Id string of a challenge</param>
        /// <returns>A view with all available information of given challenge</returns>
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || id == "")
            {
                Response.StatusCode = 400;
                @ViewBag.ErrorMessage = localizer["EmptyCID"];
                return View("NotFound");

            }
            var challenge = await _repository.GetAll()
                .Include(c => c.Company)
                .Include(c => c.LanguageChallenges)
                .FirstOrDefaultAsync(m => m.C_Id == id);

            if (challenge == null)
            {
                Response.StatusCode = 404;
                @ViewBag.ErrorMessage = localizer["CannotFoundCID", id];
                return View("NotFound");
            }
            bool canTakePartIn = false;
            if (challenge.Release_Date < DateTime.UtcNow && challenge.Deadline > DateTime.UtcNow)
            {
                canTakePartIn = true;
            }     
            var user = _pRepository.GetAll().Include(p => p.Participations).FirstOrDefault(p => p.Id.Equals(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            if (user!=null&&user.Participations != null)
            {
                foreach (var par in user.Participations)
                {
                    if (par.C_Id == id)
                    {
                        canTakePartIn = false;
                    }
                }
            }               
            var detail = new ChallengeDetailViewModel()
            {
                C_Id = challenge.C_Id,
                Title = challenge.Title,
                Bonus = challenge.Bonus,
                Content = challenge.Content,
                Release_Date =challenge.Release_Date,
                Max_Participant = challenge.Max_Participant,
                Available_Quota = GetAvailableQuota(challenge.C_Id),
                Deadline = challenge.Deadline,
                Company = challenge.Company,
                Com_ID = challenge.Com_ID,
                Winner_Id = challenge.Winner_Id,
                Best_Solution_Id = challenge.Best_Solution_Id,
                CanTakePartIn = canTakePartIn,
                IsSolutionOpen = challenge.AllowOpen
            };

            if (detail.Winner_Id != null)
            {
                detail.Winner = _pRepository.FirstOrDefault(u => u.Id == challenge.Winner_Id);
            }

            var langugaes = from c in _repository.GetAll()
                            join lc in _lcRepository.GetAll()
                            on c.C_Id equals lc.C_Id
                            join l in _lRepository.GetAll()
                            on lc.Language_Id equals l.Language_Id
                            where c.C_Id == id
                            select l;

            detail.Languages = await langugaes.ToListAsync();
            return View(detail);
        }
        #endregion

        #region Create
        /// <summary>
        /// Create a new challenge. Get the create form.
        //    This method is only authorized to company user.
        /// </summary>
        /// <returns> A view with form which must be filled out for creating challenge.</returns>
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Create()
        {
            var model = new ChallengeCreateViewModel();
            model.Release_Date = DateTime.UtcNow.Date;
            model.Deadline = DateTime.UtcNow.Date;
            model.Languages = await _lRepository.GetAllListAsync();
            model.IsSelected = new bool[model.Languages.Count];
            return View(model);
        }


        /// <summary>
        /// Create a new challenge and put it into database.
        /// This method is only authorized to company user.
        /// </summary>
        /// <param name="model">ChallengeCreateViewModel with all required information for a challenge</param>
        /// <returns>Details View of this new challenge if create succeed. Else the same view with model error info</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Create(ChallengeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                TimeZoneInfo zone = TZConvert.GetTimeZoneInfo(model.Zone);
                DateTime zone_release = TimeZoneInfo.ConvertTimeToUtc(model.Release_Date, zone);
                DateTime zone_deadline = TimeZoneInfo.ConvertTimeToUtc(model.Deadline, zone);

                List<Language> languages = await _lRepository.GetAllListAsync();
                model.Languages = languages;
                if (zone_release < TimeZoneInfo.ConvertTimeToUtc(DateTime.UtcNow))
                {
                    ModelState.AddModelError(string.Empty, localizer["OnlyReleaseInFuture"]);
                    return View(model);
                }
                if (zone_deadline <= zone_release)
                {
                    ModelState.AddModelError(string.Empty, localizer["DeadlineAfterRelease"]);
                    return View(model);
                }

                Challenge newChallenge = new Challenge
                {
                    C_Id = Guid.NewGuid().ToString(),
                    Title = model.Title,
                    Bonus = model.Bonus,
                    Content = model.Content,
                    Release_Date = zone_release,
                    Deadline = zone_deadline,
                    Max_Participant = model.Max_Participant,
                    Com_ID = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    AllowOpen = model.Visible
                };
                await _repository.InsertAsync(newChallenge);
                List<LanguageChallenge> lc = new List<LanguageChallenge>();
                for (int i = 0; i < model.IsSelected.Count(); i++)
                {
                    if (model.IsSelected[i])
                    {
                        LanguageChallenge toAdd = new LanguageChallenge()
                        {
                            C_Id = newChallenge.C_Id,
                            Language_Id = languages.ElementAt(i).Language_Id,
                        };
                        lc.Add(toAdd);
                       await _lcRepository.InsertAsync(toAdd);                                            
                    }
                }
                logger.LogInformation($"The new Challenge with id {newChallenge.C_Id} is created");         
                return RedirectToAction("Details", new { id = newChallenge.C_Id });
            }
            //if modelstate is not valid
            ModelState.AddModelError(string.Empty, localizer["FailedCreate"]);
            model.Languages = await _lRepository.GetAllListAsync();
            return View(model);
        }
        #endregion

        #region Edit
        // GET: Challenges/Edit/5
        /// <summary>
        /// Check prerequisites of editing challenge and provides information for edit form
        /// </summary>
        /// <param name="id">The Id string of a challenge</param>
        /// <returns>A view with a edit form if passed prerequisites; Else an error view with error message</returns>
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Edit(string id)
        {
            //these three if-cases prevent someone tries to edit a challenge through giving id in route 
            if (id == null || id == "")
            {
                Response.StatusCode = 400;
                @ViewBag.ErrorMessage = localizer["EmptyCID"];
                return View("NotFound");
            }
            var challenge = await _repository.GetAll()
                .Include(c => c.Company)
                .Include(c => c.LanguageChallenges)
                .FirstOrDefaultAsync(c => c.C_Id == id);
            if (challenge == null)
            {
                Response.StatusCode = 404;
                ViewBag.ErrorMessage = localizer["CannotFoundCID", id];
                return View("NotFound");
            }

            if (User.FindFirstValue(ClaimTypes.NameIdentifier) != challenge.Com_ID)
            {
                Response.StatusCode = 403;
                ViewBag.ErrorMessage = localizer["NoAccessOtherCom"];
                return View("Error");
            }

            if(challenge.IsClose)
            {
                Response.StatusCode = 403;
                ViewBag.ErrorMessage = localizer["NoEditAfterClosing"];
                return View("Error");

            }

            ChallengeEditViewModel model = new ChallengeEditViewModel();
            model.Challenge = challenge;

            model.Languages = await _lRepository.GetAllListAsync();
            model.IsSelected = new bool[model.Languages.Count];
            if (model.Challenge.Release_Date > DateTime.UtcNow)
            {
                model.AllowEditDate = true;
            }
            for (int i = 0; i < challenge.LanguageChallenges.Count(); i++)
            {
                String lId = challenge.LanguageChallenges.ElementAt(i).Language_Id;
                model.IsSelected[int.Parse(lId) - 1] = true;
            }
            return View(model);
        }
   
        /// <summary>
        ///  Check if all given para are legal, if yes then update database
        /// </summary>
        /// <param name="model">A ChallengeEditViewModel with form information</param>
        /// <returns> The detail view of edited challenge with updated information</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Edit(ChallengeEditViewModel model)
        {


            if (ModelState.IsValid)
            {
                List<Language> languages = await _lRepository.GetAllListAsync();
                model.Languages = languages;
                string errMsg = checkIllegalOp(model);
                if (errMsg != null)
                {
                    ModelState.AddModelError(string.Empty, errMsg);                  
                    return View(model);
                }
                else
                {
                                
                    var lcList = await _lcRepository.GetAllListAsync(l => l.C_Id == model.Challenge.C_Id);
                    for (int i = 0; i < model.IsSelected.Count(); i++)
                    {
                        var item = await _lcRepository.FirstOrDefaultAsync(lc => lc.Language_Id == languages.ElementAt(i).Language_Id && lc.C_Id == model.Challenge.C_Id);
                        if (model.IsSelected[i])
                        {
                            if (item == null)
                            {
                                item = new LanguageChallenge()
                                {
                                    C_Id = model.Challenge.C_Id,
                                    Language_Id = languages.ElementAt(i).Language_Id
                                };
                                await _lcRepository.InsertAsync(item);
                            }
                        }
                        else
                        {
                            if (item != null)
                            {
                                await _lcRepository.DeleteAsync(item);
                            }
                        }
                    }
                    if (model.Release_Date != DateTime.MinValue)
                    {
                        model.Challenge.Release_Date = TimeZoneInfo.ConvertTimeToUtc(model.Release_Date, TZConvert.GetTimeZoneInfo(model.Zone));
                    }
                    if (model.Deadline != DateTime.MinValue)
                    {
                        model.Challenge.Deadline = TimeZoneInfo.ConvertTimeToUtc(model.Deadline, TZConvert.GetTimeZoneInfo(model.Zone));
                    }
                    try
                    {
                        await _repository.UpdateAsync(model.Challenge);
                        logger.LogInformation($"The information of Challenge {model.Challenge.C_Id} is edited");
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ChallengeExists(model.Challenge.C_Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction("Details", new { id = model.Challenge.C_Id });
                }
            }
            //if modelstate is not valid
            ModelState.AddModelError(string.Empty, "failed to edit the challenge, please try again");
            return View(model);
        }

        private string checkIllegalOp(ChallengeEditViewModel model)
        {
            int bonus = GetCurrentBonus(model.Challenge.C_Id);
            var partiList = _particiRepository.GetAllList(c => c.C_Id == model.Challenge.C_Id);
            int alreadyParticiCount = partiList.Count;

            if (!model.AllowEditDate && bonus > model.Challenge.Bonus)
            {
                return localizer["CannotReduceBonus"];
            }
            if (alreadyParticiCount > model.Challenge.Max_Participant)
            {
                return localizer["CannotReduceParNumber"];
            }
            
            if (TimeZoneInfo.ConvertTimeToUtc(model.Release_Date, TZConvert.GetTimeZoneInfo(model.Zone)) < DateTime.UtcNow
                && model.AllowEditDate)
            {               
                return localizer["OnlyReleaseInFuture"];
            }
            if (model.Deadline < model.Release_Date && model.AllowEditDate)
            {
                return localizer["DeadlineAfterRelease"];
            }
            return null;

        }

        private int GetCurrentBonus(string c_Id)
        {
            var current = _repository.GetAll().Where(c => c.C_Id == c_Id);
            return current.AsNoTracking().First().Bonus;
        }
        #endregion

        #region Participation
 
        /// <summary>
        ///  Add user and challenge to Participation in database and update quota of the challenge
        /// </summary>
        /// <param name="id">The Id string of a challenge</param>
        /// <returns>A view with participation confirmation</returns>
        [Authorize(Roles = "Programmer")]
        public async Task<IActionResult> ParticipateChallenge(string id)
        {
            var challenge = await _repository.FirstOrDefaultAsync(m => m.C_Id == id);         

            if (GetAvailableQuota(id) <= 0 || challenge.Deadline < DateTime.UtcNow || challenge.Release_Date > DateTime.UtcNow)
            {
                { //The corresponding razor page details.cshtml it has restriction that if the conditions are true,
                  // the button which links to this method will not be showed. e.g. this else-condition is not
                  // supposed to be entered. Its an avoidance of entering by URL.
                    ViewBag.Message = localizer["NoAllowParticipation"];
                    return View("Details", new { id = id });
                }
            }

            Participation newParti = new Participation()
            {
                C_Id = id,
                P_Id = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            try
            {
                await _particiRepository.InsertAsync(newParti);
                var user = await _pRepository.FirstOrDefaultAsync(p => p.Id == newParti.P_Id);
                string subject = localizer["SuccessParTitle", challenge.Title];
                string body = localizer["Par", user.Name, challenge.Title];

                await _sender.SendEmailAsync(user.Email,subject,body);
                logger.LogInformation($"The Programmer with id {newParti.P_Id} take part in the Challenge with id {id}");
            }

            
            catch (Exception ex) when (ex is SqlException || ex is InvalidOperationException)
            {             
               throw new Exception("Participation Error");            
            }
            return View(challenge);
        }

        #endregion


        private bool ChallengeExists(string id)
        {
            return _repository.GetAll().Any(e => e.C_Id == id);
        }

        private int GetAvailableQuota(string id)
        {
            var challenge = _repository.GetAll().Where(c => c.C_Id == id);
            var partiList = _particiRepository.GetAllList(c => c.C_Id == id);
            return challenge.AsNoTracking().First().Max_Participant - partiList.Count;
        }




    }
}
