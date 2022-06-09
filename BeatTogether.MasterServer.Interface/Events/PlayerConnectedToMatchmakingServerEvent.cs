namespace BeatTogether.MasterServer.Interface.Events
{
    public record PlayerConnectedToMatchmakingServerEvent(
        string NodeEndpoint,
        string RemoteEndPoint,
        string UserId,
        string UserName,
        byte[] Random,
        byte[] PublicKey);
}
