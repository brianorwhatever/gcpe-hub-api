using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gcpe.Hub.API.IntegrationTests
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration, IHostingEnvironment env) : base(configuration, env)
        {
        }

        public override void ConfigureAuth(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test Scheme";
                options.DefaultChallengeScheme = "Test Scheme";
            }).AddTestAuth(o => { });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ReadAccess", policy => { policy.RequireAssertion(p => { return true; }); });
                options.AddPolicy("WriteAccess", policy => { policy.RequireAssertion(p => { return true; }); });
            });
        }
    }
}
