using System;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Kernel.Enums;
using BeatTogether.MasterServer.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly IMessageWriter _messageWriter;
        private readonly IEncryptedMessageWriter _encryptedMessageWriter;
        private readonly IRequestIdProvider _requestIdProvider;
        private readonly ILogger _logger;

        public MessageDispatcher(
            IMessageWriter messageWriter,
            IEncryptedMessageWriter encryptedMessageWriter,
            IRequestIdProvider requestIdProvider)
        {
            _messageWriter = messageWriter;
            _encryptedMessageWriter = encryptedMessageWriter;
            _requestIdProvider = requestIdProvider;
            _logger = Log.ForContext<MessageDispatcher>();
        }

        public void Send<T>(ISession session, T message)
            where T : class, IMessage
        {
            if (message is IReliableRequest reliableRequest)
            {
                if (reliableRequest.RequestId == 0)
                    reliableRequest.RequestId = _requestIdProvider.GetNextRequestId();
            }

            var buffer = new GrowingSpanBuffer(stackalloc byte[1024]);
            if (session.State != SessionState.New && message is IEncryptedMessage encryptedMessage)
            {
                encryptedMessage.SequenceId = session.LastSentSequenceId + 1;
                session.LastSentSequenceId = encryptedMessage.SequenceId;

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
    }
}
