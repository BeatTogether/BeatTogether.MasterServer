using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.User
{
    public class BroadcastServerRemoveRequest : IMessage
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Secret { get; set; }

        public void WriteTo(GrowingSpanBuffer buffer)
        {
            buffer.WriteString(UserId);
            buffer.WriteString(UserName);
            buffer.WriteString(Secret);
        }

        public void ReadFrom(SpanBufferReader bufferReader)
        {
            UserId = bufferReader.ReadString();
            UserName = bufferReader.ReadString();
            Secret = bufferReader.ReadString();
        }
    }
}
