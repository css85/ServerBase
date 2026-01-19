using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_store_inapp")]
    [Index(nameof(UserSeq))]
    public class UserStoreInAppInfoModel
    {
        [Key]
        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Key]
        [Column("store_inapp_index")]
        public long StoreInAppIndex { get; set; } 
                
        [Column("purchase_count")]
        public long PurchaseCount { get; set; }

        [Column("last_purchase_time")]
        public DateTime LastPurchaseTime { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }
    }
}
