using System;

namespace Shared.Packet.Models
{
    [Serializable]
    public class NewUserConfig
    {        
        public NewUserConfigType PrologueConfigType { get; set; }
        public NewUserConfigType Navi2Tutorial { get; set; }

        public NewUserConfig() 
        {
            PrologueConfigType = NewUserConfigType.On;
            Navi2Tutorial = NewUserConfigType.On;
        }
    }
}
