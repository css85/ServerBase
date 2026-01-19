using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_posting_like")]
    [Index(nameof(UserSeq))]
    [Index(nameof(UserSeq), nameof(TargetUserSeq))]

    public class UserPostingLikeModel
    {
        [Key]        
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Key]
        [Column("target_posting_seq")]
        public long TargetPostingSeq { get; set; }

        [Column("target_user_seq")]
        public long TargetUserSeq { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }
    }
}
