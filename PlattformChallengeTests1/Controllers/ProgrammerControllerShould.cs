
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Moq;
using PlattformChallenge.Controllers;
using PlattformChallenge.Core.Interfaces;
using PlattformChallenge.Core.Model;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using System.Linq;
using MockQueryable.Moq;
using System;
using Xunit;
using PlattformChallenge.ViewModels;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using PlattformChallenge.Infrastructure;

namespace PlattformChallenge_UnitTest.Controllers
{
    public class ProgrammerControllerShould
    {
        private readonly Mock<UserManager<PlatformUser>> _mockUseerManager;
        private readonly Mock<IRepository<Challenge>> _mockCRepo;
        private readonly Mock<IRepository<Participation>> _mockPRepo;
        private readonly ProgrammerController _sut;
        private readonly Mock<ConfigProviderService> _mockAfg;
        private readonly Mock<IRepository<Solution>> _mockSRepo;
        private PlatformUser _user;

        public ProgrammerControllerShould()
        {
            _mockCRepo = new Mock<IRepository<Challenge>>();
            _mockPRepo = new Mock<IRepository<Participation>>();
            _mockAfg = new Mock<ConfigProviderService>();
            _mockSRepo = new Mock<IRepository<Solution>>();
            this._mockUseerManager = MockUserManager<PlatformUser> ();
            var mock = new Mock<HttpContext>();
            var context = new ControllerContext(new ActionContext(mock.Object, new RouteData(), new ControllerActionDescriptor()));
            _user = new PlatformUser()
            {
                Id = "test-programmer",
                Name = "Zhen"
            };
            mock.Setup(p => p.User.FindFirst(ClaimTypes.NameIdentifier)).Returns(new Claim(ClaimTypes.NameIdentifier, "Pro_1"));
            _mockUseerManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(_user);
            _sut = new ProgrammerController(_mockUseerManager.Object, _mockCRepo.Object, _mockPRepo.Object,_mockSRepo.Object,_mockAfg.Object);
            _sut.ControllerContext = context;
        }
        /// <summary>
        /// [TestCase-ID: 24-1]
        /// Test whether you can get a valid list of challenges that users have participated in
        /// </summary>
        [Fact]
        public async Task ReturnVaildIndex() {
            var challenges = GetAllBuild();
            var result = await _sut.Index();
            var vaule = result as ViewResult;
            var view = vaule.Model;
            Assert.IsType<ProgrammerIndexViewModel>(view);
            var model = view as ProgrammerIndexViewModel;
            Assert.Equal(challenges.ElementAt(0), model.Challenges.ElementAt(0));
            Assert.Equal(challenges.ElementAt(1), model.Challenges.ElementAt(1));
            Assert.Equal(_user, model.Programmer);
        }
        /// <summary>
        /// [TestCase-ID: 56-1]
        /// Test whether the user can effectively exit the challenge that has already participated
        /// </summary>
        [Fact]
        public async Task ReturnVaildCancel() {
            var mockP = new List<Participation>() {
                new Participation(){
                    P_Id = "test-programmer",
                    C_Id ="c1"
                }
            }.AsQueryable().BuildMock();
            _mockPRepo.Setup(m => m.GetAll()).Returns(mockP.Object);
            _mockPRepo.Setup(m => m.DeleteAsync(It.IsAny<Expression<Func<Participation, bool>>>())).Returns(Task.CompletedTask);
            var result = await _sut.Cancel("c1");
            Assert.IsType<RedirectToActionResult>(result);
    }

        /// <summary>
        /// [TestCase-ID: 56-2]
        /// Test if the challenge that needs to be exited is not the user's participation, whether the command is not executed
        /// </summary>
        [Fact]
        public async Task InvaildCanel() {
            var mockP = new List<Participation>() {
                new Participation(){
                    P_Id = "test-error",
                    C_Id ="c1"
                }
            }.AsQueryable().BuildMock();
            _mockPRepo.Setup(m => m.GetAll()).Returns(mockP.Object);
            var result = await _sut.Cancel("c1");
            Assert.IsType<ViewResult>(result);
            _mockPRepo.Verify(m => m.DeleteAsync(It.IsAny<Expression<Func<Participation, bool>>>()), Times.Never);
        }


        private List<Challenge> GetAllBuild() {
            var challenges = new List<Challenge>() {
               new Challenge(){
                C_Id = "c1",
                Title = "test title 1",
                Bonus = 200,
                Content = "test content 1",
                Release_Date = DateTime.Now,
                Max_Participant = 8,
                Com_ID = "1111",
                Company = new PlatformUser(){
                    Id = "test1.com"
                 }
            },
                new Challenge(){
                C_Id = "c2",
                Title = "test title 2",
                Bonus = 200,
                Content = "test content 2",
                Release_Date = DateTime.Now,
                Max_Participant = 8,
                Com_ID = "1111",
                Company = new PlatformUser(){
                    Id = "test1.com"
                 }
            },
                  new Challenge(){
                C_Id = "c3",
                Title = "test title 3",
                Bonus = 200,
                Content = "test content 3",
                Release_Date = DateTime.Now,
                Max_Participant = 8,
                Com_ID = "1111",
                Company = new PlatformUser(){
                    Id = "test1.com"
                 }
            }
            };
            var mockChallenges =challenges.AsQueryable().BuildMockDbSet();
            var mockPar = new List<Participation>() {
                new Participation(){
                    C_Id="c1",
                    P_Id="test-programmer"
                },
                  new Participation(){
                    C_Id="c2",
                    P_Id="test-programmer"
                }
            }.AsQueryable().BuildMockDbSet();

            _mockCRepo.Setup(c => c.GetAll()).Returns(mockChallenges.Object);
            _mockPRepo.Setup(p => p.GetAll()).Returns(mockPar.Object);
            return challenges;
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
    }
}
