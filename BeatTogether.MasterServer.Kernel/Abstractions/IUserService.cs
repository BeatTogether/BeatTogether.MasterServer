using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Implementations;
using BeatTogether.MasterServer.Messaging.Messages.User;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IUserService
    {
        Task<AuthenticateUserResponse> Authenticate(MasterServerSession session, AuthenticateUserRequest request);
        Task BroadcastServerHeartbeat(MasterServerSession session, BroadcastServerHeartbeatRequest request);
        Task BroadcastServerRemove(MasterServerSession session, BroadcastServerRemoveRequest request);
        Task<BroadcastServerStatusResponse> BroadcastServerStatus(MasterServerSession session, BroadcastServerStatusRequest request);
        Task<ConnectToServerResponse> ConnectToServer(MasterServerSession session, ConnectToServerRequest request);
        Task<ConnectToServerResponse> ConnectToMatchmaking(MasterServerSession session, ConnectToMatchmakingRequest request);
        Task SessionKeepalive(MasterServerSession session, SessionKeepaliveMessage message);
    }
}
