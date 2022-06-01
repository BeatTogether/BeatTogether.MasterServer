using BeatTogether.DedicatedServer.Interface.Models;
using BeatTogether.MasterServer.Interface.ApiInterface.Enums;
using System.Net;

namespace BeatTogether.MasterServer.Interface.ApiInterface.Models
{
    public record SimpleServer(string Secret, string Code, string ServerName, string ServerId, int CurrentPlayerCount, GameplayServerConfiguration GameplayServerConfiguration, BeatmapDifficultyMask BeatmapDifficultyMask, GameplayModifiersMask GameplayModifiersMask, IPEndPoint EndPoint);
}
