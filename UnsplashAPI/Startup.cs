using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using UnsplashAPI.Data;
using UnsplashAPI.Modules;
using UnsplashAPI.Services;

namespace UnsplashAPI {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContext<AppDbContext>(options => {
                options.UseSqlServer(
                    Configuration.GetConnectionString("AppConnection")
                );
            });

            services.AddIdentity<User, IdentityRole>(config => {
                    config.Password.RequiredLength = 3;
                    config.Password.RequireLowercase = false;
                    config.Password.RequireNonAlphanumeric = false;
                    config.Password.RequireUppercase = false;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(config => {
                config.Cookie.Name = "Unsplash_Cookie";

                config.Cookie.SameSite = SameSiteMode.None;

                config.Events = new CookieAuthenticationEvents {
                    OnRedirectToAccessDenied = context => {
                        context.HttpContext.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    },
                    OnRedirectToLogin = context => {
                        context.HttpContext.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    },
                };
            });

            services.AddAuthorization(config => {
                config.AddPolicy("User_Policy", policyBuilder => {
                    policyBuilder.RequireRole("User");
                });
                config.AddPolicy("Admin_Policy", policyBuilder => {
                    policyBuilder.RequireRole("User");
                    policyBuilder.RequireRole("Admin");
                });
            });

            services.AddScoped<IUsersServices, UsersServices>();
            services.AddScoped<IPhotosServices, PhotosServices>();

            services.AddControllers()
                .AddNewtonsoftJson(options 
                    => options.SerializerSettings.ContractResolver = new PropertyIgnoringContractResolver());

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "UnsplashAPI", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "UnsplashAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
