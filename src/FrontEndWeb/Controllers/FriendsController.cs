using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Config;
using Shared.Services.Redis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Clock;
using Shared.Entities.Models;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.ServerApp.Config;
using Shared.ServerApp.Model;
using Shared.ServerApp.Mvc;
using Shared.ServerApp.Services;
using StackExchange.Redis;
using Shared.PacketModel;
using Shared.Packet.Models;
using System;
using Shared.ServerApp.Utility;
namespace FrontEndWeb.Controllers
{
//    [ApiVersion("2.0")]
    [Route("api/friends")]
    public class FriendsController : TokenBasedApiController
    {
        private readonly ILogger<FriendsController> _logger;
        private readonly DatabaseRepositoryService _dbRepo;
        private readonly RedisRepositoryService _redisRepo;
        private readonly CsvStoreContext _csvContext;        
        private readonly InventoryService _inventoryService;
        private readonly MissionService _missionService;
        private readonly PlayerService _playerService;
        private readonly ServerInspectionService _serverInspectionService;

        private ISubscriber _subscriber;

        public FriendsController(
            ILogger<FriendsController> logger,
            DatabaseRepositoryService dbRepo,
            CsvStoreContext csvContext,
            RedisRepositoryService redisRepo,            
            InventoryService inventoryService,
            MissionService missionService,
            PlayerService playerService,
            ServerInspectionService serverInspectionService
        )
        {
            _logger = logger;
            _dbRepo = dbRepo;
            _csvContext = csvContext;
            _redisRepo = redisRepo;
            _inventoryService = inventoryService;
            _missionService = missionService;
            _playerService = playerService;
            _serverInspectionService = serverInspectionService;
            _subscriber = _redisRepo.App.Multiplexer.GetSubscriber();
        }

        [HttpPost("enter-friends")]
        public async Task<ActionResult<EnterFriendsRes>> EnterFriendsAsync([FromBody] EnterFriendsReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<EnterFriendsRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);
            var csvData = _csvContext.GetData();

            var userFriendInfo = await userCtx.UserFriendInfos.FindAsync(userSeq);
            if( userFriendInfo == null )
            {
                var userCode = await _playerService.GetUserCodeAsync(userCtx);
                if (userCode == null)
                    return Ok<EnterFriendsRes>(ResultCode.ServerError);

                userFriendInfo = new UserFriendInfoModel
                {
                    UserSeq = userSeq,
                    UserCode = userCode,
                };
                await userCtx.UserFriendInfos.AddAsync(userFriendInfo);
                await userCtx.SaveChangesAsync();
            }

            var recommendCount = await userCtx.UserRecommends.Where(p => p.RecommendUserSeq == userSeq).SumAsync(p => p.RecommendCount);
//            var recommendCount = await userCtx.UserRecommends.CountAsync(p => p.RecommendUserSeq == userSeq);

            var friendsDbs = await userCtx.UserFriends.Where(p => p.UserSeq == userSeq).ToListAsync();            
            var friendsIndexList = friendsDbs.Select(p=>p.TargetSeq).ToList();
            var reqFriendsIndexList = await userCtx.UserReqestFriends.Where(p=>p.UserSeq == userSeq).Select(p=>p.RequestUserSeq).ToListAsync();


            var userIndexList = friendsIndexList.ToList();
            userIndexList.AddRange(reqFriendsIndexList);
            userIndexList = userIndexList.Distinct().ToList();

            
            var redisLastConValues = await redisUser.HashGetAsync(RedisKeys.hs_UserLastConnectTick, userIndexList.Select(p => new RedisValue(p.ToString())).ToArray());
            var userLastConDic = new Dictionary<long, long>();
            
            for(int i = 0; i<userIndexList.Count; i++)
            {
                var tick = redisLastConValues[i].HasValue ? (long)redisLastConValues[i] : 0;
                userLastConDic.TryAdd(userIndexList[i], tick);
            }
            

//            var accountDbs = await userCtx.Accounts.Where(p => userIndexList.Contains(p.UserSeq)).ToListAsync();
            var userInfoList = await _playerService.GetUserInfosAsync(userCtx, userIndexList);

