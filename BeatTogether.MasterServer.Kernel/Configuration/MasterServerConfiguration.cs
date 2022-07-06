namespace BeatTogether.MasterServer.Kernel.Configuration
{
    public class MasterServerConfiguration
    {
        public string EndPoint { get; set; } = "192.168.1.227:2328";
        public int SessionTimeToLive { get; set; } = 180;
        public string MasterServerVersion { get; } = "1.0";
        public string[] SupportedDediServerVersions { get; } = { "1.0" };
    }
}
