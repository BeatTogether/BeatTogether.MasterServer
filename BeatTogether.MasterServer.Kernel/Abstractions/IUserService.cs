using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions.Sessions;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.User;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IUserService
    {
        Task<AuthenticateUserResponse> AuthenticateUser(ISession session, AuthenticateUserRequest request);
        Task<BroadcastServerHeartbeatResponse> BroadcastServerHeartbeat(ISession session, BroadcastServerHeartbeatRequest request);
        Task BroadcastServerRemove(ISession session, BroadcastServerRemoveRequest request);
        Task<BroadcastServerStatusResponse> BroadcastServerStatus(ISession session, BroadcastServerStatusRequest request);
        Task<ConnectToServerResponse> ConnectToServer(ISession session, ConnectToServerRequest request);
        Task<ConnectToServerResponse> ConnectToMatchmaking(ISession session, ConnectToMatchmakingRequest request);
        Task SessionKeepalive(ISession session, SessionKeepaliveMessage request);
    }
}
