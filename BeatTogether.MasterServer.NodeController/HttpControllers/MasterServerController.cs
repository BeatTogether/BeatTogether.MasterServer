﻿using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Domain.Models;
using BeatTogether.MasterServer.NodeController.Abstractions;
using BeatTogether.MasterServer.NodeController.Configuration;
using BeatTogether.MasterServer.Messaging.Models;
using BeatTogether.MasterServer.Messaging.Models.HttpApi;
using Microsoft.AspNetCore.Mvc;

namespace BeatTogether.MasterServer.NodeController.HttpControllers
{
    [ApiController]
    public class MasterServerController : Controller
    {
        private readonly IServerRepository _ServerRepository;
        private readonly INodeRepository _NodeRepository;
        private readonly NodeControllerConfiguration _Configuration;

        public MasterServerController(
            IServerRepository serverRepository,
            INodeRepository nodeRepository,
            NodeControllerConfiguration nodeControllerConfiguration)
        {
            _ServerRepository = serverRepository;
            _NodeRepository = nodeRepository;
            _Configuration = nodeControllerConfiguration;
        }

        /// <summary>
        /// Returns a test message so players can test if the connection works
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("test")]
        public string GetTestMessage()
        {
	        return "Yay, seems like you are able to connect to the master server!";
        }

        /// <summary>
        /// Returns the amount of active instances
        /// </summary>
        [HttpGet]
        [Route("get_instance_count")]
        public async Task<int> GetInstanceCount()
        {
            return await _ServerRepository.GetServerCount();
        }

        /// <summary>
        /// Returns the amount of active public instances
        /// </summary>
        [HttpGet]
        [Route("get_public_instance_count")]
        public async Task<int> GetPublicInstanceCount()
        {
            if(DateTime.UtcNow.Ticks - _TimeOfLastPublicInstanceCount > _Configuration.TicksBetweenUpdatingCachedApiResponses)
            {
                _TimeOfLastPublicInstanceCount = DateTime.UtcNow.Ticks;
                _LastPublicServerCount = await _ServerRepository.GetPublicServerCount();
            }
            return _LastPublicServerCount;
        }
        private int _LastPublicServerCount = 0;
        private long _TimeOfLastPublicInstanceCount = 0; //TODO fix this stuff

        /// <summary>
        /// Returns the amount of active players
        /// </summary>
        [HttpGet]
        [Route("get_player_count")]
        public async Task<int> GetPlayerCount()
        {
            if (DateTime.UtcNow.Ticks - _TimeOfLastPlayerCount > _Configuration.TicksBetweenUpdatingCachedApiResponses)
            {
                _TimeOfLastPlayerCount = DateTime.UtcNow.Ticks;
                _LastPlayerCount = await _ServerRepository.GetPlayerCount();
            }
            return _LastPlayerCount; 
        }
        private int _LastPlayerCount = 0;
        private long _TimeOfLastPlayerCount = 0;

        /// <summary>
        /// Returns the amount of player joins since the last master server restart
        /// </summary>
        [HttpGet]
        [Route("get_player_count_since_last_start")]
        public async Task<long> GetPlayerCountSinceStart()
        {
            return await _ServerRepository.TotalPlayerJoins();
        }
        /// <summary>
        /// Returns the amount of instances created since the last master server restart
        /// </summary>
        [HttpGet]
        [Route("get_instance_count_since_last_start")]
        public async Task<long> GetInstancesCreatedSinceServerStart()
        {
            return await _ServerRepository.TotalServersMade();
        }

