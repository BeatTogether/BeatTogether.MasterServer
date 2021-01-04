using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.Handshake
{
    public class ClientKeyExchangeRequest : IMessage, IReliableRequest, IReliableResponse
    {
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public byte[] ClientPublicKey { get; set; }

        public void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteVarBytes(ClientPublicKey);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            ClientPublicKey = bufferReader.ReadVarBytes().ToArray();
        }
    }
}
