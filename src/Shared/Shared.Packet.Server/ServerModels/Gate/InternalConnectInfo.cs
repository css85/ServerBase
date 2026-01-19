namespace Shared.Gate
{
    public class InternalConnectInfo
    {
        public int AppGroupId;
        public string AppGroupName;
        public int AppId;
        public string AppName;
        public string InternalHost;
        public int InternalPort;
        public byte State; // enum ServerState
    }
}