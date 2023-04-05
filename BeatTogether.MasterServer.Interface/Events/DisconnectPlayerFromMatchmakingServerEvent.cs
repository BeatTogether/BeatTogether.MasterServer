namespace BeatTogether.MasterServer.Interface.Events
{
    public record DisconnectPlayerFromMatchmakingServerEvent(
        string Secret,
        string UserId,
        string UserEndPoint
        );
}
