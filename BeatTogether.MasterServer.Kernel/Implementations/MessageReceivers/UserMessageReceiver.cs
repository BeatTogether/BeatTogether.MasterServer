using System;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.User;

namespace BeatTogether.MasterServer.Kernel.Implementations.MessageReceivers
{
    public class UserMessageReceiver : BaseMessageReceiver<IUserService>
    {
        public UserMessageReceiver(
            IServiceProvider serviceProvider,
            IMultipartMessageService multipartMessageService,
            IMessageDispatcher messageDispatcher)
            : base(serviceProvider, multipartMessageService, messageDispatcher)
        {
            AddMessageHandler<AuthenticateUserRequest, AuthenticateUserResponse>(
                (service, session, message) => service.AuthenticateUser(session, message)
            );
            AddMessageHandler<BroadcastServerStatusRequest, BroadcastServerStatusResponse>(
                (service, session, message) => service.BroadcastServerStatus(session, message)
            );
            AddMessageHandler<BroadcastServerHeartbeatRequest, BroadcastServerHeartbeatResponse>(
                (service, session, message) => service.BroadcastServerHeartbeat(session, message)
            );
            AddMessageHandler<BroadcastServerRemoveRequest>(
                (service, session, message) => service.BroadcastServerRemove(session, message)
            );
            AddMessageHandler<ConnectToMatchmakingRequest, ConnectToServerResponse>(
                (service, session, message) => service.ConnectToMatchmaking(session, message)
            );
            AddMessageHandler<ConnectToServerRequest, ConnectToServerResponse>(
                (service, session, message) => service.ConnectToServer(session, message)
            );
            AddMessageHandler<SessionKeepaliveMessage>(
                (service, session, message) => service.SessionKeepalive(session, message)
            );
        }
    }
}
