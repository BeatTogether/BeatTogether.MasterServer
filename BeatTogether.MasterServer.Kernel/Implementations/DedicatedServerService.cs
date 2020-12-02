using System;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Messaging.Messages.DedicatedServer;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class DedicatedServerService : IDedicatedServerService
    {
        private readonly ILogger _logger;

        public DedicatedServerService()
        {
            _logger = Log.ForContext<DedicatedServerService>();
        }

        public Task<AuthenticateDedicatedServerResponse> Authenticate(MasterServerSession session, AuthenticateDedicatedServerRequest request)
        {
            throw new NotImplementedException();
        }

        public Task Heartbeat(MasterServerSession session, DedicatedServerHeartbeatRequest request)
        {
            throw new NotImplementedException();
        }

        public Task MatchmakingServerStatusUpdate(MasterServerSession session, MatchmakingServerStatusUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public Task NoLongerOccupied(MasterServerSession session, DedicatedServerNoLongerOccupiedRequest request)
        {
            throw new NotImplementedException();
        }

        public Task RelayServerStatusUpdate(MasterServerSession session, RelayServerStatusUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public Task ShutDown(MasterServerSession session, DedicatedServerShutDownRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
