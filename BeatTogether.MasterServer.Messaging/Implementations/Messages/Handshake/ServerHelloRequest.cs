using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake
{
    public class ServerHelloRequest : BaseReliableResponse
    {
        public byte[] Cookie { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] Signature { get; set; }

        public override void WriteTo(GrowingSpanBuffer buffer)
        {
            base.WriteTo(buffer);

            buffer.WriteBytes(Cookie);
            buffer.WriteVarBytes(PublicKey);
            buffer.WriteVarBytes(Signature);
        }

        public override void ReadFrom(SpanBufferReader bufferReader)
        {
            base.ReadFrom(bufferReader);

            Cookie = bufferReader.ReadBytes(32).ToArray();
            PublicKey = bufferReader.ReadVarBytes().ToArray();
            Signature = bufferReader.ReadVarBytes().ToArray();
        }
    }
}
