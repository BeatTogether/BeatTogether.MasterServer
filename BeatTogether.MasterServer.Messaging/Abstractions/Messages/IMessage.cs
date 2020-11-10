using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Abstractions.Messages
{
    public interface IMessage
    {
        IMessageDescriptor Descriptor { get; }

        void WriteTo(ref GrowingSpanBuffer buffer);
        void ReadFrom(ref SpanBufferReader bufferReader);
    }
}
