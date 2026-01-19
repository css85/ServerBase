using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_buy_vip_partsshop")]
//    [Index(nameof(UserSeq))]
    public class UserBuyVipPartsShopModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Key]
        [Column("product_index")]
        public long ProductIndex { get; set; }

        [Column("reward_type")]
        public RewardPaymentType RewardType { get; set; }

        [Column("reward_index")]
        public long RewardIndex { get; set; }

        [Column("reward_amount", TypeName = "varchar(128)")]
        public BigInteger RewardAmount { get; set; }


        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }
    }
}
