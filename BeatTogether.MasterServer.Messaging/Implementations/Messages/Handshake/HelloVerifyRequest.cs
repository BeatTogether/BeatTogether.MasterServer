using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake
{
    public class HelloVerifyRequest : BaseReliableResponse
    {
        public byte[] Cookie { get; set; }

        public override void WriteTo(GrowingSpanBuffer buffer)
        {
            base.WriteTo(buffer);

            buffer.WriteBytes(Cookie);
        }

        public override void ReadFrom(SpanBufferReader bufferReader)
        {
            base.ReadFrom(bufferReader);

            Cookie = bufferReader.ReadBytes(32).ToArray();
        }
    }
}
