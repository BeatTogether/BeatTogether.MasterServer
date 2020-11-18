using System;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.DedicatedServer
{
    public class GetAvailableRelayServerResponse : BaseMessage, IReliableRequest, IReliableResponse, IEncryptedMessage
    {
        public enum ResultCode : byte
        {
            Success,
            NoRelaysAvailable,
            UnknownError
        }

        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public string DedicatedServerId { get; set; }
        public DateTimeOffset DedicatedServerCreationTime { get; set; }
        public ResultCode Result { get; set; }
        public string Id { get; set; }
        public int Port { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }

        public bool Success => Result == ResultCode.Success;

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteString(DedicatedServerId);
            buffer.WriteInt64(DedicatedServerCreationTime.ToUnixTimeSeconds());
            buffer.WriteUInt8((byte)Result);
            if (!Success)
                return;

            buffer.WriteString(Id);
            buffer.WriteVarUInt((uint)Port);
            buffer.WriteBytes(Random);
            buffer.WriteVarBytes(PublicKey);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            DedicatedServerId = bufferReader.ReadString();
            DedicatedServerCreationTime = DateTimeOffset.FromUnixTimeSeconds(bufferReader.ReadInt64());
            Result = (ResultCode)bufferReader.ReadByte();
            if (!Success)
                return;

            Id = bufferReader.ReadString();
            Port = (int)bufferReader.ReadVarUInt();
            Random = bufferReader.ReadBytes(32).ToArray();
            PublicKey = bufferReader.ReadVarBytes().ToArray();
        }
    }
}
