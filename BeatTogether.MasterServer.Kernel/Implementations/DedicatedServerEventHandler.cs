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
            _autobus.Subscribe<NodeStartedEvent>(NodeStartedHandler);
            _autobus.Subscribe<NodeReceivedPlayerEncryptionEvent>(NodeReceivedPlayerEncryptionHandler);
            _autobus.Subscribe<NodeOnlineEvent>(NodeOnlineHandler);
            _autobus.Subscribe<ServerInGameplayEvent>(HandleServerInGameplay);
            //_autobus.Subscribe<UpdateStatusEvent>(HandleServerStatusChanged);
            //_autobus.Subscribe<UpdateInstanceConfigEvent>(InstanceConfigurationUpdateHandler);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _autobus.Unsubscribe<MatchmakingServerStoppedEvent>(HandleServerStop);
            _autobus.Unsubscribe<PlayerLeaveServerEvent>(HandlePlayerDisconnect);
            _autobus.Unsubscribe<NodeStartedEvent>(NodeStartedHandler);
            _autobus.Unsubscribe<NodeReceivedPlayerEncryptionEvent>(NodeReceivedPlayerEncryptionHandler);
            _autobus.Unsubscribe<NodeOnlineEvent>(NodeOnlineHandler);
            _autobus.Unsubscribe<ServerInGameplayEvent>(HandleServerInGameplay);
            //_autobus.Unsubscribe<UpdateStatusEvent>(HandleServerStatusChanged);
            //_autobus.Unsubscribe<UpdateInstanceConfigEvent>(InstanceConfigurationUpdateHandler);
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

        private async Task HandlePlayerDisconnect(PlayerLeaveServerEvent integrationEvent) //Handles player disconnects and when the player count changes
        {
            var server = await _serverRepository.GetServer(integrationEvent.Secret);
            if (server != null)
            {
                _ = _serverRepository.UpdateCurrentPlayerCount(integrationEvent.Secret, integrationEvent.NewPlayerCount);
            }
            if(integrationEvent.endPoint != string.Empty)
            {
                bool SessionExists = _masterServerSessionService.TryGetSession(IPEndPoint.Parse(integrationEvent.endPoint), out var session);
                if (SessionExists && server != null)
                    session.LastGameIp = server.ServerEndPoint.ToString();
                if (!SessionExists)
                    return;
                _masterServerSessionService.RemoveSecretFromSession(session.EndPoint);
                session.LastGameDisconnect = DateTime.UtcNow;
            }
        }
        /*
        private async Task HandleServerStatusChanged(UpdateStatusEvent updateStatusEvent)
        {
            var server = await _serverRepository.GetServer(updateStatusEvent.Secret);
            if (updateStatusEvent.GameState == DedicatedServer.Interface.Enums.MultiplayerGameState.Game)
                server.IsInGameplay = true;
            else
                server.IsInGameplay = false;
            return;
        }
        */
        private Task HandleServerInGameplay(ServerInGameplayEvent serverInGameplayEvent)
        {
            _ = _serverRepository.UpdateServerGameplayState(serverInGameplayEvent.Secret, serverInGameplayEvent.InGame);
            return Task.CompletedTask;
        }

        private Task NodeStartedHandler(NodeStartedEvent startedEvent)
        {
            if (_configuration.SupportedDediServerVersions.Where(N => startedEvent.NodeVersion.StartsWith(N)).Any())
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

        /*
        private Task InstanceConfigurationUpdateHandler(UpdateInstanceConfigEvent updateInstanceConfigEvent)
        {
            Server server = _serverRepository.GetServer(updateInstanceConfigEvent.Secret).Result;
            if(server == null)
                return Task.CompletedTask;
            server.GameplayServerConfiguration.MaxPlayerCount = updateInstanceConfigEvent.Configuration.MaxPlayerCount;
            server.GameplayServerConfiguration.DiscoveryPolicy = (Domain.Enums.DiscoveryPolicy)updateInstanceConfigEvent.Configuration.DiscoveryPolicy;
            server.GameplayServerConfiguration.InvitePolicy = (Domain.Enums.InvitePolicy)updateInstanceConfigEvent.Configuration.InvitePolicy;
            server.GameplayServerConfiguration.GameplayServerMode = (Domain.Enums.GameplayServerMode)updateInstanceConfigEvent.Configuration.GameplayServerMode;
            server.GameplayServerConfiguration.SongSelectionMode = (Domain.Enums.SongSelectionMode)updateInstanceConfigEvent.Configuration.SongSelectionMode;
            server.GameplayServerConfiguration.GameplayServerControlSettings = (Domain.Enums.GameplayServerControlSettings)updateInstanceConfigEvent.Configuration.GameplayServerControlSettings;
            server.Host.UserName = updateInstanceConfigEvent.ServerName;
            server.Code = updateInstanceConfigEvent.Code;
            return Task.CompletedTask;
        }
        */



        private Task NodeReceivedPlayerEncryptionHandler(NodeReceivedPlayerEncryptionEvent RecievedEvent)
        {
            _nodeRepository.OnNodeRecievedEncryptionParameters(IPEndPoint.Parse(RecievedEvent.endPoint), IPEndPoint.Parse(RecievedEvent.PlayerEndPoint));
            return Task.CompletedTask;
        }
        #endregion
    }
}
