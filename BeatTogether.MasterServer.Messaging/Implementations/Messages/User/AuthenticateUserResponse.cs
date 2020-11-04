using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.User
{
    public class AuthenticateUserResponse : BaseReliableResponse
    {
        public enum ResultCode : byte
        {
            Success,
            Failed,
            UnknownError
        }

        public ResultCode Result { get; set; }

        public bool Success => Result == ResultCode.Success;

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            base.WriteTo(ref buffer);

            buffer.WriteUInt8((byte)Result);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            base.ReadFrom(ref bufferReader);

            Result = (ResultCode)bufferReader.ReadByte();
        }
    }
}
