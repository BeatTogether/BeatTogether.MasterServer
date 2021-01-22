using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public class ConnectToServerRequest : IEncryptedMessage, IReliableRequest
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

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteString(UserId);
            bufferWriter.WriteString(UserName);
            bufferWriter.WriteBytes(Random);
            bufferWriter.WriteVarBytes(PublicKey);
            bufferWriter.WriteString(Secret);
            bufferWriter.WriteString(Code);
            bufferWriter.WriteString(Password);
            bufferWriter.WriteBool(UseRelay);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
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
