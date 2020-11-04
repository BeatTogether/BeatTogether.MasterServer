using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Extensions;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Models;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.User
{
    public class AuthenticateUserRequest : BaseReliableResponse
    {
        public AuthenticationToken AuthenticationToken { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            base.WriteTo(ref buffer);

            AuthenticationToken.WriteTo(ref buffer);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            base.ReadFrom(ref bufferReader);

            AuthenticationToken = new AuthenticationToken();
            AuthenticationToken.ReadFrom(ref bufferReader);
        }
    }
}
