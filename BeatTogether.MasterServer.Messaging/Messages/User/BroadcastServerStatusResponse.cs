using System.Net;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public class BroadcastServerStatusResponse : IEncryptedMessage, IReliableRequest, IReliableResponse
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

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteUInt8((byte)Result);
            if (!Success)
                return;

            bufferWriter.WriteIPEndPoint(RemoteEndPoint);
            bufferWriter.WriteString(Code);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Result = (ResultCode)bufferReader.ReadByte();
            if (!Success)
                return;

            RemoteEndPoint = bufferReader.ReadIPEndPoint();
            Code = bufferReader.ReadString();
        }
    }
}
