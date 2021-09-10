using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Domain.Enums;
using BeatTogether.MasterServer.Domain.Models;

namespace BeatTogether.MasterServer.Data.Implementations.Repositories
{
    public sealed class MemoryServerRepository : IServerRepository
    {
        private static ConcurrentDictionary<string, Server> _servers = new();
        private static ConcurrentDictionary<string, Server> _serversByCode = new();

        public Task<Server> GetServer(string secret)
        {
            if (!_servers.TryGetValue(secret, out var server))
                return Task.FromResult<Server>(null);
            return Task.FromResult(server);
        }

        public Task<Server> GetServerByCode(string code)
        {
            if (!_serversByCode.TryGetValue(code, out var server))
                return Task.FromResult<Server>(null);
            return Task.FromResult(server);
        }

        public Task<Server> GetAvailablePublicServer()
        {
            if (!_servers.Any())
                return null;
            var publicServers = _servers.Values.Where(server => server.DiscoveryPolicy == DiscoveryPolicy.Public);
            var server = publicServers.First();
            foreach (var publicServer in publicServers)
            {
                if (publicServer.CurrentPlayerCount < server.CurrentPlayerCount)
                    server = publicServer;
                if (server.CurrentPlayerCount <= 1)
                    break;
            }
            if (server.CurrentPlayerCount >= Server.MaximumPlayerCount)
                return Task.FromResult<Server>(null);
            return Task.FromResult(server);
        }

        public Task<bool> AddServer(Server server)
        {
            if (!_servers.TryAdd(server.Secret, server))
                return Task.FromResult(false);
            _serversByCode[server.Code] = server;
            return Task.FromResult(true);
        }

        public Task<bool> RemoveServer(string secret)
        {
            if (!_servers.TryRemove(secret, out var server))
                return Task.FromResult(false);
            _serversByCode.TryRemove(server.Code, out _);
            return Task.FromResult(true);
        }

        public Task<bool> IncrementCurrentPlayerCount(string secret)
        {
            if (!_servers.TryGetValue(secret, out var server))
                return Task.FromResult(false);
            server.CurrentPlayerCount++;
            return Task.FromResult(true);
        }

        public void UpdateCurrentPlayerCount(string secret, int currentPlayerCount)
        {
            if (!_servers.TryGetValue(secret, out var server))
                return;
            server.CurrentPlayerCount = currentPlayerCount;
        }
    }
}
