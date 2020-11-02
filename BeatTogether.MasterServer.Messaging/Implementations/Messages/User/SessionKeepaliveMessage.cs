using BeatTogether.MasterServer.Messaging.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.User
{
    public class SessionKeepaliveMessage : IMessage
    {
        public void ReadFrom(SpanBufferReader bufferReader)
        {
        }

        public void WriteTo(GrowingSpanBuffer buffer)
        {
        }
    }
}
