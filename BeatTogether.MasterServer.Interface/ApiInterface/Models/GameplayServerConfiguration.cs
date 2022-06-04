using BeatTogether.MasterServer.Interface.ApiInterface.Enums;

namespace BeatTogether.MasterServer.Interface.ApiInterface.Models
{
    public record GameplayServerConfiguration(
        int MaxPlayerCount,
        DiscoveryPolicy DiscoveryPolicy,
        InvitePolicy InvitePolicy,
        GameplayServerMode GameplayServerMode,
        SongSelectionMode SongSelectionMode,
        GameplayServerControlSettings GameplayServerControlSettings);
}
