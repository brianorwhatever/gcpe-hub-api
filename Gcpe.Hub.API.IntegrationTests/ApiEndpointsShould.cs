using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Gcpe.Hub.API.IntegrationTests
{
    public class ApiEndpointsShould : BaseWebApiTest
    {
        public ApiEndpointsShould(CustomWebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Theory]
        [InlineData("/", "text/html", "it should redirect to the Swagger UI page that display HTML content")]
        // [InlineData("/hc", "application/json", "it should output JSON data")]
        [InlineData("/api/Posts", "application/json", "it should output JSON data")]
        public async Task ReturnSuccessAndCorrectContentType(string url, string contentType, string because = "")
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await Client.GetAsync(url);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            response.Content.Headers.ContentType.ToString().Should().Contain(contentType, because);
        }
    }
}
