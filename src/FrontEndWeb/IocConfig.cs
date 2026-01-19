using System;
using System.Security.Claims;
using System.Text;
using FrontEndWeb.Config;
using FrontEndWeb.Connection.Services;
using FrontEndWeb.Connection.Session;
using FrontEndWeb.Services;
using Shared.CdnStore;
using SampleGame.Shared.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Shared;
using Shared.ServerApp.Config;
using Shared.ServerApp.Extensions;
using Shared.ServerApp.Mvc;
using Shared.ServerApp.Services;
using Shared.Session;
using Shared.Session.Base;
using Shared.Session.Models;
using StackExchange.Redis.Extensions.Core.Configuration;
using SampleGame.Shared.Common;

namespace FrontEndWeb
{
    public static class IocConfig
    {
        public static void AddAllServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
        {
            services.AddServerApp(config, env);

            services.AddSessionBasic();
            services.AddServerSession<ServerSession, ServerSessionService>(typeof(Program));
            services.AddUserSession<UserFrontendSession, UserFrontendSessionService>(typeof(Program));

            services.AddSingleton<FrontEndAppContextService>();
            services.AddSingleton<AppContextServiceBase, FrontEndAppContextService>(p => p.GetRequiredService<FrontEndAppContextService>());
            
            var appSettings = services.AddChangeAbleSettings<FrontEndAppSettings>(config, nameof(FrontEndAppSettings));
            var tokenSettings = services.AddChangeAbleSettings<TokenSettings>(config, nameof(TokenSettings));
            

            var jwtSecretKey = Encoding.UTF8.GetBytes(tokenSettings.Secret);

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(jwtSecretKey),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        RequireExpirationTime = true,
                        ValidateLifetime = false,
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async ctx =>
                        {
                            if (!long.TryParse(ctx.Principal.FindFirstValue("Seq"), out var userSeq))
                            {
                                ctx.Fail("");
                                ctx.HttpContext.Response.Headers["ret"] = ResultCode.InvalidToken.ToString("D");
                                return;
                            }
                            
                            if (!short.TryParse(ctx.Principal.FindFirstValue("Age"), out var age))
                            {
                                ctx.Fail("");
                                ctx.HttpContext.Response.Headers["ret"] = ResultCode.InvalidToken.ToString("D");
                                return;
                            }
                            if (!byte.TryParse(ctx.Principal.FindFirstValue("OsType"), out var osType))
                            {
                                ctx.Fail("");
                                ctx.HttpContext.Response.Headers["ret"] = ResultCode.InvalidToken.ToString("D");
                                return;
                            }

                            var tokenService = ctx.HttpContext.RequestServices.GetRequiredService<ITokenService>();
                            var result = await tokenService.ValidateTokenAsync(new JwtPayload(userSeq, osType, age)).ConfigureAwait(false);
                            if (result != ResultCode.Success)
                            {
                                ctx.Fail("");
                                ctx.HttpContext.Response.Headers["ret"] = result.ToString("D");
                            }
                        }
                    };
                });
            services.AddScoped<ErrorLogFilterAttribute>();
            services
              .AddControllers(config =>
              {
                  config.Filters.Add<ErrorLogFilterAttribute>();
              })
              .AddMvcOptions(option => { })
              .AddJsonOptions(options =>
              {
                  options.JsonSerializerOptions.PropertyNamingPolicy = null;
                  options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                  options.JsonSerializerOptions.IncludeFields = true;
                  options.JsonSerializerOptions.Converters.Add(new BigIntegerConverter());
              });
            //services
            //    .AddApiVersioning(o => { o.ReportApiVersions = true; })
            //    .AddVersionedApiExplorer(o =>
            //    {
            //        o.GroupNameFormat = "'v'VVV";
            //        o.SubstituteApiVersionInUrl = true;
            //    });


            services.AddHealthChecks();

            services.AddSingleton<FrontSubscribeService>();
            services.AddSingleton<IAPService>();
            services.AddSingleton<FrontCheckService>();
            services.AddHostedService<FrontCheckService>();
            services.AddSingleton<AttendanceService>();

            var settingsSection = config.GetSection(nameof(RedisConfiguration));
            var redisHost = config.GetValue("RedisHost", settingsSection["Hosts:0:Host"]);
            var redisPort = config.GetValue("RedisPort", settingsSection["Hosts:0:Port"]);
            var redisPassword = config.GetValue("RedisPassword", settingsSection["Password"]);

            var connectionString = $"{redisHost}:{redisPort}";
            //            services.AddSignalR();
            //services.AddSignalR().AddStackExchangeRedis(connectionString, options =>
            //{
            //    options.Configuration.Password = redisPassword;
            //});
            services.AddSignalR(huboptions =>
            {
                huboptions.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
                huboptions.KeepAliveInterval = TimeSpan.FromMinutes(1);
            }).AddStackExchangeRedis(connectionString, options =>
            {
                options.Configuration.Password = redisPassword;
            });


            switch (appSettings.CdnStoreType)
            {
                case CdnStoreType.Local:
                    services.AddSingleton<ICdnStoreService, LocalCdnStoreService>();
                    break;
                case CdnStoreType.Aws:
                    services.AddSingleton<ICdnStoreService, AwsCdnStoreService>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
