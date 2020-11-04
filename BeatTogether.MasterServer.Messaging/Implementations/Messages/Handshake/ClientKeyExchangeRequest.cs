using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake
{
    public class ClientKeyExchangeRequest : BaseReliableResponse
    {
        public byte[] ClientPublicKey { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            base.WriteTo(ref buffer);

            buffer.WriteVarBytes(ClientPublicKey);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            base.ReadFrom(ref bufferReader);

            ClientPublicKey = bufferReader.ReadVarBytes().ToArray();
        }
    }
}
