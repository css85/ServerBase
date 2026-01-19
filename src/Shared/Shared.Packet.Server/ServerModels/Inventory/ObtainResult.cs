using Shared.Packet.Models;
using System;
using System.Collections.Generic;

namespace Shared.ServerModel
{
    public class ObtainResult
    {
        public RewardsInfo RewardsInfo { get; set; } = new RewardsInfo();
        public RefreshGameItem RefreshInfo { get; set; } = new();
    }
}
