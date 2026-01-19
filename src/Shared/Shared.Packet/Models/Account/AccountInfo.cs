using System;

namespace Shared.Packet.Model
{
    [Serializable]
    public class PlayerInfoForConflict
    {
        public string nick { get; set; }
        public int level { get; set; }
        public long regDt { get; set; }
    }
}
