using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shared.Session.Base;
using Shared.Session.Data;
using Shared.Session.Features;
using Shared.Session.Services;
using Shared.Session.Settings;
using Shared.Session.Utility;

namespace Shared.Session
{
    public static class SessionDependencyInjectionExtensions
    {
        public static IServiceCollection AddSessionBasic(this IServiceCollection services)
        {
            services.AddSingleton<SessionManagementService>();

            return services;
        }

        public static IServiceCollection AddSession<TSession, TSessionService>(this IServiceCollection services, Type assemblyType)
            where TSession : UserSessionBase
            where TSessionService : UserSessionServiceBase
        {
            PacketHandlerInfoTableMap.Build(typeof(TSession),
                new[]
                {
                    new PacketHandlerTarget
                    {
                        AssemblyType = assemblyType,
                        BaseType = typeof(UserSessionBase),
                    },
                    new PacketHandlerTarget
                    {
                        AssemblyType = assemblyType,
                        BaseType = typeof(TSession),
                    },
                });

            services.AddSingleton<TSessionService>();
            services.AddSingleton<SessionServiceBase, TSessionService>(p => p.GetRequiredService<TSessionService>());
            services.AddSingleton<UserSessionServiceBase, TSessionService>(p =>
                p.GetRequiredService<TSessionService>());

            services.AddSingleton<UserSessionListenerService<TSessionService>>();

            return services;
        }
    }
}
