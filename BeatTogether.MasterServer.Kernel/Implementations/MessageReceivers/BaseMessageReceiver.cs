using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Kernel.Abstractions.Security;
using BeatTogether.MasterServer.Kernel.Delegates;
using BeatTogether.MasterServer.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Implementations.Messages;
using Krypton.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public abstract class BaseMessageReceiver<TService> : IMessageReceiver
    {
        protected abstract bool UseEncryption { get; }

        private readonly IServiceProvider _serviceProvider;
        private readonly IRequestIdProvider _requestIdProvider;
        private readonly IMultipartMessageService _multipartMessageService;
        private readonly IMessageReader _messageReader;
        private readonly IMessageWriter _messageWriter;
        private readonly ILogger _logger;

        private readonly Dictionary<Type, MessageHandler<TService>> _messageHandlerByTypeLookup;

        public BaseMessageReceiver(
            IServiceProvider serviceProvider,
            IRequestIdProvider requestIdProvider,
            IMultipartMessageService multipartMessageService,
            IMessageReader messageReader,
            IMessageWriter messageWriter)
        {
            _serviceProvider = serviceProvider;
            _requestIdProvider = requestIdProvider;
            _multipartMessageService = multipartMessageService;
            _messageReader = messageReader;
            _messageWriter = messageWriter;
            _logger = Log.ForContext<BaseMessageReceiver<TService>>();

            _messageHandlerByTypeLookup = new Dictionary<Type, MessageHandler<TService>>();

            AddReliableMessageHandler<MultipartMessage>(
                async (service, session, message) =>
                {
                    _logger.Verbose(
                        $"Handling {nameof(MultipartMessage)} " +
                        $"(MultipartMessageId={message.MultipartMessageId}, " +
                        $"Offset={message.Offset}, " +
                        $"Length={message.Length}, " +
                        $"TotalLength={message.TotalLength}, " +
                        $"Data='{BitConverter.ToString(message.Data)}')."
                    );
                    if (!_multipartMessageService.AddMultipartMessageWaiter(message.MultipartMessageId))
                    {
                        _multipartMessageService.OnReceived(message);
                        return;
                    }

                    _multipartMessageService.OnReceived(message);
                    var buffer = await _multipartMessageService.WaitForEntireMessage(message.MultipartMessageId);
                    OnReceived(session, buffer);
                }
            );
            AddMessageHandler<AcknowledgeMessage>(
                (service, session, message) =>
                {
                    _logger.Verbose(
                        $"Handling {nameof(AcknowledgeMessage)} " +
                        $"(ResponseId={message.ResponseId}, " +
                        $"MessageHandled={message.MessageHandled})."
                    );
                    // TODO: Properly acknowledge that requests were handled (and retry if they weren't)
                    return Task.CompletedTask;
                }
            );
        }

        #region Public Methods

        public void OnReceived(ISession session, ReadOnlySpan<byte> buffer)
        {
            var bufferReader = new SpanBufferReader(buffer);

            IMessage message;
            try
            {
                message = _messageReader.ReadFrom(ref bufferReader);
            }
            catch (Exception e) when (e is IndexOutOfRangeException || e is InvalidDataContractException)
            {
                _logger.Warning(e,
                    "Failed to read message " +
                    $"(EndPoint='{session.EndPoint}', " +
                    $"UserId='{session.UserId}', " +
                    $"UserName='{session.UserName}')."
                );
                return;
            }

            var messageType = message.GetType();
            if (!_messageHandlerByTypeLookup.TryGetValue(messageType, out var messageHandler))
            {
                _logger.Warning(
                    "Failed to retrieve message handler for message of type " +
                    $"'{messageType.Name}'."
                );
                return;
            }

            Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<TService>();
                return messageHandler(service, session, message);
            }).ConfigureAwait(false);
        }

        #endregion

        #region Protected Methods

        protected void AddMessageHandler<TMessage>(MessageHandler<TService, TMessage> messageHandler)
            where TMessage : class, IMessage
            => _messageHandlerByTypeLookup[typeof(TMessage)] =
                    (service, session, message) => messageHandler(service, session, (TMessage)message);

        protected void AddMessageHandler<TRequest, TResponse>(MessageHandler<TService, TRequest, TResponse> messageHandler)
            where TRequest : class, IMessage
            where TResponse : class, IMessage
            => AddMessageHandler<TRequest>(async (service, session, message) =>
            {
                var response = await messageHandler(service, session, message);
                SendResponse(session, response);
            });

        protected void AddMessageHandler<TRequest, TResponse1, TResponse2>(MessageHandler<TService, TRequest, TResponse1, TResponse2> messageHandler)
            where TRequest : class, IMessage
            where TResponse1 : class, IMessage
            where TResponse2 : class, IMessage
            => AddMessageHandler<TRequest>(async (service, session, message) =>
            {
                var (response1, response2) = await messageHandler(service, session, message);
                SendResponse(session, response1);
                SendResponse(session, response2);
            });

        protected void AddReliableMessageHandler<TMessage>(MessageHandler<TService, TMessage> messageHandler)
            where TMessage : class, IReliableMessage
            => AddMessageHandler<TMessage>((service, session, message) =>
            {
                SendResponse(session, new AcknowledgeMessage()
                {
                    ResponseId = message.RequestId,
                    MessageHandled = true
                });
                return messageHandler(service, session, message);
            });

        protected void AddReliableMessageHandler<TRequest, TResponse>(MessageHandler<TService, TRequest, TResponse> messageHandler)
            where TRequest : class, IReliableMessage
            where TResponse : BaseReliableResponse
            => AddMessageHandler<TRequest>(async (service, session, request) =>
            {
                SendResponse(session, new AcknowledgeMessage()
                {
                    ResponseId = request.RequestId,
                    MessageHandled = true
                });
                var response = await messageHandler(service, session, request);
                if (response?.ResponseId == 0)
                    response.ResponseId = request.RequestId;
                SendReliableResponse(session, response);
            });

        protected void AddReliableMessageHandler<TRequest, TResponse1, TResponse2>(MessageHandler<TService, TRequest, TResponse1, TResponse2> messageHandler)
            where TRequest : class, IReliableMessage
            where TResponse1 : BaseReliableResponse
            where TResponse2 : BaseReliableResponse
            => AddMessageHandler<TRequest>(async (service, session, request) =>
            {
                SendResponse(session, new AcknowledgeMessage()
                {
                    ResponseId = request.RequestId,
                    MessageHandled = true
                });
                var (response1, response2) = await messageHandler(service, session, request);
                if (response1?.ResponseId == 0)
                    response1.ResponseId = request.RequestId;
                if (response2?.ResponseId == 0)
                    response2.ResponseId = request.RequestId;
                SendReliableResponse(session, response1);
                SendReliableResponse(session, response2);
            });

        #endregion

        #region Private Methods

        private void SendResponse<T>(ISession session, T response)
            where T : class, IMessage
        {
            if (response == null)
                return;

            var buffer = new GrowingSpanBuffer(stackalloc byte[412]);
            _messageWriter.WriteTo(ref buffer, response);
            // TODO: Split into multipart messages if necessary
            if (UseEncryption)
                session.SendEncrypted(buffer.Data);
            else
                session.Send(buffer.Data);
        }

        private void SendReliableResponse<T>(ISession session, T response)
            where T : class, IReliableMessage
        {
            if (response?.RequestId == 0)
                response.RequestId = _requestIdProvider.GetNextRequestId();
            SendResponse(session, response);
        }

        #endregion
    }
}
