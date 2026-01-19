using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Server.Define;

namespace Shared.Entities.Models
{
    [Table("user_point_history")]
    public class PointHistoryModel
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

        [Column("before_qty")]        
        public long BeforeQty { get; set; }

        [Column("change_qty")]
        public long ChangeQty { get; set; }

        [Column("after_qty")]
        public long AfterQty { get; set; }

        [Column("reason")]
        public string Reason { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }

        
    }
}
