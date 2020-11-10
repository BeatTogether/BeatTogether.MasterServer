namespace BeatTogether.MasterServer.Messaging.Enums
{
	public enum HandshakeMessageType : uint
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
