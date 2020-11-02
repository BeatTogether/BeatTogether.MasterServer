using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Extensions;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Models;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.User
{
    public class AuthenticateUserRequest : BaseReliableResponse
    {
        public AuthenticationToken AuthenticationToken { get; set; }

        public override void WriteTo(GrowingSpanBuffer buffer)
        {
            base.WriteTo(buffer);

            AuthenticationToken.WriteTo(buffer);
        }

        public override void ReadFrom(SpanBufferReader bufferReader)
        {
            base.ReadFrom(bufferReader);

            AuthenticationToken = new AuthenticationToken();
            AuthenticationToken.ReadFrom(bufferReader);
        }
    }
}
