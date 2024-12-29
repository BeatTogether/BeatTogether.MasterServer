using System;
using System.Collections.Generic;
using BeatTogether.Core.Enums;
using BeatTogether.Core.Models;
using BeatTogether.MasterServer.Api.Util;

namespace BeatTogether.MasterServer.Api.Configuration
{
    public sealed class ApiServerConfiguration
    {
        public int SessionTimeToLive { get; set; } = 180;
        public bool AuthenticateClients { get; set; } = true;
        public HashSet<VersionRange> VersionRanges { get; set; } = new();
        public HashSet<Platform> AuthedClients { get; set; } = new();
    }
}
