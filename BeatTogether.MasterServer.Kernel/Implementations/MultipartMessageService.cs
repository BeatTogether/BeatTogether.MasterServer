using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Configuration;
using BeatTogether.MasterServer.Messaging.Implementations.Messages;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class MultipartMessageService : IMultipartMessageService
    {
        private class MultipartMessageWaiter
        {
            private readonly MultipartMessageService _multipartMessageService;
            private readonly ConcurrentDictionary<uint, MultipartMessage> _receivedMultipartMessages;

            private TaskCompletionSource<byte[]> _taskCompletionSource;
            private CancellationTokenSource _cancellationTokenSource;

            private int _totalLength;
            private int _receivedLength;

            public MultipartMessageWaiter(MultipartMessageService multipartMessageService, uint multipartMessageId)
            {
                _multipartMessageService = multipartMessageService;

                _receivedMultipartMessages = new ConcurrentDictionary<uint, MultipartMessage>();

                _taskCompletionSource = new TaskCompletionSource<byte[]>();
                if (_multipartMessageService._configuration.RequestTimeout > 0)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    _cancellationTokenSource.CancelAfter(_multipartMessageService._configuration.RequestTimeout);
                    _cancellationTokenSource.Token.Register(
                        () =>
                        {
                            if (_taskCompletionSource != null)
                                _taskCompletionSource.TrySetException(new TimeoutException());

                            if (_cancellationTokenSource != null)
                            {
                                _cancellationTokenSource.Dispose();
                                _cancellationTokenSource = null;
                            }

                            _multipartMessageService.RemoveMultipartMessageWaiter(multipartMessageId);
                        }
                    );
                }
            }

            public Task<byte[]> Wait()
                => _taskCompletionSource.Task;

            public bool AddMultipartMessage(MultipartMessage multipartMessage)
            {
                if (!_receivedMultipartMessages.TryAdd(multipartMessage.Offset, multipartMessage))
                    return false;
                _totalLength = (int)multipartMessage.TotalLength;
                Interlocked.Add(ref _receivedLength, (int)multipartMessage.Length);
                if (_receivedLength >= _totalLength)
                {
                    Finish();
                    _multipartMessageService.RemoveMultipartMessageWaiter(multipartMessage.MultipartMessageId);
                }
                return true;
            }

            public void Finish()
            {
                if (_taskCompletionSource != null)
                {
                    var buffer = new byte[_totalLength];
                    var sortedMultipartMessages = _receivedMultipartMessages
                        .ToList()
                        .OrderBy(kvp => kvp.Key)
                        .Select(kvp => kvp.Value);
                    foreach (var multipartMessage in sortedMultipartMessages)
                        Array.Copy(
                            multipartMessage.Data,
                            0,
                            buffer,
                            multipartMessage.Offset,
                            multipartMessage.Length
                        );
                    _taskCompletionSource.TrySetResult(buffer);
                }

                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
            }
        }

        private readonly MessagingConfiguration _configuration;
        private readonly ConcurrentDictionary<uint, MultipartMessageWaiter> _multipartMessageWaiters;

        public MultipartMessageService(MessagingConfiguration configuration)
        {
            _configuration = configuration;
            _multipartMessageWaiters = new ConcurrentDictionary<uint, MultipartMessageWaiter>();
        }

        public Task<byte[]> WaitForEntireMessage(uint multipartMessageId)
        {
            if (!_multipartMessageWaiters.TryGetValue(multipartMessageId, out var multipartMessageWaiter))
                return Task.FromResult(new byte[0]);
            return multipartMessageWaiter.Wait();
        }

        public void OnReceived(MultipartMessage multipartMessage)
        {
            if (!_multipartMessageWaiters.TryGetValue(
                    multipartMessage.MultipartMessageId,
                    out var multipartMessageWaiter))
                return;
            multipartMessageWaiter.AddMultipartMessage(multipartMessage);
        }

        public bool AddMultipartMessageWaiter(uint multipartMessageId)
        {
            if (!_multipartMessageWaiters.TryAdd(multipartMessageId, null))
                return false;
            _multipartMessageWaiters[multipartMessageId] = new MultipartMessageWaiter(this, multipartMessageId);
            return true;
        }

        public bool RemoveMultipartMessageWaiter(uint multipartMessageId)
            => _multipartMessageWaiters.TryRemove(multipartMessageId, out _);
    }
}
