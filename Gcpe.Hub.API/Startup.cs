﻿using AutoMapper;
using Gcpe.Hub.API.Data;
using Gcpe.Hub.Data.Entity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.HealthChecks;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;

namespace Gcpe.Hub.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper();

            // services.AddTransient<Seeder>(); uncomment to seed the database
            // services.AddScoped<IRepository, Repository>(); uncomment for use with the database

            // dependency injection for interfacing with in memory data
            services.AddSingleton<IDataContext, InMemoryDataContext>();
            services.AddSingleton<IRepository, InMemoryRepository>();

            services.AddDbContext<HubDbContext>(options => options.UseSqlServer(Configuration["HubDbContext"])
                .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning)));


            services.AddMvc()
                .AddJsonOptions(opt => opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.Authority = Configuration["Jwt:Authority"];
                o.Audience = Configuration["Jwt:Audience"];
                o.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        ctx.NoResult();

                        ctx.Response.StatusCode = 500;
                        ctx.Response.ContentType = "text/plain";
                        if (Environment.IsDevelopment())
                        {
                            return ctx.Response.WriteAsync(ctx.Exception.ToString());
                        }

                        return ctx.Response.WriteAsync("An error occurred processing your authentication");
                    }
                };
            });


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "Alpha",
                    Title = "BC Gov Hub API service",
                    Description = "The .Net Core 2.1 API for the Hub"
                });
            });

            services.AddHealthChecks(checks =>
            {
                checks.AddSqlCheck("Gcpe.Hub", Configuration["HubDbContext"]);
                // checks.AddUrlCheck("https://github.com");
                checks.AddCheck("Webserver is running", () => HealthCheckResult.Healthy("Ok"));
            });

            services.AddCors();
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

            // app.UseHttpsRedirection();

            

            // temporary CORS fix
            app.UseCors(opts => opts.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseAuthentication();

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "BC Gov Hub API service");
            });
        }
    }
}
