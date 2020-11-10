using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.User
{
    public class BroadcastServerHeartbeatResponse : BaseMessage, IEncryptedMessage
    {
        public enum ResultCode
        {
            Success,
            ServerDoesNotExist,
            UnknownError
        }

        public uint SequenceId { get; set; }
        public ResultCode Result { get; set; }

        public bool Success => Result == ResultCode.Success;

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteUInt8((byte)Result);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Result = (ResultCode)bufferReader.ReadByte();
        }
    }
}
