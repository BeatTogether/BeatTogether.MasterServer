using System;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Messaging.Implementations;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.User;
using BeatTogether.MasterServer.Messaging.Implementations.Registries;

namespace BeatTogether.MasterServer.Kernel.Implementations.MessageReceivers
{
    public class UserMessageReceiver : BaseMessageReceiver<IUserService>
    {
        protected override bool UseEncryption => true;

        public UserMessageReceiver(
            IServiceProvider serviceProvider,
            IRequestIdProvider requestIdProvider,
            IMultipartMessageService multipartMessageService,
            MessageReader<UserMessageRegistry> messageReader,
            MessageWriter<UserMessageRegistry> messageWriter)
            : base(serviceProvider, requestIdProvider, multipartMessageService, messageReader, messageWriter)
        {
            AddReliableMessageHandler<AuthenticateUserRequest, AuthenticateUserResponse>(
                (service, session, message) => service.AuthenticateUser(session, message)
            );
            AddReliableMessageHandler<BroadcastServerStatusRequest, BroadcastServerStatusResponse>(
                (service, session, message) => service.BroadcastServerStatus(session, message)
            );
            AddMessageHandler<SessionKeepaliveMessage>(
                (service, session, message) => service.SessionKeepalive(session, message)
            );
        }
    }
}