            var friendsList = new List<FriendInfo>();
            foreach (var friend in friendsDbs)
            {   
                friendsList.Add(new FriendInfo
                {
                    UserInfo = userInfoList.Where(p => p.UserSeq == friend.TargetSeq).First(),
                    LatestConnectDtTick = userLastConDic[friend.TargetSeq],
//                    LatestLoginDtTick = accountDbs.Where(p => p.UserSeq == friend.TargetSeq).First().LoginDt.Ticks,
                    CoolTimeDtTick = friend.CoolTimeDt.Ticks
                });
            }
            var reqFriendsList = new List<FriendBase>();
            foreach(var reqFriend in reqFriendsIndexList)
            {
                reqFriendsList.Add(new FriendBase
                {
                    UserInfo = userInfoList.Where(p => p.UserSeq == reqFriend).First(),
                    LatestConnectDtTick = userLastConDic[reqFriend],
//                    LatestLoginDtTick = accountDbs.Where(p => p.UserSeq == reqFriend).First().LoginDt.Ticks,
                });
            }

            var recommendRewardDBs = await userCtx.UserRecommendRewards.Where(p =>p.UserSeq == userSeq).ToListAsync();
            var rewardRecommends = recommendRewardDBs.Any() ? recommendRewardDBs.Select(p=>p.RecommendIndex).ToList() : new List<long>();

            var resFriendsDb = await userCtx.UserResponseFriends.Where(p => p.UserSeq == userSeq).ToListAsync();

            var resFriendRedDot = false;
            if (resFriendsDb.Any())
            {
                var resFriedsSeqList = resFriendsDb.Select(p => p.ResponseUserSeq).ToList();
                var redisResFriends = await redisUser.StringGetAsync(string.Format(RedisKeys.s_LastResponseFriends, userSeq));
                if (redisResFriends.HasValue == false)
                    resFriendRedDot = true;
                else
                {
                    var redisResFriendsString = (string)redisResFriends;
                    var redisSeqs = redisResFriendsString.Split(",").Select(long.Parse).ToList();
                    foreach (var dbUser in resFriedsSeqList)
                    {
                        if (redisSeqs.Contains(dbUser) == false)
                        {
                            resFriendRedDot = true;
                            break;
                        }
                    }
                }
            }

            var resp = new EnterFriendsRes
            {
                UserCode = userFriendInfo.UserCode,
                SendCode = userFriendInfo.SendCode,
                RecommendCoolTimeEndDtTick = userFriendInfo.RecommendUserCodeDt.AddMinutes(SharedDefine.RECOMMEND_CODE_COOLTIME_MIN).Ticks,
//                RecommendUserCode = userFriendInfo.RecommendUserCode,
                RecommendCount = recommendCount,
                ResponseFriendRedDot = resFriendRedDot,

                Friends = friendsList,
                RequestFriends = reqFriendsList,
                RewardRecommends = rewardRecommends
            };

            return Ok(ResultCode.Success, resp);
        }

        [HttpPost("response-friends")]
        public async Task<ActionResult<ResponseFriendsRes>> ResponseFriendsAsync([FromBody] EnterFriendsReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<ResponseFriendsRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            var responseFriendsDbs = await userCtx.UserResponseFriends.Where(p => p.UserSeq == userSeq).ToListAsync();
            if( responseFriendsDbs.Any() == false )
            {
                return Ok(ResultCode.Success, new ResponseFriendsRes());
            }


            var userIndexList = responseFriendsDbs.Select(p=>p.ResponseUserSeq).ToList();
            var redisLastConValues = await redisUser.HashGetAsync(RedisKeys.hs_UserLastConnectTick, userIndexList.Select(p => new RedisValue(p.ToString())).ToArray());
            var userLastConDic = new Dictionary<long, long>();

            for (int i = 0; i < userIndexList.Count; i++)
            {
                var tick = redisLastConValues[i].HasValue ? (long)redisLastConValues[i] : 0;
                userLastConDic.TryAdd(userIndexList[i], tick);
            }

            //            var accountDbs = await userCtx.Accounts.Where(p => userIndexList.Contains(p.UserSeq)).ToListAsync();
            var userInfoList = await _playerService.GetUserInfosAsync(userCtx, userIndexList);

            var resFriends = new List<FriendBase>();
            foreach(var seq in userIndexList)
            {
                resFriends.Add(new FriendBase
                {
                    UserInfo = userInfoList.Where(p => p.UserSeq == seq).First(),
                    LatestConnectDtTick = userLastConDic[seq],
                    //                    LatestLoginDtTick = accountDbs.Where(p => p.UserSeq == seq).First().LoginDt.Ticks,
                });
            }

            var redisKey = string.Format(RedisKeys.s_LastResponseFriends, userSeq);
            var newFriendsSeqList = new List<long>();
            var redisResFriends = await redisUser.StringGetAsync(redisKey);
            if (redisResFriends.HasValue == false)
                newFriendsSeqList = userIndexList;
            else
            {
                var redisResFriendsString = (string)redisResFriends;
                var redisSeqs = redisResFriendsString.Split(",").Select(long.Parse).ToList();
                foreach (var dbUser in userIndexList)
                {
                    if (redisSeqs.Contains(dbUser) == false)
                        newFriendsSeqList.Add(dbUser);
                }
            }
            await redisUser.StringSetAsync(redisKey, string.Join(",", userIndexList));

            return Ok(ResultCode.Success, new ResponseFriendsRes
            {
                ResponseFriends = resFriends,
                NewFriendsSeqList = newFriendsSeqList
            });
        }

