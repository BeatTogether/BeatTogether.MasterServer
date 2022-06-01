using BeatTogether.MasterServer.Interface.ApiInterface.Enums;

namespace BeatTogether.MasterServer.Interface.ApiInterface.Models
{
    public record SimpleServer(string Secret, string Code, string ServerName, string ServerId, int CurrentPlayerCount, GameplayServerConfiguration GameplayServerConfiguration, BeatmapDifficultyMask BeatmapDifficultyMask, GameplayModifiersMask GameplayModifiersMask, string EndPoint);
}
