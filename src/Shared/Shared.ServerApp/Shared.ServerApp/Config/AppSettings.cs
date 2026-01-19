namespace Shared.ServerApp.Config
{
    public class AppSettings
    {
        public int AppGroupId { get; set; }
        public string AppGroupName { get; set; }
        public int AppId { get; set; }
        public string AppName { get; set; }

        public bool IsTestMode { get; set; }
//        public bool IsLoginLink { get; set; }
        public string ExternalHost { get; set; }
        public int ExternalPort { get; set; }

        public string InternalHost { get; set; }
        public int InternalPort { get; set; }

        public string ListenHost { get; set; }
        public int ListenPort { get; set; }
        public bool EnableUnixSocket { get; set; }
    }
}