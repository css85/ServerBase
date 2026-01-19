using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Server.Define;

namespace Shared.Entities.Models
{
    [Table("gate_server_maintenance")]
    public class GateServerMaintenanceModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("server_type")]
        public ServerLocationType ServerType { get; set; }

        [Column("is_server_inspection")]
        public bool IsServerInspection { get; set; }

        [Column("inspection_from")]
        public DateTime? InspectionFrom { get; set; }

        [Column("inspection_to")]
        public DateTime? InspectionTo { get; set; }

        [Column("allow_ip_inspection")]
        public string AllowIpInspection { get; set; }

        [Column("block_country")]
        public string BlockCountry { get; set; }

        [Column("allow_ip_country")]
        public string AllowIpCountry { get; set; }

        public bool IsInspectionTime(DateTime now)
        {
            if (InspectionFrom == null || InspectionTo == null)
                return false;
            return (InspectionFrom <= now && now < InspectionTo);
        }
    }
}
