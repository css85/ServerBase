using System.Collections.Generic;
using System.Threading.Tasks;


using Shared;
using Shared.Packet.Models;
using Shared.PacketModel;
using Shared.Repository;
using Shared.ServerApp.Common.Tasks;

using Shared.Session.Data;
using static Shared.Session.Extensions.ReplyExtensions;


namespace FrontEndWeb.Connection.Session
{
    public partial class UserFrontendSession
    {
        public async Task<ResponseReply> ConnectSessionAsync(ConnectSessionReq req)
        {
            //var serverConnectInfo = _userSessionService.GateService.GetUserServerConnectInfo(req.OsType, req.AppVer);
            //if (serverConnectInfo == null)
            //    return MakeResReply<ConnectSessionRes>(ResultCode.InvalidParameter);

            //if (serverConnectInfo.IsInvalidAppVersion)
            //{
            //    return MakeResReply(new ConnectSessionRes
            //    {
            //        IsInvalidAppVersion = serverConnectInfo.IsInvalidAppVersion,
            //        BundleVersion = serverConnectInfo.BundleVersion,
            //        MarketUrl = null,
            //    });
            //}

            //var initResult = await _userSessionService.InitializeSessionAsync(this, req.UserSeq, req.Language, req.OsType, req.AppVer);
            //if (initResult != ResultCode.Success)
            //    return MakeResReply<ConnectSessionRes>(initResult);

            //return MakeResReply(new ConnectSessionRes
            //{
            //    IsInvalidAppVersion = serverConnectInfo.IsInvalidAppVersion,
            //    BundleVersion = serverConnectInfo.BundleVersion,
            //    MarketUrl = serverConnectInfo.MarketUrl,
            //    EncryptKey = SecretKey,
            //});
            return MakeResReply(new ConnectSessionRes());
        }

        public async Task<ResponseReply> GetUserLocationsAsync(GetUserLocationsReq req)
        {
            //req.Users ??= new List<long>();

            //if (req.Users.Count > 100)
            //    return MakeResReply<GetUserLocationsRes>(1);

            //using var allUserCtx = _userSessionService.DbRepo.GetAllDb<UserCtx>();

            //var users = await UserTaskProcessor.GetUserInfosAsync(allUserCtx, req.Users);

            //var results = new UserConnectInfo[users.Count];
            //for (var i = 0; i < users.Count; i++)
            //{
            //    var user = users[i];
            //    var (sessionLocation, connectTime) = await UserTaskProcessor.GetConnectInfoAsync(_userSessionService.RedisRepo.Session,
            //        _userSessionService.RedisRepo.Account, user.Seq, user.LastDt.Ticks);
            //    results[i] = new UserConnectInfo
            //    {
            //        UserSeq = user.Seq,
            //        Session = sessionLocation,
            //        ConnectTime = connectTime,
            //    };
            //}

            //return MakeResReply(new GetUserLocationsRes
            //{
            //    Connects = results,
            //});
            return MakeResReply(new GetUserLocationsRes
            {
                Connects = new UserConnectInfo[1]
            });
        }

        
       
    }
}
