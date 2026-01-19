using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_response_friends")]
    [Index(nameof(ResponseUserSeq))]
    public class UserResponseFriendsModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Key]
        [Column("response_user_seq")]
        public long ResponseUserSeq { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }

    }
}
