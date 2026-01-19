using System;
using Shared.ServerApp.Utility;
namespace WebTool.Base.UserInfoDetail
{
    public enum DetailInfoItemType : ushort
    {
        Text, 
        Button,
    }

    public class DetailInfoItem
    {
        public string Id;
        public bool IsSeparate;
        public Enum Type;
        public string LocaleName;
        public string Value;
        public string Value2;
        public bool IsCopyable;
        public bool IsEditable;
    }

    public class TableInfoItem
    {
        public string Id;
        public string[] Values;
    }
}
