using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake
{
    public class HelloVerifyRequest : BaseMessage, IReliableRequest, IReliableResponse
    {
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public byte[] Cookie { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteBytes(Cookie);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Cookie = bufferReader.ReadBytes(32).ToArray();
        }
    }
}
