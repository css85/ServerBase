using System;

namespace Shared.Packet.Models
{
    [Serializable]
    public class PartsBase
    {
        public long Index { get; set; }
        public string ColorCode { get; set; } = "#FFFFFF";

        public PartsBase() { }
        public PartsBase(long index, string colorCode = "#FFFFFF") 
        { 
            Index = index;
            ColorCode = colorCode;
        }
    }
}
