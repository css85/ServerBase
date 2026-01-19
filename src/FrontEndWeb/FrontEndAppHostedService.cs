using System;
using System.Threading;
using System.Threading.Tasks;
using FrontEndWeb.Connection.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.ServerApp.Services;
using Shared.Session.Services;

namespace FrontEndWeb.Services
{
    public class FrontEndAppHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly FrontEndAppContextService _appContext;
        private readonly InitializeService _initializeService;        
        private readonly SessionManagementService _sessionManagementService;

        private readonly ServerSessionListenerService<ServerSessionService> _serverSessionListenerService;
        private readonly UserSessionListenerService<UserFrontendSessionService> _userFrontendSessionListenerService;
        private readonly FrontSubscribeService _frontSubscribeService;
        private readonly ServerInspectionService _serverInspectionService;
        private readonly FrontCheckService _frontCheckService;

        public FrontEndAppHostedService(
            ILogger<FrontEndAppHostedService> logger,
            IHostApplicationLifetime appLifetime,
            FrontEndAppContextService appContext,
            InitializeService initializeService,            
            SessionManagementService sessionManagementService,            
            ServerSessionListenerService<ServerSessionService> serverSessionListenerService,
            UserSessionListenerService<UserFrontendSessionService> userFrontendSessionListenerService,
            FrontSubscribeService frontSubscribeService,
            FrontCheckService frontCheckService,    
            ServerInspectionService serverInspectionService
        )
        {
            _logger = logger;
            _appContext = appContext;
            _initializeService = initializeService;            
            _sessionManagementService = sessionManagementService;
            _serverSessionListenerService = serverSessionListenerService;
            _userFrontendSessionListenerService = userFrontendSessionListenerService;
            _frontSubscribeService = frontSubscribeService;
            _serverInspectionService = serverInspectionService; 
            _frontCheckService = frontCheckService;

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
                await _frontSubscribeService.OnStartAsync();
                await _serverInspectionService.InitAsync();
                await _frontCheckService.OnStartAsync();
                _userFrontendSessionListenerService.StartListen(_appContext.ListenHost, _appContext.ExternalFrontendPort);
//                _userMailSessionListenerService.StartListen(_appContext.ListenHost, _appContext.ExternalMailPort);
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
            await _userFrontendSessionListenerService.OnStopAsync();
            await _serverSessionListenerService.OnStopAsync();
        }
    }
}
