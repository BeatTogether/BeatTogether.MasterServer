using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Configuration;
using BeatTogether.MasterServer.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Implementations.Messages;
using Krypton.Buffers;
using Microsoft.Extensions.Hosting;
using NetCoreServer;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class MasterServer : UdpServer, IHostedService
    {
        private readonly MasterServerConfiguration _configuration;
        private readonly ISessionService _sessionService;
        private readonly IMessageReader _messageReader;
        private readonly IEncryptedMessageReader _encryptedMessageReader;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly ILogger _logger;

        public MasterServer(
            MasterServerConfiguration configuration,
            ISessionService sessionService,
            IMessageReader messageReader,
            IEncryptedMessageReader encryptedMessageReader,
            IMessageDispatcher messageDispatcher)
            : base(IPEndPoint.Parse(configuration.EndPoint))
        {
            _configuration = configuration;
            _sessionService = sessionService;
            _messageReader = messageReader;
            _encryptedMessageReader = encryptedMessageReader;
            _messageDispatcher = messageDispatcher;
            _logger = Log.ForContext<MasterServer>();
        }

        protected override void OnStarted() => ReceiveAsync();

        protected override void OnReceived(EndPoint endPoint, ReadOnlySpan<byte> buffer)
        {
            _logger.Verbose($"Handling OnReceived (EndPoint='{endPoint}', Size={buffer.Length}).");

            // Drop empty packets
            if (buffer.Length <= 0)
            {
                ReceiveAsync();
                return;
            }

            // Retrieve their session
            if (!_sessionService.TryGetSession(endPoint, out var session))
                session = _sessionService.OpenSession(this, endPoint);

            // Read the message
            var bufferReader = new SpanBufferReader(buffer);
            IMessage message;
            try
            {
                var isEncrypted = bufferReader.ReadBool();
                if (isEncrypted)
                    message = _encryptedMessageReader.ReadFrom(
                        ref bufferReader,
                        session.ReceiveKey, session.ReceiveMac
                    );
                else
                    message = _messageReader.ReadFrom(ref bufferReader);
            }
            catch (Exception e)
            {
                _logger.Warning(e, $"Failed to read message (EndPoint='{session.EndPoint}').");
                ReceiveAsync();
                return;
            }

            // Publish it to the session's receive channel
            if (session.MessageReceiveChannel.Writer.TryWrite(message))
            {
                if (message is IReliableRequest reliableRequest)
                    _messageDispatcher.Send(session, new AcknowledgeMessage()
                    {
                        ResponseId = reliableRequest.RequestId,
                        MessageHandled = true
                    });
            }

            ReceiveAsync();
        }

        protected override void OnError(SocketError error)
            => _logger.Error($"Handling socket error (Error={error}).");

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Starting Master Server (EndPoint='{_configuration.EndPoint}').");
            Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Stopping Master Server (EndPoint='{_configuration.EndPoint}').");
            Stop();
            return Task.CompletedTask;
        }
    }
}
