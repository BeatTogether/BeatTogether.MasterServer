using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Enums;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public class GameplayServerConfiguration : IMessage
    {
        public BeatmapDifficultyMask BeatmapDifficultyMask { get; set; }
        public GameplayModifiersMask GameplayModifiersMask { get; set; }
        public ulong SongPackBloomFilterTop { get; set; }
        public ulong SongPackBloomFilterBottom { get; set; }

        public void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteUInt8((byte)BeatmapDifficultyMask);
            buffer.WriteUInt16((ushort)GameplayModifiersMask);
            buffer.WriteUInt64(SongPackBloomFilterTop);
            buffer.WriteUInt64(SongPackBloomFilterBottom);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            BeatmapDifficultyMask = (BeatmapDifficultyMask)bufferReader.ReadByte();
            GameplayModifiersMask = (GameplayModifiersMask)bufferReader.ReadUInt16();
            SongPackBloomFilterTop = bufferReader.ReadUInt64();
            SongPackBloomFilterBottom = bufferReader.ReadUInt64();
        }
    }
}
