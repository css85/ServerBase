using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Clock;
using Shared.Entities.Models;
using Shared.Packet.Models;
using Shared.Packet.Server.Extensions;
using Shared.Repository;
using Shared.Server.Define;
using Shared.ServerApp.Services;
using Shared.Services.Redis;

namespace FrontEndWeb.Services
{
    public class AttendanceService
    {
        private readonly ILogger<AttendanceService> _logger;

        private readonly CsvStoreContext _csvContext;
        private readonly RedisRepositoryService _redisRepo;
        private readonly PlayerService _playerService;
        
     
        public AttendanceService(
            ILogger<AttendanceService> logger,        
            CsvStoreContext csvStoreContext,            
            RedisRepositoryService redisRepo,
            PlayerService playerService
            )
        {
            _logger = logger;
            _csvContext = csvStoreContext;
            _redisRepo = redisRepo;
            _playerService = playerService; 
        }
        private AttendanceType CheckReturnType(UserAttendanceModel userAttendDb)
        {
            if (userAttendDb == null)
                return AttendanceType.Normal;

            var diffDayTime = AppClock.UtcNow.Date - userAttendDb.AttendanceDt.Date;
            return diffDayTime.Days > SharedDefine.ATTENDANCE_RETURN_DAY ? AttendanceType.Return : AttendanceType.Normal;
        }
        private int MaxAttendanceDay(long attendIndex)
        {
            var csvData = _csvContext.GetData();
            if (csvData.AttendanceDicData.TryGetValue(attendIndex, out var data) == false)
                return 0;
            return data.RewardDatas.Max(p => p.DayOrderNum);

        }

        public async Task<UserAttendanceModel> NewAttendanceAsync(UserCtx userCtx, long userSeq, AttendanceType type)
        {
            var csvData = _csvContext.GetData();
            var attendanceData = csvData.AttendanceDicData.Values
                .Where(p => p.AttendanceType == type && p.IsValidTime(AppClock.UtcNow))
                .OrderByDescending(p => p.Index)
                .FirstOrDefault();

            if (attendanceData == null)
                return null;

            var newAttend = new UserAttendanceModel
            {
                UserSeq = userSeq,
                AttendanceIndex = attendanceData.Index,
                AttendanceType = attendanceData.AttendanceType,
                LastRewardDay = 0,
                AttendanceDt = AppClock.MinValue,
                RegDt = AppClock.UtcNow,
            };
            await userCtx.UserAttendances.AddAsync(newAttend);
            await userCtx.SaveChangesAsync();

            return newAttend;
        }

        public async Task<UserAttendanceModel> GetUserAttendanceDbAsync(UserCtx userCtx, long userSeq)
        {
            var csvData = _csvContext.GetData();

            var userAttendDb = await userCtx.UserAttendances.FindAsync(userSeq);
            if (userAttendDb == null)
            {
                var type = CheckReturnType(userAttendDb);
                userAttendDb = await NewAttendanceAsync(userCtx, userSeq, type);
            }
            else
            {
                bool newAttend = false;

                var diffDayTime = AppClock.UtcNow.Date - userAttendDb.AttendanceDt.Date;
                if (diffDayTime.Days > SharedDefine.ATTENDANCE_RETURN_DAY && userAttendDb.AttendanceType != AttendanceType.Newbie)
                    newAttend = true;
                else
                {
                    if (userAttendDb.LastRewardDay <= 0)
                    {
                        if (csvData.AttendanceDicData.TryGetValue(userAttendDb.AttendanceIndex, out var data) == false)
                            return null;
                        if (data.IsValidTime(AppClock.UtcNow) == false)
                            newAttend = true;
                    }
                    else if (userAttendDb.LastRewardDay >= MaxAttendanceDay(userAttendDb.AttendanceIndex))
                        newAttend = true;
                }
                if (newAttend == true)
                {
                    var type = CheckReturnType(userAttendDb);
                    await userCtx.UserAttendanceHistory.AddAsync(userAttendDb.ToAttendanceHistory());
                    userCtx.UserAttendances.Remove(userAttendDb);
                    await userCtx.SaveChangesAsync();

                    userAttendDb = await NewAttendanceAsync(userCtx, userSeq, type);
                }
            }
            return userAttendDb;
        }
        public async Task<AttendanceInfo> GetAttendanceInfoAsync(UserCtx userCtx, long userSeq)
        {
            var userAttendDb = await GetUserAttendanceDbAsync(userCtx, userSeq);
            return userAttendDb != null ? userAttendDb.ToAttendanceInfo() : new AttendanceInfo();
        }

        public async Task<AttendanceInfo> AttendanceEnterAsync(UserCtx userCtx, long userSeq, int grade = 0 )
        {
            var csvData = _csvContext.GetData();
            var redisUser = _redisRepo.GetDb(RedisDatabase.User);

            var userAttendDb = await GetUserAttendanceDbAsync(userCtx, userSeq);
            var attendInfo = userAttendDb != null ? userAttendDb.ToAttendanceInfo() : new AttendanceInfo();

            await redisUser.StringSetAsync(string.Format(RedisKeys.s_UserAttendanceType, userSeq), attendInfo.AttendanceType.ToString());
            if (attendInfo.IsReward(AppClock.UtcNow))
            {
                if (csvData.AttendanceDicData.TryGetValue(userAttendDb.AttendanceIndex, out var data) == true)
                {
                    var rewardDay = attendInfo.LastRewardDay + 1;
                    var rewards = data.RewardDatas.Where(p => p.DayOrderNum == rewardDay).Select(p => p.RewardInfo).ToList();

                    if( grade == 0 )
                    {
                        grade = await _playerService.GetUserGradeAsync(userCtx, redisUser, userSeq);
                    }

                    rewards = _playerService.GetCalcGoldItemInfos(grade, rewards);

                    var titleKey = "mailbox_attendance_normal";
                    if (attendInfo.AttendanceType == AttendanceType.Newbie)
                        titleKey = "mailbox_attendance_newbie";
                    else if (attendInfo.AttendanceType == AttendanceType.Return)
                        titleKey = "mailbox_attendance_return";

                    var mailDbs = rewards.Select(p => new UserMailModel
                    {
                        UserSeq = userSeq,
                        ObtainType = p.ItemType,
                        ObtainId = p.Index,
                        ObtainQty = p.ItemQty,
                        TitleKey = titleKey,
                        TitleKeyArg = rewardDay.ToString(),
                        LimitDt = AppClock.UtcNow.AddDays(7),
                    }).ToList();
                    await userCtx.UserMails.AddRangeAsync(mailDbs);

                    userAttendDb.LastRewardDay = rewardDay;
                    userAttendDb.AttendanceDt = AppClock.UtcNow;
                    userCtx.UserAttendances.Update(userAttendDb);
                }
            }
            return attendInfo;
        }




    }
}
