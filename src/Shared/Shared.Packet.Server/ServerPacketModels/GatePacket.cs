using System;
using System.Collections.Generic;
using Shared.Model;
using Shared.Packet;

namespace Shared.Session.PacketModel
{
    
    [Serializable]
    [NtfClass((byte)MAJOR.Internal, (byte)INTERNAL_MINOR.NTFAppVersionChanged, ProtocolType.Tcp, NetServiceType.Internal)]
    public class InternalAppVersionChangedNtf : NtfBase
    {
        public byte OsType; // enum OsType
        public HashSet<string> RemovedAppVersions;
        public string MarketUrl;
    }

    [Serializable]
    [NtfClass((byte)MAJOR.Internal, (byte)INTERNAL_MINOR.NTFBundleVersionChanged, ProtocolType.Tcp, NetServiceType.Internal)]
    public class InternalBundleVersionChangedNtf : NtfBase
    {
        public byte OsType; // enum OsType
        public long BundleVersion;
    }

    [Serializable]
    [NtfClass((byte)MAJOR.Internal, (byte)INTERNAL_MINOR.NTFOpenStateChanged, ProtocolType.Tcp,
       NetServiceType.Internal)]
    public class InternalOpenStateChangedNtf : NtfBase
    {
        public int AppGroupId { get; set; }
        public int AppId { get; set; }
        public byte OpenState { get; set; }

    }

}