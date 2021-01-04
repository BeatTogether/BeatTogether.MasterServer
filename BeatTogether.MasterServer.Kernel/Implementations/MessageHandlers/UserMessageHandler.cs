using System;
using BeatTogether.Core.Messaging.Implementations;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Messaging.Messages.User;

namespace BeatTogether.MasterServer.Kernel.Implementations.MessageReceivers
{
    public class UserMessageHandler : BaseMessageHandler<IUserService>
    {
        public UserMessageHandler(
            MasterServerMessageSource messageSource,
            MasterServerMessageDispatcher messageDispatcher,
            IServiceProvider serviceProvider)
            : base(messageSource, messageDispatcher, serviceProvider)
        {
            Register<AuthenticateUserRequest, AuthenticateUserResponse>(
                (service, session, request) => service.Authenticate(
                    (MasterServerSession)session, request
                )
            );
            Register<BroadcastServerStatusRequest, BroadcastServerStatusResponse>(
                (service, session, request) => service.BroadcastServerStatus(
                    (MasterServerSession)session, request
                )
            );
            Register<BroadcastServerHeartbeatRequest>(
                (service, session, request) => service.BroadcastServerHeartbeat(
                    (MasterServerSession)session, request
                )
            );
            Register<BroadcastServerRemoveRequest>(
                (service, session, request) => service.BroadcastServerRemove(
                    (MasterServerSession)session, request
                )
            );
            Register<ConnectToMatchmakingRequest, ConnectToServerResponse>(
                (service, session, request) => service.ConnectToMatchmaking(
                    (MasterServerSession)session, request
                )
            );
            Register<ConnectToServerRequest, ConnectToServerResponse>(
                (service, session, request) => service.ConnectToServer(
                    (MasterServerSession)session, request
                )
            );
            Register<SessionKeepaliveMessage>(
                (service, session, message) => service.SessionKeepalive(
                    (MasterServerSession)session, message
                )
            );
        }
    }
}
