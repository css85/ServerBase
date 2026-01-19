using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Shared.Server.Define;

namespace Shared.Entities.Models
{
    [Table("user_currency_history")]
    [Index(nameof(Reason), nameof(RegDt))]
    [Index(nameof(ChangeQty), nameof(RegDt))]
    public class CurrencyHistoryModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("history_seq")]
        public long HistorySeq { get; set; }

        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Column("item_type")]
        public RewardPaymentType ItemType { get; set; }

        [Column("item_id")]
        public long ItemId { get; set; }

        [Column("before_qty", TypeName = "varchar(128)")]
        public BigInteger BeforeQty { get; set; }

        [Column("change_qty", TypeName = "varchar(128)")]
        public BigInteger ChangeQty { get; set; }

        [Column("after_qty", TypeName = "varchar(128)")]
        public BigInteger AfterQty { get; set; }

        [Column("reason")]
        public string Reason { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }

        
    }
}
