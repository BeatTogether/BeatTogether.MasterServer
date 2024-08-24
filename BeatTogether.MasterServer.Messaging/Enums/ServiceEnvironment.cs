using System;

namespace BeatTogether.MasterServer.HttpApi.Models.Enums
{
    public enum ServiceEnvironment
    {
        Production,
        ReleaseCandidate,
        InternalPlayTest,
        QATesting,
        [Obsolete("Use DevelopmentA or DevelopmentB")]
        Development,
        ProductionA,
        ProductionB,
        DevelopmentA,
        DevelopmentB,
        ProductionC,
        ProductionQuest1
    }
}