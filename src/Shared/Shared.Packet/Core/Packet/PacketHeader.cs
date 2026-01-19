using System;
using Shared.Packet.Extension;

namespace Shared.Packet
{
    [Serializable]
    public struct PacketHeader
    {
        public static readonly PacketHeader None = new PacketHeader(0, 0);
        
        public PacketHeader(byte major, byte minor)
        {
            Major = major;
            Minor = minor;
        }

        public byte Major { get; }
        public byte Minor { get; }

        public override bool Equals(object obj)
        {
            if (obj is PacketHeader other)
            {
                return (Major == other.Major) && (Minor == other.Minor);
            }
            return false;

        }
        public override int GetHashCode()
        {
            return (Major * 1000) + Minor;

        }

        public override string ToString()
        {
            return this.GetCachedString();
        }
    }
}
