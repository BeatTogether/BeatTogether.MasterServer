using System.Net;
using BeatTogether.MasterServer.Data.Enums;

namespace BeatTogether.MasterServer.Data.Entities
{
    public class Server
    {
        public Player Host { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }
        public string Secret { get; set; }
        public string Code { get; set; }
        public bool IsPublic { get; set; }
        public DiscoveryPolicy DiscoveryPolicy { get; set; }
        public InvitePolicy InvitePolicy { get; set; }
        public BeatmapDifficultyMask BeatmapDifficultyMask { get; set; }
        public GameplayModifiersMask GameplayModifiersMask { get; set; }
        public ulong SongPackBloomFilterTop { get; set; }
        public ulong SongPackBloomFilterBottom { get; set; }
        public int CurrentPlayerCount { get; set; }
        public int MaximumPlayerCount { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }
    }
}
