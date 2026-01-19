using System;

namespace Shared.Packet
{
    [Serializable]
    public class ExtSessionHeader
    {
        public string SId;
        public int CustomID =-1;
    }

}
