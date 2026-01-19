using Shared.Packet;
using System;
using System.Net.Sockets;
using System.Numerics;

namespace Shared.Packet.Models
{
    [Serializable]
    public class DuplicateItemInfo
    {
        public ItemInfo OrigineItemInfo { get; set; } = new ItemInfo();        // 원본 아이템 정보 와 몇개가 중복되서 변환 됐는지 
        public ItemInfo ReplaceItemInfo { get; set; } = new ItemInfo(); // 중복되서 대체되는 재화 정보 ( 단, 1개 당 자급되는 양이 입력된다. )

        // 예시 : 
        // OrigineItemInfo => 파츠 3번 4개 
        // ReplaceItemInfo => 재화 13번 5000개 
        // 위와 같을 경우 파츠 3번이 4개가 중복되어서 재화 13번이 5000개씩 4번인 20000개가 지급 되었다. 

    }
}
