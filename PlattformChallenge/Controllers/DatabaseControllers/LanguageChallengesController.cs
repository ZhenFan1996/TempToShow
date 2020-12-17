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
    public class LanguageChallengesController : Controller
    {
        private readonly AppDbContext _context;

        public LanguageChallengesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: LanguageChallenges
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.LanguageChallenge.Include(l => l.Challenge).Include(l => l.Language);
            return View(await appDbContext.ToListAsync());
        }

        // GET: LanguageChallenges/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var languageChallenge = await _context.LanguageChallenge
                .Include(l => l.Challenge)
                .Include(l => l.Language)
                .FirstOrDefaultAsync(m => m.Language_Id == id);
            if (languageChallenge == null)
            {
                return NotFound();
            }

            return View(languageChallenge);
        }

        // GET: LanguageChallenges/Create
        public IActionResult Create()
        {
            ViewData["C_Id"] = new SelectList(_context.Challenges, "C_Id", "Content");
            ViewData["Language_Id"] = new SelectList(_context.Languages, "Language_Id", "Language_Id");
            return View();
        }

        // POST: LanguageChallenges/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Language_Id,C_Id")] LanguageChallenge languageChallenge)
        {
            if (ModelState.IsValid)
            {
                _context.Add(languageChallenge);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["C_Id"] = new SelectList(_context.Challenges, "C_Id", "Content", languageChallenge.C_Id);
            ViewData["Language_Id"] = new SelectList(_context.Languages, "Language_Id", "Language_Id", languageChallenge.Language_Id);
            return View(languageChallenge);
        }

        // GET: LanguageChallenges/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var languageChallenge = await _context.LanguageChallenge.FindAsync(id);
            if (languageChallenge == null)
            {
                return NotFound();
            }
            ViewData["C_Id"] = new SelectList(_context.Challenges, "C_Id", "Content", languageChallenge.C_Id);
            ViewData["Language_Id"] = new SelectList(_context.Languages, "Language_Id", "Language_Id", languageChallenge.Language_Id);
            return View(languageChallenge);
        }

        // POST: LanguageChallenges/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Language_Id,C_Id")] LanguageChallenge languageChallenge)
        {
            if (id != languageChallenge.Language_Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(languageChallenge);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LanguageChallengeExists(languageChallenge.Language_Id))
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
            ViewData["C_Id"] = new SelectList(_context.Challenges, "C_Id", "Content", languageChallenge.C_Id);
            ViewData["Language_Id"] = new SelectList(_context.Languages, "Language_Id", "Language_Id", languageChallenge.Language_Id);
            return View(languageChallenge);
        }

        // GET: LanguageChallenges/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var languageChallenge = await _context.LanguageChallenge
                .Include(l => l.Challenge)
                .Include(l => l.Language)
                .FirstOrDefaultAsync(m => m.Language_Id == id);
            if (languageChallenge == null)
            {
                return NotFound();
            }

            return View(languageChallenge);
        }

        // POST: LanguageChallenges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var languageChallenge = await _context.LanguageChallenge.FindAsync(id);
            _context.LanguageChallenge.Remove(languageChallenge);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LanguageChallengeExists(int id)
        {
            return _context.LanguageChallenge.Any(e => e.Language_Id == id);
        }
    }
}
