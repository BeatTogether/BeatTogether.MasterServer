using System;

namespace BeatTogether.MasterServer.Messaging.Enums
{
    [Flags]
    public enum GameplayModifiersMask : ushort
    {
        None = 0,
        BatteryEnergy = 1,
        NoFail = 2,
        InstaFail = 4,
        NoObstacles = 8,
        NoBombs = 16,
        FastNotes = 32,
        StrictAngles = 64,
        DisappearingArrows = 128,
        FasterSong = 256,
        SlowerSong = 512,
        NoArrows = 1024,
        GhostNotes = 2048,
        SuperFastSong = 4096,
        ProMode = 8192,
        ZenMode = 16384,
        SmallCubes = 32768,
        All = 65535
    }
}
