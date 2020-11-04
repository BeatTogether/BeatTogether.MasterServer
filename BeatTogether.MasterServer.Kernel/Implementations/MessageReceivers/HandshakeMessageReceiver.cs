using System;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Messaging.Implementations;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake;
using BeatTogether.MasterServer.Messaging.Implementations.Registries;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class HandshakeMessageReceiver : BaseMessageReceiver<HandshakeMessageRegistry, IHandshakeService>
    {
        public HandshakeMessageReceiver(
            IServiceProvider serviceProvider,
            MessageReader<HandshakeMessageRegistry> messageReader,
            MessageWriter<HandshakeMessageRegistry> messageWriter)
            : base(serviceProvider, messageReader, messageWriter)
        {
            AddReliableMessageHandler<ClientHelloRequest, HelloVerifyRequest>(
                (service, session, request) => service.ClientHello(session, request)
            );
            AddReliableMessageHandler<ClientHelloWithCookieRequest, ServerHelloRequest, ServerCertificateRequest>(
                (service, session, request) => service.ClientHelloWithCookie(session, request)
            );
            AddReliableMessageHandler<ClientKeyExchangeRequest, ChangeCipherSpecRequest>(
                (service, session, request) => service.ClientKeyExchange(session, request)
            );
        }
    }
}
