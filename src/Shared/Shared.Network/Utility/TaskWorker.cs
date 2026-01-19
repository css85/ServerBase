using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Shared.Network.Utility
{
    public class PacketWorkerItem
    {
        public int Id { get; }
        public object Packet { get; }
        public int Extra { get; }

        public PacketWorkerItem(int id, object packet, int extra = 0)
        {
            Id = id;
            Packet = packet;
            Extra = extra;
        }
    }

    public class TaskWorker<T> : IDisposable
    {
        private SyncEvents _syncEvents;
        private readonly Thread[] _workerThreads;
        private readonly ConcurrentQueue<T> _workerQueue;

        private readonly Func<T, Task> _workMethod;

        private bool _isDisposed;

        public TaskWorker(int threadCount, Func<T, Task> workMethod)
        {
            _workMethod = workMethod;
            _workerQueue = new ConcurrentQueue<T>();
            _syncEvents = new SyncEvents();
            _workerThreads = new Thread[threadCount];
            for (var i = 0; i < _workerThreads.Length; i++)
            {
                _workerThreads[i] = new Thread(ThreadWork);
                _workerThreads[i].Start();
            }
        }

        public void Enqueue(T item)
        {
            if (_isDisposed)
                return;

            _workerQueue.Enqueue(item);
            _syncEvents.NewItemEvent.Set();
        }

        private void ThreadWork()
        {
            while (WaitHandle.WaitAny(_syncEvents.EventArray) != 1)
            {
                while (_workerQueue.TryDequeue(out var item))
                {
                    try
                    {
                        _workMethod(item).ConfigureAwait(false);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _syncEvents.ExitThreadEvent.Set();
        }
    }

    public class SyncEvents
    {
        private readonly EventWaitHandle _newItemEvent;
        private readonly EventWaitHandle _exitThreadEvent;
        private readonly WaitHandle[] _eventArray;

        public SyncEvents()
        {
            _newItemEvent = new AutoResetEvent(false);
            _exitThreadEvent = new ManualResetEvent(false);
            _eventArray = new WaitHandle[]
            {
                _newItemEvent,
                _exitThreadEvent,
            };
        }

        public EventWaitHandle ExitThreadEvent => _exitThreadEvent;
        public EventWaitHandle NewItemEvent => _newItemEvent;
        public WaitHandle[] EventArray => _eventArray;
    }
}