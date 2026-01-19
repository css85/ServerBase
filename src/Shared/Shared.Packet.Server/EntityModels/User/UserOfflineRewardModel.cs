using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("user_offline_reward")]
    
    public class UserOfflineRewardModel
    {        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("user_seq")]
        public long UserSeq { get; set; }

        [Column("rewarded")]
        public bool Rewarded { get; set; }

        [Column("ad_rewarded")]
        public bool AdRewarded { get; set; }

        [Column("offline_time_min")]
        public int OfflineTimeMin { get; set; }

        [Column("reward_infos")]
        public string RewardInfos { get; set; }

        [Column("create_reward_dt")]        
        public DateTime CreateRewardDt { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }

    }
}
