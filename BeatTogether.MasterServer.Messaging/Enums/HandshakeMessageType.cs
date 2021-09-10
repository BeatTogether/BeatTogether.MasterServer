namespace BeatTogether.MasterServer.Messaging.Enums
{
    public enum HandshakeMessageType : uint
    {
        ClientHelloRequest = 0,
        HelloVerifyRequest = 1,
        ClientHelloWithCookieRequest = 2,
        ServerHelloRequest = 3,
        ServerCertificateRequest = 4,
        ServerCertificateResponse = 5,
        ClientKeyExchangeRequest = 6,
        ChangeCipherSpecRequest = 7,
        MessageReceivedAcknowledge = 8,
        MultipartMessage = 9
    }
}
