using System.Net;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.User
{
    public class BroadcastServerStatusResponse : BaseMessage, IReliableRequest, IReliableResponse, IEncryptedMessage
    {
        public enum ResultCode : byte
        {
            Success,
            SecretNotUnique,
            UnknownError
        }

        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public ResultCode Result { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }
        public string Code { get; set; }

        public bool Success => Result == ResultCode.Success;

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteUInt8((byte)Result);
            if (!Success)
                return;

            buffer.WriteIPEndPoint(RemoteEndPoint);
            buffer.WriteString(Code);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Result = (ResultCode)bufferReader.ReadByte();
            if (!Success)
                return;

            RemoteEndPoint = bufferReader.ReadIPEndPoint();
            Code = bufferReader.ReadString();
        }
    }
}
