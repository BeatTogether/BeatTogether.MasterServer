using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Models;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public class ConnectToMatchmakingRequest : IEncryptedMessage, IReliableRequest, IVersionedMessage
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }
        public GameplayServerConfiguration Configuration { get; set; }
        public string Secret { get; set; }
        public DiscoveryPolicy DiscoveryPolicy { get; set; }
        public string Code { get; set; }

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteString(UserId);
            bufferWriter.WriteString(UserName);
            bufferWriter.WriteBytes(Random);
            bufferWriter.WriteVarBytes(PublicKey);
            Configuration.WriteTo(ref bufferWriter);
            bufferWriter.WriteString(Secret);
        }

        public void WriteTo(ref SpanBufferWriter bufferWriter, uint protocolVersion)
        {
            WriteTo(ref bufferWriter);

            if (protocolVersion >= 3)
            {
                bufferWriter.WriteVarInt((int)DiscoveryPolicy);
                bufferWriter.WriteString(Code);
            }
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            UserId = bufferReader.ReadString();
            UserName = bufferReader.ReadString();
            Random = bufferReader.ReadBytes(32).ToArray();
            PublicKey = bufferReader.ReadVarBytes().ToArray();
            Configuration = new GameplayServerConfiguration();
            Configuration.ReadFrom(ref bufferReader);
            Secret = bufferReader.ReadString();
        }

        public void ReadFrom(ref SpanBufferReader bufferReader, uint protocolVersion)
        {
            ReadFrom(ref bufferReader);

            if (protocolVersion >= 3)
            {
                DiscoveryPolicy = (DiscoveryPolicy)bufferReader.ReadVarInt();
                Code = bufferReader.ReadString();
            }
        }
    }
}
