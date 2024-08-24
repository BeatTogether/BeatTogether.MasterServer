using System;

namespace BeatTogether.MasterServer.Api.Configuration
{
    public class ApiServerConfiguration
    {
        public int SessionTimeToLive { get; set; } = 180;
        public bool AuthenticateClients { get; set; } = true;
    }
}
