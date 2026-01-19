using System;

namespace Integration.Tests.Base
{
    public class DisposableClass<T> : IDisposable
    {
        public T Data;
        public Action<T> DisposeAction;

        public void Dispose()
        {
            DisposeAction?.Invoke(Data);
            DisposeAction = null;
        }
    }
}