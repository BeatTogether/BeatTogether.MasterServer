using BeatTogether.Core.Messaging.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public class SessionKeepaliveMessage : IEncryptedMessage
    {
        public uint SequenceId { get; set; }

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
        }
    }
}
