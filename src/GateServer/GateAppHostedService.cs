using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Config;
using GateServer.Connection.Services;
using GateServer.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.ServerApp.Config;
using Shared.ServerApp.Services;
using Shared.Session.Services;

namespace GateServer
{
    public class GateAppHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly GateAppContextService _appContext;
        private readonly InitializeService _initializeService;
        private readonly SessionManagementService _sessionManagementService;        
        private readonly ServerSessionListenerService<ServerSessionService> _serverSessionListenerService;
        private readonly GateCheckService _gateCheckService;
        public GateAppHostedService(
            ILogger<GateAppHostedService> logger,
            IHostApplicationLifetime appLifetime,
            GateAppContextService appContext,
            InitializeService initializeService,
            SessionManagementService sessionManagementService,            
            ServerSessionListenerService<ServerSessionService> serverSessionListenerService,
            GateCheckService gateCheckService
            )
        {
            _logger = logger;
            _appContext = appContext;
            _initializeService = initializeService;
            _sessionManagementService = sessionManagementService;
            _serverSessionListenerService = serverSessionListenerService;
            _gateCheckService = gateCheckService;
            appLifetime.ApplicationStarted.Register(() => OnStartedAsync()?.Wait());
            appLifetime.ApplicationStopping.Register(() => OnStoppingAsync()?.Wait());
            appLifetime.ApplicationStopped.Register(() => OnStoppedAsync()?.Wait());
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task OnStartedAsync()
        {
            try
            {
                await _appContext.StartAsync();
                await _initializeService.StartAsync();

//                _serverSessionListenerService.StartListen(_appContext.InternalHost, _appContext.InternalPort);

                await _initializeService.OnStartedAsync();
                await _gateCheckService.OnStartAsync();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "OnStartedAsync failed");
                throw;
            }
        }

        private async Task OnStoppingAsync()
        {
            await _sessionManagementService.StopAsync();
            await _initializeService.StoppingAsync();
        }

        private async Task OnStoppedAsync()
        {
            await _initializeService.StoppedAsync();
            await _serverSessionListenerService.OnStopAsync();
        }
    }
}
