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
    public class ProgrammersController : Controller
    {
        private readonly AppDbContext _context;

        public ProgrammersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Programmers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Programmers.ToListAsync());
        }

        // GET: Programmers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var programmer = await _context.Programmers
                .FirstOrDefaultAsync(m => m.User_Id == id);
            if (programmer == null)
            {
                return NotFound();
            }

            return View(programmer);
        }

        // GET: Programmers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Programmers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Firstname,Surname,User_Id,Email,Password,AccountTyp")] Programmer programmer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(programmer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(programmer);
        }

        // GET: Programmers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var programmer = await _context.Programmers.FindAsync(id);
            if (programmer == null)
            {
                return NotFound();
            }
            return View(programmer);
        }

        // POST: Programmers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Firstname,Surname,User_Id,Email,Password,AccountTyp")] Programmer programmer)
        {
            if (id != programmer.User_Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(programmer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProgrammerExists(programmer.User_Id))
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
            return View(programmer);
        }

        // GET: Programmers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var programmer = await _context.Programmers
                .FirstOrDefaultAsync(m => m.User_Id == id);
            if (programmer == null)
            {
                return NotFound();
            }

            return View(programmer);
        }

        // POST: Programmers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var programmer = await _context.Programmers.FindAsync(id);
            _context.Programmers.Remove(programmer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProgrammerExists(int id)
        {
            return _context.Programmers.Any(e => e.User_Id == id);
        }
    }
}
