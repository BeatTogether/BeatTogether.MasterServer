using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

        public void AddSession(EndPoint endPoint)
        {
            var database = _connectionMultiplexer.GetDatabase();
            database.SortedSetAdd(
                RedisKeys.SessionsByLastKeepAlive,
                endPoint.ToString(),
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                when: When.NotExists,
                flags: CommandFlags.FireAndForget
            );
        }

        public void RemoveSession(EndPoint endPoint)
        {
            var database = _connectionMultiplexer.GetDatabase();
            database.SortedSetRemove(
                RedisKeys.SessionsByLastKeepAlive,
                endPoint.ToString(),
                flags: CommandFlags.FireAndForget
            );
        }

        public void UpdateLastKeepAlive(EndPoint endPoint, DateTimeOffset lastKeepAlive)
        {
            var database = _connectionMultiplexer.GetDatabase();
            database.SortedSetAdd(
                RedisKeys.SessionsByLastKeepAlive,
                endPoint.ToString(),
                lastKeepAlive.ToUnixTimeSeconds(),
                when: When.Exists,
                flags: CommandFlags.FireAndForget
            );
        }

        public async Task<IEnumerable<EndPoint>> GetInactiveSessions(long timeToLive)
        {
            var database = _connectionMultiplexer.GetDatabase();
            if (timeToLive <= 0)
                return Enumerable.Empty<EndPoint>();
            var redisValues = await database.SortedSetRangeByScoreAsync(
                RedisKeys.SessionsByLastKeepAlive,
                stop: DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timeToLive
            );
            return redisValues.Select(rv => IPEndPoint.Parse(rv));
        }

        #endregion
    }
}
