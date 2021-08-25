using BeatTogether.Core.Messaging.Implementations.Registries;
using BeatTogether.Core.Messaging.Messages;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Messages.User;

namespace BeatTogether.MasterServer.Messaging.Implementations.Registries
{
    public class UserMessageRegistry : BaseMessageRegistry
    {
        public override uint MessageGroup => (uint)Enums.MessageGroup.User;

        public UserMessageRegistry()
        {
            Register<AuthenticateUserRequest>(UserMessageType.AuthenticateUserRequest);
            Register<AuthenticateUserResponse>(UserMessageType.AuthenticateUserResponse);
            // Register<UserServerStatusUpdateRequest>(UserMessageType.UserServerStatusUpdateRequest); -- unused?
            // Register<UserServerStatusUpdateResponse>(UserMessageType.UserServerStatusUpdateResponse); -- unused?
            // Register<UserServerHeartbeatRequest>(UserMessageType.UserServerHeartbeatRequest); -- unused?
            // Register<UserServerHeartbeatResponse>(UserMessageType.UserServerHeartbeatResponse); -- unused?
            // Register<UserServerRemoveRequest>(UserMessageType.UserServerRemoveRequest); -- unused?
            // Register<ConnectToUserServerRequest>(UserMessageType.ConnectToUserServerRequest); -- unused?
            Register<ConnectToServerResponse>(UserMessageType.ConnectToServerResponse);
            Register<ConnectToMatchmakingServerRequest>(UserMessageType.ConnectToMatchmakingServerRequest);
            Register<PrepareForConnectionRequest>(UserMessageType.PrepareForConnectionRequest);
            // Register<GetPublicServersRequest>(UserMessageType.GetPublicServersRequest); -- unused?
            // Register<GetPublicServersResponse>(UserMessageType.GetPublicServersResponse); -- unused?
            Register<AcknowledgeMessage>(UserMessageType.MessageReceivedAcknowledge);
            Register<MultipartMessage>(UserMessageType.MultipartMessage);
            Register<SessionKeepaliveMessage>(UserMessageType.SessionKeepaliveMessage);
        }
    }
}
