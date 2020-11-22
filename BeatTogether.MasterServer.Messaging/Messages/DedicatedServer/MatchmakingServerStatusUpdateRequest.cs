using System;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Extensions;
using BeatTogether.MasterServer.Messaging.Enums;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.DedicatedServer
{
    public class MatchmakingServerStatusUpdateRequest : IEncryptedMessage, IReliableRequest
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public string DedicatedServerId { get; set; }
        public DateTimeOffset DedicatedServerCreationTime { get; set; }
        public string Id { get; set; }
        public GameStateType GameState { get; set; }
        public uint CurrentPlayerCount { get; set; }

        public void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteString(DedicatedServerId);
            buffer.WriteInt64(DedicatedServerCreationTime.ToUnixTimeSeconds());
            buffer.WriteString(Id);
            buffer.WriteUInt8((byte)GameState);
            buffer.WriteVarUInt(CurrentPlayerCount);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            DedicatedServerId = bufferReader.ReadString();
            DedicatedServerCreationTime = DateTimeOffset.FromUnixTimeSeconds(bufferReader.ReadInt64());
            Id = bufferReader.ReadString();
            GameState = (GameStateType)bufferReader.ReadByte();
            CurrentPlayerCount = bufferReader.ReadVarUInt();
        }
    }
}
