using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Implementations.Messages;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.DedicatedServer;

namespace BeatTogether.MasterServer.Messaging.Implementations.Registries
{
    public class DedicatedServerMessageRegistry : BaseMessageRegistry
    {
        public override uint MessageGroup => (uint)Enums.MessageGroup.DedicatedServer;

        public DedicatedServerMessageRegistry()
        {
            Register<AuthenticateDedicatedServerRequest>(DedicatedServerMessageType.AuthenticateDedicatedServerRequest);
            Register<AuthenticateDedicatedServerResponse>(DedicatedServerMessageType.AuthenticateDedicatedServerResponse);
            Register<GetAvailableRelayServerRequest>(DedicatedServerMessageType.GetAvailableRelayServerRequest);
            Register<GetAvailableRelayServerResponse>(DedicatedServerMessageType.GetAvailableRelayServerResponse);
            Register<GetAvailableMatchmakingServerRequest>(DedicatedServerMessageType.GetAvailableMatchmakingServerRequest);
            Register<GetAvailableMatchmakingServerResponse>(DedicatedServerMessageType.GetAvailableMatchmakingServerResponse);
            Register<DedicatedServerNoLongerOccupiedRequest>(DedicatedServerMessageType.DedicatedServerNoLongerOccupiedRequest);
            Register<DedicatedServerHeartbeatRequest>(DedicatedServerMessageType.DedicatedServerHeartbeatRequest);
            Register<DedicatedServerHeartbeatResponse>(DedicatedServerMessageType.DedicatedServerHeartbeatResponse);
            Register<RelayServerStatusUpdateRequest>(DedicatedServerMessageType.RelayServerStatusUpdateRequest);
            Register<MatchmakingServerStatusUpdateRequest>(DedicatedServerMessageType.MatchmakingServerStatusUpdateRequest);
            Register<DedicatedServerShutDownRequest>(DedicatedServerMessageType.DedicatedServerShutDownRequest);
            Register<DedicatedServerPrepareForConnectionRequest>(DedicatedServerMessageType.DedicatedServerPrepareForConnectionRequest);
            Register<AcknowledgeMessage>(DedicatedServerMessageType.MessageReceivedAcknowledge);
            Register<MultipartMessage>(DedicatedServerMessageType.MultipartMessage);
        }
    }
}
