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

namespace PlattformChallenge.Controllers.Tests
{

    public class ChallengeControllerShould
    {
        private readonly Mock<IRepository<Challenge>> _mockRepository;
        private readonly Mock<IRepository<PlatformUser>> _mockPRpository;
        private readonly ChallengesController _sut;

        public ChallengeControllerShould() {

            _mockRepository = new Mock<IRepository<Challenge>>();
            _mockPRpository = new Mock<IRepository<PlatformUser>>();
            _sut = new ChallengesController(_mockRepository.Object, _mockPRpository.Object);
            var mock = new Mock<ControllerContext>();
            mock.Setup(p => p.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)).Returns("1");
            _sut.ControllerContext = mock.Object;
         }

        [Fact]

        public async Task demo() {

            Assert.True(1 == 1);
        }


        [Fact]
        public async Task CreateTest() {
       
   
            var challenge = new ChallengeCreateViewModel()
            {
                C_Id = Guid.NewGuid().ToString(),
                Title = "aaaa",
                Bonus = 2,
                Content = "aaaa",
                Release_Date = DateTime.Now,
                Max_Participant = 8,
            };

          var result= await _sut.Create(challenge);

        }
    }
}
