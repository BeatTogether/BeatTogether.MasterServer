using BeatTogether.Core.Messaging.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.Handshake
{
    public sealed class HelloVerifyRequest : IMessage, IRequest, IResponse
    {
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public byte[] Cookie { get; set; }

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteBytes(Cookie);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Cookie = bufferReader.ReadBytes(32).ToArray();
        }
    }
}
