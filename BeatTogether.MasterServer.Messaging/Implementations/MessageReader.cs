using System.Runtime.Serialization;
using BeatTogether.MasterServer.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Abstractions.Registries;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations
{
    public class MessageReader<TMessageRegistry> : IMessageReader<TMessageRegistry>
        where TMessageRegistry : class, IMessageRegistry
    {
        private const int _messageHeaderSize = 9;
        private const uint _protocolVersion = 1;

        private readonly TMessageRegistry _messageRegistry;

        public MessageReader(TMessageRegistry messageRegistry)
        {
            _messageRegistry = messageRegistry;
        }

        public IMessage ReadFrom(SpanBufferReader bufferReader)
        {
            if (bufferReader.RemainingSize < _messageHeaderSize)
                throw new InvalidDataContractException("Invalid message header");

            var messageGroup = (MessageGroup)bufferReader.ReadUInt64();
            if (!bufferReader.TryReadVarUInt(out var protocolVersion))
                throw new InvalidDataContractException("Invalid message header");
            if (protocolVersion != _protocolVersion)
                throw new InvalidDataContractException("Invalid message protocol version");
            if (!bufferReader.TryReadVarUInt(out var length))
                throw new InvalidDataContractException("Invalid message header");
            if (bufferReader.RemainingSize < length)
                throw new InvalidDataContractException("Invalid message length");
            var messageId = bufferReader.ReadVarUInt();
            if (!_messageRegistry.TryGetMessage(messageId, out var message))
                return null;

            message.ReadFrom(bufferReader);
            return message;
        }
    }
}
