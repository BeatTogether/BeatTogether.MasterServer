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
            AddMessageHandler<BroadcastServerHeartbeatRequest, BroadcastServerHeartbeatResponse>(
                (service, session, message) => service.BroadcastServerHeartbeat(session, message)
            );
            AddMessageHandler<BroadcastServerRemoveRequest>(
                (service, session, message) => service.BroadcastServerRemove(session, message)
            );
            AddReliableMessageHandler<ConnectToMatchmakingRequest, ConnectToServerResponse>(
                (service, session, message) => service.ConnectToMatchmaking(session, message)
            );
            AddReliableMessageHandler<ConnectToServerRequest, ConnectToServerResponse>(
                (service, session, message) => service.ConnectToServer(session, message)
            );
            AddMessageHandler<SessionKeepaliveMessage>(
                (service, session, message) => service.SessionKeepalive(session, message)
            );
        }
    }
}
