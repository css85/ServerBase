using System;
using Common.Config;
using Shared.Clock;
using Shared.ServerApp.Config;

namespace Shared.ServerApp.Services
{
    public class AppClockService : IDisposable
    {
        private readonly ChangeableSettings<ClockSettings> _clockSettings;
        public AppClockService(
            ChangeableSettings<ClockSettings> clockSettings)
        {
            _clockSettings = clockSettings;
            _clockSettings.AddListener(this, OnClockSettingsChanged);
            _clockSettings.IsReloadOnChanged = true;
        }

        public void Dispose()
        {
            _clockSettings.RemoveAllListeners(this);
        }

        private void OnClockSettingsChanged()
        {
            AppClock.SetOffset(_clockSettings.Value.Offset);
            AppClock.SetRenewalOffset(_clockSettings.Value.RenewalOffset);
            AppClock.SetAttendNotifyLocalHour(_clockSettings.Value.AttendNotifyLocalHour);
        }
        public void InitOffset()
        {
            AppClock.SetOffset(_clockSettings.Value.Offset);
            AppClock.SetRenewalOffset(_clockSettings.Value.RenewalOffset);
            AppClock.SetAttendNotifyLocalHour(_clockSettings.Value.AttendNotifyLocalHour);
        }
    }
}
