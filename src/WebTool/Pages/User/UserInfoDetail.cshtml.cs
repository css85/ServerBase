using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Repository.Services;
using Shared.Server.Define;
using Shared.Server.Extensions;
using Shared.ServerApp.Common.Tasks;
using Shared.ServerApp.Config;
using Shared.ServerApp.Services;
using StackExchange.Redis;
using WebTool.Base.UserInfoDetail;
using WebTool.Controllers;
using WebTool.Extensions;
using WebTool.Identity;
using Shared.Repository;
using Common.Config;
using Shared.Services.Redis;

namespace WebTool.Pages.User
{
    [Authorize(Roles = AuthorizeRoleType.AuthorizeUser)]
    public class UserInfoDetailModel : PageModel
    {
        private readonly ILogger<UserInfoDetailModel> _logger;
        private readonly DatabaseRepositoryService _dbRepo;
        private readonly IDatabaseAsync _sessionRedis;
        private readonly IDatabaseAsync _accountRedis;
        private readonly CsvStoreContext _csvStoreContext;
        private readonly ChangeableSettings<GameRuleSettings> _gameRule;

        public PlayerDetailInfo Info { get; set; }
        public PlayerDetailInfoItem[] InfoItems { get; set; }
        public UserInfoDetailTabItem[] TabItems { get; set; }
        

        public UserInfoDetailModel(
            ILogger<UserInfoDetailModel> logger,
            DatabaseRepositoryService dbRepo,
            RedisRepositoryService redisRepo,            
            ChangeableSettings<GameRuleSettings> gameRule,
            CsvStoreContext csvStoreContext
            )
        {
            _logger = logger;
            _dbRepo = dbRepo;
            _sessionRedis = redisRepo.GetDb(RedisDatabase.Session);
            _accountRedis = redisRepo.GetDb(RedisDatabase.Account);
            _gameRule = gameRule;
            _csvStoreContext = csvStoreContext;
        }

