using BeatTogether.MasterServer.Interface.ApiInterface.Enums;
using BeatTogether.MasterServer.Interface.ApiInterface.Models;

namespace BeatTogether.MasterServer.Interface.ApiInterface.Requests
{
    public record CreateServerRequest(string ManagerId, 
        GameplayServerConfiguration GameplayServerConfiguration,
        bool PermanentManager,
        float resultScreenTime,
        float BeatmapStartTime,
        float PlayersReadyCountdownTime,
        float Timeout,
        string ServerName,
        BeatmapDifficultyMask BeatmapDifficultyMask,
        GameplayModifiersMask GameplayModifiersMask,
        SongPackMask SongPackMask,
        bool AllowPerPlayerModifiers,
        bool AllowPerPlayerDifficulties,
        bool AllowPerPlayerBeatmaps,
        bool AllowChroma,
        bool AllowME,
        bool AllowNE,
        string Code = "",
        string Secret = "");
}