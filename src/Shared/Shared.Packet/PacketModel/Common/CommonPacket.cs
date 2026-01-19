using Shared.Packet;
using System;
using Shared.Model;

namespace Shared.PacketModel
{
    #region Ping
    [Serializable]
    [MultipleNtfClass((byte)MAJOR.Common, (byte)COMMON_MINOR.NTFPingCheck, MultipleNetServiceType.Sockets)]
    public class PingCheckNtf : NtfBase
    {
        public byte NetServiceType; // enum NetServiceType
    }

    [Serializable]
    [MultipleRequestClass((byte)MAJOR.Common, (byte)COMMON_MINOR.Ping, MultipleNetServiceType.Sockets)]
    public class PingReq : RequestBase
    {
    }

    [Serializable]
    [RequestClass((byte)MAJOR.Common, (byte)COMMON_MINOR.Ping, ProtocolType.Tcp, NetServiceType.FrontEnd)]
    public class FrontEndPingReq : PingReq
    {
    }

    [Serializable]
    public class PingRes : ResponseBase
    {
        public byte NetServiceType; // enum NetServiceType
    }
    #endregion

    [Serializable]
    [MultipleNtfClass((byte)MAJOR.Common, (byte)COMMON_MINOR.NTFKick, MultipleNetServiceType.Sockets)]
    public class KickNtf : NtfBase
    {
        public byte NetServiceType; // enum NetServiceType
        public int KickReason; // enum KickReasonResult
    }

}