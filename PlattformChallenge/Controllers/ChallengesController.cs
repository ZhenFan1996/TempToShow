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

        public ChallengesController(IRepository<Challenge> repository,IRepository<PlatformUser> pRepository)
        {
            _repository = repository;
            _pRepository = pRepository;
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
            ErrorViewModel errorViewModel = new ErrorViewModel();
            if (id == null)
            {
                errorViewModel.RequestId = "invalid challenge id value for details";
                return View("Error", errorViewModel);
                //return NotFound();
            }

            var challenge = await _repository.GetAll()
                .Include(c => c.Company)
                .FirstOrDefaultAsync(m => m.C_Id == id);
            if (challenge == null)
            {
                errorViewModel.RequestId = "there's no challenge with this id, please check again";
                return View("Error", errorViewModel);
            }

            return View(challenge);
        }

        // GET: Challenges/Create
        [Authorize(Roles = "Company")]
        public IActionResult Create()
        {
            return View();
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
                Challenge newChallenge = new Challenge
                {
                    C_Id = Guid.NewGuid().ToString(),
                    Title = model.Title,
                    Bonus = model.Bonus,
                    Content = model.Content,
                    Release_Date = model.Release_Date,
                    Max_Participant = model.Max_Participant,
                    Com_ID = User.FindFirstValue(ClaimTypes.NameIdentifier)

            };
                await _repository.InsertAsync(newChallenge);
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
