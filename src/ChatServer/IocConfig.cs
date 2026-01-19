using System.Security.Claims;
using System.Text;
using SampleGame.Shared.Utility;
using ChatServer.Config;
using ChatServer.Connection.Services;
using ChatServer.Connection.Session;
using ChatServer.Services;
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

using System;
using SampleGame.Shared.Common;

namespace ChatServer
{
    public static class IocConfig
    {
   
        public static void AddAllServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
        {
            services.AddServerApp(config, env);

            services.AddSessionBasic();
            services.AddServerSession<ServerSession, ServerSessionService>(typeof(Program));

            services.AddSingleton<ChatAppContextService>();
            services.AddSingleton<AppContextServiceBase, ChatAppContextService>(p => p.GetRequiredService<ChatAppContextService>());


            services.AddChangeAbleSettings<ChatAppSettings>(config, nameof(ChatAppSettings));
            var tokenSettings = services.AddChangeAbleSettings<TokenSettings>(config, nameof(TokenSettings));
            var jwtSecretKey = Encoding.ASCII.GetBytes(tokenSettings.Secret);

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

                            var tokenService = ctx.HttpContext.RequestServices.GetRequiredService<ITokenService>();
                            var result = await tokenService.ValidateTokenAsync(new JwtPayload(userSeq, 0, age)).ConfigureAwait(false);
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
              .AddMvcOptions(_ => { })
              .AddJsonOptions(options =>
              {
                  options.JsonSerializerOptions.PropertyNamingPolicy = null;
                  options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                  options.JsonSerializerOptions.IncludeFields = true;
                  options.JsonSerializerOptions.Converters.Add(new BigIntegerConverter());
              });

            services.AddHealthChecks(); 
            services.AddSingleton<ChatCheckService>();
            services.AddHostedService<ChatCheckService>();
            services.AddSingleton<ChatSubscribeService>();


            var settingsSection = config.GetSection(nameof(RedisConfiguration));
            var redisHost = config.GetValue("RedisHost", settingsSection["Hosts:0:Host"]);
            var redisPort = config.GetValue("RedisPort", settingsSection["Hosts:0:Port"]);
            var redisPassword = config.GetValue("RedisPassword", settingsSection["Password"]);

            var connectionString = $"{redisHost}:{redisPort}";

            //services.AddSignalR().AddStackExchangeRedis(connectionString, options =>
            //{
            //    options.Configuration.Password = redisPassword;
            //});
            // 클라이언트 광고 보기 시 끊기는 문제로 타임아웃 조절 ( 클라와 같이 맞춰야 함 ) 
            services.AddSignalR(huboptions =>
            {
                huboptions.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
                huboptions.KeepAliveInterval = TimeSpan.FromMinutes(1);
            }).AddStackExchangeRedis(connectionString, options =>
            {
                options.Configuration.Password = redisPassword;
            });

        }
    }
}
