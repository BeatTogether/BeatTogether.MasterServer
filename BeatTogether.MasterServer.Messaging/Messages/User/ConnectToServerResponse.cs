using System.Net;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Models;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public class ConnectToServerResponse : IEncryptedMessage, IReliableRequest, IReliableResponse, IVersionedMessage
    {
        public enum ResultCode : byte
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

        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public ResultCode Result { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Secret { get; set; }
        public DiscoveryPolicy DiscoveryPolicy { get; set; }
        public InvitePolicy InvitePolicy { get; set; }
        public int MaximumPlayerCount { get; set; }
        public GameplayServerConfiguration Configuration { get; set; }
        public bool IsConnectionOwner { get; set; }
        public bool IsDedicatedServer { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }
        public string Code { get; set; }

        public bool Success => Result == ResultCode.Success;

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteUInt8((byte)Result);
            if (!Success)
                return;

            bufferWriter.WriteString(UserId);
            bufferWriter.WriteString(UserName);
            bufferWriter.WriteString(Secret);
            bufferWriter.WriteUInt8((byte)DiscoveryPolicy);
            bufferWriter.WriteUInt8((byte)InvitePolicy);
            bufferWriter.WriteVarInt(MaximumPlayerCount);
            Configuration.WriteTo(ref bufferWriter);
            bufferWriter.WriteUInt8((byte)((IsConnectionOwner ? 1 : 0) | (IsDedicatedServer ? 2 : 0)));
            bufferWriter.WriteIPEndPoint(RemoteEndPoint);
            bufferWriter.WriteBytes(Random);
            bufferWriter.WriteVarBytes(PublicKey);
        }

        public void WriteTo(ref SpanBufferWriter bufferWriter, uint protocolVersion)
        {
            WriteTo(ref bufferWriter);
            
            if (!Success)
                return;

            if (protocolVersion >= 3)
            {
                bufferWriter.WriteString(Code);
            }
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Result = (ResultCode)bufferReader.ReadUInt8();
            if (!Success)
                return;

            UserId = bufferReader.ReadString();
            UserName = bufferReader.ReadString();
            Secret = bufferReader.ReadString();
            DiscoveryPolicy = (DiscoveryPolicy)bufferReader.ReadByte();
            InvitePolicy = (InvitePolicy)bufferReader.ReadByte();
            MaximumPlayerCount = bufferReader.ReadVarInt();
            Configuration = new GameplayServerConfiguration();
            Configuration.ReadFrom(ref bufferReader);
            var flags = bufferReader.ReadByte();
            IsConnectionOwner = (flags & 1) > 0;
            IsDedicatedServer = (flags & 2) > 0;
            RemoteEndPoint = bufferReader.ReadIPEndPoint();
            Random = bufferReader.ReadBytes(32).ToArray();
            PublicKey = bufferReader.ReadVarBytes().ToArray();
        }

        public void ReadFrom(ref SpanBufferReader bufferReader, uint protocolVersion)
        {
            ReadFrom(ref bufferReader);
            
            if (!Success)
                return;

            if (protocolVersion >= 3)
            {
                Code = bufferReader.ReadString();
            }
        }
    }
}
