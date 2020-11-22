using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Extensions;
using BeatTogether.MasterServer.Messaging.Models;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.DedicatedServer
{
    public class GetAvailableMatchmakingServerRequest : IEncryptedMessage, IReliableRequest
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public string Secret { get; set; }
        public GameplayServerConfiguration Configuration { get; set; }

        public void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteString(Secret);
            Configuration.WriteTo(ref buffer);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Secret = bufferReader.ReadString();
            Configuration = new GameplayServerConfiguration();
            Configuration.ReadFrom(ref bufferReader);
        }
    }
}
