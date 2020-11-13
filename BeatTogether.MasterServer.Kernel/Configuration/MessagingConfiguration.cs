namespace BeatTogether.MasterServer.Kernel.Configuration
{
    public class MessagingConfiguration
    {
        public int RequestTimeout { get; set; } = 10000;
        public int MaximumRequestRetries { get; set; } = 5;
        public int RequestRetryDelay { get; set; } = 500;
    }
}
