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
            Register<ConnectToMatchmakingServerRequest, ConnectToServerResponse>(
                (service, session, request) => service.ConnectToMatchmakingServer(
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
