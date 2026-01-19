using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_posting_comment")]
    [Index(nameof(TargetPostingSeq))]
    [Index(nameof(CommentSeq) ,nameof(TargetPostingSeq))]

    public class UserPostingCommentModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("comment_seq")]
        public long CommentSeq { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("user_seq")]
        public long UserSeq { get; set; }


        [Column("target_posting_seq")]
        public long TargetPostingSeq { get; set; }

        [Column("comment")]
        [MaxLength(512)]
        public string Comment { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }
    }
}
