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
    }
}



