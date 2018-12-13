using System.Net.Http;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Gcpe.Hub.API;

namespace Gcpe.Hub.API.IntegrationTests
{
    public abstract class BaseWebApiTest : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        protected readonly WebApplicationFactory<Startup> _factory;

        public HttpClient Client { get; protected set; }

        public BaseWebApiTest(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            Client = _factory.CreateClient();
        }
    }
}
