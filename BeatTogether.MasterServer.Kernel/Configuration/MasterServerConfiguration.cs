namespace BeatTogether.MasterServer.Kernel.Configuration
{
    public class MasterServerConfiguration
    {
        public string EndPoint { get; set; } = "127.0.0.1:2328";
        public int SessionTimeToLive { get; set; } = 180;
        public string MasterServerVersion { get; } = "1.1.2";
        public string[] SupportedDediServerVersions { get; } = { "1.2.4" }; //for example, if 1.1 is here, then 1.1.1, 1.1.5, 1.1.23, would all be accepted verisions and 1.2.3 would not
        public bool AuthenticateClients { get; set; } = true;
    }
}
