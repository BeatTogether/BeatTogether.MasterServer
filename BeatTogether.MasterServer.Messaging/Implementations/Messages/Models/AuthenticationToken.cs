using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.Models
{
    public class AuthenticationToken : IMessage
    {
        public Platform Platform { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public byte[] SessionToken { get; set; }

        public void WriteTo(GrowingSpanBuffer buffer)
        {
            buffer.WriteUInt8((byte)Platform);
            buffer.WriteString(UserId);
            buffer.WriteString(UserName);
            buffer.WriteVarBytes(SessionToken);
        }

        public void ReadFrom(SpanBufferReader bufferReader)
        {
            Platform = (Platform)bufferReader.ReadUInt8();
            UserId = bufferReader.ReadString();
            UserName = bufferReader.ReadString();
            SessionToken = bufferReader.ReadVarBytes().ToArray();
        }
    }
}
