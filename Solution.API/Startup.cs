using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Solution.API.Filters;
using Solution.API.Middlewares;
using Solution.DLL;
using Solution.DLL.Identity.DataModel;
using Solution.DLL.Identity.DBContext;
using Swashbuckle.AspNetCore.Swagger;

namespace Solution.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DatabaseConnection")));
            services.AddIdentity<AppUser, AppRole>().AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();

            // start token based work
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"]));
//
//            services.AddDefaultIdentity<AppUser>().AddEntityFrameworkStores<AppIdentityDbContext>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(config =>
            {
                config.RequireHttpsMetadata = false;
                config.SaveToken = true;
                config.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = signingKey,
                    ValidateAudience = true,
                    ValidAudience = this.Configuration["Tokens:Audience"],
                    ValidateIssuer = true,
                    ValidIssuer = this.Configuration["Tokens:Issuer"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });


            // end token based work

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddApiVersioning();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });

            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = "127.0.0.1:6379";
                option.InstanceName = "master";
            });


            //load general configuration from appsettings.json
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));

            //load ip rules from appsettings.json
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));


            services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
            AddALLExternalDependency(services);


            services.AddMvc(options =>
            {
                // All endpoints need authorization using our custom authorization filter
                options.Filters.Add(new CustomAuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
            });
        }

        private void AddALLExternalDependency(IServiceCollection services)
        {
            DLLDependency.Dependency(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            
            app.UseAuthentication();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
               
            }
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseMiddleware<MyIPRateMiddleware>();
            app.UseMvc();
            //SeedRoles.GenerateRole(app);
        }
    }
}
