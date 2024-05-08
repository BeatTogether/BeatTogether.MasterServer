using System;

namespace BeatTogether.MasterServer.Api.Configuration
{
    public class ApiServerConfiguration
    {
        public string EndPoint { get; set; } = "127.0.0.1:2328";
        public int SessionTimeToLive { get; set; } = 180;
        public bool AuthenticateClients { get; set; } = true;
    }
}
