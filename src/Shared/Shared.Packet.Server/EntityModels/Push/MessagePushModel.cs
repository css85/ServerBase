using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Entities.Models
{
    [Table("message_push")]
    [Index(nameof(ReservationTime), nameof(SendYn))]
    public class MessagePushModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("push_seq")]
        public long PushSeq { get; set; }

        [Column("send_yn")]        
        public bool SendYn { get; set; }

        [Column("title")]
        [MaxLength(4096)]
        public string Title { get; set; }

        [Column("message")]
        [MaxLength(4096)]
        public string Message { get; set; }

        [Column("language")]
        [MaxLength(32)]
        public string Language { get; set; }

        [Column("reservation_time")]
        public DateTime ReservationTime { get; set; }

        [Column("send_time")]
        public DateTime? SendTime { get; set; }

        [Column("reg_dt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime RegDt { get; set; }

    }
}
