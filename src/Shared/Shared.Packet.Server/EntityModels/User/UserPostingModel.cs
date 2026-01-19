using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_posting")]
    [Index(nameof(UserSeq))]
    [Index(nameof(PostingSeq), nameof(ViewPostings))]

    public class UserPostingModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("posting_seq")]
        public long PostingSeq { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Column("like_count")]
        public int LikeCount { get; set; }

        [Column("comment_count")]
        public int CommentCount { get; set; }

        [Column("parts")]
        public string DecorateInfo { get; set; }

        [Column("last_view_like_count")]
        public int LastViewLikeCount { get; set; }

        [Column("last_view_comment_count")]
        public int LastViewCommentCount { get; set; }

        [Column("title")]
        [MaxLength(1028)]
        public string Title { get; set; }

        [Column("view_postings")]
        public bool ViewPostings { get; set; }


        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }
    }
}
