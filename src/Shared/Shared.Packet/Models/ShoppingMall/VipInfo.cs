using System;
using System.Collections.Generic;

namespace Shared.Packet.Models
{
    [Serializable]
    public class VipInfo : VipBase
    {
        public AttendanceBase VipAttendanceInfo { get; set; } = new AttendanceBase();        
    }
}
