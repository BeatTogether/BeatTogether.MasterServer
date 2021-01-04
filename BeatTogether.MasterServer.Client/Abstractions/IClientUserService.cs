using System.Threading.Tasks;
using BeatTogether.MasterServer.Messaging.Messages.User;

namespace BeatTogether.MasterServer.Client.Abstractions
{
    public interface IClientUserService
    {
        Task<AuthenticateUserResponse> Authenticate(AuthenticateUserRequest request);
        Task BroadcastServerHeartbeat(BroadcastServerHeartbeatRequest request);
        Task BroadcastServerRemove(BroadcastServerRemoveRequest request);
        Task<BroadcastServerStatusResponse> BroadcastServerStatus(BroadcastServerStatusRequest request);
        Task<ConnectToServerResponse> ConnectToServer(ConnectToServerRequest request);
        Task<ConnectToServerResponse> ConnectToMatchmaking(ConnectToMatchmakingRequest request);
        Task SessionKeepalive(SessionKeepaliveMessage message);
    }
}
