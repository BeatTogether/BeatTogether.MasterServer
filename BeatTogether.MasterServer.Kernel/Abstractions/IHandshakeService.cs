using BeatTogether.MasterServer.Kernel.Models;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IHandshakeService
    {
        HelloVerifyRequest ClientHello(Session session, ClientHelloRequest request);
        (ServerHelloRequest, ServerCertificateRequest) ClientHelloWithCookie(Session session, ClientHelloWithCookieRequest request);
        ChangeCipherSpecRequest ClientKeyExchange(Session session, ClientKeyExchangeRequest request);
    }
}
