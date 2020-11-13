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
            AddMessageHandler<ClientHelloRequest, HelloVerifyRequest>(
                (service, session, request) => service.ClientHello(session, request),
                requireAcknowledgement: false
            );
            AddMessageHandler<ClientHelloWithCookieRequest, ServerHelloRequest>(
                (service, session, request) => service.ClientHelloWithCookie(session, request)
            );
            AddMessageHandler<ClientKeyExchangeRequest, ChangeCipherSpecRequest>(
                (service, session, request) => service.ClientKeyExchange(session, request)
            );
        }
    }
}
