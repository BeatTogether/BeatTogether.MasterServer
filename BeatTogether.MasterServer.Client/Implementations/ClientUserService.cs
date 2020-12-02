using System.Threading.Tasks;
using BeatTogether.MasterServer.Client.Abstractions;
using BeatTogether.MasterServer.Messaging.Messages.User;

namespace BeatTogether.MasterServer.Client.Implementations
{
    public class ClientUserService : IClientUserService
    {
        private readonly MasterServerClient _client;
        private readonly MasterServerClientMessageDispatcher _messageDispatcher;

        public ClientUserService(
            MasterServerClient client,
            MasterServerClientMessageDispatcher messageDispatcher)
        {
            _client = client;
            _messageDispatcher = messageDispatcher;
        }

        public Task<AuthenticateUserResponse> Authenticate(AuthenticateUserRequest request) =>
            _messageDispatcher.SendWithRetry<AuthenticateUserResponse>(_client.Session, request);

        public Task BroadcastServerHeartbeat(BroadcastServerHeartbeatRequest request)
        {
            _messageDispatcher.Send(_client.Session, request);
            return Task.CompletedTask;
        }

        public Task BroadcastServerRemove(BroadcastServerRemoveRequest request)
        {
            _messageDispatcher.Send(_client.Session, request);
            return Task.CompletedTask;
        }

        public Task<BroadcastServerStatusResponse> BroadcastServerStatus(BroadcastServerStatusRequest request) =>
            _messageDispatcher.SendWithRetry<BroadcastServerStatusResponse>(_client.Session, request);

        public Task<ConnectToServerResponse> ConnectToMatchmaking(ConnectToMatchmakingRequest request) =>
            _messageDispatcher.SendWithRetry<ConnectToServerResponse>(_client.Session, request);

        public Task<ConnectToServerResponse> ConnectToServer(ConnectToServerRequest request) =>
            _messageDispatcher.SendWithRetry<ConnectToServerResponse>(_client.Session, request);

        public Task SessionKeepalive(SessionKeepaliveMessage message)
        {
            _messageDispatcher.Send(_client.Session, message);
            return Task.CompletedTask;
        }
    }
}
