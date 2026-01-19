using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_info")]
    [Index(nameof(Nick))]

    public class UserInfoModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Column("nick")]
        [MaxLength(256)]
        public string Nick { get; set; }

        [Column("level")]
        public int Level { get; set; }

        [Column("grade")]
        public int Grade { get; set; }

        [Column("country_idx")]
        public int CountryIdx;
                
        [Column("login_count")]
        public int LoginCount { get; set; }

        [Column("profile_parts")]
        [MaxLength(2048)]
        public string ProfileParts { get; set; }

        [Column("comment")]
        [MaxLength(512)]
        public string Comment { get; set; }

        [Column("update_parts_sellqty_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime UpdatePartsSellQtyDt { get; set; }

        [Column("update_currency_charge_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime UpdateCurrencyChargeQtyDt { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }
    }
}
