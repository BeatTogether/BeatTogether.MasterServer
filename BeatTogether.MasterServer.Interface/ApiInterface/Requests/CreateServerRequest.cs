using BeatTogether.MasterServer.Interface.ApiInterface.Enums;
using BeatTogether.MasterServer.Interface.ApiInterface.Models;

namespace BeatTogether.MasterServer.Interface.ApiInterface.Requests
{
    public record CreateServerRequest(string ManagerId, 
        GameplayServerConfiguration GameplayServerConfiguration,
        bool PermanantManager,
        float Timeout,
        string ServerName,
        BeatmapDifficultyMask BeatmapDifficultyMask,
        GameplayModifiersMask GameplayModifiersMask,
        SongPackMask SongPackMask,
        string Code = "",
        string Secret = "");
}