        public async Task OnGetAsync(long userSeq)
        {
            Info = new PlayerDetailInfo(userSeq);

            using var userCtx = _dbRepo.GetUserDb();

            Info.Account = await userCtx.UserAccounts.FindAsync(Info.UserSeq);
            Info.User = await userCtx.UserInfos.FindAsync(Info.UserSeq);
            Info.IsBlock = Info.Account.Block;

            InfoItems = new[]
            {
                new PlayerDetailInfoItem
                {
                    LocaleName = "ID",
                    Type = PlayerDetailInfoItemType.Number,
                    Value = $"{Info.Account.UserSeq}",

                    IsSeparate = true,
                },
                new PlayerDetailInfoItem
                {
                    LocaleName = "닉네임",
                    Type = PlayerDetailInfoItemType.Nick,
                    Value = $"{Info.User.Nick}",

                    EditInfo = new PlayerDetailEditInfo
                    {
                        Type = PlayerDetailEditInfoType.UserDb,
                        ClassName = nameof(Info.User),
                        PropertyName = nameof(Info.User.Nick),
                    },
                },
                new PlayerDetailInfoItem
                {
                    IsSeparate = true,

                    LocaleName = "로그인시간",
                    Type = PlayerDetailInfoItemType.DateTime,
                    Value = $"{Info.Account.LoginDt.GetNameLanguageKey()}",
                },
                new PlayerDetailInfoItem
                {
                    LocaleName = "로그아웃시간",
                    Type = PlayerDetailInfoItemType.DateTime,
                    Value = $"{Info.Account.LogOutDt.GetNameLanguageKey()}",
                },
                new PlayerDetailInfoItem
                {
                    LocaleName = "계정생성시간",
                    Type = PlayerDetailInfoItemType.DateTime,
                    Value = Info.Account.RegDt.GetNameLanguageKey(),
                },
                new PlayerDetailInfoItem
                {
                    LocaleName = "레벨",
                    Type = PlayerDetailInfoItemType.User,
                    Value = $"{Info.User.Level.ToString() ?? "0"}",

                    EditInfo = new PlayerDetailEditInfo
                    {
                        Type = PlayerDetailEditInfoType.UserDb,
                        ClassName = nameof(Info.User),
                        PropertyName = nameof(Info.User.Level),
                    },
                },
                new PlayerDetailInfoItem
                {
                    LocaleName = "등급",
                    Type = PlayerDetailInfoItemType.Number,
                    Value = $"{Info.User.Grade}",

                    EditInfo = new PlayerDetailEditInfo
                    {
                        Type = PlayerDetailEditInfoType.UserDb,
                        ClassName = nameof(Info.User),
                        PropertyName = nameof(Info.User.Grade),
                    },
                },
                new PlayerDetailInfoItem
                {
                    LocaleName = "프로필",
                    Type = PlayerDetailInfoItemType.Profile,
                    //Type = PlayerDetailInfoItemType.Text,
                    //Value = $"{Info.User.ProfileParts}",

                    //EditInfo = new PlayerDetailEditInfo
                    //{
                    //    Type = PlayerDetailEditInfoType.UserDb,
                    //    ClassName = nameof(Info.User),
                    //    PropertyName = nameof(Info.User.ProfileParts),
                    //},
                },
                new PlayerDetailInfoItem
                {
                    LocaleName = "코멘트",
                    Type = PlayerDetailInfoItemType.Text,
                    Value = $"{Info.User.Comment}",

                    EditInfo = new PlayerDetailEditInfo
                    {
                        Type = PlayerDetailEditInfoType.UserDb,
                        ClassName = nameof(Info.User),
                        PropertyName = nameof(Info.User.Comment),
                    },
                },

              
                new PlayerDetailInfoItem
                {
                    LocaleName = "파츠",
                    Type = PlayerDetailInfoItemType.Parts,
                },
                new PlayerDetailInfoItem
                {
                    LocaleName = "내 아이템",
                    Type = PlayerDetailInfoItemType.MyItem,
                },

                //new PlayerDetailInfoItem
                //{
                //    IsSeparate = true,

                //    LocaleName = "루비",
                //    Type = PlayerDetailInfoItemType.Number,
                //    Value = $"{Info.Wallet.Ruby}",

                //    EditInfo = new PlayerDetailEditInfo
                //    {
                //        Type = PlayerDetailEditInfoType.UserDb,
                //        ClassName = nameof(Info.Wallet),
                //        PropertyName = nameof(Info.Wallet.Ruby),
                //    },
                //},
                //new PlayerDetailInfoItem
                //{
                //    LocaleName = "덴",
                //    Type = PlayerDetailInfoItemType.Number,
                //    Value = $"{Info.Wallet.Den}",

                //    EditInfo = new PlayerDetailEditInfo
                //    {
                //        Type = PlayerDetailEditInfoType.UserDb,
                //        ClassName = nameof(Info.Wallet),
                //        PropertyName = nameof(Info.Wallet.Den),
                //    },
                //},
                //new PlayerDetailInfoItem
                //{
                //    IsSeparate = true,
                //    LocaleName = "메시지 발송",
                //    Type = PlayerDetailInfoItemType.SendMessage,
                //},
                //new PlayerDetailInfoItem
                //{
                //    LocaleName = "푸쉬 발송",
                //    Type = PlayerDetailInfoItemType.SendPush,
                //},
                new PlayerDetailInfoItem
                {
                    LocaleName = "블럭 상태",
                    Type = PlayerDetailInfoItemType.Text,
                    Value = Info.IsBlock ? "정지" : "일반",
                },
            };

            for (var i = 0; i < InfoItems.Length; i++)
                InfoItems[i].Id = $"{i:D3}";

            TabItems = new[]
            {
                new UserInfoDetailTabItem
                {
                    IsActive = true,
                    Id = "Friends",
                    LocaleName = "친구",
                    Columns = UserController.FriendTableDataColumnInfos.Select(p => p.Name).ToArray()
                },
                new UserInfoDetailTabItem
                {
                    Id = "FriendRequests",
                    LocaleName = "친구보낸요청",
                    Columns = UserController.FriendRequestTableDataColumnInfos.Select(p => p.Name).ToArray()
                },
                new UserInfoDetailTabItem
                {
                    Id = "FriendResponse",
                    LocaleName = "친구받은요청",
                    Columns = UserController.FriendResponseTableDataColumnInfos.Select(p => p.Name).ToArray()
                },


            };

        }

        //private string LevelToGrade(int normalGrade, int specialGrade)
        //{
        //    bool isSpecial = (specialGrade > 0) ? true : false;
        //    int index = (isSpecial) ? specialGrade : normalGrade;
        //    string lang_key = "";

        //    if (isSpecial)
        //    {
        //        lang_key = _csvStoreContext.GetData().FanGradeSpecialDicData[index].Name;
        //    }
        //    else
        //    {
        //        lang_key = _csvStoreContext.GetData().FanGradeNormalDicData[index].Name;
        //    }

        //    return _languageService.GetText(LanguageType.Korean, lang_key);
        //}
    }
}
