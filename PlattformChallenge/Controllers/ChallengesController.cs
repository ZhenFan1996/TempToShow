using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlattformChallenge.Models;

namespace PlattformChallenge.Controllers
{
    public class ChallengesController : Controller
    {
        private readonly AppDbContext _context;

        public ChallengesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Challenges

        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Challenges.Include(c => c.Company
            ).Where(c => c.Release_Date <= DateTime.UtcNow);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Challenges/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var challenge = await _context.Challenges
                .Include(c => c.Company)
                .FirstOrDefaultAsync(m => m.C_Id == id);
            if (challenge == null)
            {
                return NotFound();
            }

            return View(challenge);
        }

        // GET: Challenges/Create
        [Authorize(Roles = "Company")]
        public IActionResult Create()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity.Name;
            ViewBag.Com_Id = userId;
            ViewBag.Com_Name = userName;
            return View();
        }

        // POST: Challenges/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Create([Bind("C_Id,Bonus,Title,Content,Release_Date,Max_Participant,Com_ID,Winner_Id,Best_Solution_Id")] Challenge challenge)
        {
            if (ModelState.IsValid)
            {
                challenge.C_Id = Guid.NewGuid().ToString();
                _context.Add(challenge);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Com_ID"] = new SelectList(_context.Set<PlatformUser>(), "Id", "Id", challenge.Com_ID);
            return View(challenge);
        }

        // GET: Challenges/Edit/5
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null)
            {
                return NotFound();
            }
            ViewData["Com_ID"] = new SelectList(_context.Set<PlatformUser>(), "Id", "Id", challenge.Com_ID);
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
                    _context.Update(challenge);
                    await _context.SaveChangesAsync();
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
            ViewData["Com_ID"] = new SelectList(_context.Set<PlatformUser>(), "Id", "Id", challenge.Com_ID);
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

            var challenge = await _context.Challenges
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
            var challenge = await _context.Challenges.FindAsync(id);
            _context.Challenges.Remove(challenge);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChallengeExists(string id)
        {
            return _context.Challenges.Any(e => e.C_Id == id);
        }
    }
}
