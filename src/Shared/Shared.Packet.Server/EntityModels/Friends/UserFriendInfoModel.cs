using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_friend_info")]
    [Index(nameof(UserCode))]
    public class UserFriendInfoModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Column("usercode")]
        [MaxLength(16)]
        public string UserCode { get; set; }

        [Column("send_code")]
        public bool SendCode { get; set; }

        [Column("send_code_dt")]
        public DateTime SendCodeDt { get; set; }

        [Column("recommend_user_code")]
        public bool RecommendUserCode { get; set; }

        [Column("recommend_user_code_dt")]
        public DateTime RecommendUserCodeDt { get; set; }


        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }
    }
}
