using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlattformChallenge.Core.Interfaces;
using PlattformChallenge.Core.Model;
using PlattformChallenge.Models;
using PlattformChallenge.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Controllers
{
    public class SolutionController : Controller
    {
        private readonly IRepository<Solution> _repository;
        private readonly IRepository<Challenge> _cRepository;

        public SolutionController(IRepository<Solution> _repository, IRepository<Challenge> _cRepository)
        {
            this._cRepository = _cRepository;
            this._repository = _repository;
        }

        
        public IActionResult Add()
        {
            return View();
        }

        /// <summary>
        /// Get the list of solutions of the selected challenge and return the best solution with highest point
        ///    default situation is return the challenges, which descending sorted by point
        /// </summary>
        /// <param name="pageNumber">Which page to show</param>
        /// <param name="sortOrder">According to which parameter to sort</param>
        /// <param name="c_Id">Challenge ID to which this list belongs</param>
        /// <returns>A view with list of solutions and best solution</returns>
        public async Task<IActionResult> List(int? pageNumber, string sortOrder,string c_Id)
        {
            ViewData["PointSortParm"] = String.IsNullOrEmpty(sortOrder) ? "Point" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            int pageSize = 10;

            if (c_Id == null) {
                throw new Exception("Invaild c_id");
            }
     
            var solutions = from s
                             in _repository.GetAll()
                             .Include(i => i.Participation) 
                             .ThenInclude(p=>p.Challenge)
                             .ThenInclude(c=>c.Company)
                             .Include(i=>i.Participation)
                             .ThenInclude(p=>p.Programmer)
                             .Where(s => s.Participation.C_Id == c_Id)
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
            var solutionsSorted = await solutions.OrderByDescending(c => c.Point).ToListAsync();

            Solution bestSolution = solutionsSorted.FirstOrDefault();
            BestSolutionViewModel bSolution;
            var winner = (from c
                            in _cRepository.GetAll()
                            .Where(c => c.C_Id == c_Id)
                         select c.Winner_Id).Single();
            bool visible=  (from c
                             in _cRepository.GetAll()
                             .Where(c => c.C_Id == c_Id)
                                  select c.AllowOpen).Single();
            bool isClose = (from c
                             in _cRepository.GetAll()
                             .Where(c => c.C_Id == c_Id)
                            select c.IsClose).Single();

            bSolution = new BestSolutionViewModel();
            bSolution.IsClose = isClose;
            bSolution.Solutions = await PaginatedList<Solution>.CreateAsync(solutions.AsNoTracking(), pageNumber ?? 1, pageSize);
            bSolution.C_ID = c_Id;
            bSolution.Winner_ID = winner;
            bSolution.Visible = visible;
            if (bestSolution != null && bestSolution.Point != null)
            {
                bSolution.Best_Name = bestSolution.Participation.Programmer.Name;
                bSolution.Best_Point = bestSolution.Point;
                bSolution.Best_URL = bestSolution.URL;
                bSolution.S_ID = bestSolution.S_Id;
            }
            else
            {
                bSolution.Best_Name = "";
                bSolution.Best_Point = null;
                bSolution.Best_URL = "";
                bSolution.S_ID = null;
            }

            return View(bSolution);
        }
   
    }
}
