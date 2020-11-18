using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Extensions;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Models;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.DedicatedServer
{
    public class GetAvailableMatchmakingServerRequest : BaseMessage, IReliableRequest, IEncryptedMessage
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public string Secret { get; set; }
        public GameplayServerConfiguration Configuration { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteString(Secret);
            Configuration.WriteTo(ref buffer);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Secret = bufferReader.ReadString();
            Configuration = new GameplayServerConfiguration();
            Configuration.ReadFrom(ref bufferReader);
        }
    }
}
