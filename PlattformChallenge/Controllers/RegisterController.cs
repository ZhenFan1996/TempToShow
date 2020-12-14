using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Controllers
{
    public class RegisterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public void ProgrammerRegister()
        {
            throw new System.NotImplementedException();
        }

        public void CompanyRegister()
        {
            throw new System.NotImplementedException();
        }
    }
}
