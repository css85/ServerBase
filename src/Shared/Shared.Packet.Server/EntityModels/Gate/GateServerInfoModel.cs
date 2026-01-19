using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Shared.Server.Define;

namespace Shared.Entities.Models
{
    [Table("gate_server_info")]
    [Index(nameof(ServerType))]
    public class GateServerInfoModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("server_type")]
        public ServerLocationType ServerType { get; set; }

        [Key]
        [Column("net_service_type")]
        public NetServiceType NetServiceType { get; set; }

        [Required]
        [Column("url")]
        [MaxLength(512)]
        public string URL { get; set; }
    }
}
