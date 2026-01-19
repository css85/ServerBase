using System;

namespace Shared.Models.Server
{
    [Serializable]
    public class ServerInfo
    {
        public int AppId { get; set; }
        public string AppName { get; set; }
    }
}
