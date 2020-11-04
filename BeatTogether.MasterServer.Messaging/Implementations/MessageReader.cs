using System;
using System.Runtime.Serialization;
using BeatTogether.MasterServer.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Abstractions.Registries;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations
{
    public class MessageReader<TMessageRegistry> : IMessageReader
        where TMessageRegistry : class, IMessageRegistry
    {
        private readonly TMessageRegistry _messageRegistry;

        public MessageReader(TMessageRegistry messageRegistry)
        {
            _messageRegistry = messageRegistry;
        }

        public IMessage ReadFrom(ref SpanBufferReader bufferReader)
        {
            bufferReader.SkipBytes(2);

            var messageGroup = (MessageGroup)bufferReader.ReadUInt32();
            if (messageGroup != _messageRegistry.MessageGroup)
                throw new InvalidDataContractException(
                    "Invalid message group " +
                    $"(MessageGroup={messageGroup}, " +
                    $"Expected={_messageRegistry.MessageGroup})."
                );
            var protocolVersion = bufferReader.ReadVarUInt();
            if (protocolVersion != _messageRegistry.ProtocolVersion)
                throw new InvalidDataContractException(
                    "Invalid message protocol version " +
                    $"(ProtocolVersion={protocolVersion}, " +
                    $"Expected={_messageRegistry.ProtocolVersion})."
                );
            var length = bufferReader.ReadVarUInt();
            if (bufferReader.RemainingSize != length)
                throw new InvalidDataContractException(
                    "Invalid message length " +
                    $"(Length={length}, Expected={bufferReader.RemainingSize})."
                );
            var messageId = (int)bufferReader.ReadVarUInt();
            if (!_messageRegistry.TryGetMessage(messageId, out var message))
                throw new InvalidDataContractException(
                    "Invalid message id " +
                    $"(MessageId={messageId})."
                );

            message.ReadFrom(ref bufferReader);
            return message;
        }
    }
}
