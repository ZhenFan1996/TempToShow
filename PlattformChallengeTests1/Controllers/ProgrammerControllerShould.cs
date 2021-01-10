using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Moq;
using PlattformChallenge.Controllers;
using PlattformChallenge.Core.Interfaces;
using PlattformChallenge.Core.Model;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace PlattformChallenge_UnitTest.Controllers
{
    public class ProgrammerControllerShould
    {
        private readonly Mock<UserManager<PlatformUser>> _mockUseerManager;
        private readonly Mock<IRepository<Challenge>> _mockCRepo;
        private readonly Mock<IRepository<Participation>> _mockPRepo;
        private readonly ProgrammerController _sut;

        public ProgrammerControllerShould()
        {
            _mockCRepo = new Mock<IRepository<Challenge>>();
            _mockPRepo = new Mock<IRepository<Participation>>();
            _mockUseerManager = new Mock<UserManager<PlatformUser>>();
            var mock = new Mock<HttpContext>();
            var context = new ControllerContext(new ActionContext(mock.Object, new RouteData(), new ControllerActionDescriptor()));
            mock.Setup(p => p.User.FindFirst(ClaimTypes.NameIdentifier)).Returns(new Claim(ClaimTypes.NameIdentifier, "1"));
            _mockUseerManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new PlatformUser() {

                Id ="test-programmer",
                Name = "Zhen"
            });
            _sut = new ProgrammerController(_mockUseerManager.Object, _mockCRepo.Object, _mockPRepo.Object);
        }



    }
}
