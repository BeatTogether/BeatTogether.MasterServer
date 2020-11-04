using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake
{
    public class ClientHelloRequest : BaseReliableRequest
    {
        public byte[] Random { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            base.WriteTo(ref buffer);

            buffer.WriteBytes(Random);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            base.ReadFrom(ref bufferReader);

            Random = bufferReader.ReadBytes(32).ToArray();
        }
    }
}