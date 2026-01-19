using System;
using Shared.Packet;
using Shared.Packet.Utility;

namespace Bluegames.Swagger
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SwaggerPacketHeaderAttribute : Attribute
    {
        public string Description { get; private set; }
        public PacketHeader Header { get; private set; }
        public NetServiceTypeInfo ServiceTypeInfo {get; private set;}

        public SwaggerPacketHeaderAttribute(Type type, string description = null)
        {
            var headerData = PacketHeaderTable.GetHeaderData(type);
            Header = headerData.Header;
            ServiceTypeInfo = headerData.ServiceType;
            Description = description;
        }

        public SwaggerPacketHeaderAttribute(byte major, byte minor, string description = null)
        {
            var headerType= PacketHeaderTable.GetReqType(major,minor);
            if(headerType==null)
                headerType = PacketHeaderTable.GetResType(major, minor);

            var headerData = PacketHeaderTable.GetHeaderData(headerType);
            Header = headerData.Header;
            ServiceTypeInfo = headerData.ServiceType;
            Description = description;
        }
    }

}
