using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.ServerApp.Services;
using Shared.Session.Services;
using WebTool.Connection.Services;
using WebTool.Services;

namespace WebTool
{
    public class WebToolAppHostedService : IHostedService
    {
        private readonly ILogger<WebToolAppHostedService> _logger;
        private readonly WebToolAppContextService _appContext;
        private readonly InitializeService _initializeService;
        private readonly SessionManagementService _sessionManagementService;
        private readonly ServerSessionListenerService<ServerSessionService> _serverSessionListenerService;

        public WebToolAppHostedService(
            ILogger<WebToolAppHostedService> logger,
            IHostApplicationLifetime applicationLifetime,
            WebToolAppContextService appContext,
            InitializeService initializeService,
            SessionManagementService sessionManagementService,
            ServerSessionListenerService<ServerSessionService> serverSessionListenerService
        )
        {
            _logger = logger;
            _appContext = appContext;
            _initializeService = initializeService;
            _sessionManagementService = sessionManagementService;
            _serverSessionListenerService = serverSessionListenerService;

            applicationLifetime.ApplicationStarted.Register(() => OnStartedAsync()?.Wait());
            applicationLifetime.ApplicationStopping.Register(() => OnStoppingAsync()?.Wait());
            applicationLifetime.ApplicationStopped.Register(() => OnStoppedAsync()?.Wait());
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
                await _sessionManagementService.StartAsync();

                _serverSessionListenerService.StartListen(_appContext.InternalHost, _appContext.InternalPort);

                await _initializeService.OnStartedAsync();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "OnStartedAsync failed");
                throw;
            }
        }

        private async Task OnStoppingAsync()
        {
            await _initializeService.StoppingAsync();
        }

        private async Task OnStoppedAsync()
        {
            await _initializeService.StoppedAsync();
            await _serverSessionListenerService.OnStopAsync();
        }
    }
}
