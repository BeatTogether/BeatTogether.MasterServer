using BeatTogether.MasterServer.Messaging.Models;

namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public sealed class AuthenticateUserRequest
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public AuthenticationToken AuthenticationToken { get; set; }
    }
}
