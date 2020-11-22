using System;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.DedicatedServer
{
    public class DedicatedServerHeartbeatRequest : IEncryptedMessage
    {
        public uint SequenceId { get; set; }
        public string DedicatedServerId { get; set; }
        public DateTimeOffset DedicatedServerCreationTime { get; set; }
        public int CpuUtilization { get; set; }
        public int OccupiedServerSlots { get; set; }
        public int UnoccupiedServerSlots { get; set; }

        public void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteString(DedicatedServerId);
            buffer.WriteInt64(DedicatedServerCreationTime.ToUnixTimeSeconds());
            buffer.WriteVarInt(CpuUtilization);
            buffer.WriteVarInt(OccupiedServerSlots);
            buffer.WriteVarInt(UnoccupiedServerSlots);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            DedicatedServerId = bufferReader.ReadString();
            DedicatedServerCreationTime = DateTimeOffset.FromUnixTimeSeconds(bufferReader.ReadInt64());
            CpuUtilization = bufferReader.ReadVarInt();
            OccupiedServerSlots = bufferReader.ReadVarInt();
            UnoccupiedServerSlots = bufferReader.ReadVarInt();
        }
    }
}
