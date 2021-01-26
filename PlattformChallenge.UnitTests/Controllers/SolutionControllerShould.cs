using System;
using System.Collections.Generic;
using PlattformChallenge.Core.Interfaces;
using PlattformChallenge.Core.Model;
using Xunit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Linq;
using PlattformChallenge.Models;
using MockQueryable.Moq;
using Moq;
using PlattformChallenge.Controllers;
using PlattformChallenge.ViewModels;

namespace PlattformChallenge.UnitTest.Controllers
{
    public class SolutionControllerShould
    {

        private readonly Mock<IRepository<Solution>> _mockSRepository;
        private readonly Mock<IRepository<Challenge>> _mockCRepository;
        private readonly SolutionController _sut;

        public SolutionControllerShould()
        {
            _mockSRepository = new Mock<IRepository<Solution>>();
            _mockCRepository = new Mock<IRepository<Challenge>>();
            _sut = new SolutionController(_mockSRepository.Object, _mockCRepository.Object);
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

            var c = new List<Challenge>() {
                 new Challenge(){
                C_Id = "test1",
                Title = "test title 1",
                Bonus = 200,
                Content = "test content 1",
                Release_Date = DateTime.Now.AddDays(-2),
                Deadline = DateTime.Now.AddDays(30),
                Max_Participant = 8,
                Com_ID = "1111",
                Company = new PlatformUser(){
                    Id = "test1.com"
                }
                },
                   new Challenge(){
                C_Id = "test2",
                Title = "test title 2",
                Bonus = 400,
                Content = "test content 2",
                Release_Date = DateTime.Now,
                Deadline = DateTime.Now.AddDays(30),
                Max_Participant = 18,
                Com_ID = "2222",
                Company = new PlatformUser(){
                    Id = "test2.com"
                }
                }
            };


            var queryc = c.AsQueryable().BuildMockDbSet();
            _mockCRepository
                .Setup(n => n.GetAll())
                .Returns((IQueryable<Challenge>)queryc.Object);

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
        //    [TestCase-ID: 62-3]
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

            var c = new List<Challenge>() {
                 new Challenge(){
                C_Id = "test1",
                Title = "test title 1",
                Bonus = 200,
                Content = "test content 1",
                Release_Date = DateTime.Now.AddDays(-2),
                Deadline = DateTime.Now.AddDays(30),
                Max_Participant = 8,
                Com_ID = "1111",
                Company = new PlatformUser(){
                    Id = "test1.com"
                }
                },
                   new Challenge(){
                C_Id = "test2",
                Title = "test title 2",
                Bonus = 400,
                Content = "test content 2",
                Release_Date = DateTime.Now,
                Deadline = DateTime.Now.AddDays(30),
                Max_Participant = 18,
                Com_ID = "2222",
                Company = new PlatformUser(){
                    Id = "test2.com"
                }
                }
            };


            var queryc = c.AsQueryable().BuildMockDbSet();
            _mockCRepository
                .Setup(n => n.GetAll())
                .Returns((IQueryable<Challenge>)queryc.Object);

            var query = s.AsQueryable().BuildMockDbSet();
            _mockSRepository
                .Setup(m => m.GetAll())
                .Returns(query.Object);

            var result = await _sut.List(null, null, "test1");
            var value = result as ViewResult;
            var t = value.Model as BestSolutionViewModel;
            var sorted = t.Solutions;
            Assert.Equal(s.ElementAt(0).Point, sorted.ElementAt(1).Point);
            Assert.Equal(s.ElementAt(2).Point, sorted.ElementAt(0).Point);


        }


        //
        // Summary:
        //    [TestCase-ID: 62-4]
        //     Test if the Solutions after sorted by date return create result.
        //
        [Fact]
        public async Task SortSolutionsByDateList()
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
            var c = new List<Challenge>() {
                 new Challenge(){
                C_Id = "test1",
                Title = "test title 1",
                Bonus = 200,
                Content = "test content 1",
                Release_Date = DateTime.Now.AddDays(-2),
                Deadline = DateTime.Now.AddDays(30),
                Max_Participant = 8,
                Com_ID = "1111",
                Company = new PlatformUser(){
                    Id = "test1.com"
                }
                },
                   new Challenge(){
                C_Id = "test2",
                Title = "test title 2",
                Bonus = 400,
                Content = "test content 2",
                Release_Date = DateTime.Now,
                Deadline = DateTime.Now.AddDays(30),
                Max_Participant = 18,
                Com_ID = "2222",
                Company = new PlatformUser(){
                    Id = "test2.com"
                }
                }
            };


