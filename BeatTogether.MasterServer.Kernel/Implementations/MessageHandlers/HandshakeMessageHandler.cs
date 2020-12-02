using System;
using BeatTogether.Core.Messaging.Implementations;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Messaging.Messages.Handshake;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class HandshakeMessageHandler : BaseMessageHandler<IHandshakeService>
    {
        public HandshakeMessageHandler(
            MasterServerMessageSource messageSource,
            MasterServerMessageDispatcher messageDispatcher,
            IServiceProvider serviceProvider)
            : base(messageSource, messageDispatcher, serviceProvider)
        {
            Register<ClientHelloRequest, HelloVerifyRequest>(
                (service, session, request) => service.ClientHello(
                    (MasterServerSession)session, request
                )
            );
            Register<ClientHelloWithCookieRequest, ServerHelloRequest>(
                (service, session, request) => service.ClientHelloWithCookie(
                    (MasterServerSession)session, request
                )
            );
            Register<ClientKeyExchangeRequest, ChangeCipherSpecRequest>(
                (service, session, request) => service.ClientKeyExchange(
                    (MasterServerSession)session, request
                )
            );
        }
    }
}
