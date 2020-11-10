using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake
{
    public class ChangeCipherSpecRequest : BaseMessage, IReliableRequest, IReliableResponse
    {
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
        }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
        }
    }
}
