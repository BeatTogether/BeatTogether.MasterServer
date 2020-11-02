using System.Threading.Tasks;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake;

namespace BeatTogether.MasterServer.Abstractions
{
    public interface IHandshakeService
    {
        Task<HelloVerifyRequest> ClientHello(ClientHelloRequest request);
        Task<ServerHelloRequest> ClientHelloWithCookie(ClientHelloWithCookieRequest request);
        Task<ChangeCipherSpecRequest> ClientKeyExchange(ClientKeyExchangeRequest request);
    }
}
