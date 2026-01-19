using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Common.Config
{
    public class ChangeableSettings<T>
    {
        public T Value { get; private set; }

        public bool IsReloadOnChanged;

        private readonly IOptionsMonitor<T> _settings;

        private readonly ConcurrentDictionary<object, List<Action>> _listenerMap = new();

        public ChangeableSettings(IOptionsMonitor<T> settings)
        {
            _settings = settings;
            Value = settings.CurrentValue;

            settings.OnChange(OnChanged);
        }

        private void OnChanged(T settings)
        {
            if (IsReloadOnChanged)
            {
                Value = settings;

                foreach (var (owner, actions) in _listenerMap)
                {
                    foreach (var action in actions)
                    {
                        action.Invoke();
                    }
                }
            }
        }

        public void Modify(T settings)
        {
            Value = settings;
            OnChanged(Value);
        }

        public void AddListener([NotNull] Action listener)
        {
            AddListener(listener.Target, listener);
        }

        public void AddListener([NotNull] object owner, [NotNull] Action listener)
        {
            Debug.Assert(owner != null);

            if (_listenerMap.TryGetValue(owner, out var actions) == false)
            {
                actions = new List<Action>();
                if (_listenerMap.TryAdd(listener.Target, actions) == false)
                {
                    AddListener(owner, listener);
                    return;
                }
            }

            actions.Add(listener);
        }

        public void RemoveAllListeners([NotNull] object owner)
        {
            _listenerMap.TryRemove(owner, out _);
        }

        public void ClearAllListeners()
        {
            _listenerMap.Clear();
        }
    }
}
