using System;

namespace BeatTogether.MasterServer.Data.Abstractions.Repositories
{
    public interface ISessionRepository
    {
        void UpdateLastKeepAlive(string userId, DateTimeOffset lastKeepAlive);
    }
}
