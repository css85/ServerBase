using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_attendance")]
    
    public class UserAttendanceModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
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

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }
        
    }
}
