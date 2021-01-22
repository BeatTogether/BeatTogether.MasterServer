using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.Handshake
{
    public class ServerHelloRequest : IMessage, IReliableRequest, IReliableResponse
    {
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] Signature { get; set; }

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteBytes(Random);
            bufferWriter.WriteVarBytes(PublicKey);
            bufferWriter.WriteVarBytes(Signature);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Random = bufferReader.ReadBytes(32).ToArray();
            PublicKey = bufferReader.ReadVarBytes().ToArray();
            Signature = bufferReader.ReadVarBytes().ToArray();
        }
    }
}
