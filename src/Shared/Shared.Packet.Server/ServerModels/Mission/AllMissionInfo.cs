using Shared.Packet.Models;
using System;
using System.Collections.Generic;

namespace Shared.ServerModel
{
    public class AllMissionInfo
    {
        public bool IsRedDot { get; set; }
        public MissionInfo HighAchievRateMission { get; set; }
        public List<MissionInfo> Missions { get; set; }
    }
}
