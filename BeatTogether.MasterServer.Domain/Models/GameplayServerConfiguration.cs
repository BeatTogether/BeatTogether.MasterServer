using BeatTogether.MasterServer.Domain.Enums;

namespace BeatTogether.MasterServer.Domain.Models
{
    public sealed class GameplayServerConfiguration
    {
        public int MaxPlayerCount { get; set; }
        public DiscoveryPolicy DiscoveryPolicy { get; set; }
        public InvitePolicy InvitePolicy { get; set; }
        public GameplayServerMode GameplayServerMode { get; set; }
        public SongSelectionMode SongSelectionMode { get; set; }
        public GameplayServerControlSettings GameplayServerControlSettings { get; set; }

        public GameplayServerConfiguration(int maxPlayerCount, DiscoveryPolicy discoveryPolicy, InvitePolicy invitePolicy, GameplayServerMode gameplayServerMode, SongSelectionMode songSelectionMode, GameplayServerControlSettings gameplayServerControlSettings)
        {
            MaxPlayerCount = maxPlayerCount;
            DiscoveryPolicy = discoveryPolicy;
            InvitePolicy = invitePolicy;
            GameplayServerMode = gameplayServerMode;
            GameplayServerControlSettings = gameplayServerControlSettings;
        }
    }
}