        /// <summary>
        /// Returns a server from a server code
        /// </summary>
        [HttpGet]
        [Route("get_server_infomation_from_code/{ServerCode}")]
        public async Task<IActionResult> GetServerInfomationFromCode(string ServerCode)
        {
            if (ServerCode == null)
                return BadRequest();
            Server server = await _ServerRepository.GetServerByCode(ServerCode);
            if(server == null)
                return NotFound();
            BeatmapLevelSelectionMask mask = new()
            {
                BeatmapDifficultyMask = (Messaging.Enums.BeatmapDifficultyMask)server.BeatmapDifficultyMask,
                SongPackMask = new(server.SongPackMasks),
                GameplayModifiersMask = (Messaging.Enums.GameplayModifiersMask)server.GameplayModifiersMask
            };

            var config = new Messaging.Models.GameplayServerConfiguration()
            {
                MaxPlayerCount = server.GameplayServerConfiguration.MaxPlayerCount,
                DiscoveryPolicy = (Messaging.Enums.DiscoveryPolicy)server.GameplayServerConfiguration.DiscoveryPolicy,
                GameplayServerControlSettings = (Messaging.Enums.GameplayServerControlSettings)server.GameplayServerConfiguration.GameplayServerControlSettings,
                GameplayServerMode = (Messaging.Enums.GameplayServerMode)server.GameplayServerConfiguration.GameplayServerMode,
                InvitePolicy = (Messaging.Enums.InvitePolicy)server.GameplayServerConfiguration.InvitePolicy,
                SongSelectionMode = (Messaging.Enums.SongSelectionMode)server.GameplayServerConfiguration.SongSelectionMode
            };
            GetServerResponse serverResponse = new(server.InstanceEndPoint.Address, server.ServerName, server.InstanceId, server.Secret, server.Code, server.GameplayServerConfiguration.DiscoveryPolicy is Core.Enums.DiscoveryPolicy.Public, server.GameState is Core.Enums.MultiplayerGameState.Game, mask, config, server.PlayerHashes.ToArray(), "");
            return new JsonResult(serverResponse);
        }

        /// <summary>
        /// Returns a server from a server secret
        /// </summary>
        [HttpGet]
        [Route("get_server_infomation_from_secret/{ServerSecret}")]
        public async Task<IActionResult> GetServerInfomationFromSecret(string ServerSecret)
        {
            if (ServerSecret == null)
                return BadRequest();
            Server server = await _ServerRepository.GetServer(ServerSecret);
            if (server == null)
                return NotFound();
            BeatmapLevelSelectionMask mask = new()
            {
                BeatmapDifficultyMask = (Messaging.Enums.BeatmapDifficultyMask)server.BeatmapDifficultyMask,
                SongPackMask = new(server.SongPackMasks),
                GameplayModifiersMask = (Messaging.Enums.GameplayModifiersMask)server.GameplayModifiersMask
            };
            var config = new Messaging.Models.GameplayServerConfiguration()
            {
                MaxPlayerCount = server.GameplayServerConfiguration.MaxPlayerCount,
                DiscoveryPolicy = (Messaging.Enums.DiscoveryPolicy)server.GameplayServerConfiguration.DiscoveryPolicy,
                GameplayServerControlSettings = (Messaging.Enums.GameplayServerControlSettings)server.GameplayServerConfiguration.GameplayServerControlSettings,
                GameplayServerMode = (Messaging.Enums.GameplayServerMode)server.GameplayServerConfiguration.GameplayServerMode,
                InvitePolicy = (Messaging.Enums.InvitePolicy)server.GameplayServerConfiguration.InvitePolicy,
                SongSelectionMode = (Messaging.Enums.SongSelectionMode)server.GameplayServerConfiguration.SongSelectionMode
            };
            GetServerResponse serverResponse = new(server.InstanceEndPoint.Address, server.ServerName, server.InstanceId, server.Secret, server.Code, server.GameplayServerConfiguration.DiscoveryPolicy is Core.Enums.DiscoveryPolicy.Public, server.GameState is Core.Enums.MultiplayerGameState.Game, mask, config, server.PlayerHashes.ToArray(), "");
            return new JsonResult(serverResponse);
        }

        /// <summary>
        /// Returns a list of public server codes
        /// </summary>
        [HttpGet]
        [Route("get_public_server_codes")]
        public async Task<string[]> GetPublicServerCodes()
        {
            if (DateTime.UtcNow.Ticks - _TimeOfLastPublicServerCodes > _Configuration.TicksBetweenUpdatingCachedApiResponses)
            {
                _TimeOfLastPublicServerCodes = DateTime.UtcNow.Ticks;
                _LastPublicServerCodes = await _ServerRepository.GetPublicServerCodes();
            }
            return _LastPublicServerCodes;
        }
        private string[] _LastPublicServerCodes = Array.Empty<string>();
        private long _TimeOfLastPublicServerCodes = 0;

