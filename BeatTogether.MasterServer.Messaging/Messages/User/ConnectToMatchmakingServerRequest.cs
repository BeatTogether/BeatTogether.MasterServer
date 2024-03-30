using BeatTogether.MasterServer.Messaging.Models;

namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public sealed class ConnectToMatchmakingServerRequest
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }
        public BeatmapLevelSelectionMask BeatmapLevelSelectionMask { get; set; } = new();
        public string Secret { get; set; }
        public string Code { get; set; }
        public GameplayServerConfiguration GameplayServerConfiguration { get; set; } = new();
    }
}
