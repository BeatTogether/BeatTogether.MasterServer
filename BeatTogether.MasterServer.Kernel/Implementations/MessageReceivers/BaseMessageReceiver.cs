﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Kernel.Delegates;
using BeatTogether.MasterServer.Kernel.Models;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly IRequestIdProvider _requestIdProvider;
        private readonly IMessageReader _messageReader;
        private readonly IMessageWriter _messageWriter;
        private readonly ILogger _logger;

        private readonly Dictionary<Type, MessageHandler> _messageHandlerByTypeLookup;

        public BaseMessageReceiver(
            IServiceProvider serviceProvider,
            IRequestIdProvider requestIdProvider,
            IMessageReader messageReader,
            IMessageWriter messageWriter)
        {
            _serviceProvider = serviceProvider;
            _requestIdProvider = requestIdProvider;
            _messageReader = messageReader;
            _messageWriter = messageWriter;
            _logger = Log.ForContext<BaseMessageReceiver<TService>>();

            _messageHandlerByTypeLookup = new Dictionary<Type, MessageHandler>();
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
                    request.ResponseId = request.RequestId;
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
                        ResponseId = request.RequestId,
                        MessageHandled = true
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
                        ResponseId = request.RequestId,
                        MessageHandled = true
                    }
                );
                responseCallback(acknowledgeBuffer.Data);

                // Send the response
                if (response != null)
                {
                    response.RequestId = _requestIdProvider.GetNextRequestId();
                    if (response.ResponseId == 0)
                        response.ResponseId = request.RequestId;

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
                        ResponseId = request.RequestId,
                        MessageHandled = true
                    }
                );
                responseCallback(acknowledgeBuffer.Data);

                // Send the first response
                if (response1 != null)
                {
                    response1.RequestId = _requestIdProvider.GetNextRequestId();
                    if (response1.ResponseId == 0)
                        response1.ResponseId = request.RequestId;

                    var response1Buffer = new GrowingSpanBuffer(span);
                    _messageWriter.WriteTo(ref response1Buffer, response1);
                    responseCallback(response1Buffer.Data);
                }

                // Send the second response
                if (response2 != null)
                {
                    response2.RequestId = _requestIdProvider.GetNextRequestId();
                    if (response1.ResponseId == 0)
                        response1.ResponseId = request.RequestId;

                    var response2Buffer = new GrowingSpanBuffer(span);
                    _messageWriter.WriteTo(ref response2Buffer, response2);
                    responseCallback(response2Buffer.Data);
                }
            });

        #endregion
    }
}
