using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Server.Define;

namespace Shared.Entities.Models
{
    [Table("user_applelogin_transer_history")]
    public class AppleLoginTransferHistoryModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("history_seq")]
        public long HistorySeq { get; set; }

        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Column("account_type")]
        public AccountType AccountType { get; set; }


        [Column("before_account_id")]
        [MaxLength(128)]
        public string BeforeAccountId { get; set; }

        [Column("after_account_id")]
        [MaxLength(128)]
        public string AfterAccountId { get; set; }

        [Column("comment")]
        [MaxLength(128)]
        public string Comment { get; set; }


        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }

        
    }
}
