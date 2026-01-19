using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_currency")]
    [Index(nameof(UserSeq))]
    public class UserCurrencyModel
    {
        [Key]
        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Key]
        [Column("obtain_type")]
        public RewardPaymentType ObtainType { get; set; }

        [Key]
        [Column("item_id")]
        public long ItemId { get; set; }

        [Column("qty", TypeName = "varchar(128)")]
//        [Column("qty")]
//        public decimal ItemQty { get; set; }
        public BigInteger ItemQty { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }
}
