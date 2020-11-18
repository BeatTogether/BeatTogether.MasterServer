using System;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.DedicatedServer
{
    public class DedicatedServerHeartbeatRequest : BaseMessage, IEncryptedMessage
    {
        public uint SequenceId { get; set; }
        public string DedicatedServerId { get; set; }
        public DateTimeOffset DedicatedServerCreationTime { get; set; }
        public int CpuUtilization { get; set; }
        public int OccupiedServerSlots { get; set; }
        public int UnoccupiedServerSlots { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteString(DedicatedServerId);
            buffer.WriteInt64(DedicatedServerCreationTime.ToUnixTimeSeconds());
            buffer.WriteVarInt(CpuUtilization);
            buffer.WriteVarInt(OccupiedServerSlots);
            buffer.WriteVarInt(UnoccupiedServerSlots);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            DedicatedServerId = bufferReader.ReadString();
            DedicatedServerCreationTime = DateTimeOffset.FromUnixTimeSeconds(bufferReader.ReadInt64());
            CpuUtilization = bufferReader.ReadVarInt();
            OccupiedServerSlots = bufferReader.ReadVarInt();
            UnoccupiedServerSlots = bufferReader.ReadVarInt();
        }
    }
}
