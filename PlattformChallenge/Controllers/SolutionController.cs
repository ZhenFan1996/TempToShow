using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlattformChallenge.Core.Interfaces;
using PlattformChallenge.Core.Model;
using PlattformChallenge.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Controllers
{
    public class SolutionController : Controller
    {
        private readonly IRepository<Solution> _repository;
        private readonly IRepository<Participation> _particiRepository;
        public IActionResult Add()
        {
            return View();
        }

        public async Task<IActionResult> List(int? pageNumber, string sortOrder)
        {
            ViewData["DateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "Point" : "";
            ViewData["BonusSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            var solutions = from s
                             in _repository.GetAll().Where(s => s.Submit_Date <= DateTime.Now)
                             select s;

            switch (sortOrder)
            {
                case "Point":
                    solutions = solutions.OrderBy(c => c.Point);
                    break;
                case "date_desc":
                    solutions = solutions.OrderByDescending(c => c.Submit_Date);
                    break;
                case "Date":
                    solutions = solutions.OrderBy(c => c.Submit_Date);
                    break;
                default:
                    solutions = solutions.OrderByDescending(c => c.Point);
                    break;
            }
            int pageSize = 10;//Temporary value, convenience for testing
            return View(await PaginatedList<Solution>.CreateAsync(solutions.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public void Delete()
        {
            throw new System.NotImplementedException();
        }

        public void ChangeStatus()
        {
            throw new System.NotImplementedException();
        }
    }
}
