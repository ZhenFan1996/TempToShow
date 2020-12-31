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

        public ChallengesController(IRepository<Challenge> repository,IRepository<PlatformUser> pRepository,IRepository<Language> lRepository,IRepository<LanguageChallenge> lcRepository)
        {
            _repository = repository;
            _pRepository = pRepository;
            _lRepository = lRepository;
            _lcRepository = lcRepository;
        }

        // GET: Challenges

        public async Task<IActionResult> Index()
        {

            var appDbContect = _repository.GetAll().Include(c => c.Company
            ).Where(c => c.Release_Date <= DateTime.Now);
            return View(await appDbContect.ToListAsync());
        }



        // GET: Challenges/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var challenge = await _repository.GetAll()
                .Include(c => c.Company)
                .Include(c => c.LanguageChallenges)
                .FirstOrDefaultAsync(m => m.C_Id == id);

            if (challenge == null)
            {
                return NotFound();
            }
            var detail = new ChallengeDetailViewModel()
            {
                C_Id = challenge.C_Id,
                Title = challenge.Title,
                Bonus = challenge.Bonus,
                Content = challenge.Content,
                Release_Date = challenge.Release_Date,
                Max_Participant = challenge.Max_Participant,
                Company = challenge.Company,
                Winner_Id = challenge.Winner_Id,
                Best_Solution_Id = challenge.Best_Solution_Id
            };

            List<Language> l = new List<Language>();
            foreach (LanguageChallenge lc in challenge.LanguageChallenges)
            {
                l.Add(_lRepository.FirstOrDefault(l => l.Language_Id == lc.Language_Id));
            }  
            detail.langugaes = l;
            return View(detail);
        }

        // GET: Challenges/Create
        [Authorize(Roles ="Company")]
        public IActionResult Create()
        {
            var model = new ChallengeCreateViewModel();
            model.Languages = _lRepository.GetAllListAsync().Result;
            model.IsSelected = new bool[model.Languages.Count];
            return View(model);
        }




        // POST: Challenges/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("C_Id,Bonus,Title,Content,Release_Date,Max_Participant,Com_ID,Winner_Id,Best_Solution_Id")] Challenge challenge)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        challenge.C_Id = Guid.NewGuid().ToString();
        //        _context.Add(challenge);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["Com_ID"] = new SelectList(_context.Set<PlatformUser>(), "Id", "Id", challenge.Com_ID);
        //    return View(challenge);
        //}


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
                return RedirectToAction("Details", new { id = newChallenge.C_Id});
            }

            return View("Index");
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

        private bool ChallengeExists(string id)
        {
            return _repository.GetAll().Any(e => e.C_Id == id);
        }
    }
}
