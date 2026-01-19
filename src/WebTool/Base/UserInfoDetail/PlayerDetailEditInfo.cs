namespace WebTool.Base.UserInfoDetail
{
    public enum PlayerDetailEditInfoType
    {
        None,
        AccountDb,
        UserDb,
    }

    public class PlayerDetailEditInfo
    {
        public PlayerDetailEditInfoType Type;
        public string ClassName;
        public string PropertyName;
    }
}
