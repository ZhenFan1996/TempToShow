using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;
using System.Net.Http;

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
        public async Task RenderApplicationForm() {
            var reponse = await _client.GetAsync("/Challenges");
            
            reponse.EnsureSuccessStatusCode();
            var reponseString = await reponse.Content.ReadAsStringAsync();
            
        }

        
    }
}
