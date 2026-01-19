using System;

namespace Shared.Models.Gate
{
    public class WaitQueueTicketModel
    {
        public int AppGroupId { get; set; }
        public long UserSeq { get; set; }

        public long TicketSeq { get; set; } //순번
        public long ExpiryTicks { get; set; }
        public long ExpiryTimeTicks { get; set; }
    }
}