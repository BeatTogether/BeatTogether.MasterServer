using BeatTogether.Core.Messaging.Implementations.Registries;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Messages;
using BeatTogether.MasterServer.Messaging.Messages.DedicatedServer;

namespace BeatTogether.MasterServer.Messaging.Registries
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
