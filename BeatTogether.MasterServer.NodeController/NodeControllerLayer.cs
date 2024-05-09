using Autobus;
using BeatTogether.Core.Abstractions;
using BeatTogether.Core.Enums;
using BeatTogether.Core.ServerMessaging.Models;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Interface.Events;
using BeatTogether.MasterServer.NodeController.Abstractions;
using BinaryRecords;
using Serilog;
using System.Net;

namespace BeatTogether.MasterServer.NodeController
{
    public class NodeControllerLayer : ILayer2
    {
        private readonly INodeRepository _nodeRepository;
        private readonly IMatchmakingService _matchmakingService;
        private readonly IServerRepository _serverRepository;
        private readonly IAutobus _autobus;
        private readonly Serilog.ILogger _logger = Log.ForContext<NodeControllerLayer>();
        public const int EncryptionRecieveTimeout = 2000;

        public NodeControllerLayer(
            INodeRepository nodeRepository,
            IMatchmakingService matchmakingService,
            IServerRepository serverRepository,
            IAutobus autobus)
        {
            _nodeRepository = nodeRepository;
            _matchmakingService = matchmakingService;
            _serverRepository = serverRepository;
            _autobus = autobus;
        }


        public Task CloseInstance(string InstanceId)
        {
            //TODO await a response that the server has closed
            _autobus.Publish(new CloseServerInstanceEvent(InstanceId));
            return Task.CompletedTask;
        }

        public async Task<bool> CreateInstance(IServerInstance serverInstance)
        {
            _logger.Information("Sending message to create matchmaking server");
            var response = await _matchmakingService.CreateMatchmakingServer(new CreateMatchmakingServerRequest(new Server(serverInstance)));
            if (response.Success)
            {
                serverInstance.InstanceEndPoint = IPEndPoint.Parse(response.RemoteEndPoint);
                return await _serverRepository.AddServer((Domain.Models.Server)serverInstance);
            }
            _logger.Warning("Dedi replied no");
            return false;
        }

        public Task DisconnectPlayer(string InstanceId, string PlayerUserId)
        {
            //TODO await a response that the player is disconnected
            _autobus.Publish(new DisconnectPlayerFromMatchmakingServerEvent(InstanceId, PlayerUserId));
            return Task.CompletedTask;
        }

        public async Task<IServerInstance?> GetAvailablePublicServer(InvitePolicy invitePolicy, GameplayServerMode serverMode, SongSelectionMode songMode, GameplayServerControlSettings serverControlSettings, BeatmapDifficultyMask difficultyMask, GameplayModifiersMask modifiersMask, string songPackMasks)
        {
            return await _serverRepository.GetAvailablePublicServer(invitePolicy, serverMode, songMode, serverControlSettings, difficultyMask, modifiersMask, songPackMasks);
        }
        public async Task<IServerInstance?> GetServer(string secret)
        {
            return await _serverRepository.GetServer(secret);
        }

        public async Task<IServerInstance?> GetServerByCode(string code)
        {
            return await _serverRepository.GetServerByCode(code);
        }

        public async Task<bool> SetPlayerSessionData(string InstanceSecret, IPlayer playerSessionData)
        {
            var instance = await _serverRepository.GetServer(InstanceSecret);
            if (instance == null)
            {
                _logger.Warning("Tried sending player data to an instance that is not in the server repository");
                return false;
            }
            _logger.Information("Sending player data to node: " + instance.InstanceEndPoint);
            return await _nodeRepository.SendAndAwaitPlayerSessionDataRecievedFromNode(instance.InstanceEndPoint, InstanceSecret, playerSessionData, EncryptionRecieveTimeout);
        }

        private bool DoesServerExist(IServerInstance server)
        {
            return _nodeRepository.EndpointExists(server.InstanceEndPoint);
        }
    }
}
