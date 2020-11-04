using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
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

        public void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteUInt8((byte)Result);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Result = (ResultCode)bufferReader.ReadByte();
        }
    }
}