            var queryc = c.AsQueryable().BuildMockDbSet();
            _mockCRepository
                .Setup(n => n.GetAll())
                .Returns((IQueryable<Challenge>)queryc.Object);

     

            var query = s.AsQueryable().BuildMockDbSet();
            _mockSRepository
                .Setup(m => m.GetAll())
                .Returns(query.Object);

            var result = await _sut.List(null, "date_desc", "test1");
            var value = result as ViewResult;
            var t = value.Model as BestSolutionViewModel;
            var sorted = t.Solutions;
            Assert.Equal(s.ElementAt(0).Point, sorted.ElementAt(1).Point);
            Assert.Equal(s.ElementAt(2).Point, sorted.ElementAt(0).Point);
        }


        //
        // Summary:
        //    [TestCase-ID: 20-1]
        //     Test if list() return correct best solution for now
        //
        [Fact]
        public async Task BestSolutionList()
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

            var c = new List<Challenge>() {
                 new Challenge(){
                C_Id = "test1",
                Title = "test title 1",
                Bonus = 200,
                Content = "test content 1",
                Release_Date = DateTime.Now.AddDays(-2),
                Deadline = DateTime.Now.AddDays(30),
                Max_Participant = 8,
                Com_ID = "1111",
                Company = new PlatformUser(){
                    Id = "test1.com"
                }
                },
                   new Challenge(){
                C_Id = "test2",
                Title = "test title 2",
                Bonus = 400,
                Content = "test content 2",
                Release_Date = DateTime.Now,
                Deadline = DateTime.Now.AddDays(30),
                Max_Participant = 18,
                Com_ID = "2222",
                Company = new PlatformUser(){
                    Id = "test2.com"
                }
                }
            };


            var queryc = c.AsQueryable().BuildMockDbSet();
            _mockCRepository
                .Setup(n => n.GetAll())
                .Returns((IQueryable<Challenge>)queryc.Object);


            var query = s.AsQueryable().BuildMockDbSet();
            _mockSRepository
                .Setup(m => m.GetAll())
                .Returns(query.Object);


            var result = await _sut.List(null, null, "test1");
            var value = result as ViewResult;
            var t = value.Model as BestSolutionViewModel;
            var bs = t.Best_Point;
            Assert.Equal(s.ElementAt(2).Point, bs);
        }


        //
        // Summary:
        //    [TestCase-ID: 20-2]
        //     Test if the Solutions after using list() is the expected result, when there is no solution in the list
        //
        [Fact]
        public async Task ListWithoutSolutions()
        {
            var s = new List<Solution>() {

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
                }
            };

            var c = new List<Challenge>() {
                 new Challenge(){
                C_Id = "test1",
                Title = "test title 1",
                Bonus = 200,
                Content = "test content 1",
                Release_Date = DateTime.Now.AddDays(-2),
                Deadline = DateTime.Now.AddDays(30),
                Max_Participant = 8,
                Com_ID = "1111",
                Company = new PlatformUser(){
                    Id = "test1.com"
                }
                },
                   new Challenge(){
                C_Id = "test2",
                Title = "test title 2",
                Bonus = 400,
                Content = "test content 2",
                Release_Date = DateTime.Now,
                Deadline = DateTime.Now.AddDays(30),
                Max_Participant = 18,
                Com_ID = "2222",
                Company = new PlatformUser(){
                    Id = "test2.com"
                }
                }
            };


            var queryc = c.AsQueryable().BuildMockDbSet();
            _mockCRepository
                .Setup(n => n.GetAll())
                .Returns((IQueryable<Challenge>)queryc.Object);

            var query = s.AsQueryable().BuildMockDbSet();
            _mockSRepository
                .Setup(m => m.GetAll())
                .Returns(query.Object);

            var result = await _sut.List(null, null, "test1");
            var value = result as ViewResult;
            var t = value.Model as BestSolutionViewModel;
            Assert.Null(t.Best_Point);
        }


    }
    
}
