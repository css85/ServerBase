#nullable enable
using System;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Threading;
using Shared;
using Shared.Packet;
using Shared.Packet.Extension;

namespace SampleGame.Shared
{
    [EventSource(Name = "App-Metrics")]
    public sealed class AppMetricsEventSource:EventSource
    {
        public static class EventIds
        {
            public const int START_CONNECTIONS = 1;
            public const int STOP_CONNECTIONS = 2;
            public const int START_PROCESSING_PACKET = 3;
            public const int STOP_PROCESSING_PACKET = 4;
            public const int SLOW_PROCESSING_PACKET = 5;
        }
        public static AppMetricsEventSource Instance = new ();

        private IncrementingPollingCounter? _connectionsPerSecondCounter;
        private IncrementingPollingCounter? _processingPacketPerSecondCounter;
        private IncrementingPollingCounter? _slowProcessingPacketPerSecondCounter;
        
        private PollingCounter? _totalConnectionCounter;
        private PollingCounter? _currentConnectionCounter;
        private PollingCounter? _totalProcessingPacketCounter;
        private PollingCounter? _currentProcessingPacketCounter;
        private PollingCounter? _totalSlowProcessingPacketCounter;
        
        private long _totalConnections;
        private long _currentConnections;
        private long _currentProcessingPackets;
        private long _totalProcessingPackets;
        private long _totalSlowProcessingPackets;

        private AppMetricsEventSource() :base(EventSourceSettings.EtwSelfDescribingEventFormat)
        {
        }

        public long TotalConnections => Volatile.Read(ref _totalConnections);
        public long TotalProcessingPackets => Volatile.Read(ref _totalProcessingPackets);
        public long CurrentConnections => Volatile.Read(ref _currentConnections);
        public long CurrentProcessingPackets => Volatile.Read(ref _currentProcessingPackets);

        [NonEvent]
        public void ConnectionStart(NetServiceType type,int connectionId)
        {
            Interlocked.Increment(ref _totalConnections);
            Interlocked.Increment(ref _currentConnections);
            if (IsEnabled(EventLevel.Informational, EventKeywords.None))
            {
                ConnectionStartEvent(type,connectionId);
            }
            
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        [Event(EventIds.START_CONNECTIONS, Level = EventLevel.Informational)]
        private void ConnectionStartEvent(NetServiceType netServiceType, int connectionId)
        {
            WriteEvent(EventIds.START_CONNECTIONS, netServiceType, connectionId);
        }
        
        [NonEvent]
        public void ConnectionStop(NetServiceType netServiceType,int connectionId)
        {
            Interlocked.Decrement(ref _currentConnections);

            if (IsEnabled(EventLevel.Informational, EventKeywords.None))
            {
                ConnectionStopEvent(netServiceType,connectionId);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Event(EventIds.STOP_CONNECTIONS, Level = EventLevel.Informational)]
        private void ConnectionStopEvent(NetServiceType netServiceType, int connectionId)
        {
            WriteEvent(EventIds.STOP_CONNECTIONS, netServiceType, connectionId);
        }


        [NonEvent]
        public void ProcessPacketStart(int sessionId, IPacketItem packetItem)
        {
            Interlocked.Increment(ref _currentProcessingPackets);
            Interlocked.Increment(ref _totalProcessingPackets);
            if (IsEnabled(EventLevel.Informational, EventKeywords.None))
            {
                ProcessingPacketStart(sessionId, packetItem.Header().Major, packetItem.Header().Minor,
                    packetItem.GetRequestId());
            }
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        [Event(EventIds.START_PROCESSING_PACKET, Level = EventLevel.Informational)]
        private void ProcessingPacketStart(int sessionId, byte major, byte minor, ushort requestId)
        {
            WriteEvent(EventIds.START_PROCESSING_PACKET, sessionId, major, minor, requestId);
        }


        [NonEvent]
        public void ProcessPacketStop(int sessionId, IPacketItem packetItem)
        {
            Interlocked.Decrement(ref _currentProcessingPackets);
            if (IsEnabled(EventLevel.Informational, EventKeywords.None))
            {
                ProcessPacketStopEvent(sessionId, packetItem.Header().Major, packetItem.Header().Minor,
                    packetItem.GetRequestId());
            }
        }
      
        [MethodImpl(MethodImplOptions.NoInlining)]
        [Event(EventIds.STOP_PROCESSING_PACKET, Level = EventLevel.Informational)]
        private void ProcessPacketStopEvent(int sessionId, byte major, byte minor, ushort requestId)
        {
            WriteEvent(EventIds.STOP_PROCESSING_PACKET, sessionId, major, minor, requestId);
        }

        [NonEvent]
        public void SlowProcessing(IPacketItem packetItem)
        {
            Interlocked.Increment(ref _totalSlowProcessingPackets);
            if(IsEnabled(EventLevel.Informational,EventKeywords.None))
            {
                SlowProcessingEvent(packetItem.Header().Major, packetItem.Header().Minor, packetItem.GetResult(),
                    packetItem.GetRequestId());
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Event(EventIds.SLOW_PROCESSING_PACKET, Level = EventLevel.Informational)]
        private void SlowProcessingEvent(byte major, byte minor,int result, int requestId)
        {
            WriteEvent(EventIds.SLOW_PROCESSING_PACKET, major,minor,result,requestId);
        }

        protected override void OnEventCommand(EventCommandEventArgs command)
        {
            if(command.Command == EventCommand.Enable)
            {
                _connectionsPerSecondCounter ??= new IncrementingPollingCounter("connections-per-second", this,
                    () => Volatile.Read(ref _totalConnections))
                {
                    DisplayName = "Connection Rate",
                    DisplayRateTimeScale = TimeSpan.FromSeconds(1)
                };
                _totalConnectionCounter ??= new PollingCounter("total-connections", this,
                    () => Volatile.Read(ref _totalConnections))
                {
                    DisplayName = "Total Connections"
                };
                _currentConnectionCounter ??= new PollingCounter("current-connections", this,
                    () => Volatile.Read(ref _currentConnections))
                {
                    DisplayName = "Current Connections"
                };
                
                _currentProcessingPacketCounter ??= new PollingCounter("current-packet-counters", this,
                    () => Volatile.Read(ref _currentProcessingPackets))
                {
                    DisplayName = "Current Packets"
                };

                _totalProcessingPacketCounter ??= new PollingCounter("total-processing-packets", this,
                    () => Volatile.Read(ref _totalProcessingPackets))
                {
                    DisplayName = "Total Processing Packets"
                };

                _processingPacketPerSecondCounter ??= new IncrementingPollingCounter("packets-per-second", this,
                    () => Volatile.Read(ref _totalProcessingPackets))
                {
                    DisplayName = "Processing Rate",
                    DisplayRateTimeScale = TimeSpan.FromSeconds(1)
                };

                _totalSlowProcessingPacketCounter ??= new PollingCounter("total-slow-processing-packets", this,
                    () => Volatile.Read(ref _totalSlowProcessingPackets))
                {
                    DisplayName = "Slow Processing Packets",
                };
                
                _slowProcessingPacketPerSecondCounter ??= new IncrementingPollingCounter("slow-packets-per-second", this,
                    () => Volatile.Read(ref _totalSlowProcessingPackets))
                {
                    DisplayName = "Slow Processing Rate",
                    DisplayRateTimeScale = TimeSpan.FromSeconds(1)
                };
            }
            base.OnEventCommand(command);
        }
    }
}