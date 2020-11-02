using BeatTogether.MasterServer.Messaging.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.User
{
    public class BroadcastServerHeartbeatResponse : IMessage
    {
        public enum ResultCode
        {
            Success,
            ServerDoesNotExist,
            UnknownError
        }

        public ResultCode Result { get; set; }

        public bool Success => Result == ResultCode.Success;

        public void WriteTo(GrowingSpanBuffer buffer)
        {
            buffer.WriteUInt8((byte)Result);
        }

        public void ReadFrom(SpanBufferReader bufferReader)
        {
            Result = (ResultCode)bufferReader.ReadUInt8();
        }
    }
}
