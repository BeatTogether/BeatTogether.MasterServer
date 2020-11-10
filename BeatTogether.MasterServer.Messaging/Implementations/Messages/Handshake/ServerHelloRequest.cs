using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake
{
    public class ServerHelloRequest : BaseMessage, IReliableRequest, IReliableResponse
    {
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] Signature { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteBytes(Random);
            buffer.WriteVarBytes(PublicKey);
            buffer.WriteVarBytes(Signature);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Random = bufferReader.ReadBytes(32).ToArray();
            PublicKey = bufferReader.ReadVarBytes().ToArray();
            Signature = bufferReader.ReadVarBytes().ToArray();
        }
    }
}
