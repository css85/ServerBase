using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Entities;
using Shared.Entities.Models;
using Shared.Packet;
using Shared.Repository.Database;
using Shared.Repository.Extensions;
using Shared.Repository.Services;
using Shared.Repository.Utility;
using Shared.Server.Define;

namespace Shared.Repository
{
    public sealed class UserCtx : PooledDbContext
    {
        public UserCtx(DbContextOptions options) : base(options)
        {
        }
        public DbSet<AccountModel> UserAccounts { get; set; }
        public DbSet<AccountLinkModel> UserAccountLinks { get; set; }
        public DbSet<UserInfoModel> UserInfos { get; set; }
        public DbSet<UserShopProductModel> UserShopProducts { get; set; }
        public DbSet<UserGameItemOwnModel> UserItems { get; set; }
        public DbSet<UserCurrencyModel> UserCurrency { get; set; }
        public DbSet<UserMailModel> UserMails { get; set; }
        public DbSet<MessagePushModel> MessagePushes { get; set; }
        public DbSet<UserNavigationMissionModel> UserNavigationMissions { get; set; }
        public DbSet<UserMissionModel> UserMissions { get; set; }
        public DbSet<UserAchievementModel> UserAchievements { get; set; }
        public DbSet<UserDailyMissionCompleteModel> UserDailyMissionCompletes { get; set; }
        public DbSet<UserPostingModel> UserPostings { get; set; }
        public DbSet<UserPostingLikeModel> UserPostingLikes { get; set; }
        public DbSet<UserPostingCommentModel> UserPostingComments { get; set; }
        public DbSet<UserFriendInfoModel> UserFriendInfos { get; set; }
        public DbSet<UserFriendsModel> UserFriends { get; set; }
        public DbSet<UserRequestFriendsModel> UserReqestFriends { get; set; }
        public DbSet<UserResponseFriendsModel> UserResponseFriends { get; set; }
        public DbSet<UserRecommendRewardModel> UserRecommendRewards { get; set; }
        public DbSet<UserRecommendModel> UserRecommends { get; set; }
        public DbSet<UserCouponModel> UserCoupons { get; set; }
        public DbSet<UserOfflineRewardModel> UserOfflineRewards { get; set; }
        public DbSet<UserAttendanceModel> UserAttendances { get; set; }
        public DbSet<UserPointModel> UserPoints { get; set; }
        public DbSet<IapProductGuideModel> IapProducts { get; set; }
        public DbSet<NewUserConfigModel> NewUserConfigs { get; set; }
        public DbSet<UserVipModel> UserVips { get; set; }
        public DbSet<UserVipDailyRewardModel> UserVipDailyRewards { get; set; }
        public DbSet<UserVipActiveModel> UserVipActives { get; set; }
        public DbSet<UserBuyVipPartsShopModel> UserBuyVipPartsShops { get; set; }

        ///////////////////////////history//////////////////////////////////////////////////////////
        public DbSet<SignInHistoryModel> SignInHistory { get; set; }
        public DbSet<ItemHistoryModel> ItemHistory { get; set; }
        public DbSet<CurrencyHistoryModel> CurrencyHistory { get; set; }
        public DbSet<IAPHistoryModel> IAPHistory { get; set; }
        public DbSet<CCUHistoryModel> CCUHistory { get; set; }
        public DbSet<UserAttendanceHistoryModel> UserAttendanceHistory { get; set; }
        public DbSet<PointHistoryModel> PointHistory { get; set; }
        public DbSet<UserMailObtainHistoryModel> UserMailObtainHistory { get; set; }
        public DbSet<VipHistoryModel> VipHistory { get; set; }
        public DbSet<AppleLoginTransferHistoryModel> AppleLoginTransferHistory { get; set; }

        private string DecimalStringSplit(string s) => s.Split('.')[0];


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8mb4", null);

            ValueComparer<List<long>> indexListComparer = new(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

            ValueConverter<List<long>, string> indexListConverter = new(
                strings => string.Join(";", strings),
                s => s.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList());
                               
            ValueConverter<BigInteger, string> bigIntegerStringConverter = new(
                b => b.ToString(),
                d => BigInteger.Parse(DecimalStringSplit(d), System.Globalization.NumberStyles.Number));
            //                d => BigInteger.Parse( d.Split(".", StringSplitOptions.RemoveEmptyEntries)[0]));

            ValueConverter<BigInteger, long> bigIntegerlongConverter = new(
               b => (long)b,
               d => new BigInteger(d));

            modelBuilder.Entity<AccountModel>(e =>
            {
                e.HasKey(p => p.UserSeq);                
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
                e.Property(p => p.Block).HasDefaultValue(false).HasConversion(new BoolToZeroOneConverter<int>());
                e.Property(p => p.BlockEndDt).HasDefaultValue(DateTime.MinValue);
                e.HasIndex(p => new { p.RegDt, p.LoginDt });
                e.HasIndex(p => p.LoginDt);
                e.Property(p=>p.AccountType).HasConversion(
                    v => (byte)v,
                    v => (AccountType)v).HasDefaultValue(AccountType.Guest);
            });

            modelBuilder.Entity<AccountLinkModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.AccountType});
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());                
                e.Property(p => p.AccountType).HasConversion(
                    v => (byte)v,
                    v => (AccountType)v).HasDefaultValue(AccountType.Guest);
            });

            modelBuilder.Entity<UserInfoModel>(e =>
            {
                e.HasKey(p => p.UserSeq);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
                e.Property(p => p.UpdatePartsSellQtyDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
                e.Property(p => p.UpdateCurrencyChargeQtyDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
                e.Property(p => p.Level).HasDefaultValue(1);
                e.Property(p => p.Grade).HasDefaultValue(1);
                e.Property(p => p.CountryIdx).HasDefaultValue(0);
                e.Property(p => p.ProfileParts).HasDefaultValue("");
            });

            modelBuilder.Entity<UserShopProductModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.Slot });
                e.Property(p => p.PartsIndex).HasDefaultValue(0);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserGameItemOwnModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.ObtainType, p.ItemId });
                e.Property(p => p.ItemQty).HasDefaultValue(0);
