using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.DedicatedServer
{
    public class DedicatedServerNoLongerOccupiedRequest : IEncryptedMessage, IReliableRequest
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public string DedicatedServerId { get; set; }
        public string Id { get; set; }

        public void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteString(DedicatedServerId);
            buffer.WriteString(Id);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            DedicatedServerId = bufferReader.ReadString();
            Id = bufferReader.ReadString();
        }
    }
}
