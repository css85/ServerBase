using System;

namespace Shared.ServerApp.Config
{
    public class TokenSettings
    {
        public string Secret { get; set; }
        public TimeSpan TokenExpires { get; set; }
        public TimeSpan WaitQueueTicketExpires { get; set; }
    }
}