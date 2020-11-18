using System;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.DedicatedServer
{
    public class RelayServerStatusUpdateRequest : BaseMessage, IReliableRequest, IEncryptedMessage
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public string DedicatedServerId { get; set; }
        public DateTimeOffset DedicatedServerCreationTime { get; set; }
        public string Id { get; set; }
        public uint CurrentPlayerCount { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteString(DedicatedServerId);
            buffer.WriteInt64(DedicatedServerCreationTime.ToUnixTimeSeconds());
            buffer.WriteString(Id);
            buffer.WriteVarUInt(CurrentPlayerCount);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            DedicatedServerId = bufferReader.ReadString();
            DedicatedServerCreationTime = DateTimeOffset.FromUnixTimeSeconds(bufferReader.ReadInt64());
            Id = bufferReader.ReadString();
            CurrentPlayerCount = bufferReader.ReadVarUInt();
        }
    }
}
