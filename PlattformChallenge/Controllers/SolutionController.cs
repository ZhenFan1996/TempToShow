using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Controllers
{
    public class SolutionController : Controller
    {
        public IActionResult Add()
        {
            return View();
        }

        public void List()
        {
            throw new System.NotImplementedException();
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
