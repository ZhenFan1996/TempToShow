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

namespace PlattformChallengeTests.Controllers
{
     public class AccountControllerShould
    {

        private readonly AccountController _sut;
        private readonly Mock<UserManager<PlatformUser>> _userManager;
        private readonly Mock<RoleManager<IdentityRole>> _roleManager;
        private readonly Mock<SignInManager<PlatformUser>> _signInManager;

        public AccountControllerShould()
        {
            _userManager = MockUserManager<PlatformUser>(new List<PlatformUser>() {
                new PlatformUser(){
                    UserName = "ubumh@student.kit.edu",
                    Email = "ubumh@student.kit.edu",
                    Name = "Zhen"
                }
            });
            _roleManager = MockRoleManager();
            _signInManager = MockSignInManager(_userManager);
            _sut = new AccountController(_userManager.Object,_signInManager.Object,_roleManager.Object);
        }

        [Fact]
        public void ReturnViewForRegister() {
            _roleManager.Setup(r => r.RoleExistsAsync(It.Is<string>(s => s.Equals("Company") || s.Equals("Programmer"))))
                .Returns( Task.FromResult(true));

            Task<IActionResult> result = _sut.Register();

            Assert.IsType<ViewResult>(result.Result);

        }

        [Fact]
        public async Task InVaildModelState() {
            _sut.ModelState.AddModelError("key","error Test");
            RegisterViewModel model = new RegisterViewModel() {
                Name = "Zhen",
                Email = "ubumh@student.kit.edu",
                Password = "a123456",
                RoleName = "Programmer"
            };

            await _sut.Register(model);
            _userManager.Verify(x => x.CreateAsync(It.IsAny<PlatformUser>(), It.IsAny<string>()),Times.Never);
            _userManager.Verify(x => x.AddToRoleAsync(It.IsAny<PlatformUser>(), It.Is<string>(s => s.Equals(model.RoleName))),Times.Never);
            _signInManager.Verify(x => x.SignInAsync(It.IsAny<PlatformUser>(), It.IsAny<bool>(), null),Times.Never);
            
        }

        [Fact]
        public async Task FailedCreateAsync() {
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
            await _sut.Register(model);
            IEnumerable<ModelError> allErrors = _sut.ModelState.Values.SelectMany(v => v.Errors);
            Assert.Equal("test error1", allErrors.ElementAt(0).ErrorMessage);
            Assert.Equal("test error2", allErrors.ElementAt(1).ErrorMessage);
            _userManager.Verify(x => x.AddToRoleAsync(It.IsAny<PlatformUser>(), It.Is<string>(s => s.Equals(model.RoleName))), Times.Never);
            _signInManager.Verify(x => x.SignInAsync(It.IsAny<PlatformUser>(), It.IsAny<bool>(), null), Times.Never);
        }

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
                  .ReturnsAsync(IdentityResult.Success).Callback<PlatformUser, string>((x, y) => {
                   userForUserManager = x; ;
                  toCheckPassword = y;
       });
            _userManager.Setup(x => x.AddToRoleAsync(It.IsAny<PlatformUser>(), It.Is<string>(s => s.Equals(model.RoleName))))
             .ReturnsAsync(IdentityResult.Failed(new IdentityError[] {
                            new IdentityError(){
                                Description = "test error1" },
                            new IdentityError(){
                                Description = "test error2"}}));
            await _sut.Register(model);
            IEnumerable<ModelError> allErrors = _sut.ModelState.Values.SelectMany(v => v.Errors);
            Assert.Equal("test error1", allErrors.ElementAt(0).ErrorMessage);
            Assert.Equal("test error2", allErrors.ElementAt(1).ErrorMessage);
            _signInManager.Verify(x => x.SignInAsync(It.IsAny<PlatformUser>(), It.IsAny<bool>(), null), Times.Never);
            Assert.Equal(userForUserManager.Name, model.Name);
            Assert.Equal(userForUserManager.Email, model.Email);
            Assert.Equal(userForUserManager.UserName, model.Email);
            Assert.Equal(toCheckPassword, model.Password);
        }

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
                .ReturnsAsync(IdentityResult.Success).Callback<PlatformUser, string>((x, y) => {
                    userForUserManager = x; ;
                    toCheckPassword = y;
                });
            _userManager.Setup(x => x.AddToRoleAsync(It.IsAny<PlatformUser>(), It.Is<string>(s => s.Equals(model.RoleName))))
                .ReturnsAsync(IdentityResult.Success).Callback<PlatformUser, string>((x, y) => {
                    userForRoleManager = x;
                    rollName = y;
                });
            _signInManager.Setup(x => x.SignInAsync(It.IsAny<PlatformUser>(), It.IsAny<bool>(), null))
                .Returns(Task.CompletedTask);
            await _sut.Register(model);

            Assert.Equal(userForRoleManager, userForUserManager);
            Assert.Equal(userForUserManager.Name, model.Name);
            Assert.Equal(userForUserManager.Email, model.Email);
            Assert.Equal(userForUserManager.UserName, model.Email);
            Assert.Equal(rollName, model.RoleName);
            Assert.Equal(toCheckPassword, model.Password);

        }




        private static Mock<UserManager<PlatformUser>> MockUserManager<TUser>(List<PlatformUser> ls) 
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

        private static Mock<RoleManager<IdentityRole>> MockRoleManager() {
            return new Mock<RoleManager<IdentityRole>>(
                      new Mock<IRoleStore<IdentityRole>>().Object,
                      new IRoleValidator<IdentityRole>[0],
                      new Mock<ILookupNormalizer>().Object,
                      new Mock<IdentityErrorDescriber>().Object,
                      new Mock<ILogger<RoleManager<IdentityRole>>>().Object);
        }

        private static Mock<SignInManager<PlatformUser>> MockSignInManager(Mock<UserManager<PlatformUser>> userManager) {
            Mock<SignInManager<PlatformUser>> mock = new Mock<SignInManager<PlatformUser>>(
                          userManager.Object,
                          new HttpContextAccessor(),
                          new Mock<IUserClaimsPrincipalFactory<PlatformUser>>().Object,
                          new Mock<IOptions<IdentityOptions>>().Object,
                          new Mock<ILogger<SignInManager<PlatformUser>>>().Object,
                          null,
                          null
                          );
            mock.Setup(
                  x => x.PasswordSignInAsync(It.IsAny<PlatformUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
              .ReturnsAsync(SignInResult.Success);
            return mock;
        }

    }
}
