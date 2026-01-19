using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_daily_mission_complete")]
    
    public class UserDailyMissionCompleteModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Key]
        [Column("date", TypeName = "Date")]
        public DateTime Date { get; set; }

        [Column("twoday_reward")]
        public bool IsTwoDayReward { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }

        [Column("reward_dt")]        
        public DateTime RewardDt { get; set; }

    }
}
