using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Implementations;
using BeatTogether.MasterServer.Messaging.Messages.User;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IUserService
    {
        Task<AuthenticateUserResponse> Authenticate(MasterServerSession session, AuthenticateUserRequest request);
        Task<ConnectToServerResponse> ConnectToMatchmakingServer(MasterServerSession session, ConnectToMatchmakingServerRequest request);
        //Task<ConnectToServerResponse> ConnectToMatchmakingServer(MasterServerSession session, Messaging.Messages.User.LegacyRequests.ConnectToMatchmakingServerRequest request);

        Task SessionKeepalive(MasterServerSession session, SessionKeepaliveMessage message);
    }
}
