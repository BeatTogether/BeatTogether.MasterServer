using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.User
{
    public class SessionKeepaliveMessage : BaseMessage, IEncryptedMessage
    {
        public uint SequenceId { get; set; }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
        }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
        }
    }
}