        /// <summary>
        /// Returns a list of public server codes
        /// </summary>
        [HttpGet]
        [Route("get_public_server_secrets")]
        public async Task<string[]> GetPublicServerSecrets()
        {
            if (DateTime.UtcNow.Ticks - _TimeOfLastPublicServerSecrets > _Configuration.TicksBetweenUpdatingCachedApiResponses)
            {
                _TimeOfLastPublicServerSecrets = DateTime.UtcNow.Ticks;
                _LastPublicServerSecrets = await _ServerRepository.GetPublicServerSecrets();
            }
            return _LastPublicServerSecrets;
        }
        private string[] _LastPublicServerSecrets = Array.Empty<string>();
        private long _TimeOfLastPublicServerSecrets = 0;

        /// <summary>
        /// Returns a list of public servers
        /// </summary>
        [HttpGet]
        [Route("get_public_servers")]
        public async Task<GetServerResponse[]> GetPublicServers()
        {


            if (DateTime.UtcNow.Ticks - _TimeOfLastPublicServers > _Configuration.TicksBetweenUpdatingCachedApiResponses)
            {
                _TimeOfLastPublicServers = DateTime.UtcNow.Ticks;
                Server[] server = await _ServerRepository.GetPublicServerList();
                GetServerResponse[] serverResponses = new GetServerResponse[server.Length];
                for (int i = 0; i < server.Length; i++)
                {
                    BeatmapLevelSelectionMask mask = new()
                    {
                        BeatmapDifficultyMask = (Messaging.Enums.BeatmapDifficultyMask)server[i].BeatmapDifficultyMask,
                        SongPackMask = new(server[i].SongPackMasks),
                        GameplayModifiersMask = (Messaging.Enums.GameplayModifiersMask)server[i].GameplayModifiersMask
                    };
                    var config = new Messaging.Models.GameplayServerConfiguration()
                    {
                        MaxPlayerCount = server[i].GameplayServerConfiguration.MaxPlayerCount,
                        DiscoveryPolicy = (Messaging.Enums.DiscoveryPolicy)server[i].GameplayServerConfiguration.DiscoveryPolicy,
                        GameplayServerControlSettings = (Messaging.Enums.GameplayServerControlSettings)server[i].GameplayServerConfiguration.GameplayServerControlSettings,
                        GameplayServerMode = (Messaging.Enums.GameplayServerMode)server[i].GameplayServerConfiguration.GameplayServerMode,
                        InvitePolicy = (Messaging.Enums.InvitePolicy)server[i].GameplayServerConfiguration.InvitePolicy,
                        SongSelectionMode = (Messaging.Enums.SongSelectionMode)server[i].GameplayServerConfiguration.SongSelectionMode
                    };
                    serverResponses[i] = new(server[i].InstanceEndPoint.Address, server[i].ServerName, server[i].InstanceId, server[i].Secret, server[i].Code, server[i].GameplayServerConfiguration.DiscoveryPolicy is Core.Enums.DiscoveryPolicy.Public, server[i].GameState is Core.Enums.MultiplayerGameState.Game, mask, config, server[i].PlayerHashes.ToArray(), "");
                }
                _LastPublicServers = serverResponses;
            }
            return _LastPublicServers;
        }
        private GetServerResponse[] _LastPublicServers = Array.Empty<GetServerResponse>();
        private long _TimeOfLastPublicServers = 0;

        /// <summary>
        /// Returns the status of the currently online nodes
        /// </summary>
        [HttpGet]
        [Route("get_nodes")]
        public async Task<GetNodeResponse[]> GetStatusOfNodes()
        {
            if (DateTime.UtcNow.Ticks - _TimeOfStatusOfNodes > _Configuration.TicksBetweenUpdatingCachedApiResponses)
            {
                _TimeOfStatusOfNodes = DateTime.UtcNow.Ticks;
                Node[] nodes = _NodeRepository.GetNodes().Values.ToArray();
                GetNodeResponse[] nodesResponse = new GetNodeResponse[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    nodesResponse[i] = new GetNodeResponse(nodes[i].Endpoint, nodes[i].Online, nodes[i].LastStart, nodes[i].LastOnline, nodes[i].NodeVersion.ToString());
                    nodesResponse[i].Players = await _ServerRepository.GetPlayerCountOnEndpoint(nodes[i].Endpoint);
                    nodesResponse[i].Servers = await _ServerRepository.GetServerCountOnEndpoint(nodes[i].Endpoint);
                }
                _LastStatusOfNodes = nodesResponse;
            }
            return _LastStatusOfNodes;
        }
        private GetNodeResponse[] _LastStatusOfNodes = Array.Empty<GetNodeResponse>();
        private long _TimeOfStatusOfNodes = 0;
    }
}