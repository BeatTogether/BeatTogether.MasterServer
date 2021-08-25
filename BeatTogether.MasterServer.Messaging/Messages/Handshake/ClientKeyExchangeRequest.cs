using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.Handshake
{
    public sealed class ClientKeyExchangeRequest : IMessage, IReliableRequest, IReliableResponse
    {
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public byte[] ClientPublicKey { get; set; }

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteVarBytes(ClientPublicKey);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            ClientPublicKey = bufferReader.ReadVarBytes().ToArray();
        }
    }
}
