using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shared.Clock;
using Shared.Entities.Models;
using Shared.Models;
using Shared.Packet;
using Shared.Packet.Models;
using Shared.Repository;
using Shared.Repository.Database;
using Shared.Repository.Extensions;
using Shared.Server.Define;
using Shared.ServerApp.Extensions;
using Shared.ServerApp.Model;
using StackExchange.Redis;

namespace Shared.ServerApp.Common.Tasks
{
    public static class UserTaskProcessor
    {
        public static async Task<(SessionLocation, long)> GetConnectInfoAsync(IDatabaseAsync sessionRedis,
            IDatabaseAsync accountRedis, long userSeq, long defaultLogoutDt)
        {
            var locationValue = await sessionRedis.HashGetAsync(RedisKeys.hs_UserSessionLocation, userSeq);
            var isConnecting = locationValue.HasValue;
            if (isConnecting)
                return (new SessionLocation(locationValue), AppClock.MaxValue.Ticks);

            var logInOutKey = string.Format(RedisKeys.hs_UserLoginOut, userSeq);
            var logOutDt = await accountRedis.HashGetAsync(logInOutKey, "logOut");

            return (SessionLocation.None, logOutDt == RedisValue.Null ? defaultLogoutDt : (long)logOutDt);
        }

        public static async Task ApplyToConnectInfoAsync(IDatabaseAsync sessionRedis,
            IDatabaseAsync accountRedis, UserSimpleInfo user)
        {
            var locationValue = await sessionRedis.HashGetAsync(RedisKeys.hs_UserSessionLocation, user.UserSeq);
            var isConnecting = locationValue.HasValue;
            if (isConnecting)
            {
                user.Session = new SessionLocation(locationValue);
                user.ConnectTime = AppClock.MaxValue.Ticks;
                return;
            }

            var logInOutKey = string.Format(RedisKeys.hs_UserLoginOut, user.UserSeq);
            var logOutDt = await accountRedis.HashGetAsync(logInOutKey, "logOut");

            user.Session = SessionLocation.None;
            user.ConnectTime = logOutDt == RedisValue.Null ? user.ConnectTime : (long)logOutDt;
        }

        public static async Task ApplyToConnectInfoAsync(IDatabaseAsync sessionRedis,
            IDatabaseAsync accountRedis, UserConnectInfo user)
        {
            var locationValue = await sessionRedis.HashGetAsync(RedisKeys.hs_UserSessionLocation, user.UserSeq);
            var isConnecting = locationValue.HasValue;
            if (isConnecting)
            {
                user.Session = new SessionLocation(locationValue);
                user.ConnectTime = AppClock.MaxValue.Ticks;
                return;
            }

            var logInOutKey = string.Format(RedisKeys.hs_UserLoginOut, user.UserSeq);
            var logOutDt = await accountRedis.HashGetAsync(logInOutKey, "logOut");

            user.Session = SessionLocation.None;
            user.ConnectTime = logOutDt == RedisValue.Null ? user.ConnectTime : (long)logOutDt;
        }

        public static async Task<SessionLocation> SetSessionLocationAsync(IDatabaseAsync sessionRedis, long userSeq,
            SessionLocationType type, int value = 0, int value2 = 0, string valueString = "")
        {
            if (type == SessionLocationType.None)
            {
                await sessionRedis.HashDeleteAsync(RedisKeys.hs_UserSessionLocation, userSeq.ToString());

                return SessionLocation.None;
            }

            var sessionLocation = new SessionLocation
            {
                Type = type,
                Value = value,
                Value2 = value2,
                ValueString = valueString
            };

            await sessionRedis.HashSetAsync(RedisKeys.hs_UserSessionLocation,
                userSeq, sessionLocation.ToString());

            return sessionLocation;
        }

       
       
    }
}
