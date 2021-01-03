using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        public ChallengesController(IRepository<Challenge> repository, IRepository<PlatformUser> pRepository,
            IRepository<Language> lRepository, IRepository<LanguageChallenge> lcRepository, IRepository<Participation> particiRepository)
        {
            _repository = repository;
            _pRepository = pRepository;
            _lRepository = lRepository;
            _lcRepository = lcRepository;
            _particiRepository = particiRepository;
        }

        //
        // Summary:
        //    Get the list of current active challenges with paging function
        //
        // Returns:
        //    A view with list of current active challenges

        public async Task<IActionResult> Index(int? pageNumber)
        {
            int pageSize = 3;//Temporary value, convenience for testing
            var result = await _repository.FindByAndCreatePaginateAsync(pageNumber ?? 1, pageSize, c => c.Release_Date <= DateTime.Now, c => c.Company);
            return View(result);
        }

        //
        // Summary:
        //    Get detail information of a certain challenge which is assigned by challenge Id
        //
        // Parameter:
        //    The Id string of a challenge
        //
        // Returns:
        //    A view with all available information of given challenge
        public async Task<IActionResult> Details(string id)
        {
            ErrorViewModel errorViewModel = new ErrorViewModel();
            if (id == null || id == "")
            {
                errorViewModel.RequestId = "invalid challenge id value for details";
                return View("Error", errorViewModel);
                //return NotFound();
            }

            var challenge = await _repository
                 .IncludeAndFindOrDefaultAsync(m => m.C_Id == id, c => c.Company, c => c.LanguageChallenges);

            if (challenge == null)
            {
                errorViewModel.RequestId = "there's no challenge with this id, please check again";
                return View("Error", errorViewModel);
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
                Company = challenge.Company,
                Winner_Id = challenge.Winner_Id,
                Best_Solution_Id = challenge.Best_Solution_Id
            };

            List<Language> l = new List<Language>();
            foreach (LanguageChallenge lc in challenge.LanguageChallenges)
            {
                l.Add(_lRepository.FirstOrDefault(l => l.Language_Id == lc.Language_Id));
            }
            detail.Languages = l;
            return View(detail);
        }

        //
        // Summary:
        //    Create a new challenge. Get the create form.
        //    This method is only authorized to company user.
        //
        // Returns:
        //    A view with form which must be filled out for creating challenge.
        [Authorize(Roles = "Company")]
        public IActionResult Create()
        {
            var model = new ChallengeCreateViewModel();
            model.Languages = _lRepository.GetAllListAsync().Result;
            model.IsSelected = new bool[model.Languages.Count];
            return View(model);
        }

        //
        // Summary:
        //    Create a new challenge. Post the create form.
        //    This method is only authorized to company user.
        //
        // Returns:
        //    A view with all the given details at creating challenge, if the given information passed validation check.
        //    A view with error message, if the given information didn't pass validation check.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Create(ChallengeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                List<Language> languages = _lRepository.GetAllListAsync().Result;

                Challenge newChallenge = new Challenge
                {
                    C_Id = Guid.NewGuid().ToString(),
                    Title = model.Title,
                    Bonus = model.Bonus,
                    Content = model.Content,
                    Release_Date = model.Release_Date,
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
                    }
                }
                return RedirectToAction("Details", new { id = newChallenge.C_Id });
            }
            //if modelstate is not valid
            ModelState.AddModelError(string.Empty, "failed to create the challenge, please try again");
            return View(model);
        }

        // GET: Challenges/Edit/5
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var challenge = await _repository.FirstOrDefaultAsync(c => c.C_Id == id);
            if (challenge == null)
            {
                return NotFound();
            }
            ViewData["Com_ID"] = new SelectList(_pRepository.GetAll(), "Id", "Id", challenge.Com_ID);
            return View(challenge);
        }

        // POST: Challenges/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Edit(string id, [Bind("C_Id,Bonus,Title,Content,Release_Date,Max_Participant,Com_ID,Winner_Id,Best_Solution_Id")] Challenge challenge)
        {
            if (id != challenge.C_Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _repository.UpdateAsync(challenge);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChallengeExists(challenge.C_Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Com_ID"] = new SelectList(_pRepository.GetAll(), "Id", "Id", challenge.Com_ID);
            return View(challenge);
        }

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

        //
        // Summary:
        //    Check if the prerequisites for participating a challenge fulfil.
        //
        // Parameter:
        //    The Id string of a challenge
        //
        // Returns:
        //    A view with participation conditions and confirmation button if prerequisites passed.
        [Authorize(Roles = "Programmer")]
        public async Task<IActionResult> ParticipationConfirm(string id)
        {
            var challenge = await _repository.IncludeAndFindOrDefaultAsync(c => c.C_Id == id);
            ErrorViewModel errorViewModel = new ErrorViewModel();
            if (GetAvailableQuota(id) <= 0)
            {
                { //The corresponding razor page details.cshtml it has restriction that if quota is less than 1,
                  // the button which links to this method will not be showed. e.g. this else-condition is not
                  // supposed to be entered
                    errorViewModel.RequestId = "Theres no place anymore";
                    return View("Error", errorViewModel);
                }
            }
            var ifAlreadyParti = await _particiRepository
                 .IncludeAndFindOrDefaultAsync(m => m.P_Id == User.FindFirstValue(ClaimTypes.NameIdentifier)
                 && m.C_Id == id);

            if (ifAlreadyParti == null)
            {
                return View(challenge);
            }

            errorViewModel.RequestId = "You have already participated this challenge";
            return View("Error", errorViewModel);

        }

        //
        // Summary:
        //    Add user and challenge to Participation in database and update quota of the challenge
        //
        // Parameter:
        //    The Id string of a challenge
        //
        // Returns:
        //    A view with participation confirmation
        [Authorize(Roles = "Programmer")]
        public async Task<IActionResult> ParticipateChallenge(string id)
        {
            var challenge = await _repository.IncludeAndFindOrDefaultAsync(m => m.C_Id == id);

            Participation newParti = new Participation()
            {
                C_Id = id,
                P_Id = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            await _particiRepository.InsertAsync(newParti);
            return View(challenge);
        }


        private bool ChallengeExists(string id)
        {
            return _repository.GetAll().Any(e => e.C_Id == id);
        }

        private int GetAvailableQuota(string id)
        {
            Challenge challenge = _repository.FirstOrDefault(c => c.C_Id == id);
            var partiList = _particiRepository.GetAllList(c => c.C_Id == id);
            return challenge.Max_Participant - partiList.Count;
        }
    }
}
