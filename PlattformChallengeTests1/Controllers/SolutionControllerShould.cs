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
using PlattformChallenge.ViewModels;

namespace PlattformChallenge_UnitTest.Controllers
{
    public class SolutionControllerShould
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


        //
        // Summary:
        //    [TestCase-ID: 62-1]
        //     Test if the view of list() is the expected type.
        //
        [Fact]
        public async Task ListReturnView()
        {
            var s = new List<Solution>() {
                 new Solution(){
                S_Id = "1abc",
                URL = "test URL 1",
                Status = StatusEnum.Rated,
                Point=10,
                Submit_Date = DateTime.Now.AddDays(-2),
                 Participation = new Participation(){
                    C_Id = "test1",
                    Programmer = new PlatformUser()
                    {
                        Name ="Xiang1",
                    }
                }
                },

                new Solution(){
                S_Id = "2abc",
                URL = "test URL 2",
                Status = StatusEnum.Rated,
                Point=100,
                Submit_Date = DateTime.Now.AddDays(+2),
                 Participation = new Participation(){
                    C_Id = "test2",
                    Programmer = new PlatformUser()
                    {
                        Name ="Xiang2",
                    }
                }
                }
            };
            var query = s.AsQueryable().BuildMockDbSet();
            _mockSRepository
                .Setup(m => m.GetAll())
                .Returns(query.Object);

            var result = await _sut.List(null, null, "test1");
            Assert.IsType<ViewResult>(result);
        }



        //
        // Summary:
        //    [TestCase-ID: 62-2]
        //     Test if the view of list() is the error type with null challenge id.
        //
        [Fact]
        public async Task ListWithoutCId()
        {
            var s = new List<Solution>() {
                 new Solution(){
                S_Id = "1abc",
                URL = "test URL 1",
                Status = StatusEnum.Rated,
                Point=10,
                Submit_Date = DateTime.Now.AddDays(-2),
                 Participation = new Participation(){
                    C_Id = "test1",
                    Programmer = new PlatformUser()
                    {
                        Name ="Xiang1",
                    }
                }
                },

                new Solution(){
                S_Id = "2abc",
                URL = "test URL 2",
                Status = StatusEnum.Rated,
                Point=100,
                Submit_Date = DateTime.Now.AddDays(+2),
                 Participation = new Participation(){
                    C_Id = "test2",
                    Programmer = new PlatformUser()
                    {
                        Name ="Xiang2",
                    }
                }
                }
            };
            var query = s.AsQueryable().BuildMockDbSet();
            _mockSRepository
                .Setup(m => m.GetAll())
                .Returns(query.Object);

            var result = await _sut.List(null, null, null);
            Assert.IsType<ViewResult>(result);
            var value = result as ViewResult;
            var errorvm = value.Model as ErrorViewModel;
            var errorInfo = errorvm.RequestId;
            Assert.Equal("Error", value.ViewName);
            Assert.Equal("invalid challenge id!", errorInfo);
        }
        //
        // Summary:
        //    [TestCase-ID: 62-2]
        //     Test if the Solutions after using list() is the expected result.
        //
        [Fact]
        public async Task SortSolutionsByPointList()
        {
            var s = new List<Solution>() {
                 new Solution(){
                S_Id = "1abc",
                URL = "test URL 1",
                Status = StatusEnum.Rated,
                Point=100,
                Submit_Date = DateTime.Now.AddDays(-2),
                 Participation = new Participation(){
                    C_Id = "test1",
                    Programmer = new PlatformUser()
                    {
                        Name ="Xiang1",
                    }
                }
                },

                new Solution(){
                S_Id = "2abc",
                URL = "test URL 2",
                Status = StatusEnum.Rated,
                Point=200,
                Submit_Date = DateTime.Now.AddDays(+2),
                 Participation = new Participation(){
                    C_Id = "test2",
                    Programmer = new PlatformUser()
                    {
                        Name ="Xiang2",
                    }
                }
                },

                new Solution(){
                S_Id = "2abc",
                URL = "test URL 2",
                Status = StatusEnum.Rated,
                Point=300,
                Submit_Date = DateTime.Now.AddDays(+4),
                 Participation = new Participation(){
                    C_Id = "test1",
                    Programmer = new PlatformUser()
                    {
                        Name ="Xiang3",
                    }
                }
                }
            };
            var query = s.AsQueryable().BuildMockDbSet();
            _mockSRepository
                .Setup(m => m.GetAll())
                .Returns(query.Object);

            var result = await _sut.List(null, null, "test1");
            var value = result as ViewResult;
            var savedSolutionList = value.Model as PaginatedList<Solution>;
            PaginatedList<Challenge> sorted = null;
            sorted = (PaginatedList<Challenge>)value.Model;



        }
    }
}
