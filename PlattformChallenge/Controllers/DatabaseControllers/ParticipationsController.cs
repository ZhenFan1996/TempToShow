using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlattformChallenge.Models;

namespace PlattformChallenge.Controllers.DatabaseControllers
{
    public class ParticipationsController : Controller
    {
        private readonly AppDbContext _context;

        public ParticipationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Participations
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Participations.Include(p => p.Challenge).Include(p => p.Programmer);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Participations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participation = await _context.Participations
                .Include(p => p.Challenge)
                .Include(p => p.Programmer)
                .FirstOrDefaultAsync(m => m.C_Id == id);
            if (participation == null)
            {
                return NotFound();
            }

            return View(participation);
        }

        // GET: Participations/Create
        public IActionResult Create()
        {
            ViewData["C_Id"] = new SelectList(_context.Challenges, "C_Id", "Content");
            ViewData["P_Id"] = new SelectList(_context.Programmers, "User_Id", "Email");
            return View();
        }

        // POST: Participations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("C_Id,P_Id,IsWinner")] Participation participation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(participation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["C_Id"] = new SelectList(_context.Challenges, "C_Id", "Content", participation.C_Id);
            ViewData["P_Id"] = new SelectList(_context.Programmers, "User_Id", "Email", participation.P_Id);
            return View(participation);
        }

        // GET: Participations/Edit/5
        public async Task<IActionResult> Edit(int? C_Id,int? P_Id)
        {
            if (C_Id == null || P_Id == null)
            {
                return NotFound();
            }

            var participation = await _context.Participations.FindAsync(C_Id);
            if (participation == null)
            {
                return NotFound();
            }
            ViewData["C_Id"] = new SelectList(_context.Challenges, "C_Id", "Content", participation.C_Id);
            ViewData["P_Id"] = new SelectList(_context.Programmers, "User_Id", "Email", participation.P_Id);
            return View(participation);
        }

        // POST: Participations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("C_Id,P_Id,IsWinner")] Participation participation)
        {
            if (id != participation.C_Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(participation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParticipationExists(participation.C_Id))
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
            ViewData["C_Id"] = new SelectList(_context.Challenges, "C_Id", "Content", participation.C_Id);
            ViewData["P_Id"] = new SelectList(_context.Programmers, "User_Id", "Email", participation.P_Id);
            return View(participation);
        }

        // GET: Participations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var participation = await _context.Participations
                .Include(p => p.Challenge)
                .Include(p => p.Programmer)
                .FirstOrDefaultAsync(m => m.C_Id == id);
            if (participation == null)
            {
                return NotFound();
            }

            return View(participation);
        }

        // POST: Participations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var participation = await _context.Participations.FindAsync(id);
            _context.Participations.Remove(participation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParticipationExists(int id)
        {
            return _context.Participations.Any(e => e.C_Id == id);
        }
    }
}
