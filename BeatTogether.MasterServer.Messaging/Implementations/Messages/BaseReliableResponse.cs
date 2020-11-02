using BeatTogether.MasterServer.Messaging.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages
{
    public abstract class BaseReliableResponse : IMessage
    {
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }

        public virtual void WriteTo(GrowingSpanBuffer buffer)
        {
            buffer.WriteUInt32(RequestId);
            buffer.WriteUInt32(ResponseId);
        }

        public virtual void ReadFrom(SpanBufferReader bufferReader)
        {
            RequestId = bufferReader.ReadUInt32();
            ResponseId = bufferReader.ReadUInt32();
        }
    }
}
