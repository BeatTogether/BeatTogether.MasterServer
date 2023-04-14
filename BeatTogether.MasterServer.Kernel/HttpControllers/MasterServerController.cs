using System.Linq;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Domain.Models;
using BeatTogether.MasterServer.Kernal.Abstractions;
using BeatTogether.MasterServer.Messaging.Models;
using BeatTogether.MasterServer.Messaging.Models.HttpApi;
using Microsoft.AspNetCore.Mvc;
using Serilog;


namespace BeatTogether.MasterServer.Kernel.HttpControllers
{
    [ApiController]
    public class MasterServerController : Controller
    {
        private readonly IServerRepository _ServerRepository;
        private readonly INodeRepository _NodeRepository;
        private readonly ILogger _logger;

        public MasterServerController(
            IServerRepository serverRepository,
            INodeRepository nodeRepository)
        {
            _ServerRepository = serverRepository;
            _NodeRepository = nodeRepository;

            _logger = Log.ForContext<MasterServerController>();
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
            return await _ServerRepository.GetPublicServerCount();
        }

        /// <summary>
        /// Returns the amount of active players
        /// </summary>
        [HttpGet]
        [Route("get_player_count")]
        public async Task<int> GetPlayerCount()
        {
            return await _ServerRepository.GetPlayerCount();
        }

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
                SongPackMask = new(server.SongPackBloomFilterTop, server.SongPackBloomFilterBottom),
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
            GetServerResponse serverResponse = new(server.ServerEndPoint.Address, server.ServerName, server.ServerId, server.Secret, server.Code, server.IsPublic, server.IsInGameplay, mask, config, server.CurrentPlayerCount);
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
                SongPackMask = new(server.SongPackBloomFilterTop, server.SongPackBloomFilterBottom),
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
            GetServerResponse serverResponse = new(server.ServerEndPoint.Address, server.ServerName, server.ServerId, server.Secret, server.Code, server.IsPublic, server.IsInGameplay, mask, config, server.CurrentPlayerCount);
            return new JsonResult(serverResponse);
        }

        /// <summary>
        /// Returns a list of public server codes
        /// </summary>
        [HttpGet]
        [Route("get_public_server_codes")]
        public async Task<string[]> GetPublicServerCodes()
        {
            return await _ServerRepository.GetPublicServerSecrets();
        }

        /// <summary>
        /// Returns a list of public server codes
        /// </summary>
        [HttpGet]
        [Route("get_public_server_secrets")]
        public async Task<string[]> GetPublicServerSecrets()
        {
            return await _ServerRepository.GetPublicServerSecrets();
        }

        /// <summary>
        /// Returns the status of the currently online nodes
        /// </summary>
        [HttpGet]
        [Route("get_nodes")]
        public async Task<GetNodeResponse[]> GetStatusOfNodes()
        {
            Node[] nodes = _NodeRepository.GetNodes().Values.ToArray();
            GetNodeResponse[] nodesResponse = new GetNodeResponse[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodesResponse[i] = new GetNodeResponse(nodes[i].endpoint, nodes[i].Online, nodes[i].LastStart, nodes[i].LastOnline, nodes[i].NodeVersion);
                nodesResponse[i].Players = await _ServerRepository.GetPlayerCountOnEndpoint(nodes[i].endpoint);
                nodesResponse[i].Servers = await _ServerRepository.GetServerCountOnEndpoint(nodes[i].endpoint);
            }
            return nodesResponse;
        }
    }
}