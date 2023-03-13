using BeatTogether.MasterServer.Interface.ApiInterface.Enums;

namespace BeatTogether.MasterServer.Interface.Events
{
    public record PlayerConnectedToMatchmakingServerEvent( //Node only uses NodeEndpoint,RemoteEndPoint and encryption keys. Other data is for API when done
        string NodeEndpoint,
        string RemoteEndPoint,
        string UserId,
        string UserName,
        Platform Platform,
        byte[] Random,
        byte[] PublicKey);
}
