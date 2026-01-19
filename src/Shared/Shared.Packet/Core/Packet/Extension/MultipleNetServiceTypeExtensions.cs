using System;

namespace Shared.Packet.Extension
{
    public static class MultipleNetServiceTypeExtensions
    {
        private static readonly NetServiceTypeInfo[][] _netServiceTypes;

        static MultipleNetServiceTypeExtensions()
        {
            var multipleNetServiceTypeLength = Enum.GetValues(typeof(MultipleNetServiceType)).Length;
            _netServiceTypes = new NetServiceTypeInfo[multipleNetServiceTypeLength][];

            _netServiceTypes[(int) MultipleNetServiceType.None] = new[]
            {
                new NetServiceTypeInfo(ProtocolType.None, NetServiceType.None),
            };

            _netServiceTypes[(int) MultipleNetServiceType.Web] = new[]
            {
                new NetServiceTypeInfo(ProtocolType.Http, NetServiceType.Gate),
                new NetServiceTypeInfo(ProtocolType.Http, NetServiceType.Auth),
                new NetServiceTypeInfo(ProtocolType.Http, NetServiceType.Api),
            };

            _netServiceTypes[(int) MultipleNetServiceType.WebSockets] = new NetServiceTypeInfo[0];

            _netServiceTypes[(int)MultipleNetServiceType.Sockets] = new[]
            {
                new NetServiceTypeInfo(ProtocolType.Tcp, NetServiceType.FrontEnd),                
                new NetServiceTypeInfo(ProtocolType.Tcp, NetServiceType.Internal),
            };
        }

        public static NetServiceTypeInfo[] GetServices(this MultipleNetServiceType type)
        {
            return _netServiceTypes[(int) type];
        }

        public static NetServiceTypeInfo[] GetServices(int type)
        {
            return _netServiceTypes[type];
        }
    }
}