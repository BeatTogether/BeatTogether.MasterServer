using System.Net;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.User
{
    public class BroadcastServerStatusResponse : BaseReliableResponse
    {
        public enum ResultCode : byte
        {
            Success,
            SecretNotUnique,
            UnknownError
        }

        public ResultCode Result { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }
        public string Code { get; set; }

        public bool Success => Result == ResultCode.Success;

        public override void WriteTo(GrowingSpanBuffer buffer)
        {
            base.WriteTo(buffer);

            buffer.WriteUInt8((byte)Result);
            if (!Success)
                return;

            buffer.WriteIPEndPoint(RemoteEndPoint);
            buffer.WriteString(Code);
        }

        public override void ReadFrom(SpanBufferReader bufferReader)
        {
            base.ReadFrom(bufferReader);

            Result = (ResultCode)bufferReader.ReadUInt8();
            if (!Success)
                return;

            RemoteEndPoint = bufferReader.ReadIPEndPoint();
            Code = bufferReader.ReadString();
        }
    }
}
