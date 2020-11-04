using System;
using BeatTogether.MasterServer.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Abstractions.Registries;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations
{
    public class MessageWriter<TMessageRegistry> : IMessageWriter
        where TMessageRegistry : class, IMessageRegistry
    {
        private readonly TMessageRegistry _messageRegistry;

        public MessageWriter(TMessageRegistry messageRegistry)
        {
            _messageRegistry = messageRegistry;
        }

        public void WriteTo<TMessage>(ref GrowingSpanBuffer buffer, TMessage message)
            where TMessage : class, IMessage
        {
            if (!_messageRegistry.TryGetMessageId<TMessage>(out var messageId))
                throw new Exception($"Message of type '{nameof(TMessage)}' does not exist in the message registry");

            buffer.WriteUInt16(2048);

            buffer.WriteUInt32((uint)_messageRegistry.MessageGroup);
            buffer.WriteVarUInt(_messageRegistry.ProtocolVersion);

            var messageBuffer = new GrowingSpanBuffer(stackalloc byte[412]);
            messageBuffer.WriteVarUInt((uint)messageId);
            message.WriteTo(ref messageBuffer);
            buffer.WriteVarUInt((uint)messageBuffer.Size);
            // TODO: Remove byte array allocation
            buffer.WriteBytes(messageBuffer.Data.ToArray());
        }
    }
}
