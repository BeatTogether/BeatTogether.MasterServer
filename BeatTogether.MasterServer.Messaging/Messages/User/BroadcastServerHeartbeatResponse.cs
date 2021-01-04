using BeatTogether.Core.Messaging.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public class BroadcastServerHeartbeatResponse : IEncryptedMessage
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
