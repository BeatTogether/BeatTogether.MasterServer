namespace BeatTogether.MasterServer.Kernel.Configuration
{
    public class MasterServerConfiguration
    {
        public string EndPoint { get; set; } = "127.0.0.1:2328";
        public int SessionTimeToLive { get; set; } = 180;
        public string MasterServerVersion { get; } = "1.1";
        public string[] SupportedDediServerVersions { get; } = { "1.1" };
        public bool AuthenticateClients { get; set; } = true;
        public bool AllowNoodle = false;
    }
}
