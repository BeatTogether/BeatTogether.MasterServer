using System;

namespace BeatTogether.MasterServer.Kernel.Configuration
{
    public class MasterServerConfiguration
    {
        public string EndPoint { get; set; } = "127.0.0.1:2328";
        public int SessionTimeToLive { get; set; } = 180;
        public Version MasterServerVersion { get; } = new(1,5,0);
        public Version[] SupportedDediServerVersions { get; } = { new(1,6,0) }; //for example, if 1.1 is here, then 1.1.1, 1.1.5, 1.1.23, would all be accepted verisions and 1.2.3 would not. Only change when dedi and master would be incompat otherwise
        public bool AuthenticateClients { get; set; } = true;
        public long TicksBetweenUpdatingCachedApiResponses { get; set; } = TimeSpan.TicksPerSecond;
    }
}
