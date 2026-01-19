using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Shared.Server.Define;

namespace Shared.Entities.Models
{
    [Table("user_ccu_history")]
    [Index(nameof(UserCount))]
    [Index(nameof(RegDt))]
    public class CCUHistoryModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("history_seq")]
        public long HistorySeq { get; set; }

        [Required]
        [Column("user_count")]
        public long UserCount { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }

        
    }
}
