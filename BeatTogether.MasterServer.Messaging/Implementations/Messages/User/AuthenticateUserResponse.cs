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

        public override void WriteTo(GrowingSpanBuffer buffer)
        {
            base.WriteTo(buffer);

            buffer.WriteUInt8((byte)Result);
        }

        public override void ReadFrom(SpanBufferReader bufferReader)
        {
            base.ReadFrom(bufferReader);

            Result = (ResultCode)bufferReader.ReadUInt8();
        }
    }
}
