using System;
using System.Collections.Generic;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Models;

namespace Shared.PacketModel
{
    [Serializable] 
    [RequestClass((byte) MAJOR.Gateway, (byte) GATEWAY_MINOR.GetServers, ProtocolType.Http, NetServiceType.Gate, RequestMethodType.Post, "/gate/get-servers")]
    public class GetServerInfosReq : RequestBase
    {
        public OSType OsType { get; set; }
        public int AppVer { get; set; }
    }

    /// <response code="-12"> BlockCountry 접속할 수 없는 국가   </response>    
    /// <response code="-14"> NotSupportVersion  사용할 수 없는 버전  </response>    
    [Serializable]
    public class GetServerInfosRes : ResponseBase
    {
        public long ServerTimeTick { get; set; }
        public bool IsServerInspection { get; set; }     // 점검중 여부 
        public long InspectionToDtTick { get; set; }
        public string AssetPatchUrl { get; set; }

        public List<ServiceServerInfo> ServiceServers { get; set; } = new List<ServiceServerInfo> ();
        public List<ContentConfig> ContentConfigs { get; set; } = new List<ContentConfig>();

    }
}