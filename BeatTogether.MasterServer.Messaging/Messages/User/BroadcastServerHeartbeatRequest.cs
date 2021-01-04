using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public class BroadcastServerHeartbeatRequest : IEncryptedMessage
    {
        public uint SequenceId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Secret { get; set; }
        public uint CurrentPlayerCount { get; set; }

        public void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteString(UserId);
            buffer.WriteString(UserName);
            buffer.WriteString(Secret);
            buffer.WriteVarUInt(CurrentPlayerCount);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            UserId = bufferReader.ReadString();
            UserName = bufferReader.ReadString();
            Secret = bufferReader.ReadString();
            CurrentPlayerCount = bufferReader.ReadVarUInt();
        }
    }
}
