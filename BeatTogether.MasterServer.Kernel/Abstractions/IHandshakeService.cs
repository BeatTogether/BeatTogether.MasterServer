using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Implementations;
using BeatTogether.MasterServer.Messaging.Messages.Handshake;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IHandshakeService
    {
        Task<HelloVerifyRequest> ClientHello(MasterServerSession session, ClientHelloRequest request);
        Task<ServerHelloRequest> ClientHelloWithCookie(MasterServerSession session, ClientHelloWithCookieRequest request);
        Task<ChangeCipherSpecRequest> ClientKeyExchange(MasterServerSession session, ClientKeyExchangeRequest request);
    }
}
