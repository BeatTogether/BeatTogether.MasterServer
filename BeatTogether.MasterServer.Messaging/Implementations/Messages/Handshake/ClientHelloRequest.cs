using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake
{
    public class ClientHelloRequest : BaseMessage, IReliableRequest
    {
        public uint RequestId { get; set; }
        public byte[] Random { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteBytes(Random);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Random = bufferReader.ReadBytes(32).ToArray();
        }
    }
}