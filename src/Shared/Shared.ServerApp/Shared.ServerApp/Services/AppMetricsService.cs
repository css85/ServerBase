using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Config;
using SampleGame.Shared;
using Microsoft.Extensions.Logging;
using Shared.Models;
using Shared.ServerApp.Config;
using Shared.ServerApp.Connection;
using Shared.Session.Base;
using static Shared.Session.Extensions.ReplyExtensions;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;
using Shared.Packet;
using Shared.Server.Extensions;
using Shared.Session.PacketModel;
using Shared.Session.Settings;

namespace Shared.ServerApp.Services
{
    public class AppMetricsService : IHostedService
    {
        private readonly AppMetricInfo _metrics;
        private Timer _timer;

        private CancellationTokenSource _stoppingCts;
        private readonly AppContextServiceBase _appContextService;

        private readonly ChangeableSettings<AppMonitorSettings> _appMonitorSettings;

        private readonly AppServerSessionServiceBase _appServerSessionService;
        private Task _doWorkTask;
        private readonly ChangeableSettings<SessionSettings> _sessionSettings;
        private readonly AsyncLock _timerAsyncLock =new();
        private readonly ILogger<AppMetricsService> _logger;

        public AppMetricsService(
            ILogger<AppMetricsService> logger,
            AppContextServiceBase appContextService,
            ChangeableSettings<AppMonitorSettings> appMonitorSettings,
            ChangeableSettings<SessionSettings> sessionSettings,
            AppServerSessionServiceBase appServerSessionService
        )
        {
            _logger = logger;
            _appContextService = appContextService;
            _appMonitorSettings = appMonitorSettings;
            _appMonitorSettings.IsReloadOnChanged = true;
            _appMonitorSettings.AddListener(OnChangeAppSettings);

            _sessionSettings = sessionSettings;
            _sessionSettings.IsReloadOnChanged = true;
            _appServerSessionService = appServerSessionService;

            _metrics = new AppMetricInfo
            {
                AppId = _appContextService.AppId
            };
        }
        
        private void OnChangeAppSettings()
        {
            _timer?.Change(0, (int) _appMonitorSettings.Value.CollectMetricsInterval.TotalMilliseconds);
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_appContextService.IsUnitTest == false)
            {
                _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _timer = new Timer(DoWork, null, TimeSpan.Zero, _appMonitorSettings.Value.CollectMetricsInterval);
            }

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            if (_appServerSessionService.IsAllServerLoaded() == false)
                return;
            
            _doWorkTask = DoWorkAsync(_stoppingCts.Token);
        }

        public async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            using (await _timerAsyncLock.LockAsync(stoppingToken))
            {
                _metrics.ConnectionsPerSec = ((AppMetricsEventSource.Instance.TotalConnections
                                               - _metrics.CurrentConnections)
                                              / _appMonitorSettings.Value.CollectMetricsInterval.TotalSeconds);

                _metrics.ProcessingPacketsPerSec = ((AppMetricsEventSource.Instance.TotalProcessingPackets
                                                     - _metrics.CurrentProcessingPackets)
                                                    / _appMonitorSettings.Value.CollectMetricsInterval.TotalSeconds);

                _metrics.CurrentConnections = AppMetricsEventSource.Instance.CurrentConnections;
                _metrics.CurrentProcessingPackets = AppMetricsEventSource.Instance.CurrentProcessingPackets;

                if (_sessionSettings.Value.MaxConnections.HasValue && _appServerSessionService.AvailableServiceTypes.Any(x=>x==NetServiceType.FrontEnd))
                {
                    var connectionRate = (double)_metrics.CurrentConnections / _sessionSettings.Value.MaxConnections.GetValueOrDefault(1)*100.00f;
                    //var beforeState = OpenState;
                   
                    //if(beforeState != OpenState)
                    {
                        await _appServerSessionService.SendAsync(
                            NetServiceType.Gate,
                            MakeNtfReply(new InternalOpenStateChangedNtf
                            {
                                AppId = _appContextService.AppId,
                                AppGroupId = _appContextService.AppGroupId,
                                OpenState = 1
                            }));
                    }
                }
            }
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_timer != null)
            {
                _timer.Change(Timeout.Infinite, 0);
                await _timer.DisposeAsync();
                _timer = null;
            }
        }
        
    }
}