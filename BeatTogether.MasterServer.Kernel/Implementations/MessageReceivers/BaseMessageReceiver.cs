using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Models;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Abstractions.Registries;
using BeatTogether.MasterServer.Messaging.Implementations;
using BeatTogether.MasterServer.Messaging.Implementations.Messages;
using Krypton.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public abstract class BaseMessageReceiver<TMessageRegistry, TService> : IMessageReceiver
        where TMessageRegistry : class, IMessageRegistry
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MessageReader<TMessageRegistry> _messageReader;
        private readonly MessageWriter<TMessageRegistry> _messageWriter;
        private readonly ILogger _logger;

        private readonly Dictionary<Type, Action<Session, IMessage, ResponseCallback>> _messageHandlerByTypeLookup;

        public BaseMessageReceiver(
            IServiceProvider serviceProvider,
            MessageReader<TMessageRegistry> messageReader,
            MessageWriter<TMessageRegistry> messageWriter)
        {
            _serviceProvider = serviceProvider;
            _messageReader = messageReader;
            _messageWriter = messageWriter;
            _logger = Log.ForContext<BaseMessageReceiver<TMessageRegistry, TService>>();

            _messageHandlerByTypeLookup = new Dictionary<Type, Action<Session, IMessage, ResponseCallback>>();
        }

        #region Public Methods

        public void OnReceived(Session session, ReadOnlySpan<byte> buffer, ResponseCallback responseCallback)
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

            messageHandler(session, message, responseCallback);
        }

        #endregion

        #region Protected Methods

        protected void AddMessageHandler<TMessage>(Action<TService, Session, TMessage, ResponseCallback> messageHandler)
            where TMessage : class, IMessage
            => _messageHandlerByTypeLookup[typeof(TMessage)] = (session, message, responseCallback) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<TService>();
                messageHandler(service, session, (TMessage)message, responseCallback);
            };

        protected void AddMessageHandler<TMessage>(Action<TService, Session, TMessage> messageHandler)
            where TMessage : class, IMessage
            => AddMessageHandler<TMessage>(
                (service, session, message, responseCallback) => messageHandler(service, session, message)
            );

        protected void AddMessageHandler<TRequest, TResponse>(Func<TService, Session, TRequest, TResponse> messageHandler)
            where TRequest : class, IMessage
            where TResponse : class, IMessage
            => AddMessageHandler<TRequest>((service, session, message, responseCallback) =>
            {
                var response = messageHandler(service, session, message);

                Span<byte> span = stackalloc byte[412];

                // Send the response
                if (response != null)
                {
                    var responseBuffer = new GrowingSpanBuffer(span);
                    _messageWriter.WriteTo(ref responseBuffer, response);
                    responseCallback(responseBuffer.Data);
                }
            });

        protected void AddMessageHandler<TRequest, TResponse1, TResponse2>(Func<TService, Session, TRequest, (TResponse1, TResponse2)> messageHandler)
            where TRequest : class, IMessage
            where TResponse1 : class, IMessage
            where TResponse2 : class, IMessage
            => AddMessageHandler<TRequest>((service, session, message, responseCallback) =>
            {
                var (response1, response2) = messageHandler(service, session, message);

                Span<byte> span = stackalloc byte[412];

                // Send the first response
                if (response1 != null)
                {
                    var response1Buffer = new GrowingSpanBuffer(span);
                    _messageWriter.WriteTo(ref response1Buffer, response1);
                    responseCallback(response1Buffer.Data);
                }

                // Send the second response
                if (response2 != null)
                {
                    var response2Buffer = new GrowingSpanBuffer(span);
                    _messageWriter.WriteTo(ref response2Buffer, response2);
                    responseCallback(response2Buffer.Data);
                }
            });

        protected void AddReliableMessageHandler<TRequest>(Action<TService, Session, TRequest, ResponseCallback> messageHandler)
            where TRequest : class, IReliableMessage
            => _messageHandlerByTypeLookup[typeof(TRequest)] = (session, message, responseCallback) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<TService>();
                var request = (TRequest)message;
                if (request is BaseReliableResponse)
                    request.ResponseId = 0;  // TODO
                // TODO: Determine if we should handle this now or later
                messageHandler(service, session, request, responseCallback);
            };

        protected void AddReliableMessageHandler<TRequest>(Action<TService, Session, TRequest> messageHandler)
            where TRequest : class, IReliableMessage
            => AddReliableMessageHandler<TRequest>((service, session, request, responseCallback) =>
            {
                messageHandler(service, session, request);

                Span<byte> span = stackalloc byte[412];

                // Send the acknowledge message
                var acknowledgeBuffer = new GrowingSpanBuffer(span);
                _messageWriter.WriteTo(
                    ref acknowledgeBuffer,
                    new AcknowledgeMessage()
                    {
                        RequestId = request.RequestId,
                        ResponseId = 0  // TODO
                    }
                );
                responseCallback(acknowledgeBuffer.Data);
            });

        protected void AddReliableMessageHandler<TRequest, TResponse>(Func<TService, Session, TRequest, TResponse> messageHandler)
            where TRequest : class, IReliableMessage
            where TResponse : BaseReliableResponse
            => AddReliableMessageHandler<TRequest>((service, session, request, responseCallback) =>
            {
                var response = messageHandler(service, session, request);

                Span<byte> span = stackalloc byte[412];

                // Send the acknowledge message
                var acknowledgeBuffer = new GrowingSpanBuffer(span);
                _messageWriter.WriteTo(
                    ref acknowledgeBuffer,
                    new AcknowledgeMessage()
                    {
                        RequestId = request.RequestId,
                        ResponseId = 0  // TODO
                    }
                );
                responseCallback(acknowledgeBuffer.Data);

                // Send the response
                if (response != null)
                {
                    response.RequestId = request.RequestId;
                    response.ResponseId = 0;  // TODO

                    var responseBuffer = new GrowingSpanBuffer(span);
                    _messageWriter.WriteTo(ref responseBuffer, response);
                    responseCallback(responseBuffer.Data);
                }
            });

        protected void AddReliableMessageHandler<TRequest, TResponse1, TResponse2>(Func<TService, Session, TRequest, (TResponse1, TResponse2)> messageHandler)
            where TRequest : class, IReliableMessage
            where TResponse1 : BaseReliableResponse
            where TResponse2 : BaseReliableResponse
            => AddReliableMessageHandler<TRequest>((service, session, request, responseCallback) =>
            {
                var (response1, response2) = messageHandler(service, session, request);

                Span<byte> span = stackalloc byte[412];

                // Send the acknowledge message
                var acknowledgeBuffer = new GrowingSpanBuffer(span);
                _messageWriter.WriteTo(
                    ref acknowledgeBuffer,
                    new AcknowledgeMessage()
                    {
                        RequestId = request.RequestId,
                        ResponseId = 0  // TODO
                    }
                );
                responseCallback(acknowledgeBuffer.Data);

                // Send the first response
                if (response1 != null)
                {
                    response1.RequestId = request.RequestId;
                    response1.ResponseId = 0;  // TODO

                    var response1Buffer = new GrowingSpanBuffer(span);
                    _messageWriter.WriteTo(ref response1Buffer, response1);
                    responseCallback(response1Buffer.Data);
                }

                // Send the second response
                if (response2 != null)
                {
                    response2.RequestId = request.RequestId;
                    response1.ResponseId = 0; // TODO

                    var response2Buffer = new GrowingSpanBuffer(span);
                    _messageWriter.WriteTo(ref response2Buffer, response2);
                    responseCallback(response2Buffer.Data);
                }
            });

        #endregion
    }
}
