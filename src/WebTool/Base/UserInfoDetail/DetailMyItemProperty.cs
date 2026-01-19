using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebTool.Base.UserInfoDetail
{
    public enum DetailMyItemType : ushort
    {
        Total,
        Material,
        MarketingLeaflet,
        SpecialBuff,
    }

    public class DetailMyItemProperty
    {
        public string TabName;
        public string TableRootId;
        public string TableId;
        public int TableType;
    }
}
