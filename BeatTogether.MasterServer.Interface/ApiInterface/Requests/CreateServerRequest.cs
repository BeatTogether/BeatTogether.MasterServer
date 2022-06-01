using BeatTogether.MasterServer.Interface.ApiInterface.Enums;
using BeatTogether.MasterServer.Interface.ApiInterface.Models;
using BeatTogether.DedicatedServer.Interface.Models;

namespace BeatTogether.MasterServer.Interface.ApiInterface.Requests
{
    public record CreateServerRequest(string ManagerId, GameplayServerConfiguration GameplayServerConfiguration, bool PermenantManager, float Timeout, string ServerName, bool IsPublic, BeatmapDifficultyMask BeatmapDifficultyMask, GameplayModifiersMask GameplayModifiersMask, SongPackMask SongPackMask, string Code = "", string Secret = "");
}
