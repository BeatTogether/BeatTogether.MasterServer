using BeatTogether.Core.Messaging.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.DedicatedServer
{
    public class AuthenticateDedicatedServerResponse : IEncryptedMessage, IReliableRequest, IReliableResponse
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
