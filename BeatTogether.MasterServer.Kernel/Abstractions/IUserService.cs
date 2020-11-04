using System.Threading.Tasks;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.User;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IUserService
    {
        Task<AuthenticateUserResponse> AuthenticateUser(AuthenticateUserRequest request);
        Task<BroadcastServerHeartbeatResponse> BroadcastServerHeartbeat(BroadcastServerHeartbeatRequest request);
        Task BroadcastServerRemove(BroadcastServerRemoveRequest request);
        Task<BroadcastServerStatusResponse> BroadcastServerStatus(BroadcastServerStatusRequest request);
        Task<ConnectToServerResponse> ConnectToServer(ConnectToServerRequest request);
        Task<ConnectToServerResponse> ConnectToMatchmaking(ConnectToMatchmakingRequest request);
        Task SessionKeepalive(SessionKeepaliveMessage request);
    }
}
