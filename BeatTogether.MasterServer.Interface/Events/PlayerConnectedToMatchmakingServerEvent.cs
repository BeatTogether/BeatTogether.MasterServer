namespace BeatTogether.MasterServer.Interface.Events
{
    public record PlayerConnectedToMatchmakingServerEvent(
        string NodeEndpoint,
        string RemoteEndPoint,
        byte[] Random,
        byte[] PublicKey,
        string PlayerSessionId,
        string ClientVersion,
        byte Platform,
        string Secret);
}