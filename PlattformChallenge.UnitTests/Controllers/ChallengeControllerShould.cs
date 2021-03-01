using System;
using System.Collections.Generic;
using Moq;
using PlattformChallenge.Core.Interfaces;
using PlattformChallenge.Core.Model;
using Xunit;
using PlattformChallenge.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Linq;
using PlattformChallenge.Models;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MockQueryable.Moq;
using Microsoft.Data.SqlClient;
using PlattformChallenge.Controllers;
using Microsoft.Extensions.Logging;
using PlattformChallenge.Services;
using Microsoft.Extensions.Localization;

namespace PlattformChallenge.UnitTest.Controllers
{

    public class ChallengeControllerShould
    {
        private readonly Mock<IRepository<Challenge>> _mockRepository;
        private readonly Mock<IRepository<PlatformUser>> _mockPRpository;
        private readonly Mock<IRepository<Language>> _mockLRepository;
        private readonly Mock<IRepository<LanguageChallenge>> _mockLCRepository;
        private readonly Mock<IRepository<Participation>> _mockPaRepository;
        private readonly Mock<IStringLocalizer<ChallengesController>> _mockLocal;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<IEmailSender> _mockSender;

        private readonly ChallengesController _sut;

        public ChallengeControllerShould()
        {

            _mockRepository = new Mock<IRepository<Challenge>>();
            _mockPRpository = new Mock<IRepository<PlatformUser>>();
            _mockLRepository = new Mock<IRepository<Language>>();
            _mockLCRepository = new Mock<IRepository<LanguageChallenge>>();
            _mockPaRepository = new Mock<IRepository<Participation>>();
            _mockLocal = new Mock<IStringLocalizer<ChallengesController>>();
            _mockLocal.SetupGet(m => m[It.IsAny<string>(), It.IsAny<string[]>()]).Returns(new LocalizedString("name", "value"));
            _mockSender = new Mock<IEmailSender>();
            _mockSender.Setup(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            var logger = new Mock<ILogger<ChallengesController>>();
            _sut = new ChallengesController(_mockRepository.Object, _mockPRpository.Object, _mockLRepository.Object, _mockLCRepository.Object, _mockPaRepository.Object,logger.Object,_mockSender.Object,_mockLocal.Object);
            _mockHttpContext = new Mock<HttpContext>();
            var context = new ControllerContext(new ActionContext(_mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));
            _mockHttpContext.Setup(p => p.User.FindFirst(ClaimTypes.NameIdentifier)).Returns(new Claim(ClaimTypes.NameIdentifier, "1"));
            _mockHttpContext.SetupGet(_ => _.Response.StatusCode).Returns(It.IsAny<int>);
            _mockLRepository.Setup(l => l.GetAllListAsync()).Returns(Task.FromResult(new List<Language>()
            {new Language()
                    {
                        Language_Id = "1",
                        DevelopmentLanguage = "Java"

                    },
                    new Language()
                    {
                        Language_Id = "2",
                        DevelopmentLanguage = "C++"

                    },
                       new Language()
                    {
                        Language_Id = "3",
                        DevelopmentLanguage = "Other"

                    }

            }));
            _sut.ControllerContext = context;
        }

        //
        // Summary:
        //    [TestCase-ID: 10-6]
        //    Test if the view of create is the expected type.
        //
        [Fact]
        public async Task ReturnViewForCreateAsync()
        {
            _mockLRepository.Setup(l => l.GetAllListAsync()).Returns(Task.FromResult(new List<Language>()
            {

                new Language(){
                    Language_Id ="1",
                    DevelopmentLanguage = "java"
                },

                 new Language(){
                    Language_Id ="2",
                    DevelopmentLanguage = "C++"
                },
                 new Language(){
                     Language_Id = "3",
                    DevelopmentLanguage = "C#"

                 }
            }));
            var challenge = new ChallengeCreateViewModel()
            {
                Title = "aaaa",
                Bonus = 2,
                Content = "wuwuwuwuwu",
                Release_Date = DateTime.UtcNow,
                Max_Participant = 8,
                IsSelected = new bool[] { true, false, true }
            };
            var result = await _sut.Create();
            Assert.IsType<ViewResult>(result);


        }

        //
        // Summary:
        //    [TestCase-ID: 10-5]
        //    Test whether the new challenge is successfully created
        //
        [Fact]
        public async Task CreateChallengeTest()
        {
            Challenge savedChallenge = null;
            List<LanguageChallenge> savedLc = new List<LanguageChallenge>();

            _mockRepository.Setup(m => m.InsertAsync(It.IsAny<Challenge>()))
                .Returns(Task.CompletedTask)
                .Callback<Challenge>(c => savedChallenge = c);

            _mockLCRepository.Setup(l => l.InsertAsync(It.IsAny<LanguageChallenge>()))
                .Returns(Task.CompletedTask)
                .Callback<LanguageChallenge>(s => savedLc.Add(s));
            var challenge = new ChallengeCreateViewModel()
            {
                Title = "aaaa",
                Bonus = 2,
                Content = "wuwuwuwuwu",
                Release_Date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.AddDays(1), TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")),
                Deadline = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.AddDays(3), TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")),
                Max_Participant = 8,
                IsSelected = new bool[] { true, false, true },
                Zone = "Europe/Berlin"
            };


            var result = await _sut.Create(challenge);
            Assert.Equal(challenge.Title, savedChallenge.Title);
            Assert.Equal(challenge.Bonus, savedChallenge.Bonus);
            Assert.Equal(challenge.Content, savedChallenge.Content);
            Assert.Equal(DateTime.UtcNow.AddDays(1).Date, savedChallenge.Release_Date.Date);
            Assert.Equal(challenge.Max_Participant, savedChallenge.Max_Participant);
            Assert.Equal("1", savedChallenge.Com_ID);
            Assert.Equal(savedLc.ElementAt(0).C_Id, savedChallenge.C_Id);
            Assert.Equal(savedLc.ElementAt(1).C_Id, savedChallenge.C_Id);
            Assert.Equal("1", savedLc.ElementAt(0).Language_Id);
            Assert.Equal("3", savedLc.ElementAt(1).Language_Id);
        }
        //
        // Summary:
        //    [TestCase-ID: 10-4]
        //    Test the returned error view if modelstate is invalid
        //
        [Fact]
        public async Task InVaildModelStateForCreate()
        {
            _sut.ModelState.AddModelError(string.Empty, "failed to create the challenge, please try again");
            var challenge = new ChallengeCreateViewModel()
            {
                Title = "aaaa",
                Bonus = 2,
                Content = "wuwuwuwuwu",
                Release_Date = DateTime.UtcNow.AddDays(2),
                Max_Participant = 8,
                IsSelected = new bool[] { true, false, true }
            };
            var result = await _sut.Create(challenge);
            Assert.IsType<ViewResult>(result);
            ViewResult value = (ViewResult)result;
            Assert.Equal(challenge, value.Model);
            IEnumerable<ModelError> allErrors = _sut.ModelState.Values.SelectMany(v => v.Errors);
            Assert.Equal("failed to create the challenge, please try again", allErrors.ElementAt(0).ErrorMessage);
            _mockLRepository.Verify(l => l.GetAllListAsync(), Times.Once);
            _mockRepository.Verify(l => l.InsertAsync(It.IsAny<Challenge>()), Times.Never);
            _mockLCRepository.Verify(l => l.InsertAsync(It.IsAny<LanguageChallenge>()), Times.Never);
        }
        //
        // Summary:
        //    [TestCase-ID: 10-1]
        //    Test the returned error view if given Id string for challenge details is empty 
        //
        [Fact]
        public async void ReturnBadRequestNoIdForDetails()
        {
            var result = await _sut.Details("");         
            Assert.IsType<ViewResult>(result);
            Assert.Equal("Invalid empty challenge id value", _sut.ViewBag.ErrorMessage);
        }

        //
        // Summary:
        //    [TestCase-ID: 10-2]
        //    Test the returned error view if given Id string for challenge details doesn't match any challenge 
        //
        [Fact]
        public async void ReturnBadRequestInvalidIdForDetails()

        {
            List<Challenge> challenges = new List<Challenge>();
            _mockRepository.Setup(m => m.GetAll()).Returns(challenges.AsQueryable().BuildMockDbSet().Object);
            var result = await _sut.Details("1");
            Assert.IsType<ViewResult>(result);
            _mockHttpContext.VerifySet(_ => _.Response.StatusCode=404, Times.Once);
        }

        //
        // Summary:
        //    [TestCase-ID: 10-3]
        //    Test if index() returns correct list of challenges 
        //
        [Fact]
        public async Task ReturnVaildIndex()
        {
            var l = new List<Challenge>() {
                 new Challenge(){
                C_Id = "1abc",
                Title = "test title 1",
                Bonus = 200,
                Content = "test content 1",
                Release_Date = DateTime.UtcNow.AddDays(-2),
                Deadline = DateTime.UtcNow.AddDays(30),
                Max_Participant = 8,
                Com_ID = "1111",
                Company = new PlatformUser(){
                    Id = "test1.com"
                }
                },
                   new Challenge(){
                C_Id = "2cde",
                Title = "test title 2",
                Bonus = 400,
                Content = "test content 2",
                Release_Date = DateTime.UtcNow,
                Deadline = DateTime.UtcNow.AddDays(30),
                Max_Participant = 18,
                Com_ID = "2222",
                Company = new PlatformUser(){
                    Id = "test2.com"
                }
                }
            };
            var query = l.AsQueryable().BuildMockDbSet();
            _mockRepository
                .Setup(m => m.GetAll())
                .Returns(query.Object);
            var result = await _sut.Index(null, null, null, new bool[0],null) ;
            Assert.IsType<ViewResult>(result);
            var value = result as ViewResult;
            var model = value.Model as  ChallengeIndexViewModel;
            Assert.Equal("test title 1", model.Challenges.ElementAt<Challenge>(1).Title);
            Assert.Equal("1111", model.Challenges.ElementAt<Challenge>(1).Com_ID);
            Assert.Equal(200, model.Challenges.ElementAt<Challenge>(1).Bonus);
            Assert.Equal("2cde", model.Challenges.ElementAt<Challenge>(0).C_Id);
            Assert.Equal(18, model.Challenges.ElementAt<Challenge>(0).Max_Participant);
            Assert.Equal(DateTime.UtcNow.Day, model.Challenges.ElementAt<Challenge>(0).Release_Date.Day);
        }

        //
        // Summary:
        //    [TestCase-ID: 7-1]
        //    Test if index() returns correct list of challenges afte challenges descending sorted by bonus
        //
        [Fact]
        public async Task SortChallengesByBonusIndex()
        {
            var l = new List<Challenge>() {
                 new Challenge(){
                C_Id = "1abc",
                Title = "test title 1",
                Bonus = 200,
                Content = "test content 1",
                Release_Date = DateTime.UtcNow.AddDays(-6),
                Deadline = DateTime.UtcNow.AddDays(30),
                Max_Participant = 8,
                Com_ID = "1111",
                Company = new PlatformUser(){
                    Id = "test1.com"
                }
                },
                   new Challenge(){
                C_Id = "2cde",
                Title = "test title 2",
                Bonus = 400,
                Content = "test content 2",
                Release_Date = DateTime.UtcNow.AddDays(-4),
                Deadline = DateTime.UtcNow.AddDays(30),
                Max_Participant = 18,
                Com_ID = "2222",
                Company = new PlatformUser(){
                    Id = "test2.com"
                }
                },
                   new Challenge(){
                C_Id = "3cde",
                Title = "test title 3",
                Bonus = 600,
                Content = "test content 3",
                Release_Date = DateTime.UtcNow,
                Deadline = DateTime.UtcNow.AddDays(30),
                Max_Participant = 118,
                Com_ID = "3333",
                Company = new PlatformUser(){
                    Id = "test3.com"
                }
                },
            };
            var query = l.AsQueryable().BuildMockDbSet();
            _mockRepository
                .Setup(m => m.GetAll())
                .Returns(query.Object);
            var result = await _sut.Index(null, "bonus_desc", null,new bool[0],null);
            var value = result as ViewResult;
            var model = value.Model as ChallengeIndexViewModel;
            var sorted = model.Challenges;
            Assert.Equal(l.ElementAt(0).Bonus, sorted.ElementAt(2).Bonus);
            Assert.Equal(l.ElementAt(1).Bonus, sorted.ElementAt(1).Bonus);
            Assert.Equal(l.ElementAt(2).Bonus, sorted.ElementAt(0).Bonus);
        }

        //
        // Summary:
        //    [TestCase-ID: 52-1]
        //    Test if index() returns correct list of challenges afte challenges descending sorted by quota
        //
        [Fact]
        public async Task SortChallengesByQuotaIndex()
        {
            var l = new List<Challenge>() {
                 new Challenge(){
                C_Id = "1abc",
                Title = "test title 1",
                Bonus = 200,
                Content = "test content 1",
                Release_Date = DateTime.UtcNow.AddDays(-5),
                 Deadline = DateTime.UtcNow.AddDays(30),
                Max_Participant = 8,
                Com_ID = "1111",
                Company = new PlatformUser(){
                    Id = "test1.com"
                }
                },
                   new Challenge(){
                C_Id = "2cde",
                Title = "test title 2",
                Bonus = 400,
                Content = "test content 2",
                Release_Date = DateTime.UtcNow.AddDays(-3),
                 Deadline = DateTime.UtcNow.AddDays(30),
                Max_Participant = 18,
                Com_ID = "2222",
                Company = new PlatformUser(){
                    Id = "test2.com"
                }
                },
                   new Challenge(){
                C_Id = "3cde",
                Title = "test title 3",
                Bonus = 600,
                Content = "test content 3",
                Release_Date = DateTime.UtcNow,
                Deadline = DateTime.UtcNow.AddDays(30),
                Max_Participant = 118,
                Com_ID = "3333",
                Company = new PlatformUser(){
                    Id = "test3.com"
                }
                },
            };
            var query = l.AsQueryable().BuildMockDbSet();
            _mockRepository
                .Setup(m => m.GetAll())
                .Returns(query.Object);
            var result = await _sut.Index(null, "quota_desc", null,new bool[0],null);
            var value = result as ViewResult;
            var model = value.Model as ChallengeIndexViewModel;
            var sorted = model.Challenges;
            Assert.Equal(l.ElementAt(0).Bonus, sorted.ElementAt(2).Bonus);
            Assert.Equal(l.ElementAt(1).Bonus, sorted.ElementAt(1).Bonus);
            Assert.Equal(l.ElementAt(2).Bonus, sorted.ElementAt(0).Bonus);

        }

        //
        // Summary:
        //    [TestCase-ID: 6-1]
        //    Test if index() returns correct list of challenges afte challenges descending sorted by date
        //
        [Fact]
        public async Task SortChallengesByDateIndex()
        {
            var l = new List<Challenge>() {
                 new Challenge(){
                C_Id = "1abc",
                Title = "test title 1",
                Bonus = 200,
                Content = "test content 1",
                Release_Date = DateTime.UtcNow.AddDays(-5),
                Deadline = DateTime.UtcNow.AddDays(30),
                Max_Participant = 8,
                Com_ID = "1111",
                Company = new PlatformUser(){
                    Id = "test1.com"
                }
                },
                   new Challenge(){
                C_Id = "2cde",
                Title = "test title 2",
                Bonus = 400,
                Content = "test content 2",
                Release_Date = DateTime.UtcNow.AddDays(-3),
                Deadline = DateTime.UtcNow.AddDays(30),
                Max_Participant = 18,
                Com_ID = "2222",
                Company = new PlatformUser(){
                    Id = "test2.com"
                }
                },
                   new Challenge(){
                C_Id = "3cde",
                Title = "test title 3",
                Bonus = 600,
                Content = "test content 3",
                Release_Date = DateTime.UtcNow,
                Deadline = DateTime.UtcNow.AddDays(30),
                Max_Participant = 118,
                Com_ID = "3333",
                Company = new PlatformUser(){
                    Id = "test3.com"
                }
                },
            };
            var query = l.AsQueryable().BuildMockDbSet();
            _mockRepository
                .Setup(m => m.GetAll())
                .Returns(query.Object);
            var result = await _sut.Index(null, "", null,new bool[0],null);
            var value = result as ViewResult;
            var model = value.Model as ChallengeIndexViewModel;
            var sorted = model.Challenges;
            Assert.Equal(l.ElementAt(0).Bonus, sorted.ElementAt(2).Bonus);
            Assert.Equal(l.ElementAt(1).Bonus, sorted.ElementAt(1).Bonus);
            Assert.Equal(l.ElementAt(2).Bonus, sorted.ElementAt(0).Bonus);

        }


        //
        // Summary:
        //    [TestCase-ID: 53-1]
        //    Test if index() returns correct list of challenges afte search by title with "2"
        //
        [Fact]
        public async Task SearchChallengesByTitle()
        {
            var l = new List<Challenge>() {
                 new Challenge(){
                C_Id = "1abc",
                Title = "test title 1",
                Bonus = 200,
                Content = "test content 1",
                Release_Date = DateTime.UtcNow.AddDays(-2),
                Deadline = DateTime.UtcNow.AddDays(30),
                Max_Participant = 8,
                Com_ID = "1111",
                Company = new PlatformUser(){
                    Id = "test1.com"
                }
                },
                   new Challenge(){
                C_Id = "2cde",
                Title = "test title 2",
                Bonus = 400,
                Content = "test content 2",
                Release_Date = DateTime.UtcNow,
                Deadline = DateTime.UtcNow.AddDays(30),
                Max_Participant = 18,
                Com_ID = "2222",
                Company = new PlatformUser(){
                    Id = "test2.com"
                }
                },
                   new Challenge(){
                C_Id = "3cde",
                Title = "test title 3",
                Bonus = 600,
                Content = "test content 3",
                Release_Date = DateTime.UtcNow.AddDays(2),
                Deadline = DateTime.UtcNow.AddDays(30),
                Max_Participant = 118,
                Com_ID = "3333",
                Company = new PlatformUser(){
                    Id = "test3.com"
                }
                },
            };
            var query = l.AsQueryable().BuildMockDbSet();
            var isSelected = new bool[0];
            _mockRepository
                .Setup(m => m.GetAll())
                .Returns(query.Object);
            var result = await _sut.Index(null, null, "2",isSelected,null);
            var value = result as ViewResult;
            var model = value.Model as ChallengeIndexViewModel;
            var searched = model.Challenges;
            Assert.Equal(l.ElementAt(1).C_Id, searched.ElementAt(0).C_Id);
            Assert.Single(searched);
        }

        //
        // Summary:
        //    [TestCase-ID: 51-1]
        //    Test if a legal participation can be done as expected
        // 
        [Fact]
        public async Task ReturnVaildParticipation()
        {
            Participation toCheck = null;
            _mockRepository.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Expression<Func<Challenge, bool>>>())).Returns(
                Task.FromResult(new Challenge()
                {
                    C_Id = "mock_challenge_RVP",
                    Title = "title_mock_challenge_RVP",
                    Bonus = 100,
                    Content = "Content_mock_challenge_RVP",
                    Max_Participant = 11,
                    Deadline = DateTime.UtcNow.AddDays(+3)
                }
                ));

