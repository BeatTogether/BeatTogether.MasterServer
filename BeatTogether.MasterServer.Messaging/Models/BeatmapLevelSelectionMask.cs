using BeatTogether.MasterServer.Messaging.Enums;
using Newtonsoft.Json;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public sealed class BeatmapLevelSelectionMask
    {
        public BeatmapLevelSelectionMask() { }
        public BeatmapLevelSelectionMask(BeatmapLevelSelectionMaskSimple simpleMask) 
        {
            BeatmapDifficultyMask = simpleMask.BeatmapDifficultyMask;
            GameplayModifiersMask = simpleMask.GameplayModifiersMask;
            SongPackMask = SongPackMask.Parse(simpleMask.SongPackMasks);
        }

        [JsonProperty("difficulties")]
        public BeatmapDifficultyMask BeatmapDifficultyMask { get; set; }
        
        [JsonProperty("modifiers")]
        public GameplayModifiersMask GameplayModifiersMask { get; set; }
        
        [JsonProperty("song_packs")]
        public SongPackMask SongPackMask { get; set; } = new();

    }
}
