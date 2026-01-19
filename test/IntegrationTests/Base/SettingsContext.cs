using System;
using System.Linq;
using Common.Config;
using SampleGame.Shared.Common;

namespace Integration.Tests.Base
{
    public enum SettingsTypes
    {
        Frontend = 0,
        Gate = 1,
    }

    public class SettingsContext<T> : IDisposable
    {
        private readonly ChangeableSettings<T>[] _settingsArray;
        private readonly string[] _savedSettingsJsonArray;

        public T this[int i] => _settingsArray[i].Value;
        public T this[SettingsTypes i] => _settingsArray[(int)i].Value;

        public SettingsContext(ChangeableSettings<T> settings)
        {
            _settingsArray = new []{ settings };
            _savedSettingsJsonArray = _settingsArray.Select(p => JsonTextSerializer.Serialize(p.Value)).ToArray();
        }

        public SettingsContext(ChangeableSettings<T>[] settingsArray)
        {
            _settingsArray = settingsArray;
            _savedSettingsJsonArray = _settingsArray.Select(p => JsonTextSerializer.Serialize(p.Value)).ToArray();
        }

        public void Modify(Action<T> modifyAction)
        {
            foreach (var settings in _settingsArray)
            {
                modifyAction.Invoke(settings.Value);
                settings.Modify(settings.Value);
            }
        }

        public void Dispose()
        {
            for (var i = 0; i < _settingsArray.Length; i++)
            {
                var settings = _settingsArray[i];
                var saveSettingsJson = _savedSettingsJsonArray[i];

                var savedGameRuleSettings = JsonTextSerializer.Deserialize<T>(saveSettingsJson);
                settings.Modify(savedGameRuleSettings);
            }
        }
    }
}
