using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages
{
    public abstract class BaseReliableRequest : IMessage
    {
        public uint RequestId { get; set; }

        public virtual void WriteTo(GrowingSpanBuffer buffer)
        {
            buffer.WriteUInt32(RequestId);
        }

        public virtual void ReadFrom(SpanBufferReader bufferReader)
        {
            RequestId = bufferReader.ReadUInt32();
        }
    }
}
