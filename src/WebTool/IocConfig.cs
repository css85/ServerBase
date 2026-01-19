using System;
using TwelveMoments.Shared.Utility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.ServerApp.Config;
using Shared.ServerApp.Extensions;
using Shared.ServerApp.Services;
using Shared.Session;
using Shared.Session.Base;
using WebTool.Config;
using WebTool.Connection.Services;
using WebTool.Connection.Session;
using WebTool.Database;
using WebTool.Identity;
using WebTool.Identity.Base;
using WebTool.Services;
using TwelveMoments.Shared.Common;

namespace WebTool
{
    public class IocConfig
    {
        public static IServiceCollection Configure(IServiceCollection services, IConfiguration config, IHostEnvironment env)
        {
            services.AddServerApp(config, env);

            var dbSettings = services.GetSettings<DatabaseSettings>(config, nameof(DatabaseSettings));
            var webToolDbConnectionString = config.GetValue("WebToolDbConnectionString", dbSettings.WebToolConnectionString);
            if (string.IsNullOrEmpty(webToolDbConnectionString) == false)
            {
                //MySQL DBContext Pooling
                services.AddDbContextPool<WebToolCtx>((options) =>
                {
                    if (env.IsDevelopment())
                    {
                        options.EnableSensitiveDataLogging(true);
                        options.EnableDetailedErrors();
                    }

                    options.UseMySql(webToolDbConnectionString, ServerVersion.AutoDetect(webToolDbConnectionString));
                }, 50);
            }

            var userDbConnectionString = config.GetValue("UserConnectionStrings", dbSettings.UserConnectionStrings);

            services.AddSessionBasic();
            services.AddServerSession<ServerSession, ServerSessionService>(typeof(Program));

            services.AddSingleton<WebToolAppContextService>();
            services.AddSingleton<AppContextServiceBase, WebToolAppContextService>(p => p.GetRequiredService<WebToolAppContextService>());

            services.AddChangeAbleSettings<WebToolAppSettings>(config, nameof(WebToolAppSettings));

            services.AddSingleton<SelectItemService>();

            services.AddIdentity<ApplicationUser, ApplicationRole>(o =>
                {
                    o.User.RequireUniqueEmail = true;

                    o.Password.RequireNonAlphanumeric = false;
                    o.Password.RequireLowercase = false;
                    o.Password.RequireUppercase = false;
                    o.Password.RequireDigit = false;
                })
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<WebToolCtx>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Identity/Account/AccessDenied");
                options.Cookie.Name = "Cookie";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(720);
                options.LoginPath = new PathString("/Identity/Account/Login");
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            });

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.Encoder =
                    System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                options.JsonSerializerOptions.WriteIndented = false;
                options.JsonSerializerOptions.MaxDepth = 0;
                options.JsonSerializerOptions.IncludeFields = true;
                options.JsonSerializerOptions.Converters.Add(new BigIntegerConverter());
            });
     
            services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("RequireAdminRole", policy =>
                    {
                        policy.RequireAuthenticatedUser().RequireRole(nameof(RoleType.Admin));
                    });
            });

            services.AddRazorPages(options =>
            {
                //options.Conventions.AuthorizeFolder("/Admin", "RequireAdminRole");
            });
            
            
           
            // services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerOptions>();
            // services
            //     .AddSwaggerGen()
            //     .AddCachedSwaggerGen(option => option.OperationFilter<SwaggerDefaultValues>());

            return services;
        }
    }
}
