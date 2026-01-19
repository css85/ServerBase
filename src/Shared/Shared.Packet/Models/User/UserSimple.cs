
using System;

namespace Shared.Packet.Models
{
    [Serializable]
    public class UserSimple : UserIdentity
    {   
        public string Nick { get; set; }
        public int Level { get; set; }
        public int ShoppingmallGrade { get; set; }   // 새싹, 브론즈...  
    }
}
