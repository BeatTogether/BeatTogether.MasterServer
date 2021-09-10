namespace BeatTogether.MasterServer.Interface.Events
{
    public record PlayerConnectedToMatchmakingServerEvent(
        string RemoteEndPoint,
        string UserId,
        string UserName,
        byte[] Random,
        byte[] PublicKey);
}
