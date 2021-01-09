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

namespace PlattformChallenge.Controllers.Tests
{

    public class ChallengeControllerShould
    {
        private readonly Mock<IRepository<Challenge>> _mockRepository;
        private readonly Mock<IRepository<PlatformUser>> _mockPRpository;
        private readonly Mock<IRepository<Language>> _mockLRepository;
        private readonly Mock<IRepository<LanguageChallenge>> _mockLCRepository;
        private readonly Mock<IRepository<Participation>> _mockPaRepository;

        private readonly ChallengesController _sut;

        public ChallengeControllerShould()
        {

            _mockRepository = new Mock<IRepository<Challenge>>();
            _mockPRpository = new Mock<IRepository<PlatformUser>>();
            _mockLRepository = new Mock<IRepository<Language>>();
            _mockLCRepository = new Mock<IRepository<LanguageChallenge>>();
            _mockPaRepository = new Mock<IRepository<Participation>>();
            _sut = new ChallengesController(_mockRepository.Object, _mockPRpository.Object, _mockLRepository.Object, _mockLCRepository.Object, _mockPaRepository.Object);
            var mock = new Mock<HttpContext>();
            var context = new ControllerContext(new ActionContext(mock.Object, new RouteData(), new ControllerActionDescriptor()));
            mock.Setup(p => p.User.FindFirst(ClaimTypes.NameIdentifier)).Returns(new Claim(ClaimTypes.NameIdentifier, "1"));
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
        public void ReturnViewForCreate()
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
                Release_Date = DateTime.Now,
                Max_Participant = 8,
                IsSelected = new bool[] { true, false, true }
            };
            var result = _sut.Create();
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
                Release_Date = DateTime.Now,
                Max_Participant = 8,
                IsSelected = new bool[] { true, false, true }
            };


            var result = await _sut.Create(challenge);
            Assert.Equal(challenge.Title, savedChallenge.Title);
            Assert.Equal(challenge.Bonus, savedChallenge.Bonus);
            Assert.Equal(challenge.Content, savedChallenge.Content);
            Assert.Equal(challenge.Release_Date, savedChallenge.Release_Date);
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
                Release_Date = DateTime.Now,
                Max_Participant = 8,
                IsSelected = new bool[] { true, false, true }
            };
            var result = await _sut.Create(challenge);
            Assert.IsType<ViewResult>(result);
            ViewResult value = (ViewResult)result;
            Assert.Equal(challenge, value.Model);
            IEnumerable<ModelError> allErrors = _sut.ModelState.Values.SelectMany(v => v.Errors);
            Assert.Equal("failed to create the challenge, please try again", allErrors.ElementAt(0).ErrorMessage);
            _mockLRepository.Verify(l => l.GetAllListAsync(), Times.Never);
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
            var value = result as ViewResult;
            var errorvm = value.Model as ErrorViewModel;
            var errorInfo = errorvm.RequestId;
            Assert.Equal("Error", value.ViewName);
            Assert.Equal("invalid challenge id value for details", errorInfo);
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
            var value = result as ViewResult;
            var errorvm = value.Model as ErrorViewModel;
            var errorInfo = errorvm.RequestId;
            Assert.Equal("Error", value.ViewName);
            Assert.Equal("there's no challenge with this id, please check again", errorInfo);

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
                Release_Date = DateTime.Now.AddDays(-2),
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
                Release_Date = DateTime.Now,
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
            var result = await _sut.Index(null, null, null);
            Assert.IsType<ViewResult>(result);
            var value = result as ViewResult;
            var savedChallengeList = value.Model as PaginatedList<Challenge>;
            Assert.Equal("test title 1", savedChallengeList.ElementAt<Challenge>(1).Title);
            Assert.Equal("1111", savedChallengeList.ElementAt<Challenge>(1).Com_ID);
            Assert.Equal(200, savedChallengeList.ElementAt<Challenge>(1).Bonus);
            Assert.Equal("2cde", savedChallengeList.ElementAt<Challenge>(0).C_Id);
            Assert.Equal(18, savedChallengeList.ElementAt<Challenge>(0).Max_Participant);
            Assert.Equal(DateTime.Now.Day, savedChallengeList.ElementAt<Challenge>(0).Release_Date.Day);
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
                Release_Date = DateTime.Now.AddDays(-6),
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
                Release_Date = DateTime.Now.AddDays(-4),
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
                Release_Date = DateTime.Now,
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
            var result = await _sut.Index(null, "bonus_desc", null);
            PaginatedList<Challenge> sorted = null;
            var value = result as ViewResult;
            sorted = (PaginatedList<Challenge>)value.Model;
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
                Release_Date = DateTime.Now.AddDays(-5),
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
                Release_Date = DateTime.Now.AddDays(-3),
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
                Release_Date = DateTime.Now,
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
            var result = await _sut.Index(null, "quota_desc", null);
            PaginatedList<Challenge> sorted = null;
            var value = result as ViewResult;
            sorted = (PaginatedList<Challenge>)value.Model;
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
                Release_Date = DateTime.Now.AddDays(-5),
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
                Release_Date = DateTime.Now.AddDays(-3),
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
                Release_Date = DateTime.Now,
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
            var result = await _sut.Index(null, "", null);
            PaginatedList<Challenge> sorted = null;
            var value = result as ViewResult;
            sorted = (PaginatedList<Challenge>)value.Model;
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
                Release_Date = DateTime.Now.AddDays(-2),
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
                Release_Date = DateTime.Now,
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
                Release_Date = DateTime.Now.AddDays(2),
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
            var result = await _sut.Index(null, null, "2");
            PaginatedList<Challenge> searched = null;
            var value = result as ViewResult;
            searched = (PaginatedList<Challenge>)value.Model;
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
                    C_Id = "3cde",
                    Title = "test title 3",
                    Bonus = 600,
                    Content = "test content 3",
                    Release_Date = DateTime.Now.AddDays(2),
                    Max_Participant = 118,
                    Com_ID = "3333",
                    Company = new PlatformUser()
                    {
                        Id = "test3.com"
                    }
                }
                ));

            _mockRepository.Setup(m => m.GetAll()).Returns(
                new List<Challenge>()
                {  new Challenge()
                {
                    C_Id = "3cde",
                    Title = "test title 3",
                    Bonus = 600,
                    Content = "test content 3",
                    Release_Date = DateTime.Now.AddDays(2),
                    Max_Participant = 118,
                    Com_ID = "3333",
                    Company = new PlatformUser()
                    {
                        Id = "test3.com"
                    }
                }
                }.AsQueryable().BuildMockDbSet().Object
                );
            _mockPaRepository.Setup(m => m.InsertAsync(It.IsAny<Participation>()))
                .Returns(Task.CompletedTask)
                .Callback<Participation>(x => toCheck = x);

            _mockPaRepository.Setup(m => m.GetAllList(It.IsAny<Expression<Func<Participation, bool>>>())).Returns(new List<Participation>()
            {
            });
            var result = await _sut.ParticipateChallenge("3cde");
            Assert.Equal("3cde", toCheck.C_Id);
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
               .Callback<Participation>(x => toCheck = x);

            _mockPaRepository.Setup(m => m.GetAllList(It.IsAny<Expression<Func<Participation, bool>>>())).Returns(new List<Participation>()
            {
            });
            await _sut.ParticipateChallenge("mock_challenge1");
            Assert.ThrowsAsync<Exception>(() => _sut.ParticipateChallenge("mock_challenge1"));
        }

       
    }
}



