using System;
using System.Collections.Generic;

namespace Shared.Packet.Models
{
    [Serializable]
    public class PurchaseData
    {
        public StoreType StoreType { get; set; }
        public string TransactionId { get; set; }
        public string ProductId { get; set; }
        public string Payload { get; set; }
    }
}
