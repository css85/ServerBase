using Shared.Packet;
using System;
using System.Net.Sockets;
using System.Numerics;

namespace Shared.Packet.Models
{
    [Serializable]
    public class FriendCoolTime
    {
        public long UserSeq { get; set; }
        public long CoolTimeDtTick { get; set; }     // 현재 서버 시간보다 쿨타임 시간이 클 경우 버튼에 시간 표시
    }
}
