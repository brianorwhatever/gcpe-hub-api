using System.Net.Http;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Gcpe.Hub.API;
using Microsoft.AspNetCore.Hosting;

namespace Gcpe.Hub.API.IntegrationTests
{
    public abstract class BaseWebApiTest : IClassFixture<CustomWebApplicationFactory<TestStartup>>
    {
        protected readonly WebApplicationFactory<TestStartup> _factory;
        public HttpClient Client { get; protected set; }

        public BaseWebApiTest(CustomWebApplicationFactory<TestStartup> factory)
        {
            _factory = factory;
            Client = _factory.CreateClient();
        }
    }
}
