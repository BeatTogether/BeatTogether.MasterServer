using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages
{
    public class AcknowledgeMessage : BaseReliableResponse
    {
        public bool MessageHandled { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            base.WriteTo(ref buffer);

            buffer.WriteBool(MessageHandled);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            base.ReadFrom(ref bufferReader);

            MessageHandled = bufferReader.ReadBool();
        }
    }
}
