namespace BeatTogether.MasterServer.Data.Configuration
{
    public class RedisConfiguration
    {
        public string Endpoint { get; set; } = "127.0.0.1:6379";
        public int ConnectionPoolSize { get; set; } = 10;
    }
}
