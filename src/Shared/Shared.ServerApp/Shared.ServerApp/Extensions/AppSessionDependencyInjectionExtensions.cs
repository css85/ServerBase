using System;
using Microsoft.Extensions.DependencyInjection;
using Shared.ServerApp.Connection;
using Shared.ServerApp.Services;
using Shared.Session;
using Shared.Session.Base;
using Shared.Session.Features;
using Shared.Session.Services;
using Shared.Session.Settings;
using Shared.Session.Utility;

namespace Shared.ServerApp.Extensions
{
    public static class AppSessionDependencyInjectionExtensions
    {
        public static IServiceCollection AddUserSession<TSession, TSessionService>(this IServiceCollection services, Type assemblyType)
            where TSession : UserSessionBase
            where TSessionService : UserSessionServiceBase
        {
            services.AddSession<TSession, TSessionService>(assemblyType);
            services.AddHostedService<SessionMonitorService<TSessionService, SessionSettings>>();

            return services;
        }

        public static IServiceCollection AddServerSession<TSession, TSessionService>(this IServiceCollection services, Type assemblyType)
            where TSession : AppServerSessionBase
            where TSessionService : AppServerSessionServiceBase
        {
            PacketHandlerInfoTableMap.Build(typeof(TSession),
                new[]
                {
                    new PacketHandlerTarget
                    {
                        AssemblyType = assemblyType,
                        BaseType = typeof(SessionBase),
                    },
                    new PacketHandlerTarget
                    {
                        AssemblyType = assemblyType,
                        BaseType = typeof(ServerSessionBase),
                    },
                    new PacketHandlerTarget
                    {
                        AssemblyType = assemblyType,
                        BaseType = typeof(AppServerSessionBase),
                    },
                    new PacketHandlerTarget
                    {
                        AssemblyType = assemblyType,
                        BaseType = typeof(TSession),
                    },
                });

            services.AddSingleton<TSessionService>();
            services.AddSingleton<SessionServiceBase, TSessionService>(p => p.GetRequiredService<TSessionService>());
            services.AddSingleton<ServerSessionServiceBase, TSessionService>(p => p.GetRequiredService<TSessionService>());
            services.AddSingleton<AppServerSessionServiceBase, TSessionService>(p => p.GetRequiredService<TSessionService>());

            services.AddHostedService<SessionMonitorService<TSessionService, ServerSessionSettings>>();

            services.AddSingleton<ServerSessionListenerService<TSessionService>>();

            return services;
        }
    }
}
