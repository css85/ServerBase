using System;
using System.Linq;
using System.Threading.Tasks;
using Shared;
using Shared.PacketModel;
using Shared.Server.Packet.Internal;
using Shared.ServerModels.Common;
using Shared.Session.PacketModel;
using static Shared.Session.Extensions.ReplyExtensions;

namespace FrontEndWeb.Connection.Session
{
    public partial class ServerSession
    {
        public Task InternalAppVersionChangedAsync(InternalAppVersionChangedNtf ntf)
        {
            foreach (var userFrontendSession in _userFrontendSessionService.GetAllUserSessions())
            {
                if (userFrontendSession.OsType == ntf.OsType &&
                    ntf.RemovedAppVersions.Contains(userFrontendSession.AppVersion))
                {
                    userFrontendSession.SendNtf(MakeNtfReply(new AppUpdateRequiredNtf
                    {
                        MarketUrl = ntf.MarketUrl,
                    }));
                }
            }

            return Task.CompletedTask;
        }

        public Task InternalBundleVersionChangedAsync(InternalBundleVersionChangedNtf ntf)
        {
            foreach (var userFrontendSession in _userFrontendSessionService.GetAllUserSessions())
            {
                if (userFrontendSession.OsType == ntf.OsType)
                {
                    userFrontendSession.SendNtf(MakeNtfReply(new BundleVersionChangedNtf
                    {
                        BundleVersion = ntf.BundleVersion,
                    }));
                }
            }

            return Task.CompletedTask;
        }
    }
}