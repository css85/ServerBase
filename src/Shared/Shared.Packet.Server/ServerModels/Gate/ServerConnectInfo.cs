using System.Collections.Generic;
using Shared.Packet.Models;

namespace Shared.Gate
{
    public class ServerConnectInfo
    {
        public bool IsInvalidAppVersion;

        // --
        // IsInvalidAppVersion == false 일때만
        public int AppGroupId;
        public long BundleVersion;
        public string BundleUrl;
        public List<ServiceServerInfo> ServiceServers;
        public bool IsQcServers;
        // --

        // --
        // IsInvalidAppVersion == true 일때만
        public string MarketUrl;
        // --
    }
}