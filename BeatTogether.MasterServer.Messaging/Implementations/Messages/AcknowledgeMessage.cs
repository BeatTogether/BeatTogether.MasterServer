using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages
{
    public class AcknowledgeMessage : BaseMessage, IReliableResponse, IEncryptedMessage
    {
        public uint SequenceId { get; set; }
        public uint ResponseId { get; set; }
        public bool MessageHandled { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteBool(MessageHandled);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            MessageHandled = bufferReader.ReadBool();
        }
    }
}
