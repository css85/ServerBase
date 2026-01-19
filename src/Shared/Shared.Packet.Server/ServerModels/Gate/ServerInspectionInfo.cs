using Shared.Clock;
using System;

namespace Shared.ServerModel
{
    public class ServerInspectionInfo
    {
        public bool IsInspection { get; set; } = false;
        public DateTime? FromDt { get; set; }
        public DateTime? ToDt { get; set; }
        public string AllowIp { get; set; }


        public bool IsInspectionTime()
        {
            if (IsInspection == false || FromDt == null || ToDt == null )
                return false;

            return AppClock.UtcNow >= FromDt && AppClock.UtcNow <= ToDt;
        }

    }
}