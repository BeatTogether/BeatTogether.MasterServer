using BeatTogether.Core.Messaging.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.Handshake
{
    public class ChangeCipherSpecRequest : IMessage, IReliableRequest, IReliableResponse
    {
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
        }
    }
}
