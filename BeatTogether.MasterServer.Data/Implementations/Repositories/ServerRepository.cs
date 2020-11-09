using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Data.Entities;
using BeatTogether.MasterServer.Data.Enums;
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
            if (!hashEntries.Any())
                return null;
            return GetServerFromHashEntries(hashEntries);
        }

        public async Task<Server> GetServerByHostUserId(string userId)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var code = await database.HashGetAsync(RedisKeys.ServersByHostUserId, userId);
            if (code.IsNull)
                return null;
            return await GetServer(code);
        }

        public async Task<Player> GetPlayer(string userId)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var hashEntries = await database.HashGetAllAsync(RedisKeys.Players(userId));
            if (!hashEntries.Any())
                return null;
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
                    hostUserId = (RedisValue)server.Host.UserId,
                    hostUserName = (RedisValue)server.Host.UserName,
                    remoteEndPoint = (RedisValue)server.RemoteEndPoint.ToString(),
                    secret = (RedisValue)server.Secret,
                    code = (RedisValue)server.Code,
                    isPublic = (RedisValue)server.IsPublic,
                    discoveryPolicy = (RedisValue)(int)server.DiscoveryPolicy,
                    invitePolicy = (RedisValue)(int)server.InvitePolicy,
                    beatmapDifficultyMask = (RedisValue)(int)server.BeatmapDifficultyMask,
                    gameplayModifiersMask = (RedisValue)(int)server.GameplayModifiersMask,
                    songPackBloomFilterTop = (RedisValue)server.SongPackBloomFilterTop,
                    songPackBloomFilterBottom = (RedisValue)server.SongPackBloomFilterBottom,
                    currentPlayerCount = (RedisValue)server.CurrentPlayerCount,
                    maximumPlayerCount = (RedisValue)server.MaximumPlayerCount,
                    random = (RedisValue)server.Random,
                    publicKey = (RedisValue)server.PublicKey
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

        public void UpdateSecret(string code, string secret)
        {
            var database = _connectionMultiplexer.GetDatabase();
            database.ScriptEvaluate(
                _updateSecretScript,
                parameters: new
                {
                    serverKey = RedisKeys.Servers(code),
                    currentPlayerCount = (RedisValue)secret
                },
                flags: CommandFlags.DemandMaster | CommandFlags.FireAndForget
            );
        }

        public void UpdateCurrentPlayerCount(string code, int currentPlayerCount)
        {
            var database = _connectionMultiplexer.GetDatabase();
            database.ScriptEvaluate(
                _updateCurrentPlayerCountScript,
                parameters: new
                {
                    serverKey = RedisKeys.Servers(code),
                    currentPlayerCount = (RedisValue)currentPlayerCount
                },
                flags: CommandFlags.DemandMaster | CommandFlags.FireAndForget
            );
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
                RemoteEndPoint = IPEndPoint.Parse(dictionary["RemoteEndPoint"]),
                Secret = dictionary["Secret"],
                Code = dictionary["Code"],
                IsPublic = (bool)dictionary["IsPublic"],
                DiscoveryPolicy = (DiscoveryPolicy)(int)dictionary["DiscoveryPolicy"],
                InvitePolicy = (InvitePolicy)(int)dictionary["InvitePolicy"],
                BeatmapDifficultyMask = (BeatmapDifficultyMask)(int)dictionary["BeatmapDifficultyMask"],
                GameplayModifiersMask = (GameplayModifiersMask)(int)dictionary["GameplayModifiersMask"],
                SongPackBloomFilterTop = (ulong)dictionary["SongPackBloomFilterTop"],
                SongPackBloomFilterBottom = (ulong)dictionary["SongPackBloomFilterBottom"],
                CurrentPlayerCount = (int)dictionary["CurrentPlayerCount"],
                MaximumPlayerCount = (int)dictionary["MaximumPlayerCount"],
                Random = dictionary["Random"],
                PublicKey = dictionary["PublicKey"]
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

        private static readonly LuaScript _updateSecretScript = LuaScript.Prepare(
            File.ReadAllText("Scripts/UpdateSecret.lua")
        );

        private static readonly LuaScript _updateCurrentPlayerCountScript = LuaScript.Prepare(
            File.ReadAllText("Scripts/UpdateCurrentPlayerCount.lua")
        );

        #endregion
    }
}
