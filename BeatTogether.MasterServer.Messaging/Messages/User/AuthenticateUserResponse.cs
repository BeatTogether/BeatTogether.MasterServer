using BeatTogether.Core.Messaging.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public enum AuthenticateUserResult : byte
    {
        Success = 0,
        Failed = 1,
        UnknownError = 2
    }

    public sealed class AuthenticateUserResponse : IEncryptedMessage, IReliableRequest, IReliableResponse
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public AuthenticateUserResult Result { get; set; }

        public bool Success => Result == AuthenticateUserResult.Success;

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteUInt8((byte)Result);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Result = (AuthenticateUserResult)bufferReader.ReadByte();
        }
    }
}
