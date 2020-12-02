using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Implementations;
using BeatTogether.MasterServer.Messaging.Messages.DedicatedServer;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IDedicatedServerService
    {
        Task<AuthenticateDedicatedServerResponse> Authenticate(MasterServerSession session, AuthenticateDedicatedServerRequest request);
        Task NoLongerOccupied(MasterServerSession session, DedicatedServerNoLongerOccupiedRequest request);
        Task Heartbeat(MasterServerSession session, DedicatedServerHeartbeatRequest request);
        Task RelayServerStatusUpdate(MasterServerSession session, RelayServerStatusUpdateRequest request);
        Task MatchmakingServerStatusUpdate(MasterServerSession session, MatchmakingServerStatusUpdateRequest request);
        Task ShutDown(MasterServerSession session, DedicatedServerShutDownRequest request);
    }
}
