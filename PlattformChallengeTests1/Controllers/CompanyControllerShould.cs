
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
    public class CompanyControllerShould
    {
        private readonly Mock<UserManager<PlatformUser>> _mockUserManager;
        private readonly Mock<IRepository<Challenge>> _mockCRepo;
        private readonly Mock<IRepository<Participation>> _mockPRepo;
        private readonly CompanyController _sut;
        private readonly Mock<ConfigProviderService> _mockAfg;
        private readonly Mock<IRepository<Solution>> _mockSRepo;
        private PlatformUser _user;

        public CompanyControllerShould()
        {
            _mockCRepo = new Mock<IRepository<Challenge>>();
            _mockPRepo = new Mock<IRepository<Participation>>();
            _mockAfg = new Mock<ConfigProviderService>();
            _mockSRepo = new Mock<IRepository<Solution>>();
            this._mockUserManager = MockUserManager<PlatformUser>();
            var mock = new Mock<HttpContext>();
            var context = new ControllerContext(new ActionContext(mock.Object, new RouteData(), new ControllerActionDescriptor()));
            _user = new PlatformUser()
            {
                Id = "test-company",

            };
            mock.Setup(p => p.User.FindFirst(ClaimTypes.NameIdentifier)).Returns(new Claim(ClaimTypes.NameIdentifier, "test-company"));
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(_user);
            _sut = new CompanyController(_mockUserManager.Object, _mockCRepo.Object, _mockPRepo.Object, _mockSRepo.Object);
            _sut.ControllerContext = context;
        }
        /// <summary>
        /// [TestCase-ID: 42-1]
        /// Test whether a company user can get a valid list of challenges that he has published
        /// </summary>
        [Fact]
        public async Task ReturnVaildIndex()
        {
            var challenges = GetAllBuildChallenge();
            var result = await _sut.Index();
            var value = result as ViewResult;
            var view = value.Model;
            Assert.IsType<CompanyIndexViewModel>(view);
            var model = view as CompanyIndexViewModel;
            Assert.Equal(challenges.ElementAt(1), model.Challenges.ElementAt(0));
            Assert.Equal(challenges.ElementAt(2), model.Challenges.ElementAt(1));
            Assert.Equal(_user, model.Company);
        }

        /// <summary>
        /// [TestCase-ID: 14-1]
        /// Test whether a company user can get a valid list of submitted solutions to a selected challenge
        /// </summary>
        [Fact]
        public async Task ReturnVaildSolutionList()
        {
            var challenges = GetAllBuildChallenge();
            var solutions = GetAllBuildSolution();
            GetAllBuildParticipation();
            var result = await _sut.AllSolutions(challenges.ElementAt(1).C_Id);
            var value = result as ViewResult;
            var view = value.Model;
            Assert.IsType<AllSolutionsViewModel>(view);
            var model = view as AllSolutionsViewModel;
            Assert.Single(model.Solutions);
            Assert.Equal(solutions.ElementAt(1), model.Solutions.ElementAt(0));
            Assert.Equal(challenges.ElementAt(1).C_Id, model.CurrChallengeId);
        }

        /// <summary>
        /// [TestCase-ID: 17-1]
        /// Test if a company user can rate solution successfully
        /// </summary>
        [Fact]
        public async Task RateSolutionSuccess()
        {
            var challenges = GetAllBuildChallenge();
            var solutions = GetAllBuildSolution();
            GetAllBuildParticipation();
            _mockCRepo.Setup(m => m.FirstOrDefault(It.IsAny<Expression<Func<Challenge, bool>>>())).Returns(
               challenges.ElementAt(1));
            _mockSRepo.Setup(s => s.GetAllListAsync(It.IsAny<Expression<Func<Solution, bool>>>())).Returns(
               Task.FromResult(solutions));
            AllSolutionsViewModel vm = new AllSolutionsViewModel
            {
                CurrChallengeId = "c2",
                Point = 55
            };
            var result = await _sut.RateSolution(vm) as RedirectToActionResult;
            _mockSRepo.Verify(s => s.UpdateAsync(It.IsAny<Solution>()), Times.Once);
            Assert.Equal("AllSolutions", result.ActionName);
            Assert.Equal("c2", result.RouteValues["id"]);
        }




            private List<Participation> GetAllBuildParticipation()
        {
            var participations = new List<Participation>() {
                new Participation(){
                    C_Id="c1",
                    P_Id="test-programmer1",
                    S_Id = "s1"
                },
                  new Participation(){
                    C_Id="c2",
                    P_Id="test-programmer2",
                    S_Id = "s2"
                }
            };

            var mockPars = participations.AsQueryable().BuildMockDbSet();
            _mockPRepo.Setup(p => p.GetAll()).Returns(mockPars.Object);
            return participations;
        }

        private List<Solution> GetAllBuildSolution()
        {
            var solutions = new List<Solution>() {
                new Solution(){
                    S_Id="s1",
                    URL = "www.test/solution/s1",
                    Status = StatusEnum.Receive,
                    Submit_Date = DateTime.Now.AddDays(-2),
                },
                  new Solution(){
                    S_Id="s2",
                    URL = "www.test/solution/s2",
                    Status = StatusEnum.Receive,
                    Submit_Date = DateTime.Now.AddDays(-4),
                },

            };
            var mockSolutions = solutions.AsQueryable().BuildMockDbSet();
            _mockSRepo.Setup(s => s.GetAll()).Returns(mockSolutions.Object);
            return solutions;
        }


        private List<Challenge> GetAllBuildChallenge()
        {
            var challenges = new List<Challenge>() {
               new Challenge(){
                C_Id = "c1",
                Title = "test title 1",
                Bonus = 200,
                Content = "test content 1",
                Release_Date = DateTime.Now,
                Max_Participant = 8,
                Com_ID = "another-company",
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
                Com_ID = "test-company",
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
                Com_ID = "test-company",
                Company = new PlatformUser(){
                    Id = "test1.com"
                 }
            }
            };
            var mockChallenges = challenges.AsQueryable().BuildMockDbSet();
            _mockCRepo.Setup(c => c.GetAll()).Returns(mockChallenges.Object);
       
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
