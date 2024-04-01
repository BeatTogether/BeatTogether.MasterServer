using Autobus;
using BeatTogether.DedicatedServer.Interface.Events;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Domain.Models;
using BeatTogether.MasterServer.Kernal.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public sealed class DedicatedServerEventHandler : IHostedService
    {
        private readonly IAutobus _autobus;
        private readonly IServerRepository _serverRepository;
        private readonly ILogger _logger = Log.ForContext<DedicatedServerEventHandler>();
        private readonly IMasterServerSessionService _masterServerSessionService;
        private readonly INodeRepository _nodeRepository;
        private readonly MasterServerConfiguration _configuration;

        public DedicatedServerEventHandler(
            IAutobus autobus,
            IServerRepository serverRepository,
            IMasterServerSessionService masterServerSessionService,
            INodeRepository nodeRepository,
            MasterServerConfiguration configuration)
        {
            _autobus = autobus;
            _serverRepository = serverRepository;
            _masterServerSessionService = masterServerSessionService;
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
            _autobus.Subscribe<NodeReceivedPlayerEncryptionEvent>(NodeReceivedPlayerEncryptionHandler);
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
            _autobus.Unsubscribe<NodeReceivedPlayerEncryptionEvent>(NodeReceivedPlayerEncryptionHandler);
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
            _ = _serverRepository.RemovePlayer(integrationEvent.Secret, integrationEvent.UserId);
            return Task.CompletedTask;
        }        
        private Task HandlePlayerJoin(PlayerJoinEvent integrationEvent)
        {
            _ = _serverRepository.AddPlayer(integrationEvent.Secret, integrationEvent.UserId);
            return Task.CompletedTask;
        }        
        private Task HandlePlayersChangedEvent(UpdatePlayersEvent integrationEvent)
        {
            _ = _serverRepository.UpdateCurrentPlayers(integrationEvent.Secret, integrationEvent.Users);
            return Task.CompletedTask;
        }

        private Task HandleServerInGameplay(ServerInGameplayEvent serverInGameplayEvent)
        {
            _ = _serverRepository.UpdateServerGameplayState(serverInGameplayEvent.Secret, serverInGameplayEvent.InGame, serverInGameplayEvent.LevelID);
            return Task.CompletedTask;
        }

        private Task NodeStartedHandler(NodeStartedEvent startedEvent)
        {
            var version = new Version(startedEvent.NodeVersion);
            if (_configuration.SupportedDediServerVersions.Where(N => version.Major == N.Major && version.Minor == N.Minor).Any())
            {
                _nodeRepository.SetNodeOnline(IPAddress.Parse(startedEvent.endPoint), startedEvent.NodeVersion);
            }
            else
            {
                _logger.Information($"Node is an incompatable version: " + startedEvent.endPoint + " Please check the master and dedicated servers are up to date");
            }
            return Task.CompletedTask;
        }

        private Task NodeOnlineHandler(NodeOnlineEvent nodeOnlineEvent)
        {
            IPAddress address = IPAddress.Parse(nodeOnlineEvent.endPoint);
            if (!_nodeRepository.GetNodes().Keys.Contains(address))
            {
                NodeStartedHandler(new NodeStartedEvent(nodeOnlineEvent.endPoint, nodeOnlineEvent.NodeVersion));
                return Task.CompletedTask;
            }
            _nodeRepository.ReceivedOK(address);
            return Task.CompletedTask;
        }

 
        private Task InstanceConfigurationUpdateHandler(UpdateInstanceConfigEvent updateInstanceConfigEvent)
        {
            GameplayServerConfiguration gameplayServerConfiguration = new
                (
                updateInstanceConfigEvent.Configuration.MaxPlayerCount,
                (Domain.Enums.DiscoveryPolicy)updateInstanceConfigEvent.Configuration.DiscoveryPolicy,
                (Domain.Enums.InvitePolicy)updateInstanceConfigEvent.Configuration.InvitePolicy,
                (Domain.Enums.GameplayServerMode)updateInstanceConfigEvent.Configuration.GameplayServerMode,
                (Domain.Enums.SongSelectionMode)updateInstanceConfigEvent.Configuration.SongSelectionMode,
                (Domain.Enums.GameplayServerControlSettings)updateInstanceConfigEvent.Configuration.GameplayServerControlSettings
                );
            _serverRepository.UpdateServerConfiguration(updateInstanceConfigEvent.Secret, gameplayServerConfiguration, updateInstanceConfigEvent.ServerName);
            return Task.CompletedTask;
        }



        private Task NodeReceivedPlayerEncryptionHandler(NodeReceivedPlayerEncryptionEvent RecievedEvent)
        {
            _nodeRepository.OnNodeRecievedEncryptionParameters(IPEndPoint.Parse(RecievedEvent.endPoint), IPEndPoint.Parse(RecievedEvent.PlayerEndPoint));
            return Task.CompletedTask;
        }
        #endregion
    }
}
