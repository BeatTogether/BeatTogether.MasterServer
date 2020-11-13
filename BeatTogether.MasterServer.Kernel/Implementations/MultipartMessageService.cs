using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Configuration;
using BeatTogether.MasterServer.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Implementations.Messages;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class MultipartMessageService : IMultipartMessageService
    {
        private class MultipartMessageWaiter
        {
            private readonly MultipartMessageService _service;
            private readonly ILogger _logger;

            private readonly ConcurrentDictionary<uint, MultipartMessage> _messages;
            private TaskCompletionSource<IMessage> _taskCompletionSource;
            private CancellationTokenSource _cancellationTokenSource;

            private uint _multipartMessageId;
            private uint _totalLength;
            private uint _receivedLength;

            public MultipartMessageWaiter(
                uint multipartMessageId,
                uint totalLength,
                MultipartMessageService service)
            {
                _service = service;
                _logger = Log.ForContext<MultipartMessageService>();

                _multipartMessageId = multipartMessageId;
                _totalLength = totalLength;

                _messages = new ConcurrentDictionary<uint, MultipartMessage>();

                _taskCompletionSource = new TaskCompletionSource<IMessage>();
                if (_service._messagingConfiguration.RequestTimeout > 0)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    _cancellationTokenSource.CancelAfter(_service._messagingConfiguration.RequestTimeout);
                    _cancellationTokenSource.Token.Register(() =>
                    {
                        if (_taskCompletionSource != null)
                            _taskCompletionSource.TrySetException(new TimeoutException());

                        if (_cancellationTokenSource != null)
                        {
                            _cancellationTokenSource.Dispose();
                            _cancellationTokenSource = null;
                        }

                        _service._multipartMessageWaiters.TryRemove(_multipartMessageId, out _);
                    });
                }
            }

            public Task<IMessage> Wait()
                => _taskCompletionSource.Task;

            public void Complete(IMessage message)
            {
                if (_taskCompletionSource != null)
                    _taskCompletionSource.TrySetResult(message);

                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
            }

            public void AddMessage(MultipartMessage message)
            {
                if (message.MultipartMessageId != _multipartMessageId)
                    return;
                if (_receivedLength >= _totalLength)
                    return;
                if (!_messages.TryAdd(message.Offset, message))
                    return;
                if (Interlocked.Add(ref _receivedLength, message.Length) >= _totalLength)
                {
                    var buffer = new GrowingSpanBuffer(stackalloc byte[(int)_totalLength]);
                    foreach (var kvp in _messages.OrderBy(kvp => kvp.Key))
                        buffer.WriteBytes(kvp.Value.Data);
                    var bufferReader = new SpanBufferReader(buffer.Data);
                    var fullMessage = _service._messageReader.ReadFrom(ref bufferReader, 0x00);
                    Complete(fullMessage);
                }
            }
        }

        private readonly MessagingConfiguration _messagingConfiguration;
        private readonly IMessageReader _messageReader;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<uint, MultipartMessageWaiter> _multipartMessageWaiters;

        public MultipartMessageService(
            MessagingConfiguration messagingConfiguration,
            IMessageReader messageReader)
        {
            _messagingConfiguration = messagingConfiguration;
            _messageReader = messageReader;
            _logger = Log.ForContext<MultipartMessageService>();

            _multipartMessageWaiters = new ConcurrentDictionary<uint, MultipartMessageWaiter>();
        }

        public Task<IMessage> HandleMultipartMessage(ISession session, MultipartMessage message)
        {
            _logger.Verbose(
                $"Handling {nameof(MultipartMessage)} " +
                $"(MultipartMessageId={message.MultipartMessageId}, " +
                $"Offset={message.Offset}, " +
                $"Length={message.Length}, " +
                $"TotalLength={message.TotalLength}, " +
                $"Data='{BitConverter.ToString(message.Data)}')."
            );
            bool isNewMultipartMessageWaiter = false;
            var multipartMessageWaiter = _multipartMessageWaiters.GetOrAdd(message.MultipartMessageId, key =>
            {
                isNewMultipartMessageWaiter = true;
                return new MultipartMessageWaiter(
                    message.MultipartMessageId,
                    message.TotalLength,
                    this
                );
            });
            multipartMessageWaiter.AddMessage(message);
            if (isNewMultipartMessageWaiter)
                return multipartMessageWaiter.Wait();
            return Task.FromResult<IMessage>(null);
        }
    }
}
