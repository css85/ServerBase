using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Shared.Server.Define;

namespace Shared.Entities.Models
{
    [Table("user_iap_history")]
    [Index(nameof(TransactionId))]
    public class IAPHistoryModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("history_seq")]
        public long HistorySeq { get; set; }

        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Column("store_type")]
        public StoreType StoreType { get; set; }

        [Column("transaction_id")]
        public string TransactionId { get; set; }  

        [Column("product_id")]
        public string ProductId { get; set; }
                
        [Column("receipt")]
        public string Receipt { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }

        
    }
}
