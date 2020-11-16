using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions.Sessions;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IHandshakeService
    {
        Task<HelloVerifyRequest> ClientHello(ISession session, ClientHelloRequest request);
        Task<ServerHelloRequest> ClientHelloWithCookie(ISession session, ClientHelloWithCookieRequest request);
        Task<ChangeCipherSpecRequest> ClientKeyExchange(ISession session, ClientKeyExchangeRequest request);
    }
}
