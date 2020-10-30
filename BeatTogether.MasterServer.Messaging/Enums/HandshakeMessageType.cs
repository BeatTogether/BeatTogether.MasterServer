namespace BeatTogether.MasterServer.Messaging.Enums
{
	public enum HandshakeMessageType
	{
		ClientHelloRequest,
		HelloVerifyRequest,
		ClientHelloWithCookieRequest,
		ServerHelloRequest,
		ServerCertificateRequest,
		ServerCertificateResponse,
		ClientKeyExchangeRequest,
		ChangeCipherSpecRequest,
		MessageReceivedAcknowledge,
		MultipartMessage
	}
}
