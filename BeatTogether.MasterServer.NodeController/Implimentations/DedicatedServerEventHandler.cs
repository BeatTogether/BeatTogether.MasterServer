using Autobus;
using BeatTogether.DedicatedServer.Interface.Events;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.NodeController.Abstractions;
using BeatTogether.MasterServer.NodeController.Configuration;
using Serilog;
using System.Net;
using ILogger = Serilog.ILogger;

namespace BeatTogether.MasterServer.NodeController.Implementations
{
    public sealed class DedicatedServerEventHandler : IHostedService
    {
        private readonly IAutobus _autobus;
        private readonly IServerRepository _serverRepository;
        private readonly ILogger _logger = Log.ForContext<DedicatedServerEventHandler>();
        private readonly INodeRepository _nodeRepository;
        private readonly NodeControllerConfiguration _configuration;

        public DedicatedServerEventHandler(
            IAutobus autobus,
            IServerRepository serverRepository,
            INodeRepository nodeRepository,
            NodeControllerConfiguration configuration)
        {
            _autobus = autobus;
            _serverRepository = serverRepository;
            _nodeRepository = nodeRepository;
            _configuration = configuration;
        }

        #region Public Methods

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _autobus.Subscribe<MatchmakingServerStoppedEvent>(HandleServerStop);
            _autobus.Subscribe<PlayerLeaveServerEvent>(HandlePlayerDisconnect);
            _autobus.Subscribe<PlayerJoinEvent>(HandlePlayerJoin);
            _autobus.Subscribe<NodeStartedEvent>(NodeStartedHandler);
            _autobus.Subscribe<NodeReceivedPlayerSessionDataEvent>(NodeReceivedPlayerSessionDataHandler);
            _autobus.Subscribe<NodeOnlineEvent>(NodeOnlineHandler);
            _autobus.Subscribe<ServerInGameplayEvent>(HandleServerInGameplay);
            _autobus.Subscribe<UpdateInstanceConfigEvent>(InstanceConfigurationUpdateHandler);
            _autobus.Subscribe<UpdatePlayersEvent>(HandlePlayersChangedEvent);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _autobus.Unsubscribe<MatchmakingServerStoppedEvent>(HandleServerStop);
            _autobus.Unsubscribe<PlayerLeaveServerEvent>(HandlePlayerDisconnect);
            _autobus.Unsubscribe<PlayerJoinEvent>(HandlePlayerJoin);
            _autobus.Unsubscribe<NodeStartedEvent>(NodeStartedHandler);
            _autobus.Unsubscribe<NodeReceivedPlayerSessionDataEvent>(NodeReceivedPlayerSessionDataHandler);
            _autobus.Unsubscribe<NodeOnlineEvent>(NodeOnlineHandler);
            _autobus.Unsubscribe<ServerInGameplayEvent>(HandleServerInGameplay);
            _autobus.Unsubscribe<UpdateInstanceConfigEvent>(InstanceConfigurationUpdateHandler);
            _autobus.Unsubscribe<UpdatePlayersEvent>(HandlePlayersChangedEvent);
            return Task.CompletedTask;
        }

        #endregion

        #region Private Methods

        private async Task HandleServerStop(MatchmakingServerStoppedEvent integrationEvent)
        {
            var secret = integrationEvent.Secret;
            _logger.Debug(
                $"Handling {nameof(MatchmakingServerStoppedEvent)} " +
                $"(Secret='{secret}').");
            await _serverRepository.RemoveServer(secret);
            return;
        }

        private Task HandlePlayerDisconnect(PlayerLeaveServerEvent integrationEvent)
        {
            _ = _serverRepository.RemovePlayer(integrationEvent.Secret, integrationEvent.HashedUserId);
            return Task.CompletedTask;
        }        
        private Task HandlePlayerJoin(PlayerJoinEvent integrationEvent)
        {
            _ = _serverRepository.AddPlayer(integrationEvent.Secret, integrationEvent.HashedUserId);
            return Task.CompletedTask;
        }        
        private Task HandlePlayersChangedEvent(UpdatePlayersEvent integrationEvent)
        {
            _ = _serverRepository.UpdateCurrentPlayers(integrationEvent.Secret, integrationEvent.HashedUserIds);
            return Task.CompletedTask;
        }

