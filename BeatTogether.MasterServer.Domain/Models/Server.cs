using System.Net;
using BeatTogether.MasterServer.Domain.Enums;

namespace BeatTogether.MasterServer.Domain.Models
{
    public sealed class Server
    {

        public string ServerName { get; set; }
        public string ServerId { get; set; }
        public IPEndPoint LiteNetEndPoint { get; set; }
        public IPEndPoint ENetEndPoint { get; set; }
        public string Secret { get; set; }
        public string Code { get; set; }
        public bool IsPublic { get; set; }
        public bool IsInGameplay { get; set; }
        public BeatmapDifficultyMask BeatmapDifficultyMask { get; set; }
        public GameplayModifiersMask GameplayModifiersMask { get; set; }
        public GameplayServerConfiguration GameplayServerConfiguration { get; set; }
        public ulong SongPackBloomFilterTop { get; set; }
        public ulong SongPackBloomFilterBottom { get; set; }
        public int CurrentPlayerCount { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }
    }
}
