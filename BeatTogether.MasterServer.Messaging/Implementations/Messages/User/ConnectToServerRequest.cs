using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.User
{
    public class ConnectToServerRequest : BaseMessage, IReliableRequest, IEncryptedMessage
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }
        public string Secret { get; set; }
        public string Code { get; set; }
        public string Password { get; set; }
        public bool UseRelay { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteString(UserId);
            buffer.WriteString(UserName);
            buffer.WriteBytes(Random);
            buffer.WriteVarBytes(PublicKey);
            buffer.WriteString(Secret);
            buffer.WriteString(Code);
            buffer.WriteString(Password);
            buffer.WriteBool(UseRelay);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
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
