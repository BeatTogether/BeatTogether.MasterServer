﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.Core.Enums;
using BeatTogether.Core.Models;
using BeatTogether.MasterServer.Domain.Models;

namespace BeatTogether.MasterServer.Data.Implementations.Repositories
{
    public sealed class MemoryServerRepository : IServerRepository
    {
        private static readonly ConcurrentDictionary<string, Server> _servers = new();
        private static readonly ConcurrentDictionary<string, string> _secretsByCode = new();
        private static long TotalJoins = 0;
        private static long _TotalServersMade = 0;

        public Task<long> TotalPlayerJoins()
        {
            return Task.FromResult(TotalJoins);
        }

        public Task<long> TotalServersMade()
        {
            return Task.FromResult(_TotalServersMade);
        }

        public Task<string[]> GetServerSecretsList()
        {
            return Task.FromResult(_servers.Keys.ToArray());
        }

        public Task<Server[]> GetServerList()
        {
            return Task.FromResult(_servers.Values.ToArray());
        }

        public Task<string[]> GetPublicServerSecrets()
        {
            List<string> secrets = new();
            foreach (var server in _servers.Values)
            {
                if (server.GameplayServerConfiguration.DiscoveryPolicy == DiscoveryPolicy.Public)
                    secrets.Add(server.Secret);
            }
            return Task.FromResult(secrets.ToArray());
        }
        public Task<string[]> GetPublicServerCodes()
        {
            List<string> codes = new();
            foreach (var server in _servers.Values)
            {
                if (server.GameplayServerConfiguration.DiscoveryPolicy == DiscoveryPolicy.Public)
                    codes.Add(server.Code);
            }
            return Task.FromResult(codes.ToArray());
        }

        public Task<Server[]> GetPublicServerList()
        {
            return Task.FromResult((_servers.Values.Where(value => value.GameplayServerConfiguration.DiscoveryPolicy == DiscoveryPolicy.Public)).ToArray());
        }

        public Task<int> GetPublicServerCount()
        {
            return Task.FromResult((_servers.Values.Where(value => value.GameplayServerConfiguration.DiscoveryPolicy == DiscoveryPolicy.Public)).Count());
        }
        public Task<int> GetServerCount()
        {
            return Task.FromResult(_servers.Count);
        }
        public Task<int> GetPlayerCount()
        {
            int count = 0;
            foreach (var server in _servers.Values)
                count += server.CurrentPlayerCount;
            return Task.FromResult(count);
        }

        public Task<Server> GetServer(string secret)
        {
            if (!_servers.TryGetValue(secret, out var server))
                return Task.FromResult<Server>(null);
            return Task.FromResult(server);
        }

        public Task<Server> GetServerByCode(string code)
        {
            if (!_secretsByCode.TryGetValue(code, out var secret))
                return Task.FromResult<Server>(null);
            if (!_servers.TryGetValue(secret, out var server))
                return Task.FromResult<Server>(null);
            return Task.FromResult(server);
        }

        public Task<Server> GetAvailablePublicServer(
	        InvitePolicy invitePolicy,
	        GameplayServerMode serverMode,
	        SongSelectionMode songMode,
	        GameplayServerControlSettings serverControlSettings,
	        BeatmapDifficultyMask difficultyMask,
	        GameplayModifiersMask modifiersMask,
	        string SongPackMasks,
	        VersionRange versionRange)
        {
	        if (!_servers.Any())
		        return Task.FromResult<Server>(null);
	        var publicServers = _servers.Values.Where(server =>
		        server.GameplayServerConfiguration.DiscoveryPolicy == DiscoveryPolicy.Public &&
		        server.GameplayServerConfiguration.InvitePolicy == invitePolicy &&
		        server.GameplayServerConfiguration.GameplayServerMode == serverMode &&
		        server.GameplayServerConfiguration.SongSelectionMode == songMode &&
		        server.GameplayServerConfiguration.GameplayServerControlSettings == serverControlSettings &&
		        server.BeatmapDifficultyMask == difficultyMask &&
		        server.GameplayModifiersMask == modifiersMask &&
		        server.SongPackMasks == SongPackMasks &&
                server.SupportedVersionRange == versionRange &&
                server.CurrentPlayerCount < server.GameplayServerConfiguration.MaxPlayerCount
	        );
            if (!publicServers.Any())
                return Task.FromResult<Server>(null);
            var server = publicServers.First();
            //Find server with most players.
            foreach (var publicServer in publicServers)
            {
                if(publicServer.CurrentPlayerCount > server.CurrentPlayerCount)
                {
                    server = publicServer;
                }
            }
            return Task.FromResult(server);
        }


