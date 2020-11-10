using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.Models
{
    public class AuthenticationToken : BaseMessage
    {
        public Platform Platform { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public byte[] SessionToken { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteUInt8((byte)Platform);
            buffer.WriteString(UserId);
            buffer.WriteString(UserName);
            buffer.WriteVarBytes(SessionToken);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Platform = (Platform)bufferReader.ReadByte();
            UserId = bufferReader.ReadString();
            UserName = bufferReader.ReadString();
            SessionToken = bufferReader.ReadVarBytes().ToArray();
        }
    }
}
