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
            mgr.Setup(x => x.CreateAsync(It.IsAny<PlatformUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<PlatformUser, string>((x, y) => ls.Add(x));
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
                          new Mock<ILogger<SignInManager<PlatformUser>>>().Object);
            mock.Setup(
                  x => x.PasswordSignInAsync(It.IsAny<PlatformUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
              .ReturnsAsync(SignInResult.Success);
            return mock;
        }

    }
}
