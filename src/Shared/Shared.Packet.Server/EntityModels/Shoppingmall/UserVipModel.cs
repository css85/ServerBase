using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_vip")]
//    [Index(nameof(UserSeq))]
    public class UserVipModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("user_seq")]
        public long UserSeq { get; set; }
        
        [Column("level")]
        public int Level { get; set; }

        [Column("point")]
        public int Point { get; set; }

        [Column("fr_dt")]        
        public DateTime FrDt { get; set; }

        [Column("to_dt")]
        public DateTime ToDt { get; set; }

        [Column("daily_reward_dt")]
        public DateTime DailyRewardDt { get; set; }

        [Column("last_Reward_attendance_day")]
        public int LastRewardAttendanceDay { get; set; }

        [Column("attendance_dt")]
        public DateTime AttendanceDt { get; set; }


        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }
    }
}
