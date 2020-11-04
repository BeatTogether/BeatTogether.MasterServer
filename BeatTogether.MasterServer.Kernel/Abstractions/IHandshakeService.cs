using System.Threading.Tasks;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IHandshakeService
    {
        Task<HelloVerifyRequest> ClientHello(ClientHelloRequest request);
        Task<(ServerHelloRequest, ServerCertificateRequest)> ClientHelloWithCookie(ClientHelloWithCookieRequest request);
        Task<ChangeCipherSpecRequest> ClientKeyExchange(ClientKeyExchangeRequest request);
    }
}
