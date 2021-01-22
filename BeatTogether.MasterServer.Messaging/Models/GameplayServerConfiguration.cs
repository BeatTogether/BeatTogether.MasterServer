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

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteUInt8((byte)BeatmapDifficultyMask);
            bufferWriter.WriteUInt16((ushort)GameplayModifiersMask);
            bufferWriter.WriteUInt64(SongPackBloomFilterTop);
            bufferWriter.WriteUInt64(SongPackBloomFilterBottom);
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
