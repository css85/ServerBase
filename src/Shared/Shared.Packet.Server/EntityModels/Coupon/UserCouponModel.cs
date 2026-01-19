using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_coupon")]
    [Index(nameof(CouponCode))]
    [Index(nameof(CouponCode), nameof(UserSeq), nameof(UseCount))]
    public class UserCouponModel
    {
        [Key]
        [Column("coupon_code")]
        public string CouponCode { get; set; }

        [Key]
        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Column("coupon_use_count")]
        public int UseCount { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }
    }
}
