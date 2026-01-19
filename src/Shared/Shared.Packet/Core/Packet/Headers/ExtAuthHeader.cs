using System;

namespace Shared.Packet
{
    [Serializable]
    public class ExtAuthHeader
    {
        public string AuthToken;
        public int Custom = -1;
    }

}
