using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake
{
    public class ClientHelloWithCookieRequest : BaseReliableRequest
    {
        public uint CertificateResponseId { get; set; }
        public byte[] Random { get; set; }
        public byte[] Cookie { get; set; }

        public override void WriteTo(GrowingSpanBuffer buffer)
        {
            base.WriteTo(buffer);

            buffer.WriteUInt32(CertificateResponseId);
            buffer.WriteBytes(Random);
            buffer.WriteBytes(Cookie);
        }

        public override void ReadFrom(SpanBufferReader bufferReader)
        {
            base.ReadFrom(bufferReader);

            CertificateResponseId = bufferReader.ReadUInt32();
            Random = bufferReader.ReadBytes(32).ToArray();
            Cookie = bufferReader.ReadBytes(32).ToArray();
        }
    }
}
