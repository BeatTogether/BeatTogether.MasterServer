using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake
{
    public class ClientKeyExchangeRequest : BaseReliableResponse
    {
        public byte[] ClientPublicKey { get; set; }

        public override void WriteTo(GrowingSpanBuffer buffer)
        {
            base.WriteTo(buffer);

            buffer.WriteVarBytes(ClientPublicKey);
        }

        public override void ReadFrom(SpanBufferReader bufferReader)
        {
            base.ReadFrom(bufferReader);

            ClientPublicKey = bufferReader.ReadVarBytes().ToArray();
        }
    }
}
