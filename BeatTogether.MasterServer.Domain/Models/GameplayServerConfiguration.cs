using BeatTogether.MasterServer.Domain.Enums;

namespace BeatTogether.MasterServer.Domain.Models
{
    public record GameplayServerConfiguration(
        int MaxPlayerCount,
        DiscoveryPolicy DiscoveryPolicy,
        InvitePolicy InvitePolicy,
        GameplayServerMode GameplayServerMode,
        SongSelectionMode SongSelectionMode,
        GameplayServerControlSettings GameplayServerControlSettings);
}
