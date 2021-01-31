using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
using PlattformChallenge.ViewModels;

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

        public ChallengesController(IRepository<Challenge> repository, IRepository<PlatformUser> pRepository,
            IRepository<Language> lRepository, IRepository<LanguageChallenge> lcRepository, IRepository<Participation> particiRepository,
            ILogger<ChallengesController> looger
            )
        {
            _repository = repository;
            _pRepository = pRepository;
            _lRepository = lRepository;
            _lcRepository = lcRepository;
            _particiRepository = particiRepository;
            this.logger = looger;
        }
        #region list

        /// <summary>
        ///  Get the list of current active challenges with paging function
        //    default situation is return the challenges, which descending sorted by date
        /// </summary>
        /// <param name="pageNumber">which page it should be shown</param>
        /// <param name="sortOrder">which sort criteria</param>
        /// <param name="searchString">which search keyword</param>
        /// <returns>A view with list of current active challenges</returns>
        public async Task<IActionResult> Index(int? pageNumber, string sortOrder, string searchString)
        {
            ViewData["DateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "Date" : "";
            ViewData["BonusSortParm"] = sortOrder == "Bonus" ? "bonus_desc" : "Bonus";
            ViewData["QuotaSortParm"] = sortOrder == "Quota" ? "quota_desc" : "Quota";
            ViewData["CurrentFilter"] = searchString;
            var challenges = from c
                             in _repository.GetAll().Where(c => c.Release_Date <= DateTime.Now).Include(c => c.Company)
                             select c;
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
                default:
                    challenges = challenges.OrderByDescending(c => c.Release_Date);
                    break;
            }
            int pageSize = 10;
            return View(await PaginatedList<Challenge>.CreateAsync(challenges.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        #endregion

        /// <summary>
        ///  Get detail information of a certain challenge which is assigned by challenge Id
        /// </summary>
        /// <param name="id"> The Id string of a challenge</param>
        /// <returns>A view with all available information of given challenge</returns>
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || id == "")
            {
                throw new Exception("invalid challenge id value for details");
            
            }
            var challenge = await _repository.GetAll()
                .Include(c => c.Company)
                .Include(c => c.LanguageChallenges)
                .FirstOrDefaultAsync(m => m.C_Id == id);

            if (challenge == null)
            {
                Response.StatusCode = 404;
                @ViewBag.ErrorMessage = $"The Challenge with id {id} can not be found";
                return View("NotFound");
            }
            bool canTakePartIn = true;
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
                Release_Date = challenge.Release_Date,
                Max_Participant = challenge.Max_Participant,
                Available_Quota = GetAvailableQuota(challenge.C_Id),
                Deadline =challenge.Deadline,
                Company = challenge.Company,
                Winner_Id = challenge.Winner_Id,
                Best_Solution_Id = challenge.Best_Solution_Id,
                CanTakePartIn = canTakePartIn
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
            model.Languages = await _lRepository.GetAllListAsync();
            model.IsSelected = new bool[model.Languages.Count];
            return View(model);
        }

      
        /// Create a new challenge. Post the create form.
        //    This method is only authorized to company user.
        /// </summary>
        /// <param name="model">A ChallengeCreateViewModel</param>
        /// <returns>A view with all the given details at creating challenge, if the given information passed validation check.
        ///   A view with error message, if the given information didn't pass validation check.
        /// <summary></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Create(ChallengeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                List<Language> languages = await _lRepository.GetAllListAsync();

                Challenge newChallenge = new Challenge
                {
                    C_Id = Guid.NewGuid().ToString(),
                    Title = model.Title,
                    Bonus = model.Bonus,
                    Content = model.Content,
                    Release_Date = model.Release_Date,
                    Deadline = model.Deadline,
                    Max_Participant = model.Max_Participant,
                    Com_ID = User.FindFirstValue(ClaimTypes.NameIdentifier),

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
                        logger.LogInformation($"The new Challenge with id {newChallenge.C_Id} is created");
                    }
                }
                return RedirectToAction("Details", new { id = newChallenge.C_Id });
            }
            //if modelstate is not valid
            ModelState.AddModelError(string.Empty, "failed to create the challenge, please try again");
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
            ErrorViewModel errorViewModel = new ErrorViewModel();
            //these three if-cases prevent someone tries to edit a challenge through giving id in route 
            if (id == null || id == "")
            {
                throw new Exception("invalid challenge id value for editing");
            }
            var challenge = await _repository.GetAll()
                .Include(c => c.Company)
                .Include(c => c.LanguageChallenges)
                .FirstOrDefaultAsync(c => c.C_Id == id);
            if (challenge == null)
            {
                Response.StatusCode = 404;
                ViewBag.ErrorMessage = $"there's no challenge with this id ={id}, please check again";
                return View("NotFound");
            }

            if (User.FindFirstValue(ClaimTypes.NameIdentifier) != challenge.Com_ID)
            {
                throw new Exception("You can't edit challenge from other company");
            }

            if(challenge.Winner_Id != null)
            {
                throw new Exception("You can not edit a closed challenge"); 
                
            }

            ChallengeEditViewModel model = new ChallengeEditViewModel();
            model.Challenge = challenge;
            model.Languages = await _lRepository.GetAllListAsync();
            model.IsSelected = new bool[model.Languages.Count];
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
                int bonus = GetCurrentBonus(model.Challenge.C_Id);
                var partiList = _particiRepository.GetAllList(c => c.C_Id == model.Challenge.C_Id);
                int alreadyParticiCount = partiList.Count;
                List<Language> languages = await _lRepository.GetAllListAsync();
                model.Languages = languages;
                if (bonus > model.Challenge.Bonus)
                {
                    ModelState.AddModelError(string.Empty, "You can't change to a less bonus");
                    return View(model);
                }
                if (alreadyParticiCount > model.Challenge.Max_Participant)
                {
                    ModelState.AddModelError(string.Empty, "You can't change maximal participation to this number, there's already more users participated");
                    return View(model);
                }

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

            //if modelstate is not valid
            ModelState.AddModelError(string.Empty, "failed to edit the challenge, please try again");
            return View(model);
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

            if (GetAvailableQuota(id) <= 0)
            {
                { //The corresponding razor page details.cshtml it has restriction that if quota is less than 1,
                  // the button which links to this method will not be showed. e.g. this else-condition is not
                  // supposed to be entered
                    throw new Exception("Theres no place anymore");
                    
                }
            }

            if (challenge.Deadline < DateTime.Now) {
                throw new Exception("Registration time has passed");
                
            }

            Participation newParti = new Participation()
            {
                C_Id = id,
                P_Id = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            try
            {
                await _particiRepository.InsertAsync(newParti);
                logger.LogInformation($"The Programmer with id {newParti.P_Id} take part in the Challenge with id {id}");
            }
            catch (Exception ex) when (ex is SqlException || ex is InvalidOperationException)
            {             
               throw new Exception("You have already participated this challenge");            
            }
            return View(challenge);
        }

        #endregion

        #region delete

        // GET: Challenges/Delete/5
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var challenge = await _repository.GetAll()
                .Include(c => c.Company)
                .FirstOrDefaultAsync(m => m.C_Id == id);
            if (challenge == null)
            {
                return NotFound();
            }

            return View(challenge);
        }

        // POST: Challenges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {

            var challenge = await _repository.FirstOrDefaultAsync(c => c.C_Id == id);
            challenge = await _repository.DeleteAsync(challenge);
            return RedirectToAction(nameof(Index));
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
