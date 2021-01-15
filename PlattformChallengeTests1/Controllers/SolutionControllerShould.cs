using System;
using System.Collections.Generic;
using System.Text;
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
using Moq;
using PlattformChallenge.Controllers;

namespace PlattformChallenge_UnitTest.Controllers
{
    class SolutionControllerShould
    {

        private readonly Mock<IRepository<Solution>> _mockSRepository;
        private readonly Mock<IRepository<Participation>> _mockPaRepository;
        private readonly SolutionController _sut;

        public SolutionControllerShould()
        {
            _mockSRepository = new Mock<IRepository<Solution>>();
            _mockPaRepository = new Mock<IRepository<Participation>>();
            _sut = new SolutionController(_mockSRepository.Object, _mockPaRepository.Object);
            var mock = new Mock<HttpContext>();
            var context = new ControllerContext(new ActionContext(mock.Object, new RouteData(), new ControllerActionDescriptor()));
            mock.Setup(p => p.User.FindFirst(ClaimTypes.NameIdentifier)).Returns(new Claim(ClaimTypes.NameIdentifier, "1"));
            _sut.ControllerContext = context;
        }




    }
}
