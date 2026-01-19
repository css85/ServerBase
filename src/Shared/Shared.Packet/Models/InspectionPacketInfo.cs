using System;

namespace Shared.Packet.Models
{
    [Serializable]
    public class InspectionPacketInfo
    {
        public long FrDtTick { get; set; }
        public long ToDtTick { get; set; }

        public InspectionPacketInfo()
        {

        }

        public InspectionPacketInfo(long frTick, long toTick)
        {
            FrDtTick = frTick;
            ToDtTick = toTick;
        }
    }
}
