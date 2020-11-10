using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake
{
    public class ClientKeyExchangeRequest : BaseMessage, IReliableRequest, IReliableResponse
    {
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public byte[] ClientPublicKey { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteVarBytes(ClientPublicKey);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            ClientPublicKey = bufferReader.ReadVarBytes().ToArray();
        }
    }
}
