using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Server.Define;

namespace Shared.Entities.Models
{
    [Table("user_vip_history")]
    public class VipHistoryModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("history_seq")]
        public long HistorySeq { get; set; }

        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Column("before_level")]
        public int BeforeLevel { get; set; }

        [Column("before_point")]        
        public int BeforePoint { get; set; }

        [Column("change_point")]
        public int ChangePoint { get; set; }

        [Column("after_level")]
        public int AfterLevel { get; set; }

        [Column("after_point")]
        public int AfterPoint { get; set; }

        [Column("reason")]
        public string Reason { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }

        
    }
}
