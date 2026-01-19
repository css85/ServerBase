using System;
using System.Numerics;

namespace Shared.Packet.Models
{
    [Serializable]
    public class CurrencyInfo
    {
        public CurrencyType Currency { get; set; }
        public BigInteger Amount { get; set; }

        public CurrencyInfo() { }
        public CurrencyInfo( CurrencyType type , BigInteger amount)
        {
            Currency = type;    
            Amount = amount;    
        }
    }
}
