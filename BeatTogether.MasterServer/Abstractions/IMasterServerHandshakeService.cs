using System.Threading.Tasks;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake;

namespace BeatTogether.MasterServer.Abstractions
{
    public interface IMasterServerHandshakeService
    {
        Task<HelloVerifyRequest> ClientHello(ClientHelloRequest request);
        Task<ServerHelloRequest> ClientHelloWithCookie(ClientHelloWithCookieRequest request);
        Task<ChangeCipherSpecRequest> ClientHello(ClientKeyExchangeRequest request);
    }
}
