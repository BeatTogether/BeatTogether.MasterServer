using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Domain.Enums;
using BeatTogether.MasterServer.Domain.Models;
using StackExchange.Redis;

namespace BeatTogether.MasterServer.Data.Implementations.Repositories
{
    public sealed class ServerRepository : IServerRepository
    {
        /* //TODO
         * Refactor redis server code
         * load scripts to the redis server now it has been updated to latest for extra performance
         * use sets instead of hashes?
         * 
         * specification: 
         * Server list - server ID to server
         * Servers by code - server code to server ID
         * Servers by secret - server secret to server ID
         * Public servers - list of public servers
         * Maybe other stuff?
         * 
         * 
         */


        public static class RedisKeys
        {
            public static RedisKey Servers(string secret) => $"Servers:{secret}";
            public static RedisKey ServersByCode = "ServersByCode";
            public static RedisKey PublicServersByPlayerCount = "PublicServersByPlayerCount";
        };

        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public ServerRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        #region Public Methods

        public Task<string[]> GetPublicServerSecretsList()
        {
            return Task.FromResult(new string[0]);
        }
        public Task<Server[]> GetPublicServerList()
        {
            return Task.FromResult(new Server[0]);
        }

        public Task<string[]> GetServerSecretsList()
        {
            return Task.FromResult(new string[0]); 
        }
        public Task<Server[]> GetServerList()
        {
            return Task.FromResult(new Server[0]); //TODO will only impliment these if they are needed
        }

        public Task<int> GetPublicServerCount()
        {
            return Task.FromResult(0);
        }
        public Task<int> GetServerCount()
        {
            return Task.FromResult(0);
        }

        public async Task<Server> GetServer(string secret)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var hashEntries = await database.HashGetAllAsync(RedisKeys.Servers(secret));
            if (!hashEntries.Any())
                return null;
            var server = GetServerFromHashEntries(hashEntries);
            server.Secret = secret;
            return server;
        }

