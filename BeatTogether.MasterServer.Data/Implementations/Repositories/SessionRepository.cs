using System;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using StackExchange.Redis;

namespace BeatTogether.MasterServer.Data.Implementations.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        public static class RedisKeys
        {
            public static RedisKey SessionsByLastKeepAlive = "SessionsByLastKeepAlive";
        };

        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public SessionRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        #region Public Methods

        public void UpdateLastKeepAlive(string userId, DateTimeOffset lastKeepAlive)
        {
            var database = _connectionMultiplexer.GetDatabase();
            database.SortedSetAdd(
                RedisKeys.SessionsByLastKeepAlive,
                userId,
                lastKeepAlive.ToUnixTimeSeconds(),
                when: When.Exists,
                flags: CommandFlags.FireAndForget
            );
        }

        #endregion
    }
}