		public Task<bool> AddServer(Server server)
        {
            if (!_servers.TryAdd(server.Secret, server))
                return Task.FromResult(false);
            if (!_secretsByCode.TryAdd(server.Code, server.Secret))
            {
                _servers.TryRemove(server.Secret, out _);
                return Task.FromResult(false);
            }
            Interlocked.Increment(ref _TotalServersMade);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveServer(string secret)
        {
            if (!_servers.TryRemove(secret, out var server))
                return Task.FromResult(false);
            _secretsByCode.TryRemove(server.Code, out _);
            return Task.FromResult(true);
        }
        public Task<bool> RemoveServersWithEndpoint(IPAddress EndPoint)
        {
            List<string> secrets = new();
            foreach (var server in _servers)
            {
                if(server.Value.InstanceEndPoint.Address.ToString() == EndPoint.ToString())
                {
                    secrets.Add(server.Key);
                }
            }
            foreach (string secret in secrets)
            {
                RemoveServer(secret);
            }
            return Task.FromResult(true);
        }

        public Task<int> GetServerCountOnEndpoint(IPAddress EndPoint)
        {
            int count = 0;

            foreach (var server in _servers)
            {
                if (server.Value.InstanceEndPoint.Address.ToString() == EndPoint.ToString())
                {
                    count++;
                }
            }
            return Task.FromResult(count);
        }
        public Task<int> GetPlayerCountOnEndpoint(IPAddress EndPoint)
        {
            int count = 0;
            foreach (var server in _servers)
            {
                if (server.Value.InstanceEndPoint.Address.ToString() == EndPoint.ToString())
                {
                    count += server.Value.CurrentPlayerCount;
                }
            }
            return Task.FromResult(count);
        }

        public Task<bool> AddPlayer(string secret, string playerHash)
        {
            if (!_servers.TryGetValue(secret, out var server))
                return Task.FromResult(false);
            if (server.PlayerHashes.Count + 1 < server.GameplayServerConfiguration.MaxPlayerCount && server.PlayerHashes.Add(playerHash))
            {
                Interlocked.Increment(ref TotalJoins);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> RemovePlayer(string secret, string playerHash)
        {
            if (!_servers.TryGetValue(secret, out var server))
                return Task.FromResult(false);
            return Task.FromResult(server.PlayerHashes.Remove(playerHash));
        }

        public Task<bool> UpdateServer(string secret, Server NewServerValues)
        {
            if (!_servers.TryGetValue(secret, out var server))
                return Task.FromResult(false);

            server.ServerName = NewServerValues.ServerName;
            server.GameState = NewServerValues.GameState;
            server.BeatmapDifficultyMask = NewServerValues.BeatmapDifficultyMask;
            server.GameplayModifiersMask = NewServerValues.GameplayModifiersMask;
            server.GameplayServerConfiguration = NewServerValues.GameplayServerConfiguration;
            server.SongPackMasks = NewServerValues.SongPackMasks;
            server.PlayerHashes = NewServerValues.PlayerHashes;
            server.ManagerId = NewServerValues.ManagerId;
            server.PermanentManager = NewServerValues.PermanentManager;
            server.ServerStartJoinTimeout = NewServerValues.ServerStartJoinTimeout;
            server.NeverCloseServer = NewServerValues.NeverCloseServer;
            server.ResultScreenTime = NewServerValues.ResultScreenTime;
            server.BeatmapStartTime = NewServerValues.BeatmapStartTime;
            server.PlayersReadyCountdownTime = NewServerValues.PlayersReadyCountdownTime;
            server.AllowPerPlayerModifiers = NewServerValues.AllowPerPlayerModifiers;
            server.AllowPerPlayerDifficulties = NewServerValues.AllowPerPlayerDifficulties;
            server.AllowChroma = NewServerValues.AllowChroma;
            server.AllowME = NewServerValues.AllowME;
            server.AllowNE = NewServerValues.AllowNE;


            return Task.FromResult(true);
        }

        public Task<bool> UpdateCurrentPlayers(string secret, string[] players)
        {
            if (!_servers.TryGetValue(secret, out var server))
                return Task.FromResult(false);
            server.PlayerHashes = players.ToHashSet();
            return Task.FromResult(true);
        }

    }
}
