using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;
using System.Net.Http;
using PlattformChallenge.IntegrationTests.Helpers;
using Newtonsoft.Json;
using PlattformChallenge.Core.Model;

namespace PlattformChallenge.IntegrationTests.IntegrationTests
{
    public class ChallengesControllerIntegrationTests:
        IClassFixture<CustomWebApplicationFactory<PlattformChallenge.Startup>>
    {
        private HttpClient _client;
        private readonly CustomWebApplicationFactory<PlattformChallenge.Startup> _factory;
                                                                                          

        public ChallengesControllerIntegrationTests(CustomWebApplicationFactory<PlattformChallenge.Startup> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            }) ;
        }

        [Fact]
        public async Task Get_IndexPage() {
            var indexPage = await _client.GetAsync("/Challenges");
            var content = await HtmlHelpers.GetDocumentAsync(indexPage);
            
           
        }

        
    }
}
