using Shared.Packet.Models;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Shared.ServerModel
{
    public class OverwriteMission
    {
        public long MissionIndex { get; set; }
        public BigInteger Count { get; set; }

        public OverwriteMission() { }

        public OverwriteMission(long index, BigInteger count)
        {
            MissionIndex = index;   
            Count = count;  
        }

    }
}
