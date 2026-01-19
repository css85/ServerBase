using Shared.Packet.Utility;

namespace Shared.Packet.Extensions
{
    public static class PacketHeaderDataExtensions
    {
        public static bool IsHttp(this PacketHeaderData headerData)
        {
            return headerData.HttpMethod != RequestMethodType.None &&
                   string.IsNullOrEmpty(headerData.RequestUri) == false;
        }
    }
}