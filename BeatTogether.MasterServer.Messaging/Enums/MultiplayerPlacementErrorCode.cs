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
        // Below are our custom error codes
        GameVersionUnknown = 50,
        GameVersionTooOld,
		GameVersionTooNew
    }
}