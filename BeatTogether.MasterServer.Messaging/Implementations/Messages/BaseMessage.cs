using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages
{
    public abstract class BaseMessage : IMessage
    {
        public IMessageDescriptor Descriptor { get; init; }

        public abstract void ReadFrom(ref SpanBufferReader bufferReader);
        public abstract void WriteTo(ref GrowingSpanBuffer buffer);
    }
}
