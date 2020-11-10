using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Configuration;
using BeatTogether.MasterServer.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Implementations.Messages;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class MultipartMessageService : IMultipartMessageService
    {
        private class MultipartMessageAggregator : IDisposable
        {
            private readonly MultipartMessageService _multipartMessageService;
            private readonly MessagingConfiguration _messagingConfiguration;
            private readonly IMessageReader _messageReader;
            private readonly ISession _session;
            private readonly ILogger _logger;

            private readonly ConcurrentDictionary<uint, MultipartMessage> _receivedMultipartMessages;
            private CancellationTokenSource _cancellationTokenSource;

            private uint _multipartMessageId;
            private int _receivedLength;

            public MultipartMessageAggregator(
                MultipartMessageService multipartMessageService,
                MessagingConfiguration messagingConfiguration,
                IMessageReader messageReader,
                ISession session,
                uint multipartMessageId)
            {
                _multipartMessageService = multipartMessageService;
                _messagingConfiguration = messagingConfiguration;
                _messageReader = messageReader;
                _session = session;
                _logger = Log.ForContext<MultipartMessageService>();

                _multipartMessageId = multipartMessageId;
                _receivedMultipartMessages = new ConcurrentDictionary<uint, MultipartMessage>();
                if (_messagingConfiguration.RequestTimeout > 0)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    _cancellationTokenSource.CancelAfter(_messagingConfiguration.RequestTimeout);
                    _cancellationTokenSource.Token.Register(() =>
                    {
                        _logger.Error(new TimeoutException(),
                            "Failed to read entire multipart message " +
                            $"(MultipartMessageId={_multipartMessageId})."
                        );
                        Dispose();
                    });
                }
            }

            public bool AddMultipartMessage(MultipartMessage multipartMessage)
            {
                if (_receivedLength >= multipartMessage.TotalLength)
                    return false;
                if (!_receivedMultipartMessages.TryAdd(multipartMessage.Offset, multipartMessage))
                    return false;
                Interlocked.Add(ref _receivedLength, (int)multipartMessage.Length);
                if (_receivedLength >= multipartMessage.TotalLength)
                    Finish();
                return true;
            }

            public void Finish()
            {
                Dispose();
                var buffer = new GrowingSpanBuffer(stackalloc byte[412]);
                var multipartMessages = _receivedMultipartMessages.OrderBy(kvp => kvp.Key);
                foreach (var kvp in multipartMessages)
                    buffer.WriteBytes(kvp.Value.Data);
                var bufferReader = new SpanBufferReader(buffer.Data);
                var message = _messageReader.ReadFrom(ref bufferReader, 0x00);
                _session.MessageReceiveChannel.Writer.TryWrite(message);
            }

            public void Dispose()
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
                _multipartMessageService._multipartMessageAggregators.TryRemove(_multipartMessageId, out _);
            }
        }

        private readonly MessagingConfiguration _messagingConfiguration;
        private readonly IMessageReader _messageReader;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<uint, MultipartMessageAggregator> _multipartMessageAggregators;

        public MultipartMessageService(
            MessagingConfiguration messagingConfiguration,
            IMessageReader messageReader)
        {
            _messagingConfiguration = messagingConfiguration;
            _messageReader = messageReader;
            _logger = Log.ForContext<MultipartMessageService>();

            _multipartMessageAggregators = new ConcurrentDictionary<uint, MultipartMessageAggregator>();
        }

        public Task HandleMultipartMessage(ISession session, MultipartMessage message)
        {
            _logger.Verbose(
                $"Handling {nameof(MultipartMessage)} " +
                $"(MultipartMessageId={message.MultipartMessageId}, " +
                $"Offset={message.Offset}, " +
                $"Length={message.Length}, " +
                $"TotalLength={message.TotalLength}, " +
                $"Data='{BitConverter.ToString(message.Data)}')."
            );
            var multipartMessageAggregator = _multipartMessageAggregators.GetOrAdd(
                message.MultipartMessageId,
                key => new MultipartMessageAggregator(
                    this,
                    _messagingConfiguration,
                    _messageReader,
                    session,
                    message.MultipartMessageId
                )
            );
            multipartMessageAggregator.AddMultipartMessage(message);
            return Task.CompletedTask;
        }
    }
}
