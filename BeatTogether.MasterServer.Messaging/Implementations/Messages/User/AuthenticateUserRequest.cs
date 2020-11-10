using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Models;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.User
{
    public class AuthenticateUserRequest : BaseMessage, IReliableRequest, IReliableResponse, IEncryptedMessage
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public AuthenticationToken AuthenticationToken { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            AuthenticationToken.WriteTo(ref buffer);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            AuthenticationToken = new AuthenticationToken();
            AuthenticationToken.ReadFrom(ref bufferReader);
        }
    }
}
