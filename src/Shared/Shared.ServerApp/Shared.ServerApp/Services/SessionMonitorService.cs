using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Clock;

using Shared.Session.Base;
using Shared.Session.Settings;

namespace Shared.ServerApp.Services
{
    public class SessionMonitorService<TSessionService, TSessionSettings> : IHostedService
        where TSessionService : SessionServiceBase
        where TSessionSettings : SessionSettingsBase
    {
        private readonly ILogger<SessionMonitorService<TSessionService, TSessionSettings>> _logger;
        private readonly TSessionService _sessionService;
        private readonly ChangeableSettings<TSessionSettings> _sessionSettings;

        private Timer _timer;
        private Task _doWorkTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        private DateTime _lastPingCheckTime = DateTime.MinValue;
        private DateTime _lastSessionLogTime = DateTime.MinValue;

        public SessionMonitorService(
            IServiceProvider serviceProvider,
            ILogger<SessionMonitorService<TSessionService, TSessionSettings>> logger,
            IHostApplicationLifetime applicationLifetime,
            ChangeableSettings<TSessionSettings> sessionSettings
            )
        {
            _sessionService = serviceProvider.GetService<TSessionService>();

            _logger = logger;
            _sessionSettings = sessionSettings;

            _sessionSettings.IsReloadOnChanged = true;

            applicationLifetime.ApplicationStarted.Register(() => OnStartedAsync()?.Wait());
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task OnStartedAsync()
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            return  Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _doWorkTask = DoWorkAsync(_stoppingCts.Token);
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            // _logger.LogTrace("Triggered SessionMonitorService.");

            if (_lastPingCheckTime + _sessionSettings.Value.PingCheckDelay <= AppClock.UtcNow)
            {
                await _sessionService.CheckTimeoutAsync();
                _lastPingCheckTime = AppClock.UtcNow;
            }

            if (_lastSessionLogTime + _sessionSettings.Value.SessionLogDelay <= AppClock.UtcNow)
            {
                LogSession();
                _lastSessionLogTime = AppClock.UtcNow;
            }
        }

        private void LogSession()
        {
            if (_sessionService.IsUserSession)
            {
                
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

            if (_doWorkTask != null)
            {
                _stoppingCts.Cancel();
            }
        }
    }
}
