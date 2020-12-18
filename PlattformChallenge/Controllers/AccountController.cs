using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Controllers
{
    public class AccountController : Controller
    {

        [HttpGet]
        public IActionResult Regiseter() {

            return View();
        }
    }
}
