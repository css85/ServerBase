using System;
using System.Collections.Generic;
using System.Numerics;

namespace Shared.Packet.Models
{
    [Serializable]
    public class TableActiveIgnoreInfo
    {
        // Ignore
        public List<long> ItemDecorationTableIgnoreIndexList { get; set; } = new List<long>();              // Item_Decoration
        public List<long> PartsTableIgnoreIndexList { get; set; } = new List<long>();                       // Parts
        public List<long> PartsSetTableIgnoreIndexList { get; set; } = new List<long>();                    // Parts_Set     
        public List<long> PartsCollectionSetListTableIgnoreIndexList { get; set; } = new List<long>();      // Parts_Collection_SetList
        public List<long> IllustSeasonGroupTableIgnoreIndexList { get; set; } = new List<long>();           // Illust_Season_Group
        public List<long> IllustBuffTableIgnoreIndexList { get; set; } = new List<long>();                  // Illust_Buff
        public List<long> StoreGachaPickupTableIgnoreIndexList { get; set; } = new List<long>();            // Store_Gacha_Pickup
        ///////////////////////////////////////////////////////////////////////////////////////////
        // Active
        

    }
}
