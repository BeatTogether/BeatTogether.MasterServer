using BeatTogether.MasterServer.Messaging.Enums;
using Newtonsoft.Json;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public sealed class BeatmapLevelSelectionMaskSimple
    {
        public BeatmapLevelSelectionMaskSimple() { }
/*        public static BeatmapLevelSelectionMaskSimple WithLegacySongPackMask(BeatmapLevelSelectionMask mask) 
        {
            BeatmapLevelSelectionMaskSimple beatmapLevelSelectionMaskSimple = new BeatmapLevelSelectionMaskSimple();
            CompatibleSongPackMask compatMask = new CompatibleSongPackMask(mask.SongPackMask);
            beatmapLevelSelectionMaskSimple.BeatmapDifficultyMask = mask.BeatmapDifficultyMask;
            beatmapLevelSelectionMaskSimple.GameplayModifiersMask = mask.GameplayModifiersMask;
            beatmapLevelSelectionMaskSimple.SongPackMasks = compatMask.LegacySongPackMask.ToShortString();
            return beatmapLevelSelectionMaskSimple;
        }*/

        public static BeatmapLevelSelectionMaskSimple WithNewSongPackMask(BeatmapLevelSelectionMask mask)
        {
            BeatmapLevelSelectionMaskSimple beatmapLevelSelectionMaskSimple = new();
            beatmapLevelSelectionMaskSimple.BeatmapDifficultyMask = mask.BeatmapDifficultyMask;
            beatmapLevelSelectionMaskSimple.GameplayModifiersMask = mask.GameplayModifiersMask;
            beatmapLevelSelectionMaskSimple.SongPackMasks = mask.SongPackMask.ToShortString();
            return beatmapLevelSelectionMaskSimple;
        }
        
        [JsonProperty("difficulties")]
        public BeatmapDifficultyMask BeatmapDifficultyMask { get; set; } = BeatmapDifficultyMask.All;

        [JsonProperty("modifiers")]
        public GameplayModifiersMask GameplayModifiersMask { get; set; } = GameplayModifiersMask.All;

        [JsonProperty("song_packs")]
        public string SongPackMasks { get; set; } = "//////////////////////////////////////////8"; // This always has to be set otherwise the client only shows CFR-9 when something fails

    }
}
