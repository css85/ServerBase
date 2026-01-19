using System;
using System.Collections.Generic;

namespace Shared.Packet.Extension
{
    public static class PacketHeaderExtension
    {
        private static readonly Dictionary<int, string> PacketHeaderStringMap;

        static PacketHeaderExtension()
        {
            PacketHeaderStringMap = new Dictionary<int, string>();

            var majorType = typeof(MAJOR);
            var memberInfos = majorType.GetMembers();
            foreach (var memberInfo in memberInfos)
            {
                var majorAttributes = memberInfo.GetCustomAttributes(typeof(PacketMajorAttribute), false);
                if (majorAttributes.Length > 0)
                {
                    var majorAttribute = (PacketMajorAttribute) majorAttributes[0];
                    if (majorAttribute.MinorType != null)
                    {
                        var minorAttributes =
                            majorAttribute.MinorType.GetCustomAttributes(typeof(PacketMinorAttribute), false);
                        if (minorAttributes.Length > 0)
                        {
                            var minorAttribute = (PacketMinorAttribute) minorAttributes[0];

                            var major = minorAttribute.Major;
                            var minorValues = Enum.GetValues(majorAttribute.MinorType);
                            foreach (byte minor in minorValues)
                            {
                                if (minor == 0)
                                    continue;

                                PacketHeaderStringMap.Add(new PacketHeader((byte) major, minor).GetHashCode(),
                                    $"{major}.{majorAttribute.MinorType.GetEnumName(minor)}");
                            }
                        }
                    }
                }
            }
        }

        public static string GetCachedString(this PacketHeader packetHeader)
        {
            var hashCode = packetHeader.GetHashCode();
            if (hashCode == 0)
                return "None.None";

            if (PacketHeaderStringMap.TryGetValue(hashCode, out var packetHeaderString))
                return packetHeaderString;

            return $"{packetHeader.Major}.{packetHeader.Minor}";
        }
    }
}