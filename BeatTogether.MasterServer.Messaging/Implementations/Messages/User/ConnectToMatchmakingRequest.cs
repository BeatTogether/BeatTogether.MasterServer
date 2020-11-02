using BeatTogether.MasterServer.Messaging.Extensions;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Models;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.User
{
    public class ConnectToMatchmakingRequest : BaseReliableRequest
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }
        public GameplayServerConfiguration Configuration { get; set; }
        public string Secret { get; set; }

        public override void WriteTo(GrowingSpanBuffer buffer)
        {
            base.WriteTo(buffer);

            buffer.WriteString(UserId);
            buffer.WriteString(UserName);
            buffer.WriteBytes(Random);
            buffer.WriteVarBytes(PublicKey);
            Configuration.WriteTo(buffer);
            buffer.WriteString(Secret);
        }

        public override void ReadFrom(SpanBufferReader bufferReader)
        {
            base.ReadFrom(bufferReader);

            UserId = bufferReader.ReadString();
            UserName = bufferReader.ReadString();
            Random = bufferReader.ReadBytes(32).ToArray();
            PublicKey = bufferReader.ReadVarBytes().ToArray();
            Configuration = new GameplayServerConfiguration();
            Configuration.ReadFrom(bufferReader);
            Secret = bufferReader.ReadString();
        }
    }
}
