namespace BeatTogether.MasterServer.Messaging.Enums
{
    public enum UserMessageType : uint
    {
        AuthenticateUserRequest = 0,
        AuthenticateUserResponse = 1,
        UserServerStatusUpdateRequest = 2,
        UserServerStatusUpdateResponse = 3,
        UserServerHeartbeatRequest = 4,
        UserServerHeartbeatResponse = 5,
        UserServerRemoveRequest = 6,
        ConnectToUserServerRequest = 7,
        ConnectToServerResponse = 8,
        ConnectToMatchmakingServerRequest = 9,
        PrepareForConnectionRequest = 10,
        GetPublicUserServersRequest = 11,
        GetPublicUserServersResponse = 12,
        MessageReceivedAcknowledge = 13,
        MultipartMessage = 14,
        SessionKeepaliveMessage = 15
    }
}
