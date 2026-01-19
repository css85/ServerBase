using System;
using System.Collections.Generic;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Models;

namespace Shared.PacketModel
{
    /// <summary>
    /// 인증 패킷
    /// id, pw 입력 받아 wallet Login 후 token 발급 처리 
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Account, (byte)ACCOUNT_MINOR.Auth, ProtocolType.Http, NetServiceType.Api
        , RequestMethodType.Post, "/api/auth/auth")]
    public class AccountAuthReq : RequestBase
    {
        public OSType OsType { get; set; }
        public AccountType AccountType { get; set; }
        public string Id { get; set; }
        public string BeforeId { get; set; }
        public string PushToken { get; set; }
    }

    /// <response code="-101"> InvalidParameter id 나 password 가 비어서 왔을 경우  </response>
    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    /// <response code="-11"> WrongAccount 계정이 없거나 잘못 되었을 경우 </response>
    [Serializable]
    public class AccountAuthRes : ResponseBase
    {
        public string HttpToken { get; set; }            //Http용 인증 토큰
        public long UserSeq { get; set; }
        public AccountType AccountType { get; set; }
        public string Id { get; set; }
        public bool IsFirstLogin { get; set; }
        public NewUserConfig NewUserConfig { get; set; } = new NewUserConfig();
    }

    /// <summary>
    /// 계정 연동
    /// 
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Account, (byte)ACCOUNT_MINOR.AccountGuestToPlatform, ProtocolType.Http, NetServiceType.Api
        , RequestMethodType.Post, "/api/auth/account-link")]
    public class AccountLinkReq : RequestBase
    {
        public AccountType AccountType;
        public string Id;
    }

    /// <response code="-101"> InvalidParameter id 나 password 가 비어서 왔을 경우  </response>
    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    /// <response code="-11"> WrongAccount 계정이 없거나 잘못 되었을 경우 </response>
    [Serializable]
    public class AccountLinkRes : ResponseBase
    {
        public AccountType AccountType { get; set; }
        public string Id { get; set; }
        public List<AccountType> AccountLinks { get; set; } = new List<AccountType>();                // 연동된 타입들
    }

    /// <summary>
    /// 계정 연동
    /// 
    /// </summary>
    [Serializable]
    [RequestClass((byte)MAJOR.Account, (byte)ACCOUNT_MINOR.RemoveAccount, ProtocolType.Http, NetServiceType.Api, RequestMethodType.Post, "/api/auth/remove-account")]
    public class RemoveAccountReq : RequestBase
    {
        
    }

    /// <response code="-10"> ServerInspection 서버 점검 중 일 경우  </response>
    [Serializable]
    public class RemoveAccountRes : ResponseBase
    {
      
    }

}