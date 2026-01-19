using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Config;
using AdminServer.Connection.Services;
using AdminServer.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.ServerApp.Config;
using Shared.ServerApp.Services;
using Shared.Session.Services;

namespace AdminServer
{
    public class AdminAppHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly AdminAppContextService _appContext;
        private readonly InitializeService _initializeService;
        private readonly SessionManagementService _sessionManagementService;
        private readonly ServerSessionListenerService<ServerSessionService> _serverSessionListenerService;        
        private readonly AdminScheduleService _adminScheduleService;
        private readonly PublishService _publishService;
        private readonly FCMService _fcmService;
        private readonly RankingScheduleService _rankingScheduleService;
        
        public AdminAppHostedService(
            ILogger<AdminAppHostedService> logger,
            IHostApplicationLifetime appLifetime,
            AdminAppContextService appContext,
            InitializeService initializeService,
            SessionManagementService sessionManagementService,
            AdminScheduleService adminScheduleService,
            PublishService publishService,
            FCMService fcmService,
            RankingScheduleService rankingScheduleService,
            ServerSessionListenerService<ServerSessionService> serverSessionListenerService
            )
        {
            _logger = logger;
            _appContext = appContext;
            _initializeService = initializeService;
            _sessionManagementService = sessionManagementService;            
            _serverSessionListenerService = serverSessionListenerService;
            _adminScheduleService = adminScheduleService;
            _publishService = publishService;
            _fcmService = fcmService;
            _rankingScheduleService = rankingScheduleService;
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
                // tcp 사용 시
//                _serverSessionListenerService.StartListen(_appContext.InternalHost, _appContext.InternalPort);

                await _initializeService.OnStartedAsync();
                await _publishService.OnStartAsync();
                await _adminScheduleService.OnStartAsync();             
                await _fcmService.OnStartAsync();   
                await _rankingScheduleService.OnStartAsync();   
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