        public async Task<Server> GetServerByCode(string code)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var secret = await database.HashGetAsync(RedisKeys.ServersByCode, code);
            if (secret.IsNull)
                return null;
            return await GetServer(secret);
        }

        public async Task<Server> GetAvailablePublicServer(InvitePolicy invitePolicy,
            GameplayServerMode serverMode,
            SongSelectionMode songMode,
            GameplayServerControlSettings serverControlSettings,
            BeatmapDifficultyMask difficultyMask,
            GameplayModifiersMask modifiersMask,
            ulong songPackTop,
            ulong songPackBottom)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var redisValues = await database.SortedSetRangeByScoreAsync(RedisKeys.PublicServersByPlayerCount, take: 1);
            var secret = redisValues.First();
            if (secret.IsNull)
                return null;
            return await GetServer(secret);
        }

        public async Task<bool> AddServer(Server server)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var redisResult = await database.ScriptEvaluateAsync(
                _addServerScript,
                parameters: new
                {
                    serverKey = RedisKeys.Servers(server.Secret),
                    serversByCodeKey = RedisKeys.ServersByCode,
                    publicServersByPlayerCountKey = RedisKeys.PublicServersByPlayerCount,
                    ServerId = (RedisValue)server.ServerId,
                    ServerName = (RedisValue)server.ServerName,
                    remoteEndPoint = (RedisValue)server.ServerEndPoint.ToString(),
                    secret = (RedisValue)server.Secret,
                    code = (RedisValue)server.Code,
                    isPublic = (RedisValue)server.IsPublic,
                    discoveryPolicy = (RedisValue)(int)server.GameplayServerConfiguration.DiscoveryPolicy,
                    invitePolicy = (RedisValue)(int)server.GameplayServerConfiguration.InvitePolicy,
                    beatmapDifficultyMask = (RedisValue)(int)server.BeatmapDifficultyMask,
                    gameplayModifiersMask = (RedisValue)(int)server.GameplayModifiersMask,
                    songPackBloomFilterTop = (RedisValue)server.SongPackBloomFilterTop,
                    songPackBloomFilterBottom = (RedisValue)server.SongPackBloomFilterBottom,
                    currentPlayerCount = (RedisValue)server.CurrentPlayerCount,
                    random = (RedisValue)server.Random,
                    publicKey = (RedisValue)server.PublicKey
                },
                flags: CommandFlags.DemandMaster
            );
            return (bool)redisResult;
        }

        public async Task<bool> RemoveServer(string secret)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var redisResult = await database.ScriptEvaluateAsync(
                _removeServerScript,
                parameters: new
                {
                    serverKey = RedisKeys.Servers(secret),
                    serversByCodeKey = RedisKeys.ServersByCode,
                    publicServersByPlayerCountKey = RedisKeys.PublicServersByPlayerCount,
                    secret = (RedisValue)secret
                },
                flags: CommandFlags.DemandMaster
            );
            return (bool)redisResult;
        }

        public Task<bool> RemoveServersWithEndpoint(IPAddress EndPoint)
        {
            return Task.FromResult(false);
        }

        public async Task<bool> IncrementCurrentPlayerCount(string secret)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var server = await GetServer(secret);
            if (server == null)
                return false;
            if (server.IsPublic)
                database.SortedSetIncrement(RedisKeys.PublicServersByPlayerCount, secret, 1.0);
            return true;
        }
        public async Task<bool> DecrementCurrentPlayerCount(string secret)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var server = await GetServer(secret);
            if (server == null)
                return false;
            if (server.IsPublic)
                database.SortedSetDecrement(RedisKeys.PublicServersByPlayerCount, secret, 1.0);
            return true;
        }

        public Task<bool> UpdateCurrentPlayerCount(string secret, int currentPlayerCount)
        {
            var database = _connectionMultiplexer.GetDatabase();
            database.ScriptEvaluate(
                _updateCurrentPlayerCountScript,
                parameters: new
                {
                    serverKey = RedisKeys.Servers(secret),
                    currentPlayerCount = (RedisValue)currentPlayerCount
                },
                flags: CommandFlags.DemandMaster | CommandFlags.FireAndForget
            );
            return Task.FromResult(true);
        }

        #endregion

        #region Private Methods

        private Server GetServerFromHashEntries(HashEntry[] hashEntries)
        {
            var dictionary = hashEntries.ToDictionary();
            return new Server()
            {
                /*
                Host = new Player()
                {
                    UserId = dictionary["HostUserId"],
                    UserName = dictionary["HostUserName"]
                },
                RemoteEndPoint = IPEndPoint.Parse(dictionary["RemoteEndPoint"]),
                Code = dictionary["Code"],
                IsPublic = (bool)dictionary["IsPublic"],
                DiscoveryPolicy = (DiscoveryPolicy)(int)dictionary["DiscoveryPolicy"],
                InvitePolicy = (InvitePolicy)(int)dictionary["InvitePolicy"],
                BeatmapDifficultyMask = (BeatmapDifficultyMask)(int)dictionary["BeatmapDifficultyMask"],
                GameplayModifiersMask = (GameplayModifiersMask)(int)dictionary["GameplayModifiersMask"],
                SongPackBloomFilterTop = (ulong)dictionary["SongPackBloomFilterTop"],
                SongPackBloomFilterBottom = (ulong)dictionary["SongPackBloomFilterBottom"],
                CurrentPlayerCount = (int)dictionary["CurrentPlayerCount"],
                Random = dictionary["Random"],
                PublicKey = dictionary["PublicKey"]
                */
            };
        }

        public Task<long> TotalPlayerJoins()
        {
            return Task.FromResult((long)0);
        }

        public Task<bool> UpdateServerGameplayState(string secret, bool InGameplay)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Scripts

        private static readonly LuaScript _addServerScript = LuaScript.Prepare(
            File.ReadAllText("Scripts/AddServer.lua")
        );

        private static readonly LuaScript _removeServerScript = LuaScript.Prepare(
            File.ReadAllText("Scripts/RemoveServer.lua")
        );

        private static readonly LuaScript _updateCurrentPlayerCountScript = LuaScript.Prepare(
            File.ReadAllText("Scripts/UpdateCurrentPlayerCount.lua")
        );

        #endregion
    }
}
