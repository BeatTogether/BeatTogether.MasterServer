using System;

namespace BeatTogether.MasterServer.Domain.Enums
{
    [Flags]
    public enum BeatmapDifficultyMask : byte
    {
        Easy = 1,
        Normal = 2,
        Hard = 4,
        Expert = 8,
        ExpertPlus = 16,
        All = 31
    }
}
