using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.DedicatedServer
{
    public class DedicatedServerHeartbeatResponse : BaseMessage, IEncryptedMessage
    {
        public uint SequenceId { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
        }
    }
}
