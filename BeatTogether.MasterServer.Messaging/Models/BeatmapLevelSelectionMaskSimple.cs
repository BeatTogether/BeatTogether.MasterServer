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
            BeatmapLevelSelectionMaskSimple beatmapLevelSelectionMaskSimple = new BeatmapLevelSelectionMaskSimple();
            beatmapLevelSelectionMaskSimple.BeatmapDifficultyMask = mask.BeatmapDifficultyMask;
            beatmapLevelSelectionMaskSimple.GameplayModifiersMask = mask.GameplayModifiersMask;
            beatmapLevelSelectionMaskSimple.SongPackMasks = mask.SongPackMask.ToShortString();
            return beatmapLevelSelectionMaskSimple;
        }


        [JsonProperty("difficulties")]
        public BeatmapDifficultyMask BeatmapDifficultyMask { get; set; }
        
        [JsonProperty("modifiers")]
        public GameplayModifiersMask GameplayModifiersMask { get; set; }
        
        [JsonProperty("song_packs")]
        public string SongPackMasks { get; set; }
    }
}
