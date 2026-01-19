using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using Shared.Entities.Models;
using Shared.Repository;
using Shared.Repository.Services;
using Shared.Clock;
using Shared.Server.Define;
using Shared.Server.Extensions;
using Shared.ServerApp.Config;
using Shared.ServerApp.Common.Tasks;
using Shared.ServerApp.Services;
using Shared.CsvParser.Common;
using Shared.CsvData;
using Shared.ServerModels.Common;
using Shared.Repository.Database;
using Shared.Server.Packet.Internal;
using static Shared.Session.Extensions.ReplyExtensions;
using StackExchange.Redis;
using WebTool.Base.DataTables;
using WebTool.Base.UserInfoDetail;
using WebTool.Extensions;
using WebTool.Database;
using TwelveMoments.Shared.Extensions;

using WebTool.Connection.Services;
using Shared.PacketModel;
using Shared.Packet.Models;
using Shared.Repository.Extensions;
using System.Reflection;
using TwelveMoments.Shared.Common;
using Shared;
using Common.Config;
using Shared.Services.Redis;
using Elastic.Apm.Api;

namespace WebTool.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DatabaseRepositoryService _dbRepo;
        private readonly IDatabaseAsync _sessionRedis;
        private readonly IDatabaseAsync _accountRedis;
        private readonly IDatabaseAsync _userRedis;
        private readonly CsvStoreContext _csvStoreContext;
        private readonly WebToolCtx _webToolCtx;
        private readonly ServerSessionService _serverSessionService;
        private readonly SequenceService _sequenceService;
        private readonly ChangeableSettings<GameRuleSettings> _gameRule;

        public UserController(
            DatabaseRepositoryService dbRepo,
            RedisRepositoryService redisRepo,
            CsvStoreContext csvStore,
            WebToolCtx webtoolCtx,
            ServerSessionService serverSessionService,
            ChangeableSettings<GameRuleSettings> gameRule,
            SequenceService sequenceService)
        {
            _dbRepo = dbRepo;
            _sessionRedis = redisRepo.GetDb(RedisDatabase.Session);
            _accountRedis = redisRepo.GetDb(RedisDatabase.Account);
            _userRedis = redisRepo.GetDb(RedisDatabase.User);
            _csvStoreContext = csvStore;
            _webToolCtx = webtoolCtx;
            _gameRule = gameRule;
            _serverSessionService = serverSessionService;
            _sequenceService = sequenceService;
        }

        public static DataTablesColumnInfo[] UserTableDataColumnInfos =
       {
            new DataTablesColumnInfo("ID", "UserSeq"),
            new DataTablesColumnInfo("닉네임", "Nick"),
            new DataTablesColumnInfo("계정타입", ""),
            new DataTablesColumnInfo("유저상태", ""),
            new DataTablesColumnInfo("마지막 접속시간", ""),
            new DataTablesColumnInfo("가입일", "RegDt"),
            new DataTablesColumnInfo("더보기", ""),
        };

        public static DataTablesColumnInfo[] SearchUserBlockInfo =
        {
            new DataTablesColumnInfo("ID", ""),
            new DataTablesColumnInfo("닉네임", ""),
            new DataTablesColumnInfo("계정 상태", ""),
            new DataTablesColumnInfo("블락 일시", ""),
            new DataTablesColumnInfo("블락 해제 일시", "")
        };

        public static DataTablesColumnInfo[] UserBlockInfo =
        {
            new DataTablesColumnInfo("ID", ""),
            new DataTablesColumnInfo("닉네임", ""),
            new DataTablesColumnInfo("블락 일시", ""),
            new DataTablesColumnInfo("블락 해제 일시", ""),
            new DataTablesColumnInfo("관리자", ""),
            new DataTablesColumnInfo("사유", ""),
            new DataTablesColumnInfo("처리 일시", ""),
        };

        public static DataTablesColumnInfo[] LoadReportInfo =
        {
            new DataTablesColumnInfo("신고자 ID", ""),
            new DataTablesColumnInfo("신고자 닉네임", ""),
            new DataTablesColumnInfo("피신고자 ID", ""),
            new DataTablesColumnInfo("피신고자 닉네임", ""),
            new DataTablesColumnInfo("신고 사유", ""),
            new DataTablesColumnInfo("이미지", ""),
            new DataTablesColumnInfo("신고한 메시지", ""),
            new DataTablesColumnInfo("신고 일자", ""),
            new DataTablesColumnInfo("처리 확인", ""),
        };


        [HttpPost("user-table-data")]
        public async Task<IActionResult> UserTableDataAsync()
        {
            var dataTablesInput = new DataTablesInput(Request.Form, UserTableDataColumnInfos);
            var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);
            decimal compareNumbericValue = 0;
            string compareTextValue = "";

            using var userCtx = _dbRepo.GetUserDb();

            var userInfos = await userCtx.UserInfos.ToListAsync();

            dataTablesInput.ApplyCount(dataTablesOutput, userInfos);
            dataTablesInput.ApplyFilter(dataTablesOutput, userInfos, searchValue =>
            {
                compareTextValue = searchValue;
                if (decimal.TryParse(searchValue, out compareNumbericValue))
                {

                    userInfos = userInfos.Where(m =>
                    m.UserSeq == compareNumbericValue || m.Nick.Equals(compareTextValue)).ToList();
                }
                else
                {
                    userInfos = userInfos.Where(m => m.Nick.Equals(compareTextValue)).ToList();
                }
            });

            var userInfoQuery = userInfos.AsQueryable();
            userInfoQuery = dataTablesInput.ApplyOrder(userInfoQuery);
            userInfoQuery = dataTablesInput.ApplyLimit(userInfoQuery);

           
            var accounts = await userCtx.UserAccounts.ToListAsync();

            dataTablesOutput.data = new string[userInfoQuery.Count()][];
            for (var i = 0; i < userInfoQuery.Count(); i++)
            {
                var avatarItem = userInfoQuery.ElementAt(i);
                var accountItem = accounts.Where(p => p.UserSeq == avatarItem.UserSeq).FirstOrDefault();

                if (accountItem == null)
                {
                    dataTablesOutput.data[i] = new[]
                    {
                        avatarItem.UserSeq.ToString(),
                        avatarItem.Nick,
                        "잘못된 정보 보유 유저",
                        "-",
                        $"-",
                        "-",
                        "-",
                        "-",
                    };
                    continue;
                }

                var (sessionLocation, logoutDt) = await UserTaskProcessor.GetConnectInfoAsync(_sessionRedis,
                    _accountRedis, avatarItem.UserSeq,
                    accountItem.LogOutDt.Ticks);

                dataTablesOutput.data[i] = new[]
                {
                    avatarItem.UserSeq.ToString(),
                    avatarItem.Nick,
                    accountItem.AccountType.ToString(),                    
                    $"-",                    
                    accountItem.LoginDt.GetNameLanguageKey(),
                    avatarItem.RegDt.GetNameLanguageKey(),
                    "",
                };
            }

            return Ok(dataTablesOutput);
        }


        public static DataTablesColumnInfo[] SimpleUserTableDataColumnInfos =
        {
            new DataTablesColumnInfo("ID", "UserSeq"),
            new DataTablesColumnInfo("닉네임", "Nick"),
            new DataTablesColumnInfo("선택", ""),
        };

        [HttpPost("simple-user-table-data")]
        public async Task<IActionResult> SimpleUserTableDataAsync()
        {
            var dataTablesInput = new DataTablesInput(Request.Form, SimpleUserTableDataColumnInfos);
            var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);

            var searchType = Request.Form.GetFormValue<string>("searchType");

            using var userCtx = _dbRepo.GetUserDb();

            

            var list = await userCtx.UserInfos.ToListAsync(); 
            

            dataTablesInput.ApplyFilter(dataTablesOutput, list, searchValue =>
            {
                if (searchType.Equals("id"))
                {
                    if (decimal.TryParse(searchValue, out var targetUserSeq))
                    {
                        list = list.Where(p => p.UserSeq == targetUserSeq).ToList();
                    }
                }
                else
                {
//                    list = await userCtx.UserInfos.Where(p => p.Nick.Equals(searchValue)).ToListAsync();
                    list = list.Where(p => p.Nick.Contains(searchValue)).ToList();
                }
            });

            dataTablesOutput.data = list.Select(p => new[]
            {
                    p.UserSeq.ToString(),
                    p.Nick,
                    "",
                }).ToArray();

            return Ok(dataTablesOutput);
        }

        //public static DataTablesColumnInfo[] AccountTableDataColumnInfos =
        //{
        //    new DataTablesColumnInfo("Provider"),
        //    new DataTablesColumnInfo("Id"),
        //};

        //[HttpPost("account-table-data")]
        //public async Task<IActionResult> AccountTableDataAsync()
        //{
        //    var dataTablesInput = new DataTablesInput(Request.Form, AccountTableDataColumnInfos);
        //    var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);

        //    if (!Request.Form.TryGetValue("userSeq", out var userSeqText))
        //    {
        //        return BadRequest("올바르지 않은 유저 정보입니다.");
        //    }
        //    if (!long.TryParse(userSeqText, out var userSeq))
        //    {
        //        return BadRequest("올바르지 않은 유저 정보입니다.");
        //    }

        //    using var accountCtx = _dbRepo.GetAccountDb();
        //    var providers = accountCtx.AccountProviders.Where(p => p.UserSeq == userSeq);

        //    await dataTablesInput.ApplyCountAsync(dataTablesOutput, providers);

        //    providers = dataTablesInput.ApplyOrder(providers);
        //    providers = dataTablesInput.ApplyLimit(providers);

        //    dataTablesOutput.data = await providers.Select(p =>
        //        new[]
        //        {
        //            $"{((Provider) p.Provider).GetNameLanguageKey()} ({p.Provider})",
        //            p.Id,
        //        }).ToArrayAsync();

        //    return Ok(dataTablesOutput);
        //}

        public static DataTablesColumnInfo[] FriendTableDataColumnInfos =
        {
            new DataTablesColumnInfo("UserSeq", "TargetSeq"),
            new DataTablesColumnInfo("닉네임"),
            new DataTablesColumnInfo("친구 맺은 시간", "RegDt"),
            new DataTablesColumnInfo("더보기"),
        };

        [HttpPost("friend-table-data")]
        public async Task<IActionResult> FriendTableDataAsync()
        {
            var dataTablesInput = new DataTablesInput(Request.Form, FriendTableDataColumnInfos);
            var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);

            if (!Request.Form.TryGetValue("userSeq", out var userSeqText))
            {
                return BadRequest("올바르지 않은 유저 정보입니다.");
            }
            if (!long.TryParse(userSeqText, out var userSeq))
            {
                return BadRequest("올바르지 않은 유저 정보입니다.");
            }

            using var userCtx = _dbRepo.GetUserDb();

            dataTablesOutput.data = await userCtx.UserFriends.Where(p => p.UserSeq == userSeq)
                .Join(userCtx.UserInfos, p => p.TargetSeq, p => p.UserSeq, (userFriend, friendInfo) => new { userFriend, friendInfo })
                .Select(p => new[]
                {
                    p.friendInfo.UserSeq.ToString(),
                    p.friendInfo.Nick,
                    p.userFriend.RegDt.GetNameLanguageKey(),
                    ""
                }).ToArrayAsync();

            return Ok(dataTablesOutput);
        }

        public static DataTablesColumnInfo[] FriendRequestTableDataColumnInfos =
        {
            new DataTablesColumnInfo("요청한 UserSeq", "friend.Seq"),
            new DataTablesColumnInfo("닉네임", "avatar.Nick"),
            new DataTablesColumnInfo("요청한 시간", "friend.RegDt"),
            new DataTablesColumnInfo("더보기"),
        };

        [HttpPost("friend-request-table-data")]
        public async Task<IActionResult> FriendRequestTableDataAsync()
        {
            var dataTablesInput = new DataTablesInput(Request.Form, FriendRequestTableDataColumnInfos);
            var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);

            if (!Request.Form.TryGetValue("userSeq", out var userSeqText))
            {
                return BadRequest("올바르지 않은 유저 정보입니다.");
            }
            if (!long.TryParse(userSeqText, out var userSeq))
            {
                return BadRequest("올바르지 않은 유저 정보입니다.");
            }

            using var userCtx = _dbRepo.GetUserDb();
            dataTablesOutput.data = await userCtx.UserReqestFriends.Where(p => p.UserSeq == userSeq)
                .Join(userCtx.UserInfos, p => p.RequestUserSeq, p => p.UserSeq, (requestFriend, friendInfo) => new { requestFriend, friendInfo })
                .Select(p => new[]
                {
                    p.friendInfo.UserSeq.ToString(),
                    p.friendInfo.Nick,
                    p.requestFriend.RegDt.GetNameLanguageKey(),
                    p.friendInfo.UserSeq.ToString()
                }).ToArrayAsync();


            return Ok(dataTablesOutput);
        }

        public static DataTablesColumnInfo[] FriendResponseTableDataColumnInfos =
        {
            new DataTablesColumnInfo("요청받은 UserSeq", "friend.Seq"),
            new DataTablesColumnInfo("닉네임", "avatar.Nick"),
            new DataTablesColumnInfo("요청받은 시간", "friend.RegDt"),
            new DataTablesColumnInfo("더보기"),
        };

        [HttpPost("friend-response-table-data")]
        public async Task<IActionResult> FriendResponseTableDataAsync()
        {
            var dataTablesInput = new DataTablesInput(Request.Form, FriendResponseTableDataColumnInfos);
            var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);

            if (!Request.Form.TryGetValue("userSeq", out var userSeqText))
            {
                return BadRequest("올바르지 않은 유저 정보입니다.");
            }
            if (!long.TryParse(userSeqText, out var userSeq))
            {
                return BadRequest("올바르지 않은 유저 정보입니다.");
            }

            using var userCtx = _dbRepo.GetUserDb();
            dataTablesOutput.data = await userCtx.UserResponseFriends.Where(p => p.UserSeq == userSeq)
                .Join(userCtx.UserInfos, p => p.ResponseUserSeq, p => p.UserSeq, (responseFriend, friendInfo) => new { responseFriend, friendInfo })
                .Select(p => (new[]
                {
                    p.friendInfo.UserSeq.ToString(),
                    p.friendInfo.Nick,
                    p.responseFriend.RegDt.GetNameLanguageKey(),
                    p.friendInfo.UserSeq.ToString()
                })).ToArrayAsync();


            return Ok(dataTablesOutput);
        }

        [HttpPost("edit-value")]
        public async Task<IActionResult> EditValueAsync()
        {
            var userSeq = long.Parse(Request.Form["userSeq"].FirstOrDefault() ?? "0");
            var editInfoType = (PlayerDetailEditInfoType)Enum.Parse(typeof(PlayerDetailEditInfoType),
                Request.Form["editType"].FirstOrDefault());
            var className = Request.Form["className"].FirstOrDefault();
            var propertyName = Request.Form["propertyName"].FirstOrDefault();
            var value = Request.Form["value"].FirstOrDefault();

            using var userCtx = _dbRepo.GetUserDb();
            var csvData = _csvStoreContext.GetData();

            string beforeForLog = "";
            string afterForLog = "";

            var classProperty = typeof(PlayerDetailInfo).GetProperty(className);
            var targetProperty = classProperty.PropertyType.GetProperty(propertyName);
//            var targetProperty = GetPropertyByValue(classProperty, propertyName, value);

            if (targetProperty == typeof(UserInfoModel).GetProperty(nameof(UserInfoModel.Nick)))
            {
                if( await userCtx.UserInfos.AnyAsync(p => p.Nick == value) )
                    return BadRequest($"Check nick failed: {value}");

                var userInfoDb = await userCtx.UserInfos.FindAsync(userSeq);
                userInfoDb.Nick = value;
                userCtx.UserInfos.Update(userInfoDb);
                await userCtx.SaveChangesAsync();

            }
            else
            {
                if (editInfoType == PlayerDetailEditInfoType.UserDb)
                {
                    try
                    {
                        var data = await userCtx.FindAsync(classProperty.PropertyType, userSeq);
                        beforeForLog = data.ToString();
                        var convertValue = Convert.ChangeType(value, targetProperty.PropertyType);
                        targetProperty.SetValue(data, convertValue);
                        afterForLog = value;
                        userCtx.Update(data);
                        await userCtx.SaveChangesAsync();
                    }
                    catch
                    {
                        return BadRequest("올바르지 않은 수정값입니다.");
                    }

                }
            }


            return Ok();
        }


        public static DataTablesColumnInfo[] MyItemDetailTableDataColumnInfos =
        {
            new DataTablesColumnInfo("RewardPaymentType", ""),
            new DataTablesColumnInfo("Index", ""),
            new DataTablesColumnInfo("개수", ""),
        };

        [HttpPost("item-detail-table-data")]
        public async Task<IActionResult> MyDetailItemTableDataAsync()
        {
            var dataTablesInput = new DataTablesInput(Request.Form, MyItemDetailTableDataColumnInfos);
            var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);

            decimal userSeq = 0;
            if (!decimal.TryParse(Request.Form["UserSeq"].FirstOrDefault(), out userSeq))
            {
                return NotFound("Failed to convert usersequence");
            }

            int type = 0;
            if (!int.TryParse(Request.Form["Type"].FirstOrDefault(), out type))
            {
                return NotFound("Failed to convert type");
            }

            var items = new List<List<UserGameItemOwnModel>>();
            for (var i = 0; i <= (int)DetailMyItemType.SpecialBuff; i++)
            {
                items.Add(new List<UserGameItemOwnModel>());
            }
            using var userCtx = _dbRepo.GetUserDb();
            using var allUserCtx = _dbRepo.GetAllDb<UserCtx>();

            items[(int)DetailMyItemType.Total].AddRange(await userCtx.UserItems.Where(p => p.UserSeq == userSeq).ToListAsync());



            switch (type)
            {
                case (int)DetailMyItemType.Material:
                    items[(int)DetailMyItemType.Material].AddRange(items[(int)DetailMyItemType.Total].Where(p => p.ObtainType == RewardPaymentType.Material).ToList());
                    break;
                case (int)DetailMyItemType.MarketingLeaflet:
                    items[(int)DetailMyItemType.MarketingLeaflet].AddRange(items[(int)DetailMyItemType.Total].Where(p => p.ObtainType == RewardPaymentType.MarketingLeaflet).ToList());
                    break;
                case (int)DetailMyItemType.SpecialBuff:
                    items[(int)DetailMyItemType.SpecialBuff].AddRange(items[(int)DetailMyItemType.Total].Where(p => (p.ObtainType == RewardPaymentType.SpecialBuff)).ToList());
                    break;
                case (int)DetailMyItemType.Total:
                    break;
                default:
                    return NotFound("Insert wrong type value");
            }

            dataTablesOutput.data = items[type].Select(p => new[]
            {
                p.ObtainType.ToString(),
                p.ItemId.ToString(),
                p.ItemQty.ToString(),
            }).ToArray();

            return Ok(dataTablesOutput);
        }

        //[Route("ExportItemCSVData")]
        //public async Task<IActionResult> ExportItemCSVData(long userSeq, int type)
        //{
        //    var items = new List<List<ItemOwn>>();
        //    for (var i = 0; i <= (int)DetailMyItemType.Coupon; i++)
        //    {
        //        items.Add(new List<ItemOwn>());
        //    }

        //    using var allUserCtx = _dbRepo.GetAllDb<UserCtx>();
        //    for (var i = 0; i < allUserCtx.Length; i++)
        //    {
        //        var userCtx = allUserCtx[i];

        //        items[(int)DetailMyItemType.Total].AddRange(await userCtx.ItemOwns.Where(p => p.UserSeq == userSeq).ToListAsync());
        //    }

        //    switch (type)
        //    {
        //        case (int)DetailMyItemType.Buff:
        //            items[(int)DetailMyItemType.Buff].AddRange(items[(int)DetailMyItemType.Total].Where(p => p.ObtainType == (byte)ObtainType.Item_Global_Buff).ToList());
        //            break;
        //        case (int)DetailMyItemType.Normal:
        //            items[(int)DetailMyItemType.Normal].AddRange(items[(int)DetailMyItemType.Total].Where(p => (p.ObtainType == (byte)ObtainType.Item_Global_Functional) ||
        //                                              (p.ObtainType == (byte)ObtainType.Item_ShoppingMall_Functional) ||
        //                                              (p.ObtainType == (byte)ObtainType.Item_ShowDancer_Functional) ||
        //                                              (p.ObtainType == (byte)ObtainType.Item_Couple_Functional) ||
        //                                              (p.ObtainType == (byte)ObtainType.Item_Event_Box) ||
        //                                              (p.ObtainType == (byte)ObtainType.Item_Costume_Deco)).ToList());
        //            break;
        //        case (int)DetailMyItemType.Coupon:
        //            items[(int)DetailMyItemType.Coupon].AddRange(items[(int)DetailMyItemType.Total].Where(p => (p.ObtainType == (byte)ObtainType.Item_ShoppingMall_Coupon)).ToList());
        //            break;
        //        case (int)DetailMyItemType.Total:
        //            break;
        //        default:
        //            return NotFound("Insert wrong type value");
        //    }

        //    var datas = items[type].Select(p => new[]
        //    {
        //        GetItemQty(p),
        //    }).ToArray();

        //    try
        //    {
        //        var fileName = GetFileName(type);

        //        var fullPath = await CSVBasicWriter.WriteAndSave(fileName, datas);

        //        return File(new FileStream(fullPath, FileMode.Open), "text/csv", $"{userSeq}-{fileName}");
        //    }
        //    catch (Exception)
        //    {
        //        return BadRequest("Failed to download file");
        //    }
        //}




        public static DataTablesColumnInfo[] MyTotalPartsDetailInfo =
        {
            new("Index", ""),
            new("FileName", ""),
            new("레벨", ""),
            new("등급", ""),
            new("개수", ""),
            new("재고량", ""),
            new("획득일자", ""),
        };
        


        [HttpPost("parts-total-table-data")]
        public async Task<IActionResult> CostumeTotalDetailInfo()
        {
            var csvData = _csvStoreContext.GetData();
            var dataTablesInput = new DataTablesInput(Request.Form, MyTotalPartsDetailInfo);
            var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);

            decimal userSeq = 0;
            if (!decimal.TryParse(Request.Form["UserSeq"].FirstOrDefault(), out userSeq))
            {
                return NotFound("Failed to convert usersequence");
            }

            using var userCtx = _dbRepo.GetUserDb();
            var partsDb = await userCtx.UserParts.Where(p => p.UserSeq == userSeq).ToListAsync();
            string[][] arr = new string[partsDb.Count][];
            var i = 0;
            foreach (var parts in partsDb)
            {
                if (csvData.PartsDicData.TryGetValue(parts.PartsIndex, out var data) == false)
                    continue;

                arr[i] = new[]
                {
                    parts.PartsIndex.ToString(),
                    data.PartsId,
                    parts.PartsLevel.ToString(),
                    parts.PartsGrade.ToString(),
                    parts.PartsQty.ToString(),
                    parts.SellQty.ToString(),
                    parts.RegDt.ToString()
                };
                i++;
            }
            dataTablesOutput.data = arr;

            return Ok(dataTablesOutput);
        }

        [Route("ExportTotalPartsData")]
        public async Task<IActionResult> ExportTotalPartsData(long userSeq)
        {
            var csvData = _csvStoreContext.GetData();
            using var userCtx = _dbRepo.GetUserDb();
            var partsDb = await userCtx.UserParts.Where(p => p.UserSeq == userSeq).ToListAsync();

            string[][] arr = new string[partsDb.Count][];
            var i = 0;
            foreach (var parts in partsDb) 
            {
                if (csvData.PartsDicData.TryGetValue(parts.PartsIndex, out var data) == false)
                    continue;

                arr[i] = new[]
                {
                    parts.PartsIndex.ToString(),
                    data.PartsId,
                    parts.PartsLevel.ToString(),
                    parts.PartsGrade.ToString(),
                    parts.PartsQty.ToString(),
                    parts.SellQty.ToString(),
                    parts.RegDt.ToString()
                };
                i++;
            }
            


            //var recordDatas = partsDb.Select(p => new[]
            //{
            //    p.PartsIndex.ToString(),
            //    p.PartsLevel.ToString(),
            //    p.PartsGrade.ToString(),
            //    p.PartsQty.ToString(),
            //    p.SellQty.ToString(),   
            //    p.RegDt.ToString(),
            //}).ToArray();

            try
            {
                var fileName = "totalparts.csv";

                var fullPath = await CSVBasicWriter.WriteAndSave(fileName, arr);

                return File(new FileStream(fullPath, FileMode.Open), "text/csv", $"{userSeq}-{fileName}");
            }
            catch (Exception)
            {
                return BadRequest("Failed to download file");
            }
        }

        public static DataTablesColumnInfo[] CustomPartsInfo =
        {
            new("Index", ""),
            new("Color", ""),
        };

        [HttpPost("profile-parts-table-data")]
        public async Task<IActionResult> ProfilePartsInfo()
        {
            var csvData = _csvStoreContext.GetData();
            var dataTablesInput = new DataTablesInput(Request.Form, CustomPartsInfo);
            var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);

            decimal userSeq = 0;
            if (!decimal.TryParse(Request.Form["UserSeq"].FirstOrDefault(), out userSeq))
            {
                return NotFound("Failed to convert usersequence");
            }

            using var userCtx = _dbRepo.GetUserDb();
            var userInfoDb = await userCtx.UserInfos.FindAsync((long)userSeq);


            var profileParts = string.IsNullOrEmpty(userInfoDb.ProfileParts) ? new List<PartsBase>() : JsonTextSerializer.Deserialize<List<PartsBase>>(userInfoDb.ProfileParts);
            
            dataTablesOutput.data = profileParts.Select(p => new[]
            {
                p.Index.ToString(),
                p.ColorCode,
            }).ToArray();

            return Ok(dataTablesOutput);
        }

        //[HttpPost("mailLog-userInfo")]
        //public async Task<IActionResult> GetUserInfoInMailLog()
        //{
        //    long sequence = 0;
        //    bool isDistinguishSeq = false;
        //    string id = "";
        //    if (!Request.Form.TryGetValue("UserID", out var userID))
        //    {
        //        return BadRequest("올바르지 않은 아이디 입니다.");
        //    }

        //    if (!Request.Form.TryGetValue("UserType", out var userIDType))
        //    {
        //        return BadRequest("올바르지 않은 아이디 구분값입니다.");
        //    }

        //    if (userIDType.Equals("ID"))
        //    {
        //        isDistinguishSeq = true;
        //        if (!long.TryParse(userID, out sequence))
        //        {
        //            return BadRequest("올바르지 않은 아이디 입니다.");
        //        }
        //    } 
        //    else if (userIDType.Equals("NICK"))
        //    {
        //        id = userID;
        //    }
        //    else
        //    {
        //        return BadRequest("올바르지 않은 아이디 구분값입니다.");
        //    }

        //    using var allUserCtx = _dbRepo.GetAllDb<UserCtx>();
        //    CurrentAvatar userData = null;
        //    for (var i = 0; i < allUserCtx.Length; i++)
        //    {
        //        var userCtx = allUserCtx[i];

        //        if (isDistinguishSeq)
        //        {
        //            userData = await userCtx.UserAvatars.FindAsync(sequence);
        //        }
        //        else
        //        {
        //            userData = await userCtx.UserAvatars.Where(p => p.Nick.Equals(id)).FirstAsync();
        //        }

        //        if (userData != null) break;
        //    }

        //    if (userData == null)
        //    {
        //        return BadRequest("유저 정보를 다시 확인해주세요.");
        //    }

        //    string gender = ByteToGender(userData.Gender);

        //    string result = $"{userData.UserSeq},{userData.Nick},{gender}";

        //    return Ok(result);
        //}

        //public DataTablesColumnInfo[] MailLogInfoTableData = 
        //{
        //    new DataTablesColumnInfo("category"),
        //    new DataTablesColumnInfo("seq"),
        //    new DataTablesColumnInfo("fam"),
        //    new DataTablesColumnInfo("senduser"),
        //    new DataTablesColumnInfo("receiveuser"),
        //    new DataTablesColumnInfo("content"),
        //    new DataTablesColumnInfo("senddate"),
        //    new DataTablesColumnInfo("recvdate"),
        //    new DataTablesColumnInfo("readdate"),
        //    new DataTablesColumnInfo("removedate"),
        //    new DataTablesColumnInfo("reward"),
        //};

        //public static DataTablesColumnInfo[] UserBlockInfo =
        //{
        //    new DataTablesColumnInfo("ID", ""),
        //    new DataTablesColumnInfo("닉네임", ""),
        //    new DataTablesColumnInfo("블락 일시", ""),
        //    new DataTablesColumnInfo("블락 해제 일시", ""),
        //    new DataTablesColumnInfo("관리자", ""),
        //    new DataTablesColumnInfo("사유", ""),
        //    new DataTablesColumnInfo("처리 일시", ""),
        //};

        //[HttpPost("lookup-block-user")]
        //public async Task<IActionResult> GetUserBlockLog()
        //{
        //    var dataTablesInput = new DataTablesInput(Request.Form, UserBlockInfo);
        //    var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);

        //    if (!Request.Form.TryGetValue("From", out var fromText))
        //    {
        //        return BadRequest("올바르지 않은 시작일 입니다.");
        //    }

        //    if (!DateTime.TryParse(fromText, out var from_dt))
        //    {
        //        return BadRequest("올바르지 않은 시작일 입니다.");
        //    }

        //    if (!Request.Form.TryGetValue("To", out var toText))
        //    {
        //        return BadRequest("올바르지 않은 종료일 입니다.");
        //    }

        //    if (!DateTime.TryParse(toText, out var to_dt))
        //    {
        //        return BadRequest("올바르지 않은 종료일 입니다.");
        //    }

        //    if (from_dt.CompareTo(to_dt) >= 0)
        //    {
        //        return BadRequest("시작일이 종료일보다 이후로 설정되어 있습니다.");
        //    }

        //    using var accountCtx = _dbRepo.GetAccountDb();
        //    using var allUserCtx = _dbRepo.GetAllDb<UserCtx>();
        //    List<CurrentAvatar> allUserData = new List<CurrentAvatar>();
        //    for (var i = 0; i < allUserCtx.Length; i++)
        //    {
        //        var userCtx = allUserCtx[i];
        //        allUserData.AddRange(await userCtx.UserAvatars.ToListAsync());
        //    }

        //    var allLogs = await accountCtx.AccountGmBlocks.Where(p => ((p.FrDt >= from_dt) && (p.FrDt <= to_dt)) ||
        //                                                                ((p.ToDt >= from_dt) && (p.ToDt <= to_dt)) ||
        //                                                                ((from_dt >= p.FrDt) && (from_dt <= p.ToDt)) ||
        //                                                                ((to_dt >= p.FrDt) && (to_dt <= p.ToDt))).ToListAsync();

        //    //var allLogs = await accountCtx.AccountGmBlocks.Where(p => p.FrDt.CompareTo(from_dt) >= 0 || 
        //    //                                                p.ToDt.CompareTo(to_dt) <= 0).ToListAsync();

        //    dataTablesOutput.data = allLogs.OrderByDescending(p => p.RegDt).Select(p => new[]
        //    {
        //        p.UserSeq.ToString(),
        //        allUserData.Where(t => t.UserSeq == p.UserSeq).FirstOrDefault().Nick,
        //        p.FrDt.GetNameLanguageKey(),
        //        p.ToDt.GetNameLanguageKey(),
        //        p.BlockGmId,
        //        p.BlockReason,
        //        p.RegDt.GetNameLanguageKey()
        //    }).ToArray();

        //    return Ok(dataTablesOutput);
        //}

        //[Route("ExportUserBlockLogToCSV")]
        //public async Task<IActionResult> ExportUserBlockLogToCSV(string From, string To)
        //{
        //    if (!DateTime.TryParse(From, out var from_dt))
        //    {
        //        return BadRequest("Failed to convert From date");
        //    }

        //    if (!DateTime.TryParse(To, out var to_dt))
        //    {
        //        return BadRequest("Failed to convert To date");
        //    }

        //    if (from_dt.CompareTo(to_dt) >= 0)
        //    {
        //        return BadRequest("시작일이 종료일보다 이후로 설정되어 있습니다.");
        //    }

        //    using var accountCtx = _dbRepo.GetAccountDb();
        //    using var allUserCtx = _dbRepo.GetAllDb<UserCtx>();
        //    List<CurrentAvatar> allUserData = new List<CurrentAvatar>();
        //    for (var i = 0; i < allUserCtx.Length; i++)
        //    {
        //        var userCtx = allUserCtx[i];
        //        allUserData.AddRange(await userCtx.UserAvatars.ToListAsync());
        //    }

        //    var allLogs = await accountCtx.AccountGmBlocks.Where(p => ((p.FrDt >= from_dt) && (p.FrDt <= to_dt)) ||
        //                                                                ((p.ToDt >= from_dt) && (p.ToDt <= to_dt)) ||
        //                                                                ((from_dt >= p.FrDt) && (from_dt <= p.ToDt)) ||
        //                                                                ((to_dt >= p.FrDt) && (to_dt <= p.ToDt))).ToListAsync();

        //    var datas = allLogs.OrderByDescending(p => p.RegDt).Select(p => new[]
        //    {
        //        $"{p.UserSeq.ToString()}\t",
        //        allUserData.Where(t => t.UserSeq == p.UserSeq).FirstOrDefault().Nick,
        //        p.FrDt.ToString(),
        //        p.ToDt.ToString(),
        //        p.BlockGmId,
        //        p.BlockReason,
        //        p.RegDt.ToString()
        //    }).ToArray();

        //    try
        //    {
        //        var fileName = "BlockLog.csv";
        //        var fullPath = await CSVBasicWriter.WriteAndSave(fileName, datas);
        //        return File(new FileStream(fullPath, FileMode.Open), "text/csv", $"{from_dt.ToString("d")}-{to_dt.ToString("d")}BlockLog.csv");
        //    }
        //    catch (Exception)
        //    {
        //        return BadRequest("Failed to download file");
        //    }
        //}

        //public static DataTablesColumnInfo[] SearchUserBlockInfo =
        //{
        //    new DataTablesColumnInfo("ID", ""),
        //    new DataTablesColumnInfo("닉네임", ""),
        //    new DataTablesColumnInfo("계정 상태", ""),
        //    new DataTablesColumnInfo("블락 일시", ""),
        //    new DataTablesColumnInfo("블락 해제 일시", "")
        //};

        //[HttpPost("search-userblock-log")]
        //public async Task<IActionResult> GetUserBlockTableData()
        //{
        //    var dataTablesInput = new DataTablesInput(Request.Form, SearchUserBlockInfo);
        //    var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);

        //    if (!Request.Form.TryGetValue("sendTargetText", out var sendTargetText))
        //    {
        //        return BadRequest("올바르지 않은 수신인입니다.");
        //    }

        //    if (string.IsNullOrEmpty(sendTargetText))
        //    {
        //        return BadRequest("수신인을 입력해주세요.");
        //    }

        //    var userTargetData = new UserTargetData
        //    {
        //        Type = UserTargetType.Selected
        //    };

        //    userTargetData.LoadUsers(sendTargetText);
        //    if (userTargetData.Users.Length == 0)
        //        return BadRequest("올바르지 않는 수신인입니다.");

        //    using var allUserCtx = _dbRepo.GetAllDb<UserCtx>();
        //    List<CurrentAvatar> allUserNick = new List<CurrentAvatar>();
        //    for (var i = 0; i < allUserCtx.Length; i++)
        //    {
        //        var userCtx = allUserCtx[i];
        //        allUserNick.AddRange(await userCtx.UserAvatars.ToListAsync());
        //    }

        //    //유저 수만큼 돌면서 뽑아온 블록 로그 중 최신 것만 넣어주자.
        //    using var accountCtx = _dbRepo.GetAccountDb();
        //    List<AccountGmBlock> recentlyRecords = new List<AccountGmBlock>();
        //    for (var i = 0; i < userTargetData.Users.Length; i++)
        //    {
        //        var userSeq = userTargetData.Users[i];
        //        var data = await accountCtx.AccountGmBlocks.Where(p => p.UserSeq == userSeq).OrderByDescending(p => p.RegDt).FirstOrDefaultAsync();
        //        if (data != null)
        //        {
        //            recentlyRecords.Add(data);
        //        }
        //    }

        //    dataTablesOutput.data = recentlyRecords.Select(p => new[]
        //    {
        //            p.UserSeq.ToString(),
        //            allUserNick?.Where(t => t.UserSeq == p.UserSeq).FirstOrDefault().Nick,
        //            IsUserBlock(p),
        //            p.FrDt.GetNameLanguageKey(),
        //            p.ToDt.GetNameLanguageKey()
        //        }).ToArray();

        //    return Ok(dataTablesOutput);
        //}

        //[HttpPost("block-extend")]
        //public async Task<IActionResult> DoExtendUserBlock()
        //{
        //    string targetText = Request.Form["targetText"].FirstOrDefault();

        //    if (!Request.Form.TryGetValue("selectDay", out var selectDayText))
        //    {
        //        return BadRequest("올바르지 않은 날짜지정입니다.");
        //    }

        //    if (!Request.Form.TryGetValue("blockreason", out var blockreason))
        //    {
        //        return BadRequest("올바르지 않은 사유입니다.");
        //    }

        //    if (string.IsNullOrEmpty(blockreason))
        //    {
        //        return BadRequest("사유를 입력해주세요.");
        //    }

        //    if (!double.TryParse(selectDayText, out var selectDay))
        //    {
        //        return BadRequest("올바르지 않은 날짜지정입니다.");
        //    }

        //    var userTargetData = new UserTargetData()
        //    {
        //        Type = UserTargetType.Selected
        //    };
        //    userTargetData.LoadUsers(targetText);
        //    if (userTargetData.Users.Length <= 0)
        //    {
        //        return BadRequest("올바르지 않은 수신인입니다.");
        //    }

        //    using var accountCtx = _dbRepo.GetAccountDb();
        //    foreach (var user in userTargetData.Users)
        //    {
        //        var data = await accountCtx.AccountGmBlocks.Where(p => p.UserSeq == user).OrderByDescending(p => p.RegDt).FirstOrDefaultAsync();

        //        if (data != null && (data.ToDt.CompareTo(DateTime.UtcNow) >= 0))
        //        {
        //            data.ToDt = (selectDay > 0.0 && !data.ToDt.IsInfinity()) ? data.ToDt.AddDays(selectDay) : AppClock.MaxValue;
        //            data.BlockGmId = User.Identity.Name;
        //            data.BlockReason = blockreason;
        //            accountCtx.AccountGmBlocks.Update(data);
        //        }
        //        else
        //        {
        //            var accounts = await accountCtx.AccountProviders.Where(p => p.UserSeq == user).FirstOrDefaultAsync();
        //            AccountGmBlock newData = new AccountGmBlock()
        //            {
        //                UserSeq = user,
        //                Provider = accounts.Provider,
        //                Id = accounts.Id,
        //                FrDt = DateTime.UtcNow,
        //                ToDt = (selectDay > 0.0) ? AppClock.UtcNow.AddDays(selectDay) : AppClock.MaxValue,
        //                BlockGmId = User.Identity.Name,
        //                BlockReason = blockreason,
        //            };

        //            await accountCtx.AccountGmBlocks.AddAsync(newData);
        //        }
        //    }

        //    await accountCtx.SaveChangesAsync();

        //    await _serverSessionService.SendAllAsync(
        //        NetServiceType.FrontEnd,
        //        MakeNtfReply(new InternalBlockUserKick()
        //        {
        //            TargetUsers = targetText,
        //        }));

        //    return Ok();
        //}

        //[HttpPost("block-release")]
        //public async Task<IActionResult> DoReleaseUserBlock()
        //{
        //    if (!Request.Form.TryGetValue("targetText", out var targetText))
        //    {
        //        return BadRequest("올바르지 않은 수신인입니다.");
        //    }

        //    if (!Request.Form.TryGetValue("unblockreason", out var reason))
        //    {
        //        return BadRequest("올바르지 않은 사유입니다.");
        //    }

        //    var userTargetData = new UserTargetData()
        //    {
        //        Type = UserTargetType.Selected
        //    };
        //    userTargetData.LoadUsers(targetText);
        //    if (userTargetData.Users.Length == 0)
        //    {
        //        return BadRequest("올바르지 않은 수신인입니다.");
        //    }

        //    using var accountCtx = _dbRepo.GetAccountDb();
        //    foreach (var user in userTargetData.Users)
        //    {
        //        var data = await accountCtx.AccountGmBlocks.Where(p => p.UserSeq == user).OrderByDescending(p => p.RegDt).FirstOrDefaultAsync();
        //        var now = AppClock.UtcNow;
        //        if (data != null && data.ToDt.CompareTo(now) > 0)
        //        {
        //            data.ToDt = now;
        //            data.UnBlockGmId = User.Identity.Name;
        //            data.UnblockReason = reason;
        //            accountCtx.AccountGmBlocks.Update(data);
        //        }
        //    }

        //    await accountCtx.SaveChangesAsync();

        //    return Ok();
        //}

        //public static DataTablesColumnInfo[] LoadPersonnalBlockHistory =
        //{
        //    new DataTablesColumnInfo("일시"),
        //    new DataTablesColumnInfo("사유"),
        //    new DataTablesColumnInfo("블락 기간"),
        //    new DataTablesColumnInfo("관리자"),
        //};

        //[HttpPost("lookup-block-personnal")]
        //public async Task<IActionResult> GetPersonnalBlockHistoryData()
        //{
        //    var dataTablesInput = new DataTablesInput(Request.Form, LoadPersonnalBlockHistory);
        //    var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);

        //    if (!Request.Form.TryGetValue("UserSeq", out var userSeqText))
        //    {
        //        return BadRequest("올바르지 않은 유저 정보입니다.");
        //    }

        //    if (!long.TryParse(userSeqText, out var userSeq))
        //    {
        //        return BadRequest("올바르지 않은 유저 정보입니다.");
        //    }

        //    using var allAccountCtx = _dbRepo.GetAllDb<AccountCtx>();
        //    List<AccountGmBlock> blockHistory = new List<AccountGmBlock>();

        //    for (var i = 0; i < allAccountCtx.Length; i++)
        //    {
        //        var accountCtx = allAccountCtx[i];

        //        var insertData = await accountCtx.AccountGmBlocks.Where(p => p.UserSeq == userSeq).ToListAsync();
        //        if (insertData != null && insertData.Count > 0)
        //        {
        //            blockHistory.AddRange(insertData);
        //        }
        //    }

        //    dataTablesOutput.data = blockHistory.OrderByDescending(p => p.RegDt).Select(p => new[]
        //    {
        //        p.RegDt.GetNameLanguageKey(),
        //        p.BlockReason,
        //        p.ToDt.GetNameLanguageKey(),
        //        p.BlockGmId
        //    }).ToArray();

        //    return Ok(dataTablesOutput);
        //}

        //public static DataTablesColumnInfo[] LoadFamMessageInfo =
        //{
        //    new DataTablesColumnInfo("sender"),
        //    new DataTablesColumnInfo("sender_grade"),
        //    new DataTablesColumnInfo("content"),
        //    new DataTablesColumnInfo("send_time"),
        //};

        ////TO DO
        //[HttpPost("load-fam-message")]
        //public IActionResult LoadFamMessage()
        //{
        //    var dataTablesInput = new DataTablesInput(Request.Form, LoadFamMessageInfo);
        //    var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);

        //    if (!Request.Form.TryGetValue("FamSeq", out var famSeqText))
        //    {
        //        return BadRequest("Failed to load Fam ID");
        //    }

        //    if (!long.TryParse(famSeqText, out var famSeq))
        //    {
        //        return BadRequest("Failed to load Fam ID");
        //    }

        //    return Ok(dataTablesOutput);
        //}

        //public static DataTablesColumnInfo[] LoadFamSeqInfo =
        //{
        //    new DataTablesColumnInfo("신청인 ID"),
        //    new DataTablesColumnInfo("닉네임"),
        //};

        //[HttpPost("select-famjoin-req")]
        //public async Task<IActionResult> LoadFamSeqData()
        //{
        //    var dataTablesInput = new DataTablesInput(Request.Form, LoadFamSeqInfo);
        //    var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);
        //    using var allUserCtx = _dbRepo.GetAllDb<UserCtx>();
        //    List<FamJoinRequest> joinReq = new List<FamJoinRequest>();
        //    List<CurrentAvatar> nickInfo = new List<CurrentAvatar>();

        //    if (!Request.Form.TryGetValue("famSeq", out var famSeqText))
        //    {
        //        return BadRequest("올바르지 않은 팸 정보입니다.");
        //    }
        //    if (!long.TryParse(famSeqText, out var famSeq))
        //    {
        //        return BadRequest("올바르지 않은 팸 정보입니다.");
        //    }

        //    for (var i = 0; i < allUserCtx.Length; i++)
        //    {
        //        var userCtx = allUserCtx[i];
        //        var datas = await userCtx.FamJoinRequests.Where(p => p.FamSeq == famSeq).ToListAsync();
        //        var nickDatas = await userCtx.UserAvatars.ToListAsync();

        //        if (datas != null && datas.Count > 0)
        //        {
        //            joinReq.AddRange(datas);
        //        }

        //        if (nickDatas != null && nickDatas.Count > 0)
        //        {
        //            nickInfo.AddRange(nickDatas);
        //        }
        //    }

        //    dataTablesOutput.data = joinReq.Select(p => new[]
        //    {
        //        p.UserSeq.ToString(),
        //        nickInfo.Where(t => t.UserSeq == p.UserSeq).FirstOrDefault().Nick,
        //    }).ToArray();

        //    return Ok(dataTablesOutput);
        //}

        //public static DataTablesColumnInfo[] LoadReportInfo =
        //{
        //    new DataTablesColumnInfo("신고자 ID", ""),
        //    new DataTablesColumnInfo("신고자 닉네임", ""),
        //    new DataTablesColumnInfo("피신고자 ID", ""),
        //    new DataTablesColumnInfo("피신고자 닉네임", ""),
        //    new DataTablesColumnInfo("신고 사유", ""),
        //    new DataTablesColumnInfo("이미지", ""),
        //    new DataTablesColumnInfo("신고한 메시지", ""),
        //    new DataTablesColumnInfo("신고 일자", ""),
        //    new DataTablesColumnInfo("처리 확인", ""),
        //};

        //[HttpPost("load-report-table")]
        //public async Task<IActionResult> GetReportListData()
        //{
        //    var dataTablesInput = new DataTablesInput(Request.Form, LoadReportInfo);
        //    var dataTablesOutput = new DataTablesOutput(dataTablesInput.draw);
        //    using var allUserCtx = _dbRepo.GetAllDb<UserCtx>();
        //    List<UserReport> allUserReport = new List<UserReport>();
        //    List<CurrentAvatar> allUserNick = new List<CurrentAvatar>();

        //    if (!Request.Form.TryGetValue("from_dt", out var fromDtText))
        //    {
        //        return BadRequest("올바른 날짜값이 아닙니다.");
        //    }

        //    if (!DateTime.TryParse(fromDtText, out var from_dt))
        //    {
        //        return BadRequest("올바른 날짜값이 아닙니다.");
        //    }

        //    if (!Request.Form.TryGetValue("to_dt", out var toDtText))
        //    {
        //        return BadRequest("올바른 날짜값이 아닙니다.");
        //    }

        //    if (!DateTime.TryParse(toDtText, out var to_dt))
        //    {
        //        return BadRequest("올바른 날짜값이 아닙니다.");
        //    }

        //    if (from_dt.CompareTo(to_dt) >= 0)
        //    {
        //        return BadRequest("시작일자가 마지막 일자보다 이후입니다.");
        //    }

        //    for (var i = 0; i < allUserCtx.Length; i++)
        //    {
        //        var userCtx = allUserCtx[i];
        //        var data = await userCtx.UserReports.ToListAsync();
        //        var nickData = await userCtx.UserAvatars.ToListAsync();
        //        if (data != null && data.Count > 0)
        //        {
        //            allUserReport.AddRange(data);
        //        }
        //        if (nickData != null && nickData.Count > 0)
        //        {
        //            allUserNick.AddRange(nickData);
        //        }
        //    }

        //    dataTablesOutput.data = allUserReport.Select(p => new[]
        //    {
        //        p.ReportId.ToString(),
        //        allUserNick.Where(t => t.UserSeq == p.UserSeq).FirstOrDefault().Nick,
        //        p.TargetSeq.ToString(),
        //        allUserNick.Where(t => t.UserSeq == p.TargetSeq).FirstOrDefault().Nick,
        //        ReportTypeToString(p.ReportType),
        //        null,
        //        p.Reason,
        //        p.CreateDt.GetNameLanguageKey(),
        //        null,
        //    }).ToArray();

        //    return Ok(dataTablesOutput);
        //}

      

    }
}
