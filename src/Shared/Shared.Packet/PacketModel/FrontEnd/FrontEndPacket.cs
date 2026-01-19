using Shared.Model;
using Shared.Packet;
using System;
using System.Collections.Generic;
using Shared.Packet.Models;

namespace Shared.PacketModel
{
    /// <summary>
    /// Frontend 접속
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Frontend, (byte)FRONTEND_MINOR.ConnectSession, ProtocolType.Tcp, NetServiceType.FrontEnd)]
    public class ConnectSessionReq : RequestBase
    {
        public long UserSeq;
        public byte Language;

        public byte OsType; // enum OsType
        public string AppVer;
    }

    [Serializable]
    public class ConnectSessionRes : ResponseBase
    {
        public bool IsInvalidAppVersion;

        // --
        // IsInvalidAppVersion == false 일때만
        public long BundleVersion;
        // --

        // --
        // IsInvalidAppVersion == true 일때만
        public string MarketUrl;
        // --

        //새로운 복호화 키 전달
        public string EncryptKey;
    }

    /// <summary>
    /// 알림 메시지
    /// </summary>
    [Serializable]
    [NtfClass((byte)MAJOR.Frontend, (byte)FRONTEND_MINOR.NTFMessageNotification, ProtocolType.Tcp, NetServiceType.FrontEnd)]
    public class MessageNotificationNtf : NtfBase
    {
        public byte SceneLocationFlags; // enum SceneLocationType
        public string Message;
    }

    /// <summary>
    /// 번들 버전 변경됨
    /// </summary>
    [Serializable]
    [NtfClass((byte)MAJOR.Frontend, (byte)FRONTEND_MINOR.NTFBundleVersionChanged, ProtocolType.Tcp, NetServiceType.FrontEnd)]
    public class BundleVersionChangedNtf : NtfBase
    {
        public long BundleVersion;
    }

    /// <summary>
    /// 앱 업데이트 필요
    /// </summary>
    [Serializable]
    [NtfClass((byte)MAJOR.Frontend, (byte)FRONTEND_MINOR.NTFAppVersionChanged, ProtocolType.Tcp, NetServiceType.FrontEnd)]
    public class AppUpdateRequiredNtf : NtfBase
    {
        public string MarketUrl;
    }

    /// <summary>
    /// 유저 세션정보 가져오기
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Frontend, (byte)FRONTEND_MINOR.GetUserLocations, ProtocolType.Tcp, NetServiceType.FrontEnd)]
    public class GetUserLocationsReq : RequestBase
    {
        public List<long> Users;
    }

    /// <response code="1"> 너무 많은 유저리스트를 요청함 (100개 초과시) </response>
    [Serializable]
    public class GetUserLocationsRes : ResponseBase
    {
        public UserConnectInfo[] Connects;
    }
}
