using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages
{
    public abstract class BaseResponse : IMessage
    {
        public uint ResponseId { get; set; }

        public virtual void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteUInt32(ResponseId);
        }

        public virtual void ReadFrom(ref SpanBufferReader bufferReader)
        {
            ResponseId = bufferReader.ReadUInt32();
        }
    }
}
