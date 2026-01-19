using System;
using Shared.Model;
using Shared.Packet;

namespace Shared.Session.PacketModel
{
    [Serializable]
    [NtfClass((byte)MAJOR.Internal, (byte)INTERNAL_MINOR.NTFScheduleWorkCompleted, ProtocolType.Tcp, NetServiceType.Internal)]
    public class InternalScheduleWorkCompletedNtf : NtfBase
    {
        public short ScheduleType; // enum ScheduleType
        public int AppId;
        public long WorkCompleteTime;
        public string JsonData;
    }
}