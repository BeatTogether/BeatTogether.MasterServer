using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Extensions;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Models;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.User
{
    public class BroadcastServerStatusRequest : BaseReliableRequest
    {
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

        public override void WriteTo(GrowingSpanBuffer buffer)
        {
            base.WriteTo(buffer);

            buffer.WriteString(ServerName);
            buffer.WriteString(UserId);
            buffer.WriteString(UserName);
            buffer.WriteString(Secret);
            buffer.WriteString(Password);
            buffer.WriteVarInt(CurrentPlayerCount);
            buffer.WriteVarInt(MaximumPlayerCount);
            buffer.WriteUInt8((byte)DiscoveryPolicy);
            buffer.WriteUInt8((byte)InvitePolicy);
            Configuration.WriteTo(buffer);
            buffer.WriteBytes(Random);
            buffer.WriteVarBytes(PublicKey);
        }

        public override void ReadFrom(SpanBufferReader bufferReader)
        {
            base.ReadFrom(bufferReader);

            ServerName = bufferReader.ReadString();
            UserId = bufferReader.ReadString();
            UserName = bufferReader.ReadString();
            Secret = bufferReader.ReadString();
            Password = bufferReader.ReadString();
            CurrentPlayerCount = bufferReader.ReadVarInt();
            MaximumPlayerCount = bufferReader.ReadVarInt();
            DiscoveryPolicy = (DiscoveryPolicy)bufferReader.ReadUInt8();
            InvitePolicy = (InvitePolicy)bufferReader.ReadUInt8();
            Configuration = new GameplayServerConfiguration();
            Configuration.ReadFrom(bufferReader);
            Random = bufferReader.ReadBytes(32).ToArray();
            PublicKey = bufferReader.ReadVarBytes().ToArray();
        }
    }
}