        [HttpPost("recommend-friends")]
        public async Task<ActionResult<RecommendFriendsRes>> RecommendFriendsAsync([FromBody] RecommendFriendsReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<RecommendFriendsRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);
            
            var friends = await userCtx.UserFriends.Where(p=>p.UserSeq == userSeq).Select(p=>p.TargetSeq).ToListAsync();
            var requestFriends = await userCtx.UserReqestFriends.Where(p=>p.UserSeq == userSeq).Select(p=>p.RequestUserSeq).ToListAsync();

            var exceptUserList = new List<long> { userSeq };
            exceptUserList.AddRange(friends);
            exceptUserList.AddRange(requestFriends);

            var rand = new Random();
            var accountDbs = await userCtx.UserAccounts.Where(p => p.LoginDt >= AppClock.UtcNow.AddDays(-2) && !exceptUserList.Contains(p.UserSeq)).Select(p => p.UserSeq).ToListAsync();
            var userIndexList = accountDbs.OrderBy(_ => rand.Next()).Take(15).ToList();

            if ( userIndexList.Any() == false )
                return Ok(ResultCode.Success, new RecommendFriendsRes());

            var userInfoList = await _playerService.GetUserInfosAsync(userCtx, userIndexList);
            var redisLastConValues = await redisUser.HashGetAsync(RedisKeys.hs_UserLastConnectTick, userIndexList.Select(p => new RedisValue(p.ToString())).ToArray());
            var userLastConDic = new Dictionary<long, long>();

            for (int i = 0; i < userIndexList.Count; i++)
            {
                if(redisLastConValues[i].HasValue)
                    userLastConDic.TryAdd(userIndexList[i], (long)redisLastConValues[i]);
            }

            var recommends = new List<FriendBase>();
            foreach (var seq in userIndexList)
            {
                if( seq == userSeq) 
                    continue;

                var info = userInfoList.Where(p => p.UserSeq == seq).FirstOrDefault();
                if( info != null )
                {
                    if( userLastConDic.TryGetValue(seq, out var tick))
                    {
                        recommends.Add(new FriendBase
                        {
                            UserInfo = info,
                            LatestConnectDtTick = tick,
                        });
                    }
                }
            }

