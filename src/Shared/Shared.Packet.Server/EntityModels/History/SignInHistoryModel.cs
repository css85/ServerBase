using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Server.Define;

namespace Shared.Entities.Models
{
    [Table("user_signin_history")]
    public class SignInHistoryModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("history_seq")]
        public long HistorySeq { get; set; }

        [Required]
        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }

        
    }
}
