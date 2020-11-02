using System;
using BeatTogether.MasterServer.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Abstractions.Registries;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations
{
    public class MessageWriter<TMessageRegistry> : IMessageWriter<TMessageRegistry>
        where TMessageRegistry : class, IMessageRegistry
    {
        private const int _protocolVersion = 1;
        private const int _maxMessageSize = 412;

        private readonly TMessageRegistry _messageRegistry;

        public MessageWriter(TMessageRegistry messageRegistry)
        {
            _messageRegistry = messageRegistry;
        }

        public void WriteTo<TMessage>(GrowingSpanBuffer buffer, TMessage message)
            where TMessage : class, IMessage
        {
            if (!_messageRegistry.TryGetMessageId<TMessage>(out var messageId))
                throw new Exception($"Message of type '{nameof(TMessage)}' does not exist in the message registry");

            buffer.WriteUInt64((ulong)_messageRegistry.MessageGroup);
            buffer.WriteVarUInt(_protocolVersion);

            var messageBuffer = new GrowingSpanBuffer(stackalloc byte[_maxMessageSize]);
            message.WriteTo(messageBuffer);
            buffer.WriteVarUInt((uint)messageBuffer.Size);
            buffer.WriteVarUInt((uint)messageId);
            buffer.WriteBytes(messageBuffer);
        }
    }
}
