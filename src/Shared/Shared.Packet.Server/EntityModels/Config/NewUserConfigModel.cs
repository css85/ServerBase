using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("newuser_config")]
    [Index(nameof(IsActive))]
    public class NewUserConfigModel
    {
        [Key]
        [Column("config_seq")]
        public long ConfigSeq { get; set; }

        [Column("active")]
        public bool IsActive { get; set; }

        [Column("prologue_config_type")]
        public NewUserConfigType PrologueConfigType { get; set; }

        [Column("navi2_tutorial")]
        public NewUserConfigType Navi2Tutorial { get; set; }
    }
}
