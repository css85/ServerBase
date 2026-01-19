using Shared.Packet.Models;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Shared.ServerModel
{
    public class CountUpMission
    {
        public long MissionIndex { get; set; }
        public BigInteger Count { get; set; }

        public CountUpMission() { }

        public CountUpMission(long index, BigInteger count)
        {
            MissionIndex = index;   
            Count = count;  
        }

    }
}
