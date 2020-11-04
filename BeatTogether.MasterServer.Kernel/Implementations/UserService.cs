using System;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.User;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class UserService : IUserService
    {
        private readonly ILogger _logger;

        public UserService()
        {
            _logger = Log.ForContext<UserService>();
        }

        public AuthenticateUserResponse AuthenticateUser(AuthenticateUserRequest request)
        {
            throw new NotImplementedException();
        }

        public BroadcastServerHeartbeatResponse BroadcastServerHeartbeat(BroadcastServerHeartbeatRequest request)
        {
            throw new NotImplementedException();
        }

        public void BroadcastServerRemove(BroadcastServerRemoveRequest request)
        {
            throw new NotImplementedException();
        }

        public BroadcastServerStatusResponse BroadcastServerStatus(BroadcastServerStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public ConnectToServerResponse ConnectToMatchmaking(ConnectToMatchmakingRequest request)
        {
            throw new NotImplementedException();
        }

        public ConnectToServerResponse ConnectToServer(ConnectToServerRequest request)
        {
            throw new NotImplementedException();
        }

        public void SessionKeepalive(SessionKeepaliveMessage request)
        {
            throw new NotImplementedException();
        }
    }
}
