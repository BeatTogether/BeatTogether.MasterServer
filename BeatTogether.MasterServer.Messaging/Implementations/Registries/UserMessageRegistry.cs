using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Implementations.Messages;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.User;

namespace BeatTogether.MasterServer.Messaging.Implementations.Registries
{
    public class UserMessageRegistry : BaseMessageRegistry
    {
        public override uint MessageGroup => (uint)Enums.MessageGroup.User;

        public UserMessageRegistry()
        {
            Register<AuthenticateUserRequest>(UserMessageType.AuthenticateUserRequest);
            Register<AuthenticateUserResponse>(UserMessageType.AuthenticateUserResponse);
            Register<BroadcastServerStatusRequest>(UserMessageType.BroadcastServerStatusRequest);
            Register<BroadcastServerStatusResponse>(UserMessageType.BroadcastServerStatusResponse);
            Register<BroadcastServerHeartbeatRequest>(UserMessageType.BroadcastServerHeartbeatRequest);
            Register<BroadcastServerHeartbeatResponse>(UserMessageType.BroadcastServerHeartbeatResponse);
            Register<BroadcastServerRemoveRequest>(UserMessageType.BroadcastServerRemoveRequest);
            Register<ConnectToServerRequest>(UserMessageType.ConnectToServerRequest);
            Register<ConnectToServerResponse>(UserMessageType.ConnectToServerResponse);
            Register<ConnectToMatchmakingRequest>(UserMessageType.ConnectToMatchmakingRequest);
            Register<PrepareForConnectionRequest>(UserMessageType.PrepareForConnectionRequest);
            // Register<GetPublicServersRequest>(UserMessageType.GetPublicServersRequest); -- unused?
            // Register<GetPublicServersResponse>(UserMessageType.GetPublicServersResponse); -- unused?
            Register<AcknowledgeMessage>(UserMessageType.MessageReceivedAcknowledge);
            Register<MultipartMessage>(UserMessageType.MultipartMessage);
            Register<SessionKeepaliveMessage>(UserMessageType.SessionKeepaliveMessage);
        }
    }
}
