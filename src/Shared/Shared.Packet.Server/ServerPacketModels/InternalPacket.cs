using System;
using System.Collections.Generic;
using Shared.Entities;
using Shared.Entities.Models;
using Shared.Model;
using Shared.Packet;

namespace Shared.Server.Packet.Internal
{
    [Serializable]
    [NtfClass((byte)MAJOR.Internal, (byte)INTERNAL_MINOR.NTFConnectRequest, ProtocolType.Tcp, NetServiceType.Internal)]
    public class InternalConnectRequestNtf : NtfBase
    {
        public int AppId;
        public string AppName;
        public NetServiceType[] AvailableServiceTypes;
    }

    [Serializable]
    [NtfClass((byte)MAJOR.Internal, (byte)INTERNAL_MINOR.NTFConnectAccept, ProtocolType.Tcp, NetServiceType.Internal)]
    public class InternalConnectAcceptNtf : NtfBase
    {
    }

    /// <summary>
    /// Ntf 패킷만 가능
    /// </summary>
    [Serializable]
    [NtfClass((byte)MAJOR.Internal, (byte)INTERNAL_MINOR.NTFForward, ProtocolType.Tcp, NetServiceType.Internal)]
    public class InternalForwardNtf : NtfBase
    {
        public long UserSeq;
        public NetServiceType ServiceType;
        public byte Major;
        public byte Minor;
        public string Body;
    }

    [Serializable]
    [NtfClass((byte)MAJOR.Internal, (byte)INTERNAL_MINOR.NTFPing, ProtocolType.Tcp, NetServiceType.Internal)]
    public class InternalPingNtf : NtfBase
    {
    }

    [Serializable]
    [MultipleNtfClass((byte)MAJOR.Internal, (byte)INTERNAL_MINOR.NTFRefreshServer, MultipleNetServiceType.Sockets)]
    public class InternalRefreshServerNtf : NtfBase
    {
    }


 
    [Serializable]
    [NtfClass((byte)MAJOR.Internal, (byte)INTERNAL_MINOR.NTFUpdatedGateEncryptKey, ProtocolType.Tcp, NetServiceType.Internal)]
    public class InternalUpdatedGateEncryptKey : NtfBase
    {
        public string GatewayEncryptKey;
    }

    [Serializable]
    [RequestClass((byte)MAJOR.Internal, (byte)INTERNAL_MINOR.GetGateWayEncryptKey, ProtocolType.Tcp, NetServiceType.Internal)]
    public class InternalGetGatewayEncryptKeyReq : RequestBase
    {
    }

    [Serializable]
    public class InternalGetGatewayEncryptKeyRes : ResponseBase
    {
        public string GatewayEncryptKey;
    }
}