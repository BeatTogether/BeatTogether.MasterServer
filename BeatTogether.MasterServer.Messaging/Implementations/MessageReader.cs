using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BeatTogether.MasterServer.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Abstractions.Registries;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations
{
    public class MessageReader : IMessageReader
    {
        protected virtual uint ProtocolVersion => 1;

        private readonly Dictionary<uint, IMessageRegistry> _messageRegistries;

        public MessageReader(IEnumerable<IMessageRegistry> messageRegistries)
        {
            _messageRegistries = messageRegistries.ToDictionary(
                messageRegistry => messageRegistry.MessageGroup
            );
        }

        /// <inheritdoc cref="IMessageReader.ReadFrom"/>
        public IMessage ReadFrom(ref SpanBufferReader bufferReader, byte packetProperty)
        {
            if (packetProperty != 0x00)
            {
                var readPacketProperty = bufferReader.ReadUInt8();
                if (readPacketProperty != packetProperty)
                    throw new InvalidDataContractException(
                        "Invalid packet property " +
                        $"(PacketProperty={readPacketProperty}, Expected={packetProperty})."
                    );
            }
            var messageGroup = bufferReader.ReadUInt32();
            if (!_messageRegistries.TryGetValue(messageGroup, out var messageRegistry))
                throw new InvalidDataContractException($"Invalid message group (MessageGroup={messageGroup}).");
            var protocolVersion = bufferReader.ReadVarUInt();
            if (protocolVersion != ProtocolVersion)
                throw new InvalidDataContractException($"Invalid message protocol version (ProtocolVersion={protocolVersion}).");
            var length = bufferReader.ReadVarUInt();
            if (bufferReader.RemainingSize < length)
                throw new InvalidDataContractException($"Message truncated (RemainingSize={bufferReader.RemainingSize}, Expected={length}).");
            var messageId = bufferReader.ReadVarUInt();
            if (!messageRegistry.TryCreateMessage(messageId, out var message))
                throw new InvalidDataContractException($"Invalid message identifier (MessageId={messageId}).");
            if (message is IReliableRequest)
                ((IReliableRequest)message).RequestId = bufferReader.ReadUInt32();
            if (message is IReliableResponse)
                ((IReliableResponse)message).ResponseId = bufferReader.ReadUInt32();
            message.ReadFrom(ref bufferReader);
            return message;
        }
    }
}
