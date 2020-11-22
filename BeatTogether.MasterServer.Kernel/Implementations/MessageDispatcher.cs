using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Sessions;
using BeatTogether.MasterServer.Kernel.Configuration;
using BeatTogether.MasterServer.Kernel.Enums;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class MessageDispatcher : IMessageDispatcher
    {
        private class RequestAcknowledgementWaiter : IDisposable
        {
            public Type RequestType { get; }

            private readonly TaskCompletionSource<bool> _taskCompletionSource;
            private readonly CancellationTokenRegistration _cancellationTokenRegistration;

            public RequestAcknowledgementWaiter(
                Type requestType,
                CancellationToken cancellationToken = default)
            {
                RequestType = requestType;

                _taskCompletionSource = new TaskCompletionSource<bool>();
                _cancellationTokenRegistration = cancellationToken.Register(() => Cancel());
            }

            public Task<bool> Wait()
                => _taskCompletionSource.Task;

            public bool IsWaiting =>
                !_taskCompletionSource.Task.IsCompleted &&
                !_taskCompletionSource.Task.IsCanceled &&
                !_taskCompletionSource.Task.IsFaulted;

            public void Complete(bool handled = false)
                => _taskCompletionSource.SetResult(handled);

            public void Cancel()
                => _taskCompletionSource.TrySetCanceled();

            public void Dispose()
            {
                _cancellationTokenRegistration.Dispose();
            }
        }

        private readonly MessagingConfiguration _messagingConfiguration;
        private readonly IMessageWriter _messageWriter;
        private readonly IEncryptedMessageWriter _encryptedMessageWriter;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<uint, RequestAcknowledgementWaiter> _requestAcknowledgementWaiters;

        public MessageDispatcher(
            MessagingConfiguration messagingConfiguration,
            IMessageWriter messageWriter,
            IEncryptedMessageWriter encryptedMessageWriter)
        {
            _messagingConfiguration = messagingConfiguration;
            _messageWriter = messageWriter;
            _encryptedMessageWriter = encryptedMessageWriter;
            _logger = Log.ForContext<MessageDispatcher>();

            _requestAcknowledgementWaiters = new ConcurrentDictionary<uint, RequestAcknowledgementWaiter>();
        }

        public async Task Send<T>(
            ISession session,
            T message,
            bool requireAcknowledgement = true,
            CancellationToken cancellationToken = default)
            where T : class, IMessage
        {
            var request = message as IReliableRequest;
            if (request is not null)
            {
                if (request.RequestId == 0)
                    request.RequestId = session.GetNextRequestId();
            }

            if (request is null || !requireAcknowledgement)
            {
                SendInternal(session, message);
                return;
            }

            var requestAcknowledgementWaiter = new RequestAcknowledgementWaiter(request.GetType());
            if (!_requestAcknowledgementWaiters.TryAdd(request.RequestId, requestAcknowledgementWaiter))
                return;

            _logger.Verbose(
                "Waiting for acknowledgement for request of type " +
                $"'{requestAcknowledgementWaiter.RequestType.Name}' " +
                $"(RequestId={request.RequestId})."
            );
            try
            {
                var retryCount = 0;
                while (requestAcknowledgementWaiter.IsWaiting &&
                       retryCount < _messagingConfiguration.MaximumRequestRetries)
                {
                    if (retryCount != 0)
                        _logger.Verbose(
                            "Retrying request of type " +
                            $"'{requestAcknowledgementWaiter.RequestType.Name}' " +
                            $"due to lack of acknowledgement (RequestId={request.RequestId})."
                        );
                    SendInternal(session, request);
                    await Task.WhenAny(
                        requestAcknowledgementWaiter.Wait(),
                        Task.Delay(_messagingConfiguration.RequestRetryDelay, cancellationToken)
                    );
                    retryCount += 1;
                }
            }
            catch (TaskCanceledException)
            {
                requestAcknowledgementWaiter.Cancel();
                throw;
            }
            catch (TimeoutException)
            {
                if (session.State == SessionState.Authenticated)
                    _logger.Debug(
                        "Timed out while waiting for acknowledgement " +
                        $"(EndPoint='{session.EndPoint}')."
                    );
                else
                    _logger.Debug(
                        "Timed out while waiting for acknowledgement " +
                        $"(EndPoint='{session.EndPoint}', " +
                        $"Platform={session.Platform}, " +
                        $"UserId='{session.UserId}', " +
                        $"UserName='{session.UserName}', " +
                        $"Secret='{session.Secret}')."
                    );
            }
            catch (Exception e)
            {
                LogError(session, e);
            }
            finally
            {
                _requestAcknowledgementWaiters.TryRemove(request.RequestId, out _);
            }
        }

        public void AcknowledgeMessage(uint responseId, bool messageHandled)
        {
            if (!_requestAcknowledgementWaiters.TryGetValue(responseId, out var requestAcknowledgementWaiter))
            {
                _logger.Debug(
                    "Received acknowledgement for an invalid request " +
                    $"(ResponseId={responseId}, " +
                    $"MessageHandled={messageHandled})."
                );
                return;
            }
            _logger.Verbose(
                "Received acknowledgement for request of type " +
                $"'{requestAcknowledgementWaiter.RequestType.Name}' " +
                $"(ResponseId={responseId}, " +
                $"MessageHandled={messageHandled})."
            );
            requestAcknowledgementWaiter.Complete(messageHandled);
        }

        #region Private Methods

        private void SendInternal<T>(ISession session, T message)
            where T : class, IMessage
        {
            var buffer = new GrowingSpanBuffer(stackalloc byte[412]);
            if (session.State != SessionState.New && message is IEncryptedMessage encryptedMessage)
            {
                encryptedMessage.SequenceId = session.GetNextRequestId();
                buffer.WriteBool(true);
                _encryptedMessageWriter.WriteTo(ref buffer, message, session.SendKey, session.SendMac);
            }
            else
            {
                buffer.WriteBool(false);
                _messageWriter.WriteTo(ref buffer, message);
            }

            _logger.Verbose(
                "Sending message " +
                $"(EndPoint='{session.EndPoint}', " +
                $"Data='{BitConverter.ToString(buffer.Data.ToArray())}')."
            );
            session.MasterServer.SendAsync(session.EndPoint, buffer.Data);
        }

        private void LogError(
            ISession session,
            Exception e,
            [CallerMemberName] string callerMemberName = "")
        {
            if (session.State == SessionState.Authenticated)
                _logger.Error(e,
                    $"Error during {callerMemberName} " +
                    $"(EndPoint='{session.EndPoint}', " +
                    $"Platform={session.Platform}, " +
                    $"UserId='{session.UserId}', " +
                    $"UserName='{session.UserName}', " +
                    $"Secret='{session.Secret}')."
                );
            else
                _logger.Error(e,
                    $"Error during {callerMemberName} " +
                    $"(EndPoint='{session.EndPoint}')."
                );
        }

        #endregion
    }
}