//                e.Property(p => p.ItemQty).HasConversion(bigIntegerDecimalConverter);
                e.Property(p => p.ObtainType).HasConversion(
                    v => (byte)v,
                    v => (RewardPaymentType)v);
            });

            modelBuilder.Entity<UserCurrencyModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.ObtainType, p.ItemId });
                //                e.Property(p => p.ItemQty).HasDefaultValue(0);
                e.Property(p => p.ItemQty).HasConversion(bigIntegerStringConverter);
                e.Property(p => p.ObtainType).HasConversion(
                    v => (byte)v,
                    v => (RewardPaymentType)v);
            });

            modelBuilder.Entity<UserPointModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.ObtainType, p.ItemId });
                e.Property(p => p.ItemQty).HasDefaultValue(0);
                e.Property(p => p.ObtainType).HasConversion(
                    v => (byte)v,
                    v => (RewardPaymentType)v);
            });

            modelBuilder.Entity<UserMailModel>(e =>
            {
                e.HasKey(p => p.MailSeq);
                e.Property(p => p.IsObtain).HasDefaultValue(false).HasConversion(new BoolToZeroOneConverter<int>());
                e.Property(p => p.ObtainQty).HasConversion(bigIntegerStringConverter);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
               
            });

            modelBuilder.Entity<MessagePushModel>(e =>
            {
                e.HasKey(p => p.PushSeq);
                e.Property(p => p.SendYn).HasDefaultValue(false);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserNavigationMissionModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.MissionIndex });
                e.Property(p => p.IsReward).HasDefaultValue(false).HasConversion(new BoolToZeroOneConverter<int>());
                e.Property(p => p.MissionCount).HasConversion(bigIntegerStringConverter);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserMissionModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.MissionIndex });
                e.Property(p => p.MissionCount).HasConversion(bigIntegerStringConverter);
                e.Property(p => p.IsReward).HasDefaultValue(false).HasConversion(new BoolToZeroOneConverter<int>());
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserAchievementModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.MissionIndex });
                e.Property(p => p.LastRewardOrderNum).HasDefaultValue(0);
                e.Property(p => p.MissionCount).HasConversion(bigIntegerStringConverter);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserDailyMissionCompleteModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.Date });
                e.Property(p => p.IsTwoDayReward).HasDefaultValue(false).HasConversion(new BoolToZeroOneConverter<int>());
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserPostingModel>(e =>
            {
                e.HasKey(p => p.PostingSeq);
                e.Property(p => p.DecorateInfo).HasDefaultValue("");
                e.Property(p => p.LikeCount).HasDefaultValue(0);
                e.Property(p => p.Title).HasDefaultValue("");
                e.Property(p => p.ViewPostings).HasDefaultValue(false).HasConversion(new BoolToZeroOneConverter<int>());
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserPostingLikeModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.TargetPostingSeq });
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserPostingCommentModel>(e =>
            {
                e.HasKey(p => p.CommentSeq);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserFriendInfoModel>(e =>
            {
                e.HasKey(p => p.UserSeq);                
                e.Property(p => p.SendCode).HasDefaultValue(false).HasConversion(new BoolToZeroOneConverter<int>());
                e.Property(p => p.RecommendUserCode).HasDefaultValue(false).HasConversion(new BoolToZeroOneConverter<int>());
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserFriendsModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.TargetSeq});
                e.Property(p => p.CoolTimeDt).HasDefaultValue(DateTime.MinValue);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserRequestFriendsModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.RequestUserSeq });
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserResponseFriendsModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.ResponseUserSeq });
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserRecommendRewardModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.RecommendIndex });
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserRecommendModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.RecommendUserSeq });
                e.Property(p => p.RecommendCount).HasDefaultValue(1);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });


            modelBuilder.Entity<UserCouponModel>(e =>
            {
                e.HasKey(p => new { p.CouponCode, p.UserSeq });
                e.Property(p => p.UseCount).HasDefaultValue(1);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserOfflineRewardModel>(e =>
            {
                e.HasKey(p => p.UserSeq);
                e.Property(p => p.Rewarded).HasDefaultValue(false);
                e.Property(p => p.AdRewarded).HasDefaultValue(false);
                e.Property(p => p.OfflineTimeMin).HasDefaultValue(0);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserAttendanceModel>(e =>
            {
                e.HasKey(p => p.UserSeq);
                e.Property(p => p.LastRewardDay).HasDefaultValue(0);
                e.Property(p => p.AttendanceType).HasConversion(v => (byte)v, v => (AttendanceType)v);
                e.Property(p => p.AttendanceDt).HasDefaultValue(DateTime.MinValue);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<IapProductGuideModel>(e =>
            {
                e.HasKey(p => p.ProductId);
            });

            modelBuilder.Entity<NewUserConfigModel>(e =>
            {
                e.HasKey(p => p.ConfigSeq);                
                e.Property(p => p.PrologueConfigType).HasConversion(
                    v => (byte)v,
                    v => (NewUserConfigType)v);

                e.Property(p => p.Navi2Tutorial).HasConversion(
                    v => (byte)v,
                    v => (NewUserConfigType)v);
            });

            // 점검 시 전체 넣어줘야 함
            modelBuilder.Entity<UserVipModel>(e =>
            {
                e.HasKey(p => p.UserSeq);
                e.Property(p => p.FrDt).HasDefaultValue(DateTime.MinValue);
                e.Property(p => p.ToDt).HasDefaultValue(DateTime.MinValue);
                e.Property(p => p.DailyRewardDt).HasDefaultValue(DateTime.MinValue);
                e.Property(p => p.AttendanceDt).HasDefaultValue(DateTime.MinValue);
                e.Property(p=>p.Level).HasDefaultValue(0);  
                e.Property(p => p.Point).HasDefaultValue(0);    
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserVipDailyRewardModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.DateCycleKey, p.Level});                
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserVipActiveModel>(e =>
            {
                e.HasKey(p => p.HistorySeq);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });


            modelBuilder.Entity<UserBuyVipPartsShopModel>(e =>
            {
                e.HasKey(p => new { p.UserSeq, p.ProductIndex});
                e.Property(p => p.RewardType).HasConversion(
                    v => (byte)v,
                    v => (RewardPaymentType)v);
                e.Property(p => p.RewardAmount).HasConversion(bigIntegerStringConverter);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });


            //////////////////////////////////////////////////////////////////////////
            ///  History
            //////////////////////////////////////////////////////////////////////////

            modelBuilder.Entity<SignInHistoryModel>(e =>
            {
                e.HasKey(p => p.HistorySeq);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<ItemHistoryModel>(e =>
            {
                e.HasKey(p => p.HistorySeq);
                e.Property(p => p.ItemType).HasConversion(
                    v => (byte)v,
                    v => (RewardPaymentType)v);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<CurrencyHistoryModel>(e =>
            {
                e.HasKey(p => p.HistorySeq);
                e.Property(p => p.BeforeQty).HasConversion(bigIntegerStringConverter);
                e.Property(p => p.ChangeQty).HasConversion(bigIntegerStringConverter);
                e.Property(p => p.AfterQty).HasConversion(bigIntegerStringConverter);
                e.Property(p => p.ItemType).HasConversion(
                    v => (byte)v,
                    v => (RewardPaymentType)v);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<IAPHistoryModel>(e =>
            {
                e.HasKey(p => p.HistorySeq);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<CCUHistoryModel>(e =>
            {
                e.HasKey(p => p.HistorySeq);
                e.Property(p => p.UserCount).HasDefaultValue(0);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());

            });

            modelBuilder.Entity<UserAttendanceHistoryModel>(e =>
            {
                e.HasKey(p => p.HistorySeq);
                e.Property(p => p.AttendanceType).HasConversion(v => (byte)v, v => (AttendanceType)v);
                e.Property(p => p.LastRewardDay).HasDefaultValue(0);
                e.Property(p => p.AttendanceDt).HasDefaultValue(DateTime.MinValue);
                e.Property(p => p.CreateDt).HasDefaultValue(DateTime.MinValue);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<PointHistoryModel>(e =>
            {
                e.HasKey(p => p.HistorySeq);
                e.Property(p => p.ItemType).HasConversion(
                    v => (byte)v,
                    v => (RewardPaymentType)v);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<UserMailObtainHistoryModel>(e =>
            {
                e.HasKey(p => p.MailSeq);
                e.Property(p => p.IsObtain).HasDefaultValue(false).HasConversion(new BoolToZeroOneConverter<int>());
                e.Property(p => p.ObtainQty).HasConversion(bigIntegerStringConverter);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());

            });

            modelBuilder.Entity<VipHistoryModel>(e =>
            {
                e.HasKey(p => p.HistorySeq);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });

            modelBuilder.Entity<AppleLoginTransferHistoryModel>(e =>
            {
                e.HasKey(p => p.HistorySeq);
                e.Property(p => p.AccountType).HasConversion(
                    v => (byte)v,
                    v => (AccountType)v);
                e.Property(p => p.RegDt).HasDefaultValueSql(Provider.GetDateTimeDefaultSql());
            });


            base.OnModelCreating(modelBuilder);
        }


    }
}

