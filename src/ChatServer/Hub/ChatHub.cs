
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
using Shared;
using Microsoft.AspNetCore.Authorization;
using Shared.ServerApp.SignalR;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Shared.ServerApp.Utility;

namespace ChatServer.SignalR
{

    [Authorize(AuthenticationSchemes = DefineConsts.JwtAuthenticationScheme)]
    public class ChatHub : TokenBasedHub
    {  

        public override async Task OnConnectedAsync()
        {
            //            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
            //var name = Context.User.Identity.Name;
            //var connectId = Context.ConnectionId;
            //var userSeq= GetUserSeq();
            await base.OnConnectedAsync();

        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);

        }
        
        public async Task WorldChatMessage(WorldChatMessageReq req, 
            RedisRepositoryService _redisRepo, DatabaseRepositoryService _dbRepo, 
            PlayerService _playerService, MissionService _missionService, InventoryService _inventoryService)
        {
            var userSeq = GetUserSeq();
            var connId = Context.ConnectionId;
            
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);
            var redisRank = _redisRepo.GetDb(RedisDatabase.Ranking);
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();

            var refreshInfo = new RefreshInventoryInfo();

            var userRank = 0;
            var userInfo = new UserInfo();

            var myRankInfo = _playerService.GetShoppingmallUserRankInfo(userSeq);
            if (myRankInfo == null)
            {
                var userRankRedis = await redisRank.SortedSetRankAsync(RedisKeys.ss_ShoppingmallRank, userSeq, Order.Descending);
                userRank = userRankRedis.HasValue ? (int)userRankRedis.Value + 1 : 0;
                userInfo = await _playerService.GetUserInfoModelAsync(userCtx, redisUser, userSeq);
            }
            else
            {
                userRank = myRankInfo.Rank;
                userInfo = myRankInfo.UserInfo;
            }
          

            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();

            var res = new WorldChatMessageRes
            {
                Rank = userRank,    
                UserInfo = userInfo,
                Message = req.Message,
                RefreshInfo = refreshInfo,
            };
            await Clients.All.SendAsync("WorldChatMessage", res);
        }

        public async Task WorldChatPosting(WorldChatPostingReq req,
            RedisRepositoryService _redisRepo, DatabaseRepositoryService _dbRepo,
            PlayerService _playerService, MissionService _missionService, InventoryService _inventoryService)
        {
            var userSeq = GetUserSeq();

            var redisUser = _redisRepo.GetDb(RedisDatabase.User);
            var redisRank = _redisRepo.GetDb(RedisDatabase.Ranking);
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();
            var refreshInfo = new RefreshInventoryInfo();

            var userRank = 0;
            var userInfo = new UserInfo();

            var myRankInfo = _playerService.GetShoppingmallUserRankInfo(userSeq);
            if (myRankInfo == null)
            {
                var userRankRedis = await redisRank.SortedSetRankAsync(RedisKeys.ss_ShoppingmallRank, userSeq, Order.Descending);
                userRank = userRankRedis.HasValue ? (int)userRankRedis.Value + 1 : 0;
                userInfo = await _playerService.GetUserInfoModelAsync(userCtx, redisUser, userSeq);
            }
            else
            {
                userRank = myRankInfo.Rank;
                userInfo = myRankInfo.UserInfo;
            }

            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();
            var res = new WorldChatPostingRes
            {
                Rank = userRank,
                UserInfo = userInfo,
                RefreshInfo = refreshInfo
            };
            await Clients.All.SendAsync("WorldChatPosting", res);

        }


    }
}
