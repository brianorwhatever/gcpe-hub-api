using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gcpe.Hub.API.IntegrationTests
{
    public class SecuredPagesShould : BaseWebApiTest
    {
        public SecuredPagesShould(CustomWebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Theory(Skip = "Not available yet...")]
        [InlineData("/SecurePage")]
        public async Task RequireAnAuthenticatedUser(string url)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var client = _factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await client.GetAsync(url);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.OriginalString.Should().StartWith("http://localhost/Identity/Account/Login");
        }
    }
}
