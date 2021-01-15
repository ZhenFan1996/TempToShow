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
        private readonly IRepository<Participation> _particiRepository;

        public SolutionController(IRepository<Solution> _repository, IRepository<Participation> _particiRepository)
        {
            this._particiRepository = _particiRepository;
            this._repository = _repository;
        }
        public IActionResult Add()
        {
            return View();
        }

        public async Task<IActionResult> List(int? pageNumber, string sortOrder, string c_Id)
        {
            ViewData["PointSortParm"] = String.IsNullOrEmpty(sortOrder) ? "Point" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ErrorViewModel errorViewModel = new ErrorViewModel();
            if (c_Id == null || c_Id == "")
            {
                errorViewModel.RequestId = "invalid challenge id!";
                return View("Error", errorViewModel);
            }
            var solutions = from s
                             in _repository.GetAll().Include(i => i.Participation).Where(s => s.Participation.C_Id == c_Id)
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
            var solutionsSorted = solutions.OrderByDescending(c => c.Point).ToList();

            Solution bestSolution = solutionsSorted.FirstOrDefault();
            BestSolutionViewModel bSolution;
            if (bestSolution != null && bestSolution.Point!=null)
            {
                bSolution = new BestSolutionViewModel()
                {
                    Solutions = solutions.ToList(),
                    C_ID = c_Id,
                    Best_Name = bestSolution.Participation.Programmer.Name,
                    Best_Point = bestSolution.Point,
                    Best_URL = bestSolution.URL,
                };
            }
            else
            {
                bSolution = new BestSolutionViewModel()
                {
                    Solutions = solutions.ToList(),
                    C_ID = c_Id,
                    Best_Name = "",
                    Best_Point = 0,
                    Best_URL = "",
                };
            }

            int pageSize = 10;
            return View(bSolution);
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
