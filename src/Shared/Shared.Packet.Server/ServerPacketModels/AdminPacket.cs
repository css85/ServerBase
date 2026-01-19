using System;
using System.Collections.Generic;
using Shared.Entities;
using Shared.Entities.Models;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Models;

namespace Shared.PacketModel
{
    [Serializable]
    [RequestClass((byte) MAJOR.Admin, (byte) ADMIN_MINOR.Command, ProtocolType.Http, NetServiceType.Admin, RequestMethodType.Post, httpPostPath: "/admin/command")]
    public class AdminCommandReq : RequestBase
    {
        public string Command;
        public string Option1;
        public string Option2;
    }

    [Serializable]
    public class AdminCommandRes : ResponseBase
    {
        public string Result1;
    }

 

}