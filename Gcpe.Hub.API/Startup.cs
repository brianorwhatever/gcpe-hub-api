using System.Collections.Generic;
using System.Data.SqlClient;
using AutoMapper;
using Gcpe.Hub.API.Helpers;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Gcpe.Hub.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        private IConfiguration Configuration { get; }
        private IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper();

            services.AddDbContext<HubDbContext>(options => options.UseSqlServer(Configuration["HubDbContext"])
                .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning)));

            this.ConfigureAuth(services);

            services.AddMvc()
                .AddJsonOptions(opt => {
                    opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc("v1", new Info
                {
                    Version = "Alpha",
                    Title = "BC Gov Hub API service",
                    Description = "The .Net Core API for the Hub"
                });
                setupAction.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = Configuration["AzureAd:AuthorizationUrl"],
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "openid login scope" },
                        { "profile", "profile scope" },
                        { "email", "email scope" },
                    }
                });
                setupAction.OperationFilter<SecurityRequirementsOperationFilter>();
                setupAction.OperationFilter<OperationIdCorrectionFilter>();
            });

            services.AddHealthChecks()
                .AddCheck("sql", () =>
                {
                    using (var connection = new SqlConnection(Configuration["HubDbContext"]))
                    {
                        try
                        {
                            connection.Open();
                        }
                        catch (SqlException)
                        {
                            return HealthCheckResult.Unhealthy();
                        }

                        return HealthCheckResult.Healthy();
                    }
                })
                .AddCheck("Webserver is running", () => HealthCheckResult.Healthy("Ok"));

            services.AddCors();
        }

        public virtual void ConfigureAuth(IServiceCollection services)
        {
            services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
                .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));
                
            services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
            {
                options.Authority = options.Authority + "/v2.0/";
                options.TokenValidationParameters.ValidAudiences = new string[] { options.Audience, $"api://{options.Audience}" };
                options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.ValidateAadIssuer;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ReadAccess", policy => policy.RequireRole("Viewer", "Contributor"));
                options.AddPolicy("WriteAccess", policy => policy.RequireRole("Contributor"));
            });


        }

        private class OperationIdCorrectionFilter : IOperationFilter
        { // GetActivity() instead of ApiActivitiesByIdGet()
            public void Apply(Operation operation, OperationFilterContext context)
            {
                if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
                {
                    operation.OperationId = actionDescriptor.ActionName;
                }
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // app.UseHsts();
            }

            app.UseHealthChecks("/hc", new HealthCheckOptions { AllowCachingResponses = false });

            // app.UseHttpsRedirection();

            // temporary CORS fix
            app.UseCors(opts => opts.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed((host) => true).AllowCredentials());

            app.UseAuthentication();

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.OAuthClientId(Configuration["AzureAd:ClientId"]);
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "BC Gov Hub API service");
            });
        }
    }
}
