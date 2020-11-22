using BeatTogether.Core.Messaging.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.DedicatedServer
{
    public class DedicatedServerHeartbeatResponse : IEncryptedMessage
    {
        public uint SequenceId { get; set; }

        public void WriteTo(ref GrowingSpanBuffer buffer)
        {
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
        }
    }
}
