using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Models;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public class BroadcastServerStatusRequest : IEncryptedMessage, IReliableRequest
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public string ServerName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Secret { get; set; }
        public string Password { get; set; }
        public int CurrentPlayerCount { get; set; }
        public int MaximumPlayerCount { get; set; }
        public DiscoveryPolicy DiscoveryPolicy { get; set; }
        public InvitePolicy InvitePolicy { get; set; }
        public GameplayServerConfiguration Configuration { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteString(ServerName);
            bufferWriter.WriteString(UserId);
            bufferWriter.WriteString(UserName);
            bufferWriter.WriteString(Secret);
            bufferWriter.WriteString(Password);
            bufferWriter.WriteVarInt(CurrentPlayerCount);
            bufferWriter.WriteVarInt(MaximumPlayerCount);
            bufferWriter.WriteUInt8((byte)DiscoveryPolicy);
            bufferWriter.WriteUInt8((byte)InvitePolicy);
            Configuration.WriteTo(ref bufferWriter);
            bufferWriter.WriteBytes(Random);
            bufferWriter.WriteVarBytes(PublicKey);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            ServerName = bufferReader.ReadString();
            UserId = bufferReader.ReadString();
            UserName = bufferReader.ReadString();
            Secret = bufferReader.ReadString();
            Password = bufferReader.ReadString();
            CurrentPlayerCount = bufferReader.ReadVarInt();
            MaximumPlayerCount = bufferReader.ReadVarInt();
            DiscoveryPolicy = (DiscoveryPolicy)bufferReader.ReadByte();
            InvitePolicy = (InvitePolicy)bufferReader.ReadByte();
            Configuration = new GameplayServerConfiguration();
            Configuration.ReadFrom(ref bufferReader);
            Random = bufferReader.ReadBytes(32).ToArray();
            PublicKey = bufferReader.ReadVarBytes().ToArray();
        }
    }
}
