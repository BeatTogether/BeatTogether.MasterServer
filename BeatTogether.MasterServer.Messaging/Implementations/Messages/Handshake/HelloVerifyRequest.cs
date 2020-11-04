using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake
{
    public class HelloVerifyRequest : BaseReliableResponse
    {
        public byte[] Cookie { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            base.WriteTo(ref buffer);

            buffer.WriteBytes(Cookie);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            base.ReadFrom(ref bufferReader);

            Cookie = bufferReader.ReadBytes(32).ToArray();
        }
    }
}
