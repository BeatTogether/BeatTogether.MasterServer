using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Models.LegacyModels;
using Krypton.Buffers;
using Newtonsoft.Json;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public sealed class BeatmapLevelSelectionMaskSimple
    {
        public BeatmapLevelSelectionMaskSimple() { }
        public BeatmapLevelSelectionMaskSimple(BeatmapLevelSelectionMask mask) 
        {
            CompatibleSongPackMask compatMask = new CompatibleSongPackMask(mask.SongPackMask);
            BeatmapDifficultyMask = mask.BeatmapDifficultyMask;
            GameplayModifiersMask = mask.GameplayModifiersMask;
            SongPackMasks = compatMask.LegacySongPackMask.ToShortString();
        }


        [JsonProperty("difficulties")]
        public BeatmapDifficultyMask BeatmapDifficultyMask { get; set; }
        
        [JsonProperty("modifiers")]
        public GameplayModifiersMask GameplayModifiersMask { get; set; }
        
        [JsonProperty("song_packs")]
        public string SongPackMasks { get; set; }
    }
}
