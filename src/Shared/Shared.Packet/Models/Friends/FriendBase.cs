using Shared.Packet;
using System;
using System.Net.Sockets;
using System.Numerics;

namespace Shared.Packet.Models
{
    [Serializable]
    public class FriendBase
    {
        public UserInfo UserInfo { get; set; } = new UserInfo();
        public long LatestConnectDtTick { get; set; }
    }
}
