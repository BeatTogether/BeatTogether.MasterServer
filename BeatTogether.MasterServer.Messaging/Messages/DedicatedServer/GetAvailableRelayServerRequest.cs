using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.DedicatedServer
{
    public class GetAvailableRelayServerRequest : IEncryptedMessage, IReliableRequest
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public string Secret { get; set; }

        public void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteString(Secret);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Secret = bufferReader.ReadString();
        }
    }
}
