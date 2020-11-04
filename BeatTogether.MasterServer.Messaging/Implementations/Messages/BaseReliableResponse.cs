using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages
{
    public abstract class BaseReliableResponse : IReliableMessage
    {
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }

        public virtual void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteUInt32(RequestId);
            buffer.WriteUInt32(ResponseId);
        }

        public virtual void ReadFrom(ref SpanBufferReader bufferReader)
        {
            RequestId = bufferReader.ReadUInt32();
            ResponseId = bufferReader.ReadUInt32();
        }
    }
}
