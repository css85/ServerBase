using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_recommend")]
    [Index(nameof(RecommendUserSeq))]
    public class UserRecommendModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Key]
        [Column("recommend_user_seq")]
        public long RecommendUserSeq { get; set; }

        [Column("recommend_count")]
        public int RecommendCount { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }

    }
}
