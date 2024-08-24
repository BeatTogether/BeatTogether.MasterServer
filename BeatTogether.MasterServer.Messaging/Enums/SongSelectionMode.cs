namespace BeatTogether.MasterServer.Messaging.Enums
{
    public enum SongSelectionMode
    {
        // Base game
        Vote = 0,
        Random = 1,
        OwnerPicks = 2,
        RandomPlayerPicks = 3,
        // Modded
        ServerPicks = 4
    }
}
