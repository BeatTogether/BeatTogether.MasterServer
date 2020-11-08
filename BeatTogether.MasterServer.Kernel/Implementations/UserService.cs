using System;
using System.Net;
using System.Threading.Tasks;
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

        public Task<AuthenticateUserResponse> AuthenticateUser(ISession session, AuthenticateUserRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(AuthenticateUserRequest)} " +
                $"(Platform={request.AuthenticationToken.Platform}, " +
                $"UserId='{request.AuthenticationToken.UserId}', " +
                $"UserName='{request.AuthenticationToken.UserName}')."
            );
            // TODO: Verify that there aren't any other sessions with the same user ID
            // TODO: Validate session token?
            session.Platform = (Enums.Platform)request.AuthenticationToken.Platform;
            session.UserId = request.AuthenticationToken.UserId;
            session.UserName = request.AuthenticationToken.UserName;
            return Task.FromResult(new AuthenticateUserResponse()
            {
                Result = AuthenticateUserResponse.ResultCode.Success
            });
        }

        public Task<BroadcastServerStatusResponse> BroadcastServerStatus(ISession session, BroadcastServerStatusRequest request)
        {
            return Task.FromResult(new BroadcastServerStatusResponse()
            {
                Result = BroadcastServerStatusResponse.ResultCode.Success,
                Code = "00000",
                RemoteEndPoint = (IPEndPoint)session.EndPoint
            });
        }

        public Task SessionKeepalive(ISession session, SessionKeepaliveMessage message)
        {
            _logger.Verbose(
                $"Handling {nameof(SessionKeepalive)} " +
                $"(EndPoint='{session.EndPoint}', " +
                $"UserId='{session.UserId}', " +
                $"UserName='{session.UserName}')."
            );
            // TODO: Keep the session alive
            return Task.CompletedTask;
        }

        public Task<BroadcastServerHeartbeatResponse> BroadcastServerHeartbeat(ISession session, BroadcastServerHeartbeatRequest request)
        {
            throw new NotImplementedException();
        }

        public Task BroadcastServerRemove(ISession session, BroadcastServerRemoveRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<ConnectToServerResponse> ConnectToMatchmaking(ISession session, ConnectToMatchmakingRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<ConnectToServerResponse> ConnectToServer(ISession session, ConnectToServerRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
