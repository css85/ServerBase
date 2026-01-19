using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_achievement")]
    public class UserAchievementModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Key]
        [Column("mission_index")]
        public long MissionIndex { get; set; }

        [Column("mission_count", TypeName = "varchar(128)")]
        public BigInteger MissionCount { get; set; }

        [Column("last_reward_ordernum")]
        public int LastRewardOrderNum { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }

        [Column("reward_dt")]        
        public DateTime RewardDt { get; set; }

    }
}
