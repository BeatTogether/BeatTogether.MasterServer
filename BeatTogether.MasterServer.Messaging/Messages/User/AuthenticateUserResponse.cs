namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public enum AuthenticateUserResult : byte
    {
        Success = 0,
        Failed = 1,
        UnknownError = 2
    }

    public sealed class AuthenticateUserResponse
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public AuthenticateUserResult Result { get; set; }

        public bool Success => Result == AuthenticateUserResult.Success;
    }
}
