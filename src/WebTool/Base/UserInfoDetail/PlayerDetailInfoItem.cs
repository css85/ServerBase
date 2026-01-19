namespace WebTool.Base.UserInfoDetail
{
    public enum PlayerDetailInfoItemType
    {
        Text,
        Number,
        Bool,
        Color,
        DateTime,                
        Parts,
        Profile,
        MyItem,
        BuyHistory,
        PurchaseHistory,
        SendPush,
        UserBlock,
        Nick,
        User,
    }


    public class PlayerDetailInfoItem
    {
        public string Id;
        public bool IsSeparate;
        public string LocaleName;
        public PlayerDetailInfoItemType Type;
        public PlayerDetailEditInfo EditInfo;
        public string Value;
        public string Value2;

        public bool IsReadonly()
        {
            return EditInfo == null;
        }

        public bool IsCopyable()
        {
            return Type switch
            {
                PlayerDetailInfoItemType.Text => true,
                PlayerDetailInfoItemType.Number => true,
                PlayerDetailInfoItemType.Color => true,
                PlayerDetailInfoItemType.DateTime => true,
                PlayerDetailInfoItemType.User => true,
                _ => false
            };
        }
    }
}
