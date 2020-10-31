using System.IO;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Data.Entities;
using StackExchange.Redis;

namespace BeatTogether.MasterServer.Data.Implementations.Repositories
{
    public class ServerRepository : IServerRepository
    {
        public static class RedisKeys
        {
            public static RedisKey Servers(string code) => $"Servers:{code}";
            public static RedisKey Players(string userId) => $"Players:{userId}";
            public static RedisKey ServersByHostUserId = "ServersByHostUserId";
            public static RedisKey PublicServersByPlayerCount = "PublicServersByPlayerCount";
            public static RedisKey PrivateServersByPlayerCount = "PrivateServersByPlayerCount";
        };

        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public ServerRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        #region Public Methods

        public async Task<Server> GetServer(string code)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var hashEntries = await database.HashGetAllAsync(RedisKeys.Servers(code));
            return GetServerFromHashEntries(hashEntries);
        }

        public async Task<Player> GetPlayer(string userId)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var hashEntries = await database.HashGetAllAsync(RedisKeys.Players(userId));
            return GetPlayerFromHashEntries(hashEntries);
        }

        public async Task<bool> AddServer(Server server)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var redisResult = await database.ScriptEvaluateAsync(
                _addServerScript,
                parameters: new
                {
                    serverKey = RedisKeys.Servers(server.Code),
                    playerKey = RedisKeys.Players(server.Host.UserId),
                    serversByHostUserIdKey = RedisKeys.ServersByHostUserId,
                    serversByPlayerCountKey = server.IsPublic
                        ? RedisKeys.PublicServersByPlayerCount
                        : RedisKeys.PrivateServersByPlayerCount,
                    hostUserId = server.Host.UserId,
                    hostUserName = server.Host.UserName,
                    code = server.Code,
                    isPublic = server.IsPublic,
                    maximumPlayerCount = server.MaximumPlayerCount
                },
                flags: CommandFlags.DemandMaster
            );
            return (bool)redisResult;
        }

        public async Task<bool> RemoveServer(string code)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var redisResult = await database.ScriptEvaluateAsync(
                _removeServerScript,
                parameters: new
                {
                    serverKey = RedisKeys.Servers(code),
                    serversByHostUserIdKey = RedisKeys.ServersByHostUserId,
                    publicServersByPlayerCountKey = RedisKeys.PublicServersByPlayerCount,
                    privateServersByPlayerCountKey = RedisKeys.PrivateServersByPlayerCount
                },
                flags: CommandFlags.DemandMaster
            );
            return (bool)redisResult;
        }

        public async Task<Server> GetAvailablePublicServerAndAddPlayer(string userId, string userName)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var redisResult = await database.ScriptEvaluateAsync(
                _getAvailablePublicServerAndAddPlayerScript,
                parameters: new
                {
                    playerKey = RedisKeys.Players(userId),
                    publicServersByPlayerCountKey = RedisKeys.PublicServersByPlayerCount,
                    userId = userId,
                    userName = userName
                },
                flags: CommandFlags.DemandMaster
            );
            if (redisResult.IsNull)
                return null;
            return await GetServer((string)redisResult);
        }

        public async Task<Server> GetServerWithCodeAndAddPlayer(string code, string userId, string userName)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var redisResult = await database.ScriptEvaluateAsync(
                _getServerWithCodeAndAddPlayerScript,
                parameters: new
                {
                    playerKey = RedisKeys.Players(userId),
                    serverKey = RedisKeys.Servers(code),
                    publicServersByPlayerCountKey = RedisKeys.PublicServersByPlayerCount,
                    privateServersByPlayerCountKey = RedisKeys.PrivateServersByPlayerCount,
                    userId = userId,
                    userName = userName,
                    code = code
                },
                flags: CommandFlags.DemandMaster
            );
            if (redisResult.IsNull)
                return null;
            return await GetServer((string)redisResult);
        }

        public async Task<bool> RemovePlayer(string userId)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var redisResult = await database.ScriptEvaluateAsync(
                _removePlayerScript,
                parameters: new
                {
                    playerKey = RedisKeys.Players(userId),
                    publicServersByPlayerCountKey = RedisKeys.PublicServersByPlayerCount,
                    privateServersByPlayerCountKey = RedisKeys.PrivateServersByPlayerCount
                },
                flags: CommandFlags.DemandMaster
            );
            return (bool)redisResult;
        }

        #endregion

        #region Private Methods

        private Server GetServerFromHashEntries(HashEntry[] hashEntries)
        {
            var dictionary = hashEntries.ToDictionary();
            return new Server()
            {
                Host = new Player()
                {
                    UserId = dictionary["HostUserId"],
                    UserName = dictionary["HostUserName"],
                    CurrentServerCode = dictionary["Code"]
                },
                Code = dictionary["Code"],
                IsPublic = (bool)dictionary["IsPublic"],
                MaximumPlayerCount = (int)dictionary["MaximumPlayerCount"]
            };
        }

        private Player GetPlayerFromHashEntries(HashEntry[] hashEntries)
        {
            var dictionary = hashEntries.ToDictionary();
            return new Player()
            {
                UserId = dictionary["UserId"],
                UserName = dictionary["UserName"],
                CurrentServerCode = dictionary["CurrentServerCode"]
            };
        }

        #endregion

        #region Scripts

        private static readonly LuaScript _addServerScript = LuaScript.Prepare(
            File.ReadAllText("Scripts/AddServer.lua")
        );

        private static readonly LuaScript _removeServerScript = LuaScript.Prepare(
            File.ReadAllText("Scripts/RemoveServer.lua")
        );

        private static readonly LuaScript _getAvailablePublicServerAndAddPlayerScript = LuaScript.Prepare(
            File.ReadAllText("Scripts/GetAvailablePublicServerAndAddPlayer.lua")
        );

        private static readonly LuaScript _getServerWithCodeAndAddPlayerScript = LuaScript.Prepare(
            File.ReadAllText("Scripts/GetServerWithCodeAndAddPlayer.lua")
        );

        private static readonly LuaScript _removePlayerScript = LuaScript.Prepare(
            File.ReadAllText("Scripts/RemovePlayer.lua")
        );

        #endregion
    }
}
