using BeatTogether.Core.Messaging.Implementations.Registries;
using BeatTogether.Core.Messaging.Messages;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Messages.Handshake;

namespace BeatTogether.MasterServer.Messaging.Implementations.Registries
{
    public class HandshakeMessageRegistry : BaseMessageRegistry
    {
        public override uint MessageGroup => (uint)Enums.MessageGroup.Handshake;

        public HandshakeMessageRegistry()
        {
            Register<ClientHelloRequest>(HandshakeMessageType.ClientHelloRequest);
            Register<HelloVerifyRequest>(HandshakeMessageType.HelloVerifyRequest);
            Register<ClientHelloWithCookieRequest>(HandshakeMessageType.ClientHelloWithCookieRequest);
            Register<ServerHelloRequest>(HandshakeMessageType.ServerHelloRequest);
            Register<ServerCertificateRequest>(HandshakeMessageType.ServerCertificateRequest);
            // Register<ServerCertificateResponse>(HandshakeMessageType.ServerCertificateResponse); -- unused?
            Register<ClientKeyExchangeRequest>(HandshakeMessageType.ClientKeyExchangeRequest);
            Register<ChangeCipherSpecRequest>(HandshakeMessageType.ChangeCipherSpecRequest);
            Register<AcknowledgeMessage>(HandshakeMessageType.MessageReceivedAcknowledge);
            Register<MultipartMessage>(HandshakeMessageType.MultipartMessage);
        }
    }
}
