using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_attendance_history")]
    
    public class UserAttendanceHistoryModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("history_seq")]
        public long HistorySeq { get; set; }

        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Column("attendance_index")]
        public long AttendanceIndex { get; set; }

        [Column("attendance_Type")]
        public AttendanceType AttendanceType { get; set; }

        [Column("last_Reward_day")]
        public int LastRewardDay { get; set; }

        [Column("attendance_dt")]        
        public DateTime AttendanceDt { get; set; }

        [Column("create_dt")]
        public DateTime CreateDt { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }
        
    }
}
