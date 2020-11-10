using System;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class HandshakeMessageReceiver : BaseMessageReceiver<IHandshakeService>
    {
        public HandshakeMessageReceiver(
            IServiceProvider serviceProvider,
            IMultipartMessageService multipartMessageService,
            IMessageDispatcher messageDispatcher)
            : base(serviceProvider, multipartMessageService, messageDispatcher)
        {
            AddReliableMessageHandler<ClientHelloRequest, HelloVerifyRequest>(
                (service, session, request) => service.ClientHello(session, request)
            );
            AddReliableMessageHandler<ClientHelloWithCookieRequest, ServerHelloRequest>(
                (service, session, request) => service.ClientHelloWithCookie(session, request)
            );
            AddReliableMessageHandler<ClientKeyExchangeRequest, ChangeCipherSpecRequest>(
                (service, session, request) => service.ClientKeyExchange(session, request)
            );
        }
    }
}
