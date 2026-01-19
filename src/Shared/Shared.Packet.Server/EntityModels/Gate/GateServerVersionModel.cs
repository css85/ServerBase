using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Server.Define;

namespace Shared.Entities.Models
{
    [Table("gate_server_version")]
    public class GateServerVersionModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("version")]
        public int ClientVersion { get; set; }

        [Key]
        [Column("os_type")]
        public OSType OsType { get; set; }

        [Required]
        [Column("server_type")]
        public ServerLocationType ServerType { get; set; } 
    }
}
