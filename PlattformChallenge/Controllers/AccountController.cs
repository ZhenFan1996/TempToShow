using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PlattformChallenge.Models;
using PlattformChallenge.ViewModels;
using System.Security.Claims;
using PlattformChallenge.Core.Model;

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
        /// <summary>
        /// Enter the registration page
        /// </summary>
        /// <returns>the registration page</returns>
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        /// <summary>
        /// Obtain registration information on the page and form a user joining database
        /// </summary>
        /// <param name="model"></param>Registration information on the page
        /// <returns>The Index of Home </returns>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {

                var user = new PlatformUser
                {
                    Name = model.Name,
                    Email = model.Email,
                    UserName = model.Email
                };
                var result1 = await _userManager.CreateAsync(user, model.Password);
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                if (result1.Succeeded)
                {
                    var result2 = await _userManager.AddToRoleAsync(user, model.RoleName);
                    if (result2.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, false);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        foreach (var error in result2.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                foreach (var error in result1.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);

        }

        #region login
        /// <summary>
        /// Enter  the page of login
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult LogIn()
        {
            return View();
        }
        /// <summary>
        /// Get the login information on the page and try to log in
        /// </summary>
        /// <param name="logInViewModel">the login information</param> 
        /// <returns>the Index of home</returns>
        [HttpPost]
        public async Task<IActionResult> LogIn(LogInViewModel logInViewModel)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(logInViewModel.Email, logInViewModel.Password, logInViewModel.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Error to login, please try again");

            }
            return View(logInViewModel);
        }
        #endregion
        /// <summary>
        /// Log out for the user
        /// </summary>
        /// <returns>The index of Home</returns>
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        /// <summary>
        /// Jump to login failure page
        /// </summary>
        /// <returns></returns>
        public IActionResult AccessDenied()
        {
            var roles = ((ClaimsIdentity)User.Identity).Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
            ViewBag.CurrentRole = roles.FirstOrDefault();
            return View();
        }

        /// <summary>
        /// Go to the page of change password
        /// </summary>
        /// <param name="email"> the email of user</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ChangePassword(string email)
        {
            if (email == null) {
                ModelState.AddModelError("", "no email");
            }
            return View();
        }
        /// <summary>
        /// Get the information in the page and try to change the password
        /// </summary>
        /// <param name="model">the information on the page</param>
        /// <returns>The index and log out</returns>
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var checkOk= await _userManager.CheckPasswordAsync(user,model.Orginal);
                if (!checkOk) {
                    ModelState.AddModelError("","please check the orgin password");
                    return View();
                }
                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignOutAsync();
                        return RedirectToAction("Index","Home");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }
            return View(model);
        }
    }
    }
