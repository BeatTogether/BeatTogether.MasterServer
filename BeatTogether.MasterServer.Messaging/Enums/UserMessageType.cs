namespace BeatTogether.MasterServer.Messaging.Enums
{
	public enum UserMessageType
	{
		AuthenticateUserRequest,
		AuthenticateUserResponse,
		BroadcastServerStatusRequest,
		BroadcastServerStatusResponse,
		BroadcastServerHeartbeatRequest,
		BroadcastServerHeartbeatResponse,
		BroadcastServerRemoveRequest,
		ConnectToServerRequest,
		ConnectToServerResponse,
		ConnectToMatchmakingRequest,
		PrepareForConnectionRequest,
		GetPublicServersRequest,
		GetPublicServersResponse,
		MessageReceivedAcknowledge,
		MultipartMessage,
		SessionKeepaliveMessage
	}
}
