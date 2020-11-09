using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Security;
using BeatTogether.MasterServer.Kernel.Configuration;
using BeatTogether.MasterServer.Kernel.Enums;
using BeatTogether.MasterServer.Kernel.Implementations.MessageReceivers;
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
        private readonly ICryptoService _cryptoService;
        private readonly HandshakeMessageReceiver _handshakeMessageReceiver;
        private readonly UserMessageReceiver _userMessageReceiver;
        private readonly ILogger _logger;

        private const byte _packetProperty = 0x08;  // LiteNetLib.PacketProperty.UnconnectedMessage

        public MasterServer(
            MasterServerConfiguration configuration,
            ISessionService sessionService,
            ICryptoService cryptoService,
            HandshakeMessageReceiver handshakeMessageReceiver,
            UserMessageReceiver userMessageReceiver)
            : base(IPEndPoint.Parse(configuration.EndPoint))
        {
            _configuration = configuration;
            _sessionService = sessionService;
            _cryptoService = cryptoService;
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
                _logger.Debug($"Session created (EndPoint='{endPoint}').");
                session = new Session(this, endPoint);
                _sessionService.AddSession(session);
            }

            // Determine whether or not this packet is encrypted
            // Unencrypted packets will always go to the handshake message receiver
            var bufferReader = new SpanBufferReader(buffer);
            bool isEncrypted;
            try
            {
                isEncrypted = bufferReader.ReadBool();
                if (isEncrypted)
                {
                    // All encrypted messages should be sequenced
                    var sequenceId = bufferReader.ReadUInt32();
                    // TODO: Message sequencing

                    // Decrypt the message
                    var iv = bufferReader.ReadBytes(_cryptoService.IvLength).ToArray();
                    var decryptedBuffer = bufferReader.ReadBytes(bufferReader.RemainingSize).ToArray();
                    _cryptoService.Decrypt(decryptedBuffer, session.ReceiveKey, iv);

                    var hashBuffer = new GrowingSpanBuffer(stackalloc byte[decryptedBuffer.Length + 4]);
                    var padByteCount = decryptedBuffer[decryptedBuffer.Length - 1] + 1;
                    hashBuffer.WriteBytes(decryptedBuffer.AsSpan().Slice(0, decryptedBuffer.Length - padByteCount - 10));
                    hashBuffer.WriteUInt32(sequenceId);
                    var hmac = decryptedBuffer.AsSpan().Slice(decryptedBuffer.Length - padByteCount - 10, 10).ToArray();
                    var compare = session.ReceiveMac.ComputeHash(hashBuffer.Data.ToArray());
                    bufferReader = new SpanBufferReader(decryptedBuffer.AsSpan().Slice(0, decryptedBuffer.Length - padByteCount - 10));
                    // TODO: Validate HMAC
                }

                var packetProperty = bufferReader.ReadUInt8();
                if (packetProperty != _packetProperty)
                {
                    _logger.Warning(
                        "Invalid packet property " +
                        $"(PacketProperty={packetProperty}, " +
                        $"Expected={_packetProperty})."
                    );
                    return;
                }
            }
            catch (Exception e)
            {
                _logger.Warning(e,
                    "Failed to read message " +
                    $"(EndPoint='{session.EndPoint}', " +
                    $"UserId='{session.UserId}', " +
                    $"UserName='{session.UserName}')."
                );
                ReceiveAsync();
                return;
            }

            // Pass it off to one of the message receivers
            if (!isEncrypted)
                _handshakeMessageReceiver.OnReceived(session, bufferReader.RemainingData);
            else
                _userMessageReceiver.OnReceived(session, bufferReader.RemainingData);
            ReceiveAsync();
        }

        protected override void OnError(SocketError error)
            => _logger.Error($"Handling socket error (Error={error}).");

        public bool SendAsync(ISession session, ReadOnlySpan<byte> buffer)
        {
            _logger.Verbose(
                "Sending message " +
                $"(EndPoint='{session.EndPoint}', " +
                $"Data='{BitConverter.ToString(buffer.ToArray())}')."
            );
            var sendBuffer = new GrowingSpanBuffer(stackalloc byte[412]);
            sendBuffer.WriteBool(false);
            sendBuffer.WriteUInt8(_packetProperty);
            sendBuffer.WriteBytes(buffer);
            return SendAsync(session.EndPoint, sendBuffer.Data);
        }

        public bool SendEncryptedAsync(ISession session, ReadOnlySpan<byte> buffer)
        {
            _logger.Verbose(
                "Sending message " +
                $"(EndPoint='{session.EndPoint}', " +
                $"Data='{BitConverter.ToString(buffer.ToArray())}')."
            );
            if (session.State == SessionState.New)
            {
                _logger.Warning("Attempted to send encrypted message to a session that isn't established.");
                return false;
            }
            
            session.LastSentSequenceId += 1;
            
            var encryptionInputBuffer = new GrowingSpanBuffer(stackalloc byte[412]);
            encryptionInputBuffer.WriteUInt8(_packetProperty);
            encryptionInputBuffer.WriteBytes(buffer);

            var hashBuffer = new GrowingSpanBuffer(stackalloc byte[encryptionInputBuffer.Size + 4]);
            hashBuffer.WriteBytes(encryptionInputBuffer.Data);
            hashBuffer.WriteUInt32(session.LastSentSequenceId);
            var hmac = session.SendMac.ComputeHash(hashBuffer.Data.ToArray()).AsSpan().Slice(0, 10);
            encryptionInputBuffer.WriteBytes(hmac);

            var iv = _cryptoService.GetIv();

            var padByteCount = (16 - (encryptionInputBuffer.Size + 1 & 15)) & 15;
            var padBytes = new byte[padByteCount + 1];
            for (var i = 0; i < padByteCount + 1; i++)
                padBytes[i] = (byte)(padByteCount);
            encryptionInputBuffer.WriteBytes(padBytes);
            var encryptedBuffer = encryptionInputBuffer.Data.ToArray();
            _cryptoService.Encrypt(encryptedBuffer, session.SendKey, iv);

            var sendBuffer = new GrowingSpanBuffer(stackalloc byte[412]);
            sendBuffer.WriteBool(true);
            sendBuffer.WriteUInt32(session.LastSentSequenceId);
            sendBuffer.WriteBytes(iv);
            sendBuffer.WriteBytes(encryptedBuffer);
            return SendAsync(session.EndPoint, sendBuffer.Data);
        }

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
