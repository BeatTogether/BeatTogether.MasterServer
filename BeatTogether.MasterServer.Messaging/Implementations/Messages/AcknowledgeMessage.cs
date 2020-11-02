using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages
{
    public class AcknowledgeMessage : BaseReliableResponse
    {
        public bool MessageHandled { get; set; }

        public override void WriteTo(GrowingSpanBuffer buffer)
        {
            base.WriteTo(buffer);

            buffer.WriteBool(MessageHandled);
        }

        public override void ReadFrom(SpanBufferReader bufferReader)
        {
            base.ReadFrom(bufferReader);

            MessageHandled = bufferReader.ReadBool();
        }
    }
}
