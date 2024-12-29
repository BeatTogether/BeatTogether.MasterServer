namespace BeatTogether.MasterServer.Messaging.Enums
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
        MatchmakingTimeout,
        LobbyHostVersionMismatch = 50
    }
}