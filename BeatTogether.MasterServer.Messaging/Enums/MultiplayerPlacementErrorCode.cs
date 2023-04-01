namespace BeatTogether.MasterServer.HttpApi.Models.Enums
{
    public enum MultiplayerPlacementErrorCode
    {
        Success,
        Unknown,
        ConnectionCanceled,
        ServerDoesNotExist,
        ServerAtCapacity,
        AuthenticationFailed,
        RequestTimeout,
        MatchmakingTimeout
    }
}