using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Delegates;
using BeatTogether.MasterServer.Kernel.Enums;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Implementations.Messages;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public abstract class BaseMessageReceiver<TService> : IMessageReceiver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMultipartMessageService _multipartMessageService;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly ILogger _logger;

        private readonly Dictionary<Type, MessageHandler<TService>> _messageHandlers;

        public BaseMessageReceiver(
            IServiceProvider serviceProvider,
            IMultipartMessageService multipartMessageService,
            IMessageDispatcher messageDispatcher)
        {
            _serviceProvider = serviceProvider;
            _multipartMessageService = multipartMessageService;
            _messageDispatcher = messageDispatcher;
            _logger = Log.ForContext<BaseMessageReceiver<TService>>();

            _messageHandlers = new Dictionary<Type, MessageHandler<TService>>();

            AddMessageHandler<MultipartMessage>(
                async (service, session, message) =>
                {
                    var fullMessage = await _multipartMessageService.HandleMultipartMessage(session, message);
                    if (fullMessage != null)
                        await OnReceived(session, fullMessage);
                }
            );
            AddMessageHandler<AcknowledgeMessage>(
                (service, session, message) =>
                {
                    _messageDispatcher.AcknowledgeMessage(message.ResponseId, message.MessageHandled);
                    return Task.CompletedTask;
                }
            );
        }

        #region Public Methods

        public async Task OnReceived(ISession session, IMessage message)
        {
            try
            {
                var messageType = message.GetType();
                if (!_messageHandlers.TryGetValue(messageType, out var messageHandler))
                {
                    _logger.Warning(
                        $"'{GetType().Name}' failed to retrieve handler for message of type " +
                        $"'{messageType.Name}'."
                    );
                    if (message is IReliableRequest)
                        await SendAcknowledgeMessage(session, (IReliableRequest)message, false).ConfigureAwait(false);
                    return;
                }

                if (message is IReliableRequest)
                    await SendAcknowledgeMessage(session, (IReliableRequest)message, true).ConfigureAwait(false);

                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<TService>();
                await messageHandler(service, session, message).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                LogError(session, e);
            }
        }

        #endregion

        #region Protected Methods

        protected void AddMessageHandler<TMessage>(MessageHandler<TService, TMessage> messageHandler)
            where TMessage : class, IMessage
            => _messageHandlers[typeof(TMessage)] =
                    (service, session, message) => messageHandler(service, session, (TMessage)message);

        protected void AddMessageHandler<TRequest, TResponse>(
            MessageHandler<TService, TRequest, TResponse> messageHandler,
            bool requireAcknowledgement = true)
            where TRequest : class, IMessage
            where TResponse : class, IMessage
            => AddMessageHandler<TRequest>(async (service, session, request) =>
            {
                var response = await messageHandler(service, session, request);
                if (response == null)
                    return;
                if (request is IReliableRequest reliableRequest &&
                    response is IReliableResponse reliableResponse)
                {
                    if (reliableResponse.ResponseId == 0)
                        reliableResponse.ResponseId = reliableRequest.RequestId;
                }
                await _messageDispatcher.Send(session, response, requireAcknowledgement);
            });

        #endregion

        #region Private Methods

        private Task SendAcknowledgeMessage<TRequest>(ISession session, TRequest request, bool messageHandled)
            where TRequest : class, IReliableRequest
        {
            var acknowledgeMessage = new AcknowledgeMessage()
            {
                ResponseId = request.RequestId,
                MessageHandled = messageHandled
            };
            _logger.Verbose(
                $"Sending acknowledgement for request of type '{request.GetType().Name}' " +
                $"(ResponseId={acknowledgeMessage.ResponseId}, " +
                $"MessageHandled={acknowledgeMessage.MessageHandled})."
            );
            return _messageDispatcher.Send(session, acknowledgeMessage);
        }

        private void LogError(
            ISession session,
            Exception e,
            [CallerMemberName] string callerMemberName = "")
        {
            if (session.State == SessionState.Authenticated)
                _logger.Error(e,
                    $"Error during {callerMemberName} " +
                    $"(EndPoint={session.EndPoint}, " +
                    $"Platform={session.Platform}, " +
                    $"UserId='{session.UserId}', " +
                    $"UserName='{session.UserName}', " +
                    $"Secret='{session.Secret}')."
                );
            else
                _logger.Error(e,
                    $"Error during {callerMemberName} " +
                    $"(EndPoint={session.EndPoint})."
                );
        }

        #endregion
    }
}
