using BeatTogether.MasterServer.Messaging.Implementations.Messages.User;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IUserService
    {
        AuthenticateUserResponse AuthenticateUser(AuthenticateUserRequest request);
        BroadcastServerHeartbeatResponse BroadcastServerHeartbeat(BroadcastServerHeartbeatRequest request);
        void BroadcastServerRemove(BroadcastServerRemoveRequest request);
        BroadcastServerStatusResponse BroadcastServerStatus(BroadcastServerStatusRequest request);
        ConnectToServerResponse ConnectToServer(ConnectToServerRequest request);
        ConnectToServerResponse ConnectToMatchmaking(ConnectToMatchmakingRequest request);
        void SessionKeepalive(SessionKeepaliveMessage request);
    }
}