        private async Task HandleServerInGameplay(ServerInGameplayEvent serverInGameplayEvent)
        {
            var Server = await _serverRepository.GetServer(serverInGameplayEvent.Secret);
            Server.GameState = serverInGameplayEvent.MultiplayerGameState;
            await _serverRepository.UpdateServer(serverInGameplayEvent.Secret, Server);
        }

        private Task NodeStartedHandler(NodeStartedEvent startedEvent)
        {
            var version = new Version(startedEvent.NodeVersion);
            if (_configuration.SupportedDediServerVersions.Where(N => version.Major == N.Major && version.Minor == N.Minor).Any())
            {
                _nodeRepository.SetNodeOnline(IPAddress.Parse(startedEvent.EndPoint), startedEvent.NodeVersion);
            }
            else
            {
                _logger.Information($"Node is an incompatable version: " + startedEvent.EndPoint + " Please check the master and dedicated servers are up to date");
            }
            return Task.CompletedTask;
        }

        private Task NodeOnlineHandler(NodeOnlineEvent nodeOnlineEvent)
        {
            IPAddress address = IPAddress.Parse(nodeOnlineEvent.EndPoint);
            if (!_nodeRepository.GetNodes().ContainsKey(address))
            {
                NodeStartedHandler(new NodeStartedEvent(nodeOnlineEvent.EndPoint, nodeOnlineEvent.NodeVersion));
                return Task.CompletedTask;
            }
            _nodeRepository.ReceivedOK(address);
            return Task.CompletedTask;
        }

 
        private async Task InstanceConfigurationUpdateHandler(UpdateInstanceConfigEvent updateInstanceConfigEvent)
        {
            var Server = await _serverRepository.GetServer(updateInstanceConfigEvent.ServerInsance.Secret);
            Server.GameplayServerConfiguration = updateInstanceConfigEvent.ServerInsance.GameplayServerConfiguration;

            Server.GameplayModifiersMask = updateInstanceConfigEvent.ServerInsance.GameplayModifiersMask;
            Server.BeatmapDifficultyMask = updateInstanceConfigEvent.ServerInsance.BeatmapDifficultyMask;
            Server.SongPackMasks = updateInstanceConfigEvent.ServerInsance.SongPackMasks;

            Server.ServerName = updateInstanceConfigEvent.ServerInsance.ServerName;

            Server.AllowChroma = updateInstanceConfigEvent.ServerInsance.AllowChroma;
            Server.AllowME = updateInstanceConfigEvent.ServerInsance.AllowME;
            Server.AllowNE = updateInstanceConfigEvent.ServerInsance.AllowNE;

            Server.AllowPerPlayerDifficulties = updateInstanceConfigEvent.ServerInsance.AllowPerPlayerDifficulties;
            Server.AllowPerPlayerModifiers = updateInstanceConfigEvent.ServerInsance.AllowPerPlayerModifiers;

            Server.BeatmapStartTime = updateInstanceConfigEvent.ServerInsance.BeatmapStartTime;
            Server.PlayersReadyCountdownTime = updateInstanceConfigEvent.ServerInsance.PlayersReadyCountdownTime;
            Server.ResultScreenTime = updateInstanceConfigEvent.ServerInsance.ResultScreenTime;

            Server.PermanentManager = updateInstanceConfigEvent.ServerInsance.PermanentManager;
            Server.NeverCloseServer = updateInstanceConfigEvent.ServerInsance.NeverCloseServer;
            Server.ServerStartJoinTimeout = updateInstanceConfigEvent.ServerInsance.ServerStartJoinTimeout;

            Server.ManagerId = updateInstanceConfigEvent.ServerInsance.ManagerId;

            await _serverRepository.UpdateServer(Server.Secret, Server);
        }



        private Task NodeReceivedPlayerSessionDataHandler(NodeReceivedPlayerSessionDataEvent RecievedEvent)
        {
            _nodeRepository.OnNodeRecievedSessionDataParameters(IPEndPoint.Parse(RecievedEvent.EndPoint), RecievedEvent.PlayerSessionId);
            return Task.CompletedTask;
        }
        #endregion
    }
}
