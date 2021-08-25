using System.Net;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.MasterServer.Messaging.Models;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public enum ConnectToServerResult : byte
    {
        Success,
        InvalidSecret,
        InvalidCode,
        InvalidPassword,
        ServerAtCapacity,
        NoAvailableDedicatedServers,
        VersionMismatch,
        ConfigMismatch,
        UnknownError
    }

    public sealed class ConnectToServerResponse : IEncryptedMessage, IReliableRequest, IReliableResponse
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public ConnectToServerResult Result { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Secret { get; set; }
        public BeatmapLevelSelectionMask BeatmapLevelSelectionMask { get; set; } = new();
        public bool IsConnectionOwner { get; set; }
        public bool IsDedicatedServer { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }
        public string Code { get; set; }
        public GameplayServerConfiguration Configuration { get; set; } = new();
        public string ManagerId { get; set; }

        public bool Success => Result == ConnectToServerResult.Success;

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteUInt8((byte)Result);
            if (!Success)
                return;

            bufferWriter.WriteString(UserId);
            bufferWriter.WriteString(UserName);
            bufferWriter.WriteString(Secret);
            BeatmapLevelSelectionMask.WriteTo(ref bufferWriter);
            bufferWriter.WriteUInt8((byte)((IsConnectionOwner ? 1 : 0) | (IsDedicatedServer ? 2 : 0)));
            bufferWriter.WriteIPEndPoint(RemoteEndPoint);
            bufferWriter.WriteBytes(Random);
            bufferWriter.WriteVarBytes(PublicKey);
            bufferWriter.WriteString(Code);
            Configuration.WriteTo(ref bufferWriter);
            bufferWriter.WriteString(ManagerId);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Result = (ConnectToServerResult)bufferReader.ReadUInt8();
            if (!Success)
                return;

            UserId = bufferReader.ReadString();
            UserName = bufferReader.ReadString();
            Secret = bufferReader.ReadString();
            BeatmapLevelSelectionMask.ReadFrom(ref bufferReader);
            var flags = bufferReader.ReadByte();
            IsConnectionOwner = (flags & 1) > 0;
            IsDedicatedServer = (flags & 2) > 0;
            RemoteEndPoint = bufferReader.ReadIPEndPoint();
            Random = bufferReader.ReadBytes(32).ToArray();
            PublicKey = bufferReader.ReadVarBytes().ToArray();
            Code = bufferReader.ReadString();
            Configuration.ReadFrom(ref bufferReader);
            ManagerId = bufferReader.ReadString();
        }
    }
}
