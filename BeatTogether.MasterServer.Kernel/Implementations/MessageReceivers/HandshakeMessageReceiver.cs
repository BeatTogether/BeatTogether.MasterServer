using System;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Messaging.Implementations;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake;
using BeatTogether.MasterServer.Messaging.Implementations.Registries;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class HandshakeMessageReceiver : BaseMessageReceiver<IHandshakeService>
    {
        public HandshakeMessageReceiver(
            IServiceProvider serviceProvider,
            IRequestIdProvider requestIdProvider,
            MessageReader<HandshakeMessageRegistry> messageReader,
            MessageWriter<HandshakeMessageRegistry> messageWriter)
            : base(serviceProvider, requestIdProvider, messageReader, messageWriter)
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
