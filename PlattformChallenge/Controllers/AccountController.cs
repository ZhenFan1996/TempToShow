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
using Microsoft.Extensions.Logging;
using PlattformChallenge.Services;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace PlattformChallenge.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<PlatformUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ILogger<AccountController> logger;
        private readonly IEmailSender _sender;
        private readonly IStringLocalizer<AccountController> localizer;
        private SignInManager<PlatformUser> _signInManager;

        /// <summary>
        /// Constructor of AccountController
        /// </summary>
        /// <param name="userManager">Instance of UserManager provided by Identity. Generic type is PlatformUser</param>
        /// <param name="signInManager">Instance of signInManager provided by Identity. Generic type is PlatformUser</param>
        /// <param name="roleManager">Instance of roleManager provided by Identity. Generic type is IdentityRole</param>
        /// <param name="logger">Instance of log. Generic type is AccountController</param>
        /// <param name="sender">Instance of Email sender.</param>
        /// <param name="localizer">Instance of StringLocalizer</param>
        public AccountController(UserManager<PlatformUser> userManager, SignInManager<PlatformUser> signInManager, RoleManager<IdentityRole> roleManager,ILogger<AccountController> logger, IEmailSender sender
            , IStringLocalizer<AccountController> localizer)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._roleManager = roleManager;
            this.logger = logger;
            this._sender = sender;
            this.localizer = localizer;
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
                if (result1.Succeeded)
                {
                    var result2 = await _userManager.AddToRoleAsync(user, model.RoleName);
                    if (result2.Succeeded)
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmationLink = Url.Action("ConfirmEmail", "Account", new
                        {
                            userId = user.Id,
                            token = token
                        }, Request.Scheme);
                        logger.Log(LogLevel.Warning, confirmationLink);

                        string subject = localizer["ConfirmEmailTitle"];
                        string body =
                         localizer["EmailConfirm",user.Name, confirmationLink];

                        await _sender.SendEmailAsync(user.Email, subject, body);
                        ViewBag.Message = localizer["SentConfirmEmail"]; 
                        return View("ActivateUserEmail");
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

        /// <summary>
        /// Confirm user email after register.
        /// </summary>
        /// <param name="userId">current user Id</param>
        /// <param name="token">token which has been sent to user's email</param>
        /// <returns>Successful or Error View</returns>
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId,string token) {

            if (userId == null || token == null)
            {
                return RedirectToAction("index", "home");
            }
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"Current ID :{userId} is invaild";
                return View("NotFound");
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return View();
            }

            ViewBag.ErrorMessage = "Failed Confirm";
            return View("Error");


        }


        #region login
        /// <summary>
        /// Enter the page of login
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult LogIn()
        {
            var model = new LogInViewModel() {
                Not_Confirmed = false
            };

            return View(model);
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
                var user = await _userManager.FindByEmailAsync(logInViewModel.Email);
                if (user != null && !user.EmailConfirmed) {
                    logInViewModel.Not_Confirmed = true;
                    return View(logInViewModel);
                }

                var result = await _signInManager.PasswordSignInAsync(logInViewModel.Email, logInViewModel.Password, logInViewModel.RememberMe, false);
                if (result.Succeeded)
                {
                    logger.LogInformation($"The account {logInViewModel.Email} is successly log in");
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Error to login, please try again");

            }
            return View(logInViewModel);
        }
        #endregion

        /// <summary>
        /// If an email still has not been confirmed, use this function to try to active it.
        /// </summary>
        /// <param name="email">Email address of current user</param>
        /// <returns>ActiveUserEmail with given ViewBag.Message</returns>
        [HttpGet]
        public async Task<IActionResult> ActivateUserEmail(string email)
        {
       
                var user = await _userManager.FindByEmailAsync(email);

                if (user != null)
                {
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var confirmationLink = Url.Action("ConfirmEmail", "Account",
                    new { userId = user.Id, token = token }, Request.Scheme);

                    logger.Log(LogLevel.Warning, confirmationLink);

                    string subject = localizer["ConfirmEmailTitle"];

                    string body = localizer["EmailConfirm", user.Name, confirmationLink];

                    await _sender.SendEmailAsync(user.Email, subject, body);
                    ViewBag.Message = localizer["SentConfirmEmail"];
                    return View();
                }
                else {
                    ViewBag.Message = localizer["ConfirmedLogIn"];
                    return View();
                }
                }
            throw new Exception("The Email is invaild");
         
        }
        /// <summary>
        /// HttpGet - Load the site in situation of forgotting password
        /// </summary>
        /// <returns>ForgotPassword View</returns>
        [HttpGet]
        public IActionResult ForgotPassword() {

            return View();
        }

        /// <summary>
        /// HttpPost - Send a confirmation email to the given email adress in order to reset the password
        /// </summary>
        /// <param name="model">ForgotPasswordViewModel with given email adress</param>
        /// <returns>Returns ForgotPasswordConfirmation View if succeed. Else show the formular (ForgotPasswordViewModel) again</returns>
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model) {
            if (ModelState.IsValid)
            {              
                var user = await _userManager.FindByEmailAsync(model.Email);
              
                if (user != null && await _userManager.IsEmailConfirmedAsync(user))
                {                   
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    
                    var passwordResetLink = Url.Action("ResetPassword", "Account",
                            new { email = model.Email, token = token }, Request.Scheme);

                    string subject = localizer["ForgotPasswordTitle"]; 

                    string body = localizer["ForgotPassword", user.Name, passwordResetLink];                      

                    await _sender.SendEmailAsync(user.Email, subject, body);
                    ViewBag.Message = localizer["SentPasswordEmail"];
                    return View("ForgotPasswordConfirmation");
                }

                return View("ForgotPasswordConfirmation");
            }

            return View(model);

        }

        /// <summary>
        /// HttpGet - Load the site of resetting password
        /// </summary>
        /// <param name="token">random generated token (sent to email)</param>
        /// <param name="email">current user's email</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ResetPassword(string token, string email) {

            if (token == null || email == null) {
                ModelState.AddModelError("", "Invaild Token");

            }
            return View();
        }

        /// <summary>
        /// HttpPost - Set the new password of current user in database
        /// </summary>
        /// <param name="model">ResetPasswordViewModel with email, new password, confirmed password and token</param>
        /// <returns>ChangePasswordConfirm View if succeed; Notfound View if user email is invalid; 
        /// View with model error message if other failure</returns>
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model) {
            if (ModelState.IsValid) {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        return View("ChangePasswordConfirm");
                    }
                    else
                    {

                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }
                }
                else {
                    Response.StatusCode = 400;
                    @ViewBag.ErrorMessage = "Invalid user email";
                    return View("NotFound");
                }
                
            }
            return View(model);
        }



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
        public async Task<IActionResult> ChangePassword(string email)
        {
            if (email == null) {
                ModelState.AddModelError("", "no email");
            }

            var cur = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (!cur.Email.Equals(email)) 
            {
                ErrorViewModel errorViewModel = new ErrorViewModel();
                errorViewModel.RequestId = localizer["CurrentUserWrong"];
                return View("Error", errorViewModel);
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
                var checkOk= await _userManager.CheckPasswordAsync(user,model.Original);
                if (!checkOk) {
                    ModelState.AddModelError("", localizer["OriginPasswordWrong"]);
                    return View();
                }
                if (user != null)
                {                  
                    var result = await _userManager.ChangePasswordAsync(user, model.Original, model.Password);
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
