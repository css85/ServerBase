using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_mail")]
    [Index(nameof(UserSeq), nameof(IsObtain), nameof(LimitDt))]
    [Index(nameof(MailSeq), nameof(UserSeq))]
    [Index(nameof(UserSeq), nameof(LimitDt))]
    public class UserMailModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("mail_seq")]
        public long MailSeq { get; set; }

        [Column("user_seq")]
        public long UserSeq { get; set; }   

        [Column("title_key")]
        public string TitleKey { get; set; }

        [Column("title_key_arg")]
        [MaxLength(512)]
        public string TitleKeyArg { get; set; }

        [Column("is_obtain")]
        public bool IsObtain { get; set; }

        [Column("obtain_type")]
        public RewardPaymentType ObtainType { get; set; }

        [Column("obtain_id")]
        public long ObtainId { get; set; }

        [Column("obtain_qty", TypeName = "varchar(128)")]
        public BigInteger ObtainQty { get; set; }

        [Column("limit_dt")]        
        public DateTime LimitDt { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }

        [Column("receive_dt")]
        public DateTime? ReceiveDt { get; set; }

    }
}
