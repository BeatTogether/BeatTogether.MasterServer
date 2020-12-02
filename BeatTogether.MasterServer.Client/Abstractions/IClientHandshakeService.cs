using System.Threading.Tasks;
using BeatTogether.MasterServer.Messaging.Messages.Handshake;

namespace BeatTogether.MasterServer.Client.Abstractions
{
    public interface IClientHandshakeService
    {
        Task<HelloVerifyRequest> ClientHello(ClientHelloRequest request);
        Task<ServerHelloRequest> ClientHelloWithCookie(ClientHelloWithCookieRequest request);
        Task<ChangeCipherSpecRequest> ClientKeyExchange(ClientKeyExchangeRequest request);
    }
}
