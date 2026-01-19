using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_point")]
    [Index(nameof(UserSeq))]
    public class UserPointModel
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

        [Column("qty")]
        public long ItemQty { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
