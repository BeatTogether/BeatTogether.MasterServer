using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Models.LegacyModels;
using Krypton.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public class CompatibleSongPackMask
    {
        public SongPackMask SongPackMask { get; set; } = new(BitMask256.MaxValue);
        public LegacySongPackMask LegacySongPackMask { get; set; } = new(BitMask128.MaxValue);

        static readonly string[] knownOSTs = { "OstVol1", "OstVol2", "OstVol3", "OstVol4", "OstVol5", "OstVol6", "Extras", "Camellia" };

        static readonly string[] knownDLCs = { "Monstercat", "RocketLeague", "GreenDay", "Timbaland", "BTS", "Interscope", "Skrillex", "BillieEilish", "LadyGaga", "FallOutBoy", "EDM", "Lizzo", "RockMixtape", "ImagineDragons", "PanicAtTheDisco", "Queen", "TheWeeknd", "LinkinPark", "LinkinPark2", "TheRollingStones" };

        static readonly string customLevelPackName = "custom_levelpack_CustomLevels";

        static readonly string[] allPacks = knownOSTs.Concat(knownDLCs).Concat(new string[] { customLevelPackName }).ToArray();

        IReadOnlyDictionary<SongPackMask, LegacySongPackMask> knownSongPackMasks = new Dictionary<SongPackMask, LegacySongPackMask>()
        {
            { new(BitMask256.MinValue), new(BitMask128.MinValue) },
            { new(BitMask256.MaxValue), new(BitMask128.MaxValue) },
            { new(knownOSTs), new(knownOSTs) },
            { new(knownDLCs), new(knownDLCs) },
            { new(customLevelPackName), new(customLevelPackName) },
            { new(allPacks), new(allPacks)}
        };

        public CompatibleSongPackMask(SongPackMask songPackMask)
        {
            SongPackMask = songPackMask;

            if (!knownSongPackMasks.TryGetValue(songPackMask, out LegacySongPackMask legacyPack))
                return;

            LegacySongPackMask = legacyPack;
        }

        public CompatibleSongPackMask(LegacySongPackMask legacySongPackMask)
        {
            LegacySongPackMask = legacySongPackMask;

            foreach(var kvp in knownSongPackMasks)
            {
                if (kvp.Value == legacySongPackMask)
                {
                    SongPackMask = kvp.Key;
                    return;
                }
            }
        }
    }
}
