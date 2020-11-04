using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Implementations.Messages;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake;

namespace BeatTogether.MasterServer.Messaging.Implementations.Registries
{
    public class HandshakeMessageRegistry : BaseMessageRegistry
    {
        public override MessageGroup MessageGroup => MessageGroup.Handshake;
        public override uint ProtocolVersion => 1;

        public HandshakeMessageRegistry()
        {
            Register<ClientHelloRequest>(HandshakeMessageType.ClientHelloRequest);
            Register<HelloVerifyRequest>(HandshakeMessageType.HelloVerifyRequest);
            Register<ClientHelloWithCookieRequest>(HandshakeMessageType.ClientHelloWithCookieRequest);
            Register<ServerHelloRequest>(HandshakeMessageType.ServerHelloRequest);
            Register<ServerCertificateRequest>(HandshakeMessageType.ServerCertificateRequest);
            // Register<ServerCertificateResponse>(HandshakeMessageType.ServerCertificateResponse);
            Register<ClientKeyExchangeRequest>(HandshakeMessageType.ClientKeyExchangeRequest);
            Register<ChangeCipherSpecRequest>(HandshakeMessageType.ChangeCipherSpecRequest);
            Register<AcknowledgeMessage>(HandshakeMessageType.MessageReceivedAcknowledge);
            Register<MultipartMessage>(HandshakeMessageType.MultipartMessage);
        }
    }
}
