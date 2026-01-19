
using Microsoft.AspNetCore.SignalR;
using Shared.Packet.Models;
using Shared.Services.Redis;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Shared.PacketModel;
using Shared.Server.Define;
using StackExchange.Redis;
using Shared.ServerApp.Services;
using Shared.Repository.Services;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Shared.ServerApp.SignalR;

namespace FrontEndWeb.SignalR
{

    [Authorize(AuthenticationSchemes = DefineConsts.JwtAuthenticationScheme)]
    public class FrontInfoHub : TokenBasedHub
    {  

        public override async Task OnConnectedAsync()
        {   
//            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnConnectedAsync();

        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);

        }
        
        public async Task WorldChatMessage(WorldChatMessageReq req, 
            RedisRepositoryService _redisRepo, DatabaseRepositoryService _dbRepo, 
            PlayerService _playerService, MissionService _missionService)
        {
            var userSeq = GetUserSeq();
            var connId = Context.ConnectionId;
            
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);
            var redisRank = _redisRepo.GetDb(RedisDatabase.Ranking);
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();

            var userRankRedis = await redisRank.SortedSetRankAsync(RedisKeys.ss_ShoppingmallRank, userSeq, Order.Descending);
            var userRank = userRankRedis.HasValue ? userRankRedis.Value + 1 : 0;

            var userInfoRedis = await redisUser.HashGetAllAsync(string.Format(RedisKeys.hs_UserInfo, userSeq));
            if (userInfoRedis.Length <= 0)
                userInfoRedis = await _playerService.SetUserInfoRedisAsync(userCtx, redisUser, userSeq);

            var userInfoDic = userInfoRedis
               .Select(e => new { key = (string)e.Name, Value = e.Value })
               .ToDictionary(e => e.key, e => e.Value);

            var userInfo = new UserInfo
            {
                UserSeq = (long)userInfoDic["userSeq"],
                Nick = userInfoDic["nick"],
                Level = (int)userInfoDic["level"],
                ShoppingmallGrade = (int)userInfoDic["grade"],
                ProfileInfo = _playerService.ToProfileInfo((long)userInfoDic["userSeq"], userInfoDic["profileParts"]),
            };

         

            var res = new WorldChatMessageRes
            {
                Rank = userRank,    
                UserInfo = userInfo,
                Message = req.Message,
            };
            await Clients.All.SendAsync("WorldChatMessage", res);

        }

        

    }
}
