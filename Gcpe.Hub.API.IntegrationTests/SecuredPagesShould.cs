using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Gcpe.Hub.API.IntegrationTests
{
    public class SecuredPagesShould : BaseWebApiTest
    {
        public SecuredPagesShould(CustomWebApplicationFactory<TestStartup> factory) : base(factory) { }

        [Theory]
        [InlineData("/api/messages", "POST")]
        [InlineData("/api/messages/123", "PUT")]
        [InlineData("/api/messages/123", "DELETE")]
        [InlineData("/api/posts", "POST")]
        [InlineData("/api/posts/123", "PUT")]
        [InlineData("/api/activities", "POST")]
        [InlineData("/api/activities/123", "PUT")]
        [InlineData("/api/socialmediaposts", "POST")]
        [InlineData("/api/socialmediaposts/123", "PUT")]
        [InlineData("/api/socialmediaposts/123", "DELETE")]
        public async Task RequireAnAuthenticatedUser(string url, string requestType)
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.UseStartup<Startup>();
            }).CreateClient();
            //var builder = new WebHostBuilder().UseStartup<Startup>();

            //var testServer = new TestServer(builder);
            //var client = testServer.CreateClient();
            var content = new StringContent("test");

            var response = new HttpResponseMessage();
            switch (requestType)
            {
                case "POST":
                    response = await client.PostAsync(url, content);
                    break;
                case "PUT":
                    response = await client.PutAsync(url, content);
                    break;
                case "DELETE":
                    response = await client.DeleteAsync(url) ;
                    break;
            }

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Theory]
        [InlineData("/api/posts")]
        [InlineData("/api/posts/latest/7")]
        public async Task NotRequireAnAuthenticatedUser(string url)
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.UseStartup<Startup>();
            }).CreateClient();

            var response = await client.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
