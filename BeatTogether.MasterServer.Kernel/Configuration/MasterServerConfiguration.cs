namespace BeatTogether.MasterServer.Kernel.Configuration
{
    public class MasterServerConfiguration
    {
        public string EndPoint { get; set; } = "192.168.1.227:2328";
        public int SessionTimeToLive { get; set; } = 180;
    }
}
