namespace BeatTogether.MasterServer.Data.Entities
{
    public class Server
    {
        public Player Host { get; set; }
        public string Code { get; set; }
        public bool IsPublic { get; set; }
        public int MaximumPlayerCount { get; set; }
    }
}