            _mockRepository.Setup(m => m.GetAll()).Returns(
                new List<Challenge>()
                {  new Challenge()
                {
                   C_Id = "mock_challenge_RVP",
                    Title = "title_mock_challenge_RVP",
                    Bonus = 100,
                    Content = "Content_mock_challenge_RVP",
                    Max_Participant = 11,
                    Deadline = DateTime.UtcNow.AddDays(+3)
                }
                }.AsQueryable().BuildMockDbSet().Object
                );
            _mockPaRepository.Setup(m => m.InsertAsync(It.IsAny<Participation>()))
                .Returns(Task.CompletedTask)
                .Callback<Participation>(x => toCheck = x);

            _mockPRpository.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Expression<Func<PlatformUser, bool>>>())).ReturnsAsync(new PlatformUser()
            {
                Id = "test",
                Email = "test@kit.edu",
                Name = "test"

            });
            _mockPaRepository.Setup(m => m.GetAllList(It.IsAny<Expression<Func<Participation, bool>>>())).Returns(new List<Participation>()
            {
            });
            var result = await _sut.ParticipateChallenge("mock_challenge_RVP");
            Assert.Equal("mock_challenge_RVP", toCheck.C_Id);
            Assert.Equal("1", toCheck.P_Id);
        }

        //
        // Summary:
        //    [TestCase-ID: 51-2]
        //    Test if repeated participation on a same challenge throws exception
        //
        [Fact]
        public async Task ExceptionRepeatedParticipation()
        {
            Participation toCheck = null;
            _mockRepository.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Expression<Func<Challenge, bool>>>())).Returns(
                Task.FromResult(new Challenge()
                {
                    C_Id = "mock_challenge1",
                    Title = "title_mock_challenge1",
                    Bonus = 100,
                    Content = "Content_mock_challenge1",
                    Deadline = DateTime.UtcNow.AddDays(20),
                    Max_Participant = 11
                }
                ));
            _mockRepository.Setup(m => m.GetAll()).Returns(
                new List<Challenge>()
                {  new Challenge()
                {
                   C_Id = "mock_challenge1",
                    Title = "title_mock_challenge1",
                    Bonus = 100,
                    Content = "Content_mock_challenge1",
                     Max_Participant = 11
                }
                }.AsQueryable().BuildMockDbSet().Object
                );
            _mockPaRepository.Setup(m => m.InsertAsync(It.IsAny<Participation>()))
               .Returns(Task.CompletedTask)
               .Callback<Participation>(x => toCheck = x).Throws(new InvalidOperationException());

            _mockPaRepository.Setup(m => m.GetAllList(It.IsAny<Expression<Func<Participation, bool>>>())).Returns(new List<Participation>()
            {
            });
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.ParticipateChallenge("mock_challenge1"));
            Assert.Equal("You have already participated this challenge", ex.Message);
        }

        //
        // Summary:
        //    [TestCase-ID: 11-1]
        //    Test if edit method returns error view if a user tries to edit challenge from other users
        //
        [Fact]
        public async Task EditNotOwnChallenge()
        {
            _mockRepository.Setup(m => m.GetAll()).Returns(
                new List<Challenge>()
                {  new Challenge()
                {
                   C_Id = "Id_mock_editNotOwnChallenge",
                    Title = "Title_mock_editNotOwnChallenge",
                    Bonus = 60,
                    Content = "Content_mock_editNotOwnChallenge",
                    Com_ID = "Com_ID_mock_editNotOwnChallenge"
                }
                }.AsQueryable().BuildMockDbSet().Object
                );

            //var ex = await Assert.ThrowsAsync<Exception>(() => _sut.Edit("Id_mock_editNotOwnChallenge"));
            //Assert.Equal("You can't edit challenge from other company", ex.Message);
            var result = await _sut.Edit("Id_mock_editNotOwnChallenge");
            Assert.IsType<ViewResult>(result);
            ViewResult value = (ViewResult)result;
            Assert.Equal("Error", value.ViewName);
        }
    }
}



