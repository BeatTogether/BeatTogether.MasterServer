using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Models;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public sealed class ConnectToMatchmakingServerRequest : IEncryptedMessage, IReliableRequest
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }
        public BeatmapLevelSelectionMask BeatmapLevelSelectionMask { get; set; } = new();
        public string Secret { get; set; }
        public string Code { get; set; }

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteString(UserId);
            bufferWriter.WriteString(UserName);
            bufferWriter.WriteBytes(Random);
            bufferWriter.WriteVarBytes(PublicKey);
            BeatmapLevelSelectionMask.WriteTo(ref bufferWriter);
            bufferWriter.WriteString(Secret);
            bufferWriter.WriteString(Code);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            UserId = bufferReader.ReadString();
            UserName = bufferReader.ReadString();
            Random = bufferReader.ReadBytes(32).ToArray();
            PublicKey = bufferReader.ReadVarBytes().ToArray();
            BeatmapLevelSelectionMask.ReadFrom(ref bufferReader);
            Secret = bufferReader.ReadString();
            Code = bufferReader.ReadString();
        }
    }
}
