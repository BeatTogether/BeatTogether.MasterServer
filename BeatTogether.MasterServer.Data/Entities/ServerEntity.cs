namespace BeatTogether.MasterServer.Data.Entities
{
    public class ServerEntity
    {
        public PlayerEntity Host { get; set; }
        public string Code { get; set; }
        public bool IsPublic { get; set; }
        public int MaximumPlayerCount { get; set; }
    }
}
