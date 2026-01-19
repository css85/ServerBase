using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Shared.Server.Define;

namespace Shared.Entities.Models
{
    [Table("user_account")]    
    [Index(nameof(AccountType), nameof(AccountId))]
    [Index(nameof(UserSeq), nameof(LoginDt))]

    public class AccountModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Column("api_token")]
        [MaxLength(2048)]
        public string ApiToken { get; set; }

        [Column("block")]
        public bool Block { get; set; }

        [Column("block_end_dt")]        
        public DateTime BlockEndDt { get; set; }

        [Column("today_first_login_dt")]
        public DateTime TodayFirstLoginDt { get; set; }

        [Column("login_dt")]
        public DateTime LoginDt { get; set; }

        [Column("logout_dt")]
        public DateTime LogOutDt { get; set; }

        [Column("token_expire_dt")]
        public DateTime? TokenExpireDt { get; set; }

        [Column("account_type")]
        public AccountType AccountType { get; set; }    

        [Column("account_id")]
        public string AccountId { get; set; }

        [Column("push_token")]
        public string PushToken { get; set; }

        [Column("country_code")]
        public string CountryCode { get; set; }

        [Column("server_push")]
        public bool ServerPush { get; set; }

        [Column("night_push")]
        public bool NightPush { get; set; }


        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }

        
    }
}
