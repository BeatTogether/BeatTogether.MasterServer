using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.DedicatedServer
{
    public class AuthenticateDedicatedServerResponse : BaseMessage, IReliableRequest, IReliableResponse, IEncryptedMessage
    {
        public enum ResultCode : byte
        {
            Success,
            Failed,
            UnknownError
        }

        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
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
