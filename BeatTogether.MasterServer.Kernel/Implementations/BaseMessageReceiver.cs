using System;
using System.Buffers;
using System.Collections.Generic;
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
    public abstract class BaseMessageReceiver<TService, TMessageRegistry> : IMessageReceiver
        where TMessageRegistry : class, IMessageRegistry
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MessageReader<TMessageRegistry> _messageReader;
        private readonly MessageWriter<TMessageRegistry> _messageWriter;
        private readonly ILogger _logger;

        private readonly Dictionary<Type, Action<Session, IMessage, ReadOnlySpanAction<byte, Session>>> _messageHandlerByTypeLookup;

        public BaseMessageReceiver(
            IServiceProvider serviceProvider,
            MessageReader<TMessageRegistry> messageReader,
            MessageWriter<TMessageRegistry> messageWriter)
        {
            _serviceProvider = serviceProvider;
            _messageReader = messageReader;
            _messageWriter = messageWriter;
            _logger = Log.ForContext<BaseMessageReceiver<TService, TMessageRegistry>>();
        }

        #region Public Methods

        public void OnReceived(Session session, ReadOnlySpan<byte> data, ReadOnlySpanAction<byte, Session> responseCallback)
        {
            var bufferReader = new SpanBufferReader(data);
            IMessage message;
            try
            {
                message = _messageReader.ReadFrom(bufferReader);
            }
            catch (IndexOutOfRangeException e)
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

        protected void AddMessageHandler<TMessage>(Action<TService, TMessage> messageHandler)
            where TMessage : class, IMessage
            => _messageHandlerByTypeLookup[typeof(TMessage)] = (session, message, responseCallback) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<TService>();
                messageHandler(service, (TMessage)message);
            };

        protected void AddMessageHandler<TRequest, TResponse>(Func<TService, TRequest, TResponse> messageHandler)
            where TRequest : class, IMessage
            where TResponse : class, IMessage
            => _messageHandlerByTypeLookup[typeof(TRequest)] = (session, message, responseCallback) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<TService>();
                var response = messageHandler(service, (TRequest)message);

                Span<byte> span = stackalloc byte[412];

                // Send the response
                var responseBuffer = new GrowingSpanBuffer(span);
                _messageWriter.WriteTo(responseBuffer, response);
                responseCallback(responseBuffer.Data, session);
            };

        protected void AddMessageHandler<TRequest, TResponse1, TResponse2>(Func<TService, TRequest, (TResponse1, TResponse2)> messageHandler)
            where TRequest : class, IMessage
            where TResponse1 : class, IMessage
            where TResponse2 : class, IMessage
            => _messageHandlerByTypeLookup[typeof(TRequest)] = (session, message, responseCallback) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<TService>();
                var (response1, response2) = messageHandler(service, (TRequest)message);

                Span<byte> span = stackalloc byte[412];

                // Send the first response
                var response1Buffer = new GrowingSpanBuffer(span);
                _messageWriter.WriteTo(response1Buffer, response1);
                responseCallback(response1Buffer.Data, session);

                // Send the second response
                var response2Buffer = new GrowingSpanBuffer(span);
                _messageWriter.WriteTo(response2Buffer, response2);
                responseCallback(response2Buffer.Data, session);
            };

        protected void AddReliableMessageHandler<TMessage>(Action<TService, TMessage> messageHandler)
            where TMessage : BaseReliableRequest
            => _messageHandlerByTypeLookup[typeof(TMessage)] = (session, message, responseCallback) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<TService>();
                var request = (TMessage)message;
                messageHandler(service, request);

                Span<byte> span = stackalloc byte[412];

                // Send the acknowledge message
                var acknowledgeBuffer = new GrowingSpanBuffer(span);
                _messageWriter.WriteTo(
                    acknowledgeBuffer,
                    new AcknowledgeMessage()
                    {
                        RequestId = request.RequestId,
                        ResponseId = 0  // TODO
                    }
                );
                responseCallback(acknowledgeBuffer.Data, session);
            };

        protected void AddReliableMessageHandler<TRequest, TResponse>(Func<TService, TRequest, TResponse> messageHandler)
            where TRequest : BaseReliableRequest
            where TResponse : BaseReliableResponse
            => _messageHandlerByTypeLookup[typeof(TRequest)] = (session, message, responseCallback) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<TService>();
                var request = (TRequest)message;
                var response = messageHandler(service, request);
                response.RequestId = request.RequestId;
                response.ResponseId = 0;  // TODO

                Span<byte> span = stackalloc byte[412];

                // Send the acknowledge message
                var acknowledgeBuffer = new GrowingSpanBuffer(span);
                _messageWriter.WriteTo(
                    acknowledgeBuffer,
                    new AcknowledgeMessage()
                    {
                        RequestId = request.RequestId,
                        ResponseId = 0  // TODO
                    }
                );
                responseCallback(acknowledgeBuffer.Data, session);

                // Send the response
                var responseBuffer = new GrowingSpanBuffer(span);
                _messageWriter.WriteTo(responseBuffer, response);
                responseCallback(responseBuffer.Data, session);
            };

        protected void AddReliableMessageHandler<TRequest, TResponse1, TResponse2>(Func<TService, TRequest, (TResponse1, TResponse2)> messageHandler)
            where TRequest : BaseReliableRequest
            where TResponse1 : BaseReliableResponse
            where TResponse2 : BaseReliableResponse
            => _messageHandlerByTypeLookup[typeof(TRequest)] = (session, message, responseCallback) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<TService>();
                var request = (TRequest)message;
                var (response1, response2) = messageHandler(service, request);
                response1.RequestId = request.RequestId;
                response1.ResponseId = 0;  // TODO
                response2.RequestId = request.RequestId;
                response1.ResponseId = 0; // TODO

                Span<byte> span = stackalloc byte[412];

                // Send the acknowledge message
                var acknowledgeBuffer = new GrowingSpanBuffer(span);
                _messageWriter.WriteTo(
                    acknowledgeBuffer,
                    new AcknowledgeMessage()
                    {
                        RequestId = request.RequestId,
                        ResponseId = 0  // TODO
                    }
                );
                responseCallback(acknowledgeBuffer.Data, session);

                // Send the first response
                var response1Buffer = new GrowingSpanBuffer(span);
                _messageWriter.WriteTo(response1Buffer, response1);
                responseCallback(response1Buffer.Data, session);

                // Send the second response
                var response2Buffer = new GrowingSpanBuffer(span);
                _messageWriter.WriteTo(response2Buffer, response2);
                responseCallback(response2Buffer.Data, session);
            };

        #endregion
    }
}
