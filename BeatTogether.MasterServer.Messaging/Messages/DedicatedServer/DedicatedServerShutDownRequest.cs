using System;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.DedicatedServer
{
    public class DedicatedServerShutDownRequest : IEncryptedMessage
    {
        public uint SequenceId { get; set; }
        public string DedicatedServerId { get; set; }
        public DateTimeOffset DedicatedServerCreationTime { get; set; }

        public void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteString(DedicatedServerId);
            buffer.WriteInt64(DedicatedServerCreationTime.ToUnixTimeSeconds());
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            DedicatedServerId = bufferReader.ReadString();
            DedicatedServerCreationTime = DateTimeOffset.FromUnixTimeSeconds(bufferReader.ReadInt64());
        }
    }
}
