using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Shared.Server.Define;

namespace Shared.Entities.Models
{
    [Table("user_account_link")]    
    [Index(nameof(AccountType), nameof(AccountId))]
    
    public class AccountLinkModel
    {
        [Key]        
        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Key]
        [Column("account_type")]
        public AccountType AccountType { get; set; }    

        [Column("account_id")]
        public string AccountId { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }

        
    }
}
