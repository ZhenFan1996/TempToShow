using Microsoft.AspNetCore.Identity;
using Moq;
using PlattformChallenge.Controllers;
using Xunit;
using System;
using System.Collections.Generic;
using System.Text;
using PlattformChallenge.Core.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using PlattformChallenge.ViewModels;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PlattformChallenge.UnitTest.Controllers
{
    public class AccountControllerShould
    {

        private readonly AccountController _sut;
        private readonly Mock<UserManager<PlatformUser>> _userManager;
        private readonly Mock<RoleManager<IdentityRole>> _roleManager;
        private readonly Mock<SignInManager<PlatformUser>> _signInManager;

        public AccountControllerShould()
        {
            _userManager = MockUserManager<PlatformUser>();
            _roleManager = MockRoleManager();
            _signInManager = MockSignInManager(_userManager);
            _sut = new AccountController(_userManager.Object, _signInManager.Object, _roleManager.Object);
        }

    
        /// <summary>
        /// [TestCase-ID: 1-1] Test if the view of register is the expected type.
        /// </summary>
        [Fact]
        public void ReturnViewForRegister()
        {
            _roleManager.Setup(r => r.RoleExistsAsync(It.Is<string>(s => s.Equals("Company") || s.Equals("Programmer"))))
                .Returns(Task.FromResult(true));

            IActionResult result = _sut.Register();

            Assert.IsType<ViewResult>(result);

        }
        /// <summary>
        /// [TestCase-ID: 1-2] Test if the register is unsuccess,when the modalstate of page is unvaild
        /// </summary>
        [Fact]
        public async Task InVaildModelStateForRegister()
        {
            _sut.ModelState.AddModelError("key", "error Test");
            RegisterViewModel model = new RegisterViewModel()
            {
                Name = "Zhen",
                Email = "ubumh@student.kit.edu",
                Password = "a123456",
                RoleName = "Programmer"
            };

           var result= await _sut.Register(model);
            Assert.IsType<ViewResult>(result);
            var value = result as ViewResult;
            Assert.Equal(model, value.Model);
            _userManager.Verify(x => x.CreateAsync(It.IsAny<PlatformUser>(), It.IsAny<string>()), Times.Never);
            _userManager.Verify(x => x.AddToRoleAsync(It.IsAny<PlatformUser>(), It.Is<string>(s => s.Equals(model.RoleName))), Times.Never);
            _signInManager.Verify(x => x.SignInAsync(It.IsAny<PlatformUser>(), It.IsAny<bool>(), null), Times.Never);

        }
        /// <summary>
        /// [TestCase-ID: 1-3] Test if the register is unsuccess, if the create methode failed
        /// </summary>
        [Fact]
        public async Task FailedCreateAsync()
        {
            RegisterViewModel model = new RegisterViewModel()
            {
                Name = "Zhen",
                Email = "ubumh@student.kit.edu",
                Password = "a123456",
                RoleName = "Programmer"
            };
            _userManager.Setup(x => x.CreateAsync(It.IsAny<PlatformUser>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Failed(new IdentityError[] {
                            new IdentityError(){
                                Description = "test error1" },
                            new IdentityError(){
                                Description = "test error2"}
        }));
            var result=await _sut.Register(model);
            Assert.IsType<ViewResult>(result);
            var value = result as ViewResult;
            Assert.Equal(model, value.Model);
            IEnumerable<ModelError> allErrors = _sut.ModelState.Values.SelectMany(v => v.Errors);
            Assert.Equal("test error1", allErrors.ElementAt(0).ErrorMessage);
            Assert.Equal("test error2", allErrors.ElementAt(1).ErrorMessage);
            _userManager.Verify(x => x.AddToRoleAsync(It.IsAny<PlatformUser>(), It.Is<string>(s => s.Equals(model.RoleName))), Times.Never);
            _signInManager.Verify(x => x.SignInAsync(It.IsAny<PlatformUser>(), It.IsAny<bool>(), null), Times.Never);
        }
        /// <summary>
        /// [TestCase-ID: 1-4] Test if the reigister is unsuccess, when the addtoRole function failed
        /// </summary>
        [Fact]
        public async Task FailedAddToRoleAsync()
        {
            PlatformUser userForUserManager = null;
            string toCheckPassword = null;
            RegisterViewModel model = new RegisterViewModel()
            {
                Name = "Zhen",
                Email = "ubumh@student.kit.edu",
                Password = "a123456",
                RoleName = "Programmer"
            };
            _userManager.Setup(x => x.CreateAsync(It.IsAny<PlatformUser>(), It.IsAny<string>()))
                  .ReturnsAsync(IdentityResult.Success).Callback<PlatformUser, string>((x, y) =>
                  {
                      userForUserManager = x; ;
                      toCheckPassword = y;
                  });
            _userManager.Setup(x => x.AddToRoleAsync(It.IsAny<PlatformUser>(), It.Is<string>(s => s.Equals(model.RoleName))))
             .ReturnsAsync(IdentityResult.Failed(new IdentityError[] {
                            new IdentityError(){
                                Description = "test error1" },
                            new IdentityError(){
                                Description = "test error2"}}));
           var result= await _sut.Register(model);
            IEnumerable<ModelError> allErrors = _sut.ModelState.Values.SelectMany(v => v.Errors);
            Assert.Equal("test error1", allErrors.ElementAt(0).ErrorMessage);
            Assert.Equal("test error2", allErrors.ElementAt(1).ErrorMessage);
            _signInManager.Verify(x => x.SignInAsync(It.IsAny<PlatformUser>(), It.IsAny<bool>(), null), Times.Never);
            Assert.Equal(userForUserManager.Name, model.Name);
            Assert.Equal(userForUserManager.Email, model.Email);
            Assert.Equal(userForUserManager.UserName, model.Email);
            Assert.Equal(toCheckPassword, model.Password);
            Assert.IsType<ViewResult>(result);
        }
        /// <summary>
        /// [TestCase-ID: 1-5] Test if the register is success, when all the information are vaild 
        /// </summary>
        [Fact]
        public async Task SaveUserInfoAndReturnViewAsync()
        {
            PlatformUser userForUserManager = null;
            PlatformUser userForRoleManager = null;
            string toCheckPassword = null;
            string rollName = null;
            RegisterViewModel model = new RegisterViewModel()
            {
                Name = "Zhen",
                Email = "ubumh@student.kit.edu",
                Password = "a123456",
                RoleName = "Programmer"
            };
            _userManager.Setup(x => x.CreateAsync(It.IsAny<PlatformUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success).Callback<PlatformUser, string>((x, y) =>
                {
                    userForUserManager = x; 
                    toCheckPassword = y;
                });
            _userManager.Setup(x => x.AddToRoleAsync(It.IsAny<PlatformUser>(), It.Is<string>(s => s.Equals(model.RoleName))))
                .ReturnsAsync(IdentityResult.Success).Callback<PlatformUser, string>((x, y) =>
                {
                    userForRoleManager = x;
                    rollName = y;
                });
            _signInManager.Setup(x => x.SignInAsync(It.IsAny<PlatformUser>(), It.IsAny<bool>(), null))
                .Returns(Task.CompletedTask);
           var result = await _sut.Register(model);

            Assert.Equal(userForRoleManager, userForUserManager);
            Assert.Equal(userForUserManager.Name, model.Name);
            Assert.Equal(userForUserManager.Email, model.Email);
            Assert.Equal(userForUserManager.UserName, model.Email);
            Assert.Equal(rollName, model.RoleName);
            Assert.Equal(toCheckPassword, model.Password);
            Assert.IsType<RedirectToActionResult>(result);
            var value = result as RedirectToActionResult;
            Assert.Equal("Index", value.ActionName);
            Assert.Equal("Home", value.ControllerName);

        }
        /// <summary>
        /// [TestCase-ID: 3-1] Test if the view of login is the expected type.
        /// </summary>
        [Fact]
        public void ReturnViewForLogIn()
        {

            IActionResult result = _sut.Register();
            Assert.IsType<ViewResult>(result);
        }
        /// <summary>
        /// [TestCase-ID: 3-2] Test if the log in is success, when all the information are true .
        /// </summary>
        [Fact]
        public async Task VaildLogIn()
        {
            string toCheckEmail = null;
            string toCheckPassword = null;
            bool toCheckRememberMe = true;
            bool toCheckLockOut = true;

            LogInViewModel model = new LogInViewModel()
            {
                Email = "ubumh@student.kit.edu",
                Password = "a123456",
                RememberMe = false
            };

            _signInManager.Setup(
             x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
              .ReturnsAsync(SignInResult.Success)
              .Callback<string, string, bool, bool>((x, y, z, k) =>
              {
                  toCheckEmail = x;
                  toCheckPassword = y;
                  toCheckRememberMe = z;
                  toCheckLockOut = k;
              });
            var result =await _sut.LogIn(model);
            Assert.Equal(toCheckEmail, model.Email);
            Assert.Equal(toCheckPassword, model.Password);
            Assert.Equal(toCheckRememberMe, model.RememberMe);
            Assert.False(toCheckLockOut);
            Assert.IsType<RedirectToActionResult>(result);
            var value = result as RedirectToActionResult;
            Assert.Equal("Index", value.ActionName);
            Assert.Equal("Home", value.ControllerName);
        }
        /// <summary>
        /// [TestCase-ID: 3-3] Test if the login is unsuccess, if the modelstate is invaild
        /// </summary>
        [Fact]
        public async Task InVaildModelStateForLogIn()
        {
            _sut.ModelState.AddModelError("key", "test error");
            LogInViewModel model = new LogInViewModel()
            {
                Email = "ubumh@student.kit.edu",
                Password = "a123456",
                RememberMe = false
            };
           var result = await _sut.LogIn(model);
            Assert.IsType<ViewResult>(result);
            var value = result as ViewResult;
            Assert.Equal(model, value.Model);
            _signInManager.Verify(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);

        }
        /// <summary>
        /// [TestCase-ID: 3-4] Test if the log in is unsucccess, when the method passwordsigninAsync failed
        /// </summary>
        [Fact]
        public async Task FailedLogIn()
        {
            _signInManager.Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
             .ReturnsAsync(SignInResult.Failed);
            LogInViewModel model = new LogInViewModel()
            {
                Email = "ubumh@student.kit.edu",
                Password = "a123456",
                RememberMe = false
            };
            var result = await _sut.LogIn(model);
            Assert.IsType<ViewResult>(result);
            var value = result as ViewResult;
            Assert.Equal(model, value.Model);
            IEnumerable<ModelError> allErrors = _sut.ModelState.Values.SelectMany(v => v.Errors);
            Assert.Equal("Error to login, please try again", allErrors.FirstOrDefault().ErrorMessage);
        }
        /// <summary>
        /// [TestCase-ID: 3-5] Test the funktion of log out
        /// </summary>
        [Fact]
        public async Task ReturnViewLogOut() {
            _signInManager.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);
            var result = await _sut.Logout();
            Assert.IsType<RedirectToActionResult>(result);
            RedirectToActionResult value = result as RedirectToActionResult;
            Assert.Equal("Index", value.ActionName);
            Assert.Equal("Home", value.ControllerName);
        }

        private static Mock<UserManager<PlatformUser>> MockUserManager<TUser>()
        {
            var mgr = new Mock<UserManager<PlatformUser>>(
            new Mock<IUserStore<PlatformUser>>().Object,
                  new Mock<IOptions<IdentityOptions>>().Object,
                  new Mock<IPasswordHasher<PlatformUser>>().Object,
                  new[] { new Mock<IUserValidator<PlatformUser>>().Object },
                    new[] { new Mock<IPasswordValidator<PlatformUser>>().Object },
                  new Mock<ILookupNormalizer>().Object,
                  new Mock<IdentityErrorDescriber>().Object,
                  new Mock<IServiceProvider>().Object,
                  new Mock<ILogger<UserManager<PlatformUser>>>().Object);
            mgr.Object.UserValidators.Add(new UserValidator<PlatformUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<PlatformUser>());

            mgr.Setup(x => x.DeleteAsync(It.IsAny<PlatformUser>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.UpdateAsync(It.IsAny<PlatformUser>())).ReturnsAsync(IdentityResult.Success);

            return mgr;
        }

        private static Mock<RoleManager<IdentityRole>> MockRoleManager()
        {
            return new Mock<RoleManager<IdentityRole>>(
                      new Mock<IRoleStore<IdentityRole>>().Object,
                      new IRoleValidator<IdentityRole>[0],
                      new Mock<ILookupNormalizer>().Object,
                      new Mock<IdentityErrorDescriber>().Object,
                      new Mock<ILogger<RoleManager<IdentityRole>>>().Object);
        }

        private static Mock<SignInManager<PlatformUser>> MockSignInManager(Mock<UserManager<PlatformUser>> userManager)
        {
            Mock<SignInManager<PlatformUser>> mock = new Mock<SignInManager<PlatformUser>>(
                          userManager.Object,
                          new HttpContextAccessor(),
                          new Mock<IUserClaimsPrincipalFactory<PlatformUser>>().Object,
                          new Mock<IOptions<IdentityOptions>>().Object,
                          new Mock<ILogger<SignInManager<PlatformUser>>>().Object,
                          null,
                          null
                          );
            return mock;
        }
    }
}