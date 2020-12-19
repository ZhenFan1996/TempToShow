using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PlattformChallenge.Models;
using PlattformChallenge.ViewModels;

namespace PlattformChallenge.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<PlatformUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private SignInManager<PlatformUser> _signInManager;

        public AccountController(UserManager<PlatformUser> userManager, SignInManager<PlatformUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._roleManager = roleManager;

        }

        [HttpGet]
        public async Task<IActionResult> Register() {
            await EnsureRolesAsync(_roleManager, "Programmer");
            await EnsureRolesAsync(_roleManager, "Company");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid) {

                var user = new PlatformUser
                {
                    UserName = model.Name,
                    Email = model.Email
                };

                var result1 = await _userManager.CreateAsync(user, model.Password);
                var result2 = await _userManager.AddToRoleAsync(user, model.RoleName);

                if (result1.Succeeded && result2.Succeeded) {
                    await _signInManager.SignInAsync(user,isPersistent: false);
                    return RedirectToAction("index", "home");
                }
                foreach (var error in result1.Errors) {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                foreach (var error in result2.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
             
            }
            return View(model);

        }
                     
          private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager, string Rolename)
            {
                var alreadyExists = await roleManager.RoleExistsAsync(Rolename);
                if (alreadyExists) return;
                await roleManager.CreateAsync(new IdentityRole(Rolename));
            }
    }
}
