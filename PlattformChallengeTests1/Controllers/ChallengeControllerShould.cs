using PlattformChallenge.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using PlattformChallenge.Core.Interfaces;
using PlattformChallenge.Core.Model;
using Xunit;
using PlattformChallenge.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Linq;
using PlattformChallenge.Models;
using System.Linq.Expressions;

namespace PlattformChallenge.Controllers.Tests
{

    public class ChallengeControllerShould
    {
        private readonly Mock<IRepository<Challenge>> _mockRepository;
        private readonly Mock<IRepository<PlatformUser>> _mockPRpository;
        private readonly Mock<IRepository<Language>> _mockLRepository;
        private readonly Mock<IRepository<LanguageChallenge>> _mockLCRepository;
        private readonly ChallengesController _sut;

        public ChallengeControllerShould()
        {

            _mockRepository = new Mock<IRepository<Challenge>>();
            _mockPRpository = new Mock<IRepository<PlatformUser>>();
            _mockLRepository = new Mock<IRepository<Language>>();
            _mockLCRepository = new Mock<IRepository<LanguageChallenge>>();
            _sut = new ChallengesController(_mockRepository.Object, _mockPRpository.Object, _mockLRepository.Object, _mockLCRepository.Object);
            var mock = new Mock<HttpContext>();
            var context = new ControllerContext(new ActionContext(mock.Object, new RouteData(), new ControllerActionDescriptor()));
            mock.Setup(p => p.User.FindFirst(ClaimTypes.NameIdentifier)).Returns(new Claim(ClaimTypes.NameIdentifier, "1"));
            _sut.ControllerContext = context;
        }


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


        [Fact]
        public async Task CreateChallengeTest()
        {
            Challenge savedChallenge = null;
            List<LanguageChallenge> savedLc = new List<LanguageChallenge>();

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
            _mockRepository.Setup(m => m.InsertAsync(It.IsAny<Challenge>()))
                .Returns(Task.CompletedTask)
                .Callback<Challenge>(c => savedChallenge = c);

            _mockLCRepository.Setup(lc => lc.InsertAsync(It.IsAny<LanguageChallenge>()))
                .Returns(Task.CompletedTask)
                .Callback<LanguageChallenge>(x => savedLc.Add(x));

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
            _mockRepository
                .Setup(m => m.FindByAndToListAsync(It.IsAny<Expression<Func<Challenge, bool>>>(), It.IsAny<Expression<Func<Challenge, object>>[]>()))
                .Returns(Task.FromResult<List<Challenge>>(null));
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
                Release_Date = DateTime.Now,
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
            IQueryable<Challenge> query = l.AsQueryable();
            _mockRepository
                .Setup(m => m.FindByAndToListAsync(It.IsAny<Expression<Func<Challenge, bool>>>(), It.IsAny<Expression<Func<Challenge, object>>[]>()))
                .Returns(Task.FromResult(l));
            var result = await _sut.Index();
            Assert.IsType<ViewResult>(result);
            var value = result as ViewResult;
            var savedChallengeList = (Task<List<Challenge>>)value.Model;
            Assert.Equal("test title 1", savedChallengeList.Result.ElementAt<Challenge>(0).Title);
            Assert.Equal("1111", savedChallengeList.Result.ElementAt<Challenge>(0).Com_ID);
            Assert.Equal(200, savedChallengeList.Result.ElementAt<Challenge>(0).Bonus);
            Assert.Equal("2cde", savedChallengeList.Result.ElementAt<Challenge>(1).C_Id);
            Assert.Equal(18, savedChallengeList.Result.ElementAt<Challenge>(1).Max_Participant);
            Assert.Equal(DateTime.Now.Day, savedChallengeList.Result.ElementAt<Challenge>(1).Release_Date.Day);
        }






    }





    
}



