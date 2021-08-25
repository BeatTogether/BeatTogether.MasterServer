using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Models;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public sealed class AuthenticateUserRequest : IEncryptedMessage, IReliableRequest, IReliableResponse
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public AuthenticationToken AuthenticationToken { get; set; }

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            AuthenticationToken.WriteTo(ref bufferWriter);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            AuthenticationToken = new AuthenticationToken();
            AuthenticationToken.ReadFrom(ref bufferReader);
        }
    }
}
