using System.Threading.Tasks;
using BeatTogether.MasterServer.Client.Abstractions;
using BeatTogether.MasterServer.Messaging.Messages.Handshake;

namespace BeatTogether.MasterServer.Client.Implementations
{
    public class ClientHandshakeService : IClientHandshakeService
    {
        private readonly MasterServerClient _client;
        private readonly MasterServerClientMessageDispatcher _messageDispatcher;

        public ClientHandshakeService(
            MasterServerClient client,
            MasterServerClientMessageDispatcher messageDispatcher)
        {
            _client = client;
            _messageDispatcher = messageDispatcher;
        }

        public Task<HelloVerifyRequest> ClientHello(ClientHelloRequest request) =>
            _messageDispatcher.SendWithRetry<HelloVerifyRequest>(_client.Session, request);

        public Task<ServerHelloRequest> ClientHelloWithCookie(ClientHelloWithCookieRequest request) =>
            _messageDispatcher.SendWithRetry<ServerHelloRequest>(_client.Session, request);

        public Task<ChangeCipherSpecRequest> ClientKeyExchange(ClientKeyExchangeRequest request) =>
            _messageDispatcher.SendWithRetry<ChangeCipherSpecRequest>(_client.Session, request);
    }
}
