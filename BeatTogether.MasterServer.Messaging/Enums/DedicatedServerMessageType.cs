namespace BeatTogether.MasterServer.Messaging.Enums
{
    public enum DedicatedServerMessageType : uint
    {
        AuthenticateDedicatedServerRequest,
        AuthenticateDedicatedServerResponse,
        GetAvailableRelayServerRequest,
        GetAvailableRelayServerResponse,
        GetAvailableMatchmakingServerRequest,
        GetAvailableMatchmakingServerResponse,
        DedicatedServerNoLongerOccupiedRequest,
        DedicatedServerHeartbeatRequest,
        DedicatedServerHeartbeatResponse,
        RelayServerStatusUpdateRequest,
        MatchmakingServerStatusUpdateRequest,
        DedicatedServerShutDownRequest,
        DedicatedServerPrepareForConnectionRequest,
        MessageReceivedAcknowledge,
        MultipartMessage
    }
}
