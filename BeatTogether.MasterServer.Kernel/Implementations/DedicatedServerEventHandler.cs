using Autobus;
using BeatTogether.DedicatedServer.Interface.Events;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Domain.Models;
using BeatTogether.MasterServer.Kernal.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
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

            _autobus.Subscribe<UpdateInstanceConfigEvent>(InstanceConfigurationUpdateHandler);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _autobus.Unsubscribe<MatchmakingServerStoppedEvent>(HandleServerStop);
            _autobus.Unsubscribe<PlayerLeaveServerEvent>(HandlePlayerDisconnect);
            _autobus.Unsubscribe<NodeStartedEvent>(NodeStartedHandler);
            _autobus.Unsubscribe<NodeReceivedPlayerEncryptionEvent>(NodeReceivedPlayerEncryptionHandler);
            _autobus.Unsubscribe<NodeOnlineEvent>(NodeOnlineHandler);


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
            if(_serverRepository.GetServer(integrationEvent.Secret) != null)
            {
                _serverRepository.UpdateCurrentPlayerCount(integrationEvent.Secret, integrationEvent.NewPlayerCount);
                if (!_masterServerSessionService.TryGetSession((EndPoint)IPEndPoint.Parse(integrationEvent.endPoint), out var session))
                    return Task.CompletedTask;
                _masterServerSessionService.RemoveSecretFromSession(session.EndPoint);
            }
            return Task.CompletedTask;
        }

        private Task NodeStartedHandler(NodeStartedEvent startedEvent)
        {
            if (_configuration.SupportedDediServerVersions.Contains(startedEvent.NodeVersion))
            {
                _logger.Information($"Node is online: " + startedEvent.endPoint);
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
            Server server = _serverRepository.GetServer(updateInstanceConfigEvent.Secret).Result;
            if(server == null)
                return Task.CompletedTask;
            server.Code = updateInstanceConfigEvent.Code;
            GameplayServerConfiguration confgiuration = new(
                updateInstanceConfigEvent.Configuration.MaxPlayerCount,
                (Domain.Enums.DiscoveryPolicy)updateInstanceConfigEvent.Configuration.DiscoveryPolicy,
                (Domain.Enums.InvitePolicy)updateInstanceConfigEvent.Configuration.InvitePolicy,
                (Domain.Enums.GameplayServerMode)updateInstanceConfigEvent.Configuration.GameplayServerMode,
                (Domain.Enums.SongSelectionMode)updateInstanceConfigEvent.Configuration.SongSelectionMode,
                (Domain.Enums.GameplayServerControlSettings)updateInstanceConfigEvent.Configuration.GameplayServerControlSettings
                );
            server.GameplayServerConfiguration = confgiuration;
            Player Host = new()
            {
                UserName = updateInstanceConfigEvent.ServerName,
                UserId = server.Host.UserId
            };
            server.Host = Host;
            if(_serverRepository.GetServerByCode(updateInstanceConfigEvent.Code) == null)
                server.Code = updateInstanceConfigEvent.Code;
            return Task.CompletedTask;
        }



        private Task NodeReceivedPlayerEncryptionHandler(NodeReceivedPlayerEncryptionEvent RecievedEvent)
        {
            _nodeRepository.OnNodeRecievedEncryptionParameters(IPEndPoint.Parse(RecievedEvent.endPoint), (EndPoint)IPEndPoint.Parse(RecievedEvent.PlayerEndPoint));
            return Task.CompletedTask;
        }
        #endregion
    }
}