            return Ok(ResultCode.Success, new RecommendFriendsRes
            {
                RecommendFriends = recommends,
            });
        }

        [HttpPost("request-friends")]
        public async Task<ActionResult<RequestFriendsRes>> RequestFriendsAsync([FromBody] RequestFriendsReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<RequestFriendsRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            if ( req.TargetUserSeq == userSeq)
                return Ok<RequestFriendsRes>(ResultCode.InvalidParameter);

            // 내가 보낸 요청수 20개최대 안넘는지 
            var reqFriendsDbs = await userCtx.UserReqestFriends.Where(p => p.UserSeq == userSeq).ToListAsync();
            var reqCount = reqFriendsDbs.Count;
            if (reqCount >= SharedDefine.MAX_REQUEST_FRIENDS)
                return Ok<RequestFriendsRes>(ResultCode.MaxRequestFriends);
            if(reqFriendsDbs.Exists(p =>p.RequestUserSeq == req.TargetUserSeq))
                return Ok<RequestFriendsRes>(ResultCode.AlreadyRequestFriends);

            // 상대방 받은 초대 수 20개 최대 안넘었는지
            var resCount = await userCtx.UserResponseFriends.CountAsync(p => p.UserSeq == req.TargetUserSeq);
            if (resCount >= SharedDefine.MAX_RESPONSE_FRIENDS)
                return Ok<RequestFriendsRes>(ResultCode.MaxResponseFriends);

            // 상대방 친구가 50명 최대 인지.
            // 이미 친구 인지 
            var targetUserFriendsDbs = await userCtx.UserFriends.Where(p => p.UserSeq == req.TargetUserSeq).ToListAsync();
            if( targetUserFriendsDbs.Count >= SharedDefine.MAX_FRIENDS )
                return Ok<RequestFriendsRes>(ResultCode.MaxFriends);
            if( targetUserFriendsDbs.Exists(p=>p.TargetSeq == userSeq))
                return Ok<RequestFriendsRes>(ResultCode.AlreadyFriends);

            var isAcceptFriends = false;
            var friendInfo = new FriendInfo();
            var requestFriend = new FriendBase();

            // 내가 받은 친구 요청에 상대가 요청 했던게 있다면 바로 수락
            // 없다면 각 요청 db 에 넣어줌 

            var reqUserInfo = await _playerService.GetUserInfoModelAsync(userCtx, redisUser, req.TargetUserSeq);

            var redisTick = await redisUser.HashGetAsync(RedisKeys.hs_UserLastConnectTick, req.TargetUserSeq);
            var lastConnectTick = redisTick.HasValue ? (long)redisTick : 0;


            var userResponseDb = await userCtx.UserResponseFriends.Where(p => p.UserSeq == userSeq && p.ResponseUserSeq == req.TargetUserSeq).FirstOrDefaultAsync();
            if (userResponseDb == null)
            {
                await userCtx.UserReqestFriends.AddAsync(new UserRequestFriendsModel
                {
                    UserSeq = userSeq,
                    RequestUserSeq = req.TargetUserSeq,
                });

                await userCtx.UserResponseFriends.AddAsync(new UserResponseFriendsModel
                {
                    UserSeq = req.TargetUserSeq,
                    ResponseUserSeq = userSeq,
                });

                
                requestFriend = new FriendBase
                {
                    UserInfo = reqUserInfo,
                    LatestConnectDtTick = lastConnectTick,
                    //                    LatestLoginDtTick = reqAccountDb.LoginDt.Ticks,
                };
            }
            else
            {
                isAcceptFriends = true;
                userCtx.UserResponseFriends.Remove(userResponseDb);

                var userReqDb = await userCtx.UserReqestFriends.Where(p=>p.UserSeq == req.TargetUserSeq && p.RequestUserSeq == userSeq).FirstOrDefaultAsync();
                if( userReqDb != null )
                    userCtx.UserReqestFriends.Remove(userReqDb);

                await userCtx.UserFriends.AddAsync(new UserFriendsModel
                {
                    UserSeq = userSeq,
                    TargetSeq = req.TargetUserSeq,
                    CoolTimeDt = AppClock.UtcNow.AddDays(1),
                });
                await userCtx.UserFriends.AddAsync(new UserFriendsModel
                {
                    UserSeq = req.TargetUserSeq,
                    TargetSeq = userSeq,
                    CoolTimeDt = AppClock.UtcNow.AddDays(1),
                });

                friendInfo = new FriendInfo
                {
                    UserInfo = reqUserInfo,
                    LatestConnectDtTick = lastConnectTick,
//                    LatestLoginDtTick = reqAccountDb.LoginDt.Ticks,
                    CoolTimeDtTick = AppClock.UtcNow.AddDays(1).Ticks,
                };

                await _missionService.AddMissionCountUpAsync(userCtx, storeEventCtx, userSeq, Cond1Type.Friend, Cond2Type.Making, 0, 1);
                await _missionService.AddMissionCountUpAsync(userCtx, storeEventCtx, req.TargetUserSeq, Cond1Type.Friend, Cond2Type.Making, 0, 1);
            }
            await _missionService.AddMissionCountUpAsync(userCtx, storeEventCtx, userSeq, Cond1Type.Friend, Cond2Type.Send, 0, 1);

            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();

            return Ok(ResultCode.Success, new RequestFriendsRes
            {
                IsAcceptFriends = isAcceptFriends,
                FriendInfo = friendInfo,
                RequestFriend = requestFriend,
            });
        }

        [HttpPost("cancel-request-friends")]
        public async Task<ActionResult<CancelRequestFriendsRes>> CancelRequestFriendsAsync([FromBody] CancelRequestFriendsReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<CancelRequestFriendsRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();

            if (await userCtx.UserFriends.CountAsync(p => p.UserSeq == userSeq && p.TargetSeq == req.TargetUserSeq) > 0)
                return Ok<CancelRequestFriendsRes>(ResultCode.AlreadyFriends);

            var reqFriendsDb = await userCtx.UserReqestFriends.Where(p => p.UserSeq == userSeq && p.RequestUserSeq == req.TargetUserSeq).FirstOrDefaultAsync();
            if( reqFriendsDb == null)
                return Ok<CancelRequestFriendsRes>(ResultCode.NotExistReqFriends);

            var resFriendsDb = await userCtx.UserResponseFriends.Where(p => p.UserSeq == req.TargetUserSeq && p.ResponseUserSeq == userSeq).FirstOrDefaultAsync();
            if (resFriendsDb == null)
            {
                userCtx.UserReqestFriends.Remove(reqFriendsDb);
                await userCtx.SaveChangesAsync();
                return Ok<CancelRequestFriendsRes>(ResultCode.NotExistResFriends);
            }   

            userCtx.UserReqestFriends.Remove(reqFriendsDb);
            userCtx.UserResponseFriends.Remove(resFriendsDb);
            await userCtx.SaveChangesAsync();


            return Ok(ResultCode.Success, new CancelRequestFriendsRes
            {
                TargetUserSeq = req.TargetUserSeq,
            });
        }

        [HttpPost("accept-friends")]
        public async Task<ActionResult<AcceptFriendsRes>> AcceptFriendsAsync([FromBody] AcceptFriendsReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<AcceptFriendsRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            // 상대방 친구가 50명 최대 인지.
            // 이미 친구 인지 
            var targetUserFriendsDbs = await userCtx.UserFriends.Where(p => p.UserSeq == req.TargetUserSeq).ToListAsync();
            if (targetUserFriendsDbs.Count >= SharedDefine.MAX_FRIENDS)
                return Ok<AcceptFriendsRes>(ResultCode.MaxFriends);
            if (targetUserFriendsDbs.Exists(p => p.TargetSeq == userSeq))
                return Ok<AcceptFriendsRes>(ResultCode.AlreadyFriends);

            if( await userCtx.UserFriends.CountAsync(p => p.UserSeq == userSeq) >= SharedDefine.MAX_FRIENDS)
                return Ok<AcceptFriendsRes>(ResultCode.MaxFriends);

            var resFriendDb = await userCtx.UserResponseFriends.Where(p=>p.UserSeq == userSeq && p.ResponseUserSeq == req.TargetUserSeq).FirstOrDefaultAsync();
            if( resFriendDb == null)
                return Ok<AcceptFriendsRes>(ResultCode.NotExistResFriends);


            var reqFriendDb = await userCtx.UserReqestFriends.Where(p => p.UserSeq == req.TargetUserSeq && p.RequestUserSeq == userSeq).FirstOrDefaultAsync();
            if (reqFriendDb != null)
                userCtx.UserReqestFriends.Remove(reqFriendDb);
            userCtx.UserResponseFriends.Remove(resFriendDb);

            

            await userCtx.UserFriends.AddAsync(new UserFriendsModel
            {
                UserSeq = userSeq,
                TargetSeq = req.TargetUserSeq,
                CoolTimeDt = AppClock.UtcNow.AddDays(1),
            });
            await userCtx.UserFriends.AddAsync(new UserFriendsModel
            {
                UserSeq = req.TargetUserSeq,
                TargetSeq = userSeq,
                CoolTimeDt = AppClock.UtcNow.AddDays(1),
            });

            await _missionService.AddMissionCountUpAsync(userCtx, storeEventCtx, userSeq, Cond1Type.Friend, Cond2Type.Making, 0, 1);
            await _missionService.AddMissionCountUpAsync(userCtx, storeEventCtx, req.TargetUserSeq, Cond1Type.Friend, Cond2Type.Making, 0, 1);

            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();

            var reqUserInfo = await _playerService.GetUserInfoModelAsync(userCtx, redisUser, req.TargetUserSeq);

            var redisTick = await redisUser.HashGetAsync(RedisKeys.hs_UserLastConnectTick, req.TargetUserSeq);
            var lastConnectTick = redisTick.HasValue ? (long)redisTick : 0;

            var friendInfo = new FriendInfo
            {
                UserInfo = reqUserInfo,
                LatestConnectDtTick = lastConnectTick,
                CoolTimeDtTick = AppClock.UtcNow.AddDays(1).Ticks,
            };

            return Ok(ResultCode.Success, new AcceptFriendsRes
            {
                FriendInfo = friendInfo,
            });
        }

        [HttpPost("reject-friends")]
        public async Task<ActionResult<RejectFriendsRes>> RejectFriendsAsync([FromBody] RejectFriendsReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<AcceptFriendsRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();

            var resFriendDb = await userCtx.UserResponseFriends.Where(p => p.UserSeq == userSeq && p.ResponseUserSeq == req.TargetUserSeq).FirstOrDefaultAsync();
            if (resFriendDb == null)
            {
                var reqFriendDb = await userCtx.UserReqestFriends.Where(p => p.UserSeq == req.TargetUserSeq && p.RequestUserSeq == userSeq).FirstOrDefaultAsync();
                if (reqFriendDb != null)
                    userCtx.UserReqestFriends.Remove(reqFriendDb);
            }
            else
            {
                var reqFriendDb = await userCtx.UserReqestFriends.Where(p => p.UserSeq == req.TargetUserSeq && p.RequestUserSeq == userSeq).FirstOrDefaultAsync();
                if (reqFriendDb != null)
                    userCtx.UserReqestFriends.Remove(reqFriendDb);
                userCtx.UserResponseFriends.Remove(resFriendDb);
            }

            await userCtx.SaveChangesAsync();

            return Ok(ResultCode.Success, new RejectFriendsRes
            {
                TargetUserSeq = req.TargetUserSeq,
            });
        }

        [HttpPost("delete-friends")]
        public async Task<ActionResult<DeleteFriendsRes>> DeleteFriendsAsync([FromBody] DeleteFriendsReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<DeleteFriendsRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();

            var friendsDb = await userCtx.UserFriends.Where(p => p.UserSeq == userSeq && p.TargetSeq == req.TargetUserSeq).FirstOrDefaultAsync();
            if (friendsDb == null)
            {
                var opponentFriendDb = await userCtx.UserFriends.Where(p => p.UserSeq == req.TargetUserSeq && p.TargetSeq == userSeq).FirstOrDefaultAsync();
                if (opponentFriendDb != null)
                    userCtx.UserFriends.Remove(opponentFriendDb);
            }
            else
            {
                var opponentFriendDb = await userCtx.UserFriends.Where(p => p.UserSeq == req.TargetUserSeq && p.TargetSeq == userSeq).FirstOrDefaultAsync();
                if (opponentFriendDb != null)
                    userCtx.UserFriends.Remove(opponentFriendDb);
                userCtx.UserFriends.Remove(friendsDb);

            }

            await userCtx.SaveChangesAsync();

            return Ok(ResultCode.Success, new DeleteFriendsRes
            {
                TargetUserSeq = req.TargetUserSeq,
            });
        }

        [HttpPost("send-crystal")]
        public async Task<ActionResult<SendCrystalRes>> SendCrystalFriendsAsync([FromBody] SendCrystalReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<SendCrystalRes>(ResultCode.ServerInspection);

            if( req.TargetUsers.Any() == false )
                return Ok<SendCrystalRes>(ResultCode.InvalidParameter);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();

            var friendsDbs = await userCtx.UserFriends.Where(p => p.UserSeq == userSeq && req.TargetUsers.Contains(p.TargetSeq)).ToListAsync();
            var userInfoDbs = await userCtx.UserInfos.Where(p => req.TargetUsers.Contains(p.UserSeq)).ToListAsync();

            var coolTimeUsers = new List<FriendCoolTime>();
            var mailDbs = new List<UserMailModel>();    
            foreach(var friend in req.TargetUsers)
            {
                var friendDb = friendsDbs.Find(p => p.TargetSeq == friend);
                if ( friendDb != null )
                {
                    if( friendDb.CoolTimeDt <= AppClock.UtcNow)
                    {
                        friendDb.CoolTimeDt = AppClock.UtcNow.AddDays(1).Date;
                        var nick = userInfoDbs.Find(p => p.UserSeq == friend).Nick;
                        // 메일 보내기 
                        mailDbs.Add(new UserMailModel
                        {
                            UserSeq = friend,
                            ObtainType = RewardPaymentType.Currency,
                            ObtainId = (long)CurrencyType.Crystal,
                            ObtainQty = 1,
                            TitleKey = "mailbox_crystals",
                            TitleKeyArg = nick,
                            LimitDt = AppClock.UtcNow.AddDays(7),
                        });
                    }

                }
                coolTimeUsers.Add(new FriendCoolTime { UserSeq = friend, CoolTimeDtTick = friendDb.CoolTimeDt.Ticks});
            }

            userCtx.UserFriends.UpdateRange(friendsDbs);
            await userCtx.UserMails.AddRangeAsync(mailDbs);
            await userCtx.SaveChangesAsync();

            return Ok(ResultCode.Success, new SendCrystalRes
            {
                CoolTimeUsers = coolTimeUsers,
            });
        }

        [HttpPost("search-friends")]
        public async Task<ActionResult<SearchFrinedsRes>> SearchFriendsAsync([FromBody] SearchFrinedsReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<SearchFrinedsRes>(ResultCode.ServerInspection);

            if (string.IsNullOrEmpty(req.SearchNick))
                return Ok<SearchFrinedsRes>(ResultCode.InvalidParameter);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            var friends = await userCtx.UserFriends.Where(p => p.UserSeq == userSeq).Select(p => p.TargetSeq).ToListAsync();
            var requestFriends = await userCtx.UserReqestFriends.Where(p => p.UserSeq == userSeq).Select(p => p.RequestUserSeq).ToListAsync();

            var exceptUserList = new List<long> { userSeq };
            exceptUserList.AddRange(friends);
            exceptUserList.AddRange(requestFriends);


            var userInfos = new List<UserInfo>();   
            var equalNickDb = await userCtx.UserInfos.Where(p => p.Nick.Equals(req.SearchNick) && !exceptUserList.Contains(p.UserSeq)).FirstOrDefaultAsync();
            if (equalNickDb != null)
            {
                var userInfo = await _playerService.GetUserInfoAsync(userCtx, equalNickDb);
                userInfos.Add(userInfo);
            }

            var randUserDbs = await userCtx.UserInfos.Where(p => p.Nick.Contains(req.SearchNick) && !exceptUserList.Contains(p.UserSeq)).OrderBy(p => Guid.NewGuid()).Take(10).ToListAsync();
            if (randUserDbs.Any())
            {
                var userSeqs = randUserDbs.Select(p=>p.UserSeq).ToList();
                var infos = await _playerService.GetUserInfosAsync(userCtx, userSeqs);
                userInfos.AddRange(infos);
            }   

            if (userInfos.Any() == false)
                return Ok(ResultCode.Success, new SearchFrinedsRes());

            userInfos = userInfos.Distinct(new UserInfoComparer()).ToList();
            var userIndexList = userInfos.Select(p=>p.UserSeq).ToList();

            var redisLastConValues = await redisUser.HashGetAsync(RedisKeys.hs_UserLastConnectTick, userIndexList.Select(p => new RedisValue(p.ToString())).ToArray());
            var userLastConDic = new Dictionary<long, long>();

            for (int i = 0; i < userIndexList.Count; i++)
            {
                var tick = redisLastConValues[i].HasValue ? (long)redisLastConValues[i] : 0;
                userLastConDic.TryAdd(userIndexList[i], tick);
            }


            var searchUsers = new List<FriendBase>();
            foreach(var user in userInfos)
            {
                searchUsers.Add(new FriendBase
                {
                    UserInfo = user,
                    LatestConnectDtTick = userLastConDic[user.UserSeq]
                });
            }

            await userCtx.SaveChangesAsync();

            return Ok(ResultCode.Success, new SearchFrinedsRes
            {
                SearchUsers = searchUsers
            });
        }

        [HttpPost("reward-recommend")]
        public async Task<ActionResult<RewardRecommendRes>> RewardRecommendAsync([FromBody] RewardRecommendReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<RewardRecommendRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            using var storeEventCtx = _dbRepo.GetStoreEventDb();
            var csvData = _csvContext.GetData();

            if (csvData.RecommendDicData.TryGetValue(req.RewardIndex, out var data) == false)
                return Ok<RewardRecommendRes>(ResultCode.NotFound);

            var recommendCount = await userCtx.UserRecommends.Where(p => p.RecommendUserSeq == userSeq).SumAsync(p => p.RecommendCount);
            if ( recommendCount < data.RecommendNum)
                return Ok<RewardRecommendRes>(ResultCode.InvalidParameter);

            var recommendRewardDb = await userCtx.UserRecommendRewards.Where(p => p.UserSeq == userSeq && p.RecommendIndex == req.RewardIndex).FirstOrDefaultAsync();
            if( recommendRewardDb != null )
                return Ok<RewardRecommendRes>(ResultCode.AlreadyRewardRecommend);


            var rewardResult = await _inventoryService.ObtainItemCurrencyAsync(userCtx, storeEventCtx, userSeq, $"reward_recommend", new List<ItemInfo> { data.RewardInfo});
            if (rewardResult.resultCode != ResultCode.Success)
                return Ok<RewardRecommendRes>(rewardResult.resultCode);


            await userCtx.UserRecommendRewards.AddAsync(new UserRecommendRewardModel
            {
                UserSeq = userSeq,
                RecommendIndex = req.RewardIndex,
            });

            await userCtx.SaveChangesAsync();
            await storeEventCtx.SaveChangesAsync();

            return Ok(ResultCode.Success, new RewardRecommendRes
            {
                RewardIndex = req.RewardIndex,
                RewardsInfo = rewardResult.obtainResult.RewardsInfo,
                RefreshInfo = rewardResult.obtainResult.RefreshInfo.ToRefreshInventoryInfo(),
            });
        }

       

        [HttpPost("request-friends-tab")]
        public async Task<ActionResult<RequestFriendsTabRes>> RequestFriendsTabAsync([FromBody] RequestFriendsTabReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<RequestFriendsTabRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            var csvData = _csvContext.GetData();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            var reqFriendsIndexList = await userCtx.UserReqestFriends.Where(p => p.UserSeq == userSeq).Select(p => p.RequestUserSeq).ToListAsync();

            var redisLastConValues = await redisUser.HashGetAsync(RedisKeys.hs_UserLastConnectTick, reqFriendsIndexList.Select(p => new RedisValue(p.ToString())).ToArray());
            var userLastConDic = new Dictionary<long, long>();

            for (int i = 0; i < reqFriendsIndexList.Count; i++)
            {
                var tick = redisLastConValues[i].HasValue ? (long)redisLastConValues[i] : 0;
                userLastConDic.TryAdd(reqFriendsIndexList[i], tick);
            }
            var userInfoList = await _playerService.GetUserInfosAsync(userCtx, reqFriendsIndexList);

            var reqFriendsList = new List<FriendBase>();
            foreach (var reqFriend in reqFriendsIndexList)
            {
                reqFriendsList.Add(new FriendBase
                {
                    UserInfo = userInfoList.Where(p => p.UserSeq == reqFriend).First(),
                    LatestConnectDtTick = userLastConDic[reqFriend],
                });
            }

            return Ok(ResultCode.Success, new RequestFriendsTabRes
            {
                RequestFriends = reqFriendsList,
            });
        }

        [HttpPost("friends-tab")]
        public async Task<ActionResult<FriendsTabRes>> FriendsTabAsync([FromBody] FriendsTabReq req)
        {
            if (_serverInspectionService.CheckInspectionError(GetIP()))
                return Ok<FriendsTabRes>(ResultCode.ServerInspection);

            var userSeq = GetUserSeq();
            using var userCtx = _dbRepo.GetUserDb();
            var csvData = _csvContext.GetData();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            var friendsDbs = await userCtx.UserFriends.Where(p => p.UserSeq == userSeq).ToListAsync();
            var friendsIndexList = friendsDbs.Select(p => p.TargetSeq).ToList();        

            var redisLastConValues = await redisUser.HashGetAsync(RedisKeys.hs_UserLastConnectTick, friendsIndexList.Select(p => new RedisValue(p.ToString())).ToArray());
            var userLastConDic = new Dictionary<long, long>();

            for (int i = 0; i < friendsIndexList.Count; i++)
            {
                var tick = redisLastConValues[i].HasValue ? (long)redisLastConValues[i] : 0;
                userLastConDic.TryAdd(friendsIndexList[i], tick);
            }
            var userInfoList = await _playerService.GetUserInfosAsync(userCtx, friendsIndexList);

            var friendsList = new List<FriendInfo>();
            foreach (var friend in friendsDbs)
            {
                friendsList.Add(new FriendInfo
                {
                    UserInfo = userInfoList.Where(p => p.UserSeq == friend.TargetSeq).First(),
                    LatestConnectDtTick = userLastConDic[friend.TargetSeq],
                    CoolTimeDtTick = friend.CoolTimeDt.Ticks
                });
            }


            return Ok(ResultCode.Success, new FriendsTabRes
            {
                Friends = friendsList
            });
        }
    }
}


