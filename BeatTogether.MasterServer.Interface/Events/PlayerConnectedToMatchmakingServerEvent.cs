using BeatTogether.MasterServer.Interface.ApiInterface.Enums;

namespace BeatTogether.MasterServer.Interface.Events
{
    public record PlayerConnectedToMatchmakingServerEvent(
        string NodeEndpoint,
        string RemoteEndPoint,
        string UserId,
        string UserName,
        Platform Platform,
        byte[] Random,
        byte[] PublicKey);
}
