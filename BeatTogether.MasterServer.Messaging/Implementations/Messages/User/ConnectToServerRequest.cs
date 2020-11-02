using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.User
{
    public class ConnectToServerRequest : BaseReliableRequest
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }
        public string Secret { get; set; }
        public string Code { get; set; }
        public string Password { get; set; }
        public bool UseRelay { get; set; }

        public override void WriteTo(GrowingSpanBuffer buffer)
        {
            base.WriteTo(buffer);

            buffer.WriteString(UserId);
            buffer.WriteString(UserName);
            buffer.WriteBytes(Random);
            buffer.WriteVarBytes(PublicKey);
            buffer.WriteString(Secret);
            buffer.WriteString(Code);
            buffer.WriteString(Password);
            buffer.WriteBool(UseRelay);
        }

        public override void ReadFrom(SpanBufferReader bufferReader)
        {
            base.ReadFrom(bufferReader);

            UserId = bufferReader.ReadString();
            UserName = bufferReader.ReadString();
            Random = bufferReader.ReadBytes(32).ToArray();
            PublicKey = bufferReader.ReadVarBytes().ToArray();
            Secret = bufferReader.ReadString();
            Code = bufferReader.ReadString();
            Password = bufferReader.ReadString();
            UseRelay = bufferReader.ReadBool();
        }
    }
}
