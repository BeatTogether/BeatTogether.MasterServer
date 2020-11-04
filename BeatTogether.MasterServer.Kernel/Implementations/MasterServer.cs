using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Configuration;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Enums;
using BeatTogether.MasterServer.Kernel.Implementations.MessageReceivers;
using BeatTogether.MasterServer.Kernel.Models;
using Microsoft.Extensions.Hosting;
using NetCoreServer;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class MasterServer : UdpServer, IHostedService
    {
        private readonly MasterServerConfiguration _configuration;
        private readonly ISessionService _sessionService;
        private readonly HandshakeMessageReceiver _handshakeMessageReceiver;
        private readonly UserMessageReceiver _userMessageReceiver;
        private readonly ILogger _logger;

        public MasterServer(
            MasterServerConfiguration configuration,
            ISessionService sessionService,
            HandshakeMessageReceiver handshakeMessageReceiver,
            UserMessageReceiver userMessageReceiver)
            : base(IPEndPoint.Parse(configuration.EndPoint))
        {
            _configuration = configuration;
            _sessionService = sessionService;
            _handshakeMessageReceiver = handshakeMessageReceiver;
            _userMessageReceiver = userMessageReceiver;
            _logger = Log.ForContext<MasterServer>();
        }

        protected override void OnStarted() => ReceiveAsync();

        protected override void OnReceived(EndPoint endPoint, ReadOnlySpan<byte> buffer)
        {
            _logger.Verbose($"Handling OnReceived (EndPoint='{endPoint}', Size={buffer.Length}).");
            if (buffer.Length <= 0)
            {
                ReceiveAsync();
                return;
            }

            // Create a new session if one doesn't already exist
            if (!_sessionService.TryGetSession(endPoint, out var session))
            {
                _logger.Debug($"Creating new session (EndPoint='{endPoint}').");
                session = new Session()
                {
                    EndPoint = endPoint,
                    State = SessionState.New
                };
                _sessionService.AddSession(session);
            }

            // Pass the message off to a message receiver
            switch (session.State)
            {
                case SessionState.New:
                    _handshakeMessageReceiver.OnReceived(
                        session, buffer,
                        responseBuffer =>
                        {
                            _logger.Verbose($"Sending response (Data='{BitConverter.ToString(responseBuffer.ToArray())}').");
                            SendAsync(session.EndPoint, responseBuffer);
                        }
                    );
                    break;
                default:
                    _userMessageReceiver.OnReceived(
                        session, buffer,
                        responseBuffer => SendAsync(session.EndPoint, responseBuffer)
                    );
                    break;
            }

            ReceiveAsync();
        }

        protected override void OnError(SocketError error)
            => _logger.Error($"Handling socket error (Error={error}).");

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Starting Master Server (Endpoint='{_configuration.EndPoint}').");
            Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Stopping Master Server (Endpoint='{_configuration.EndPoint}').");
            Stop();
            return Task.CompletedTask;
        }
    }
}
