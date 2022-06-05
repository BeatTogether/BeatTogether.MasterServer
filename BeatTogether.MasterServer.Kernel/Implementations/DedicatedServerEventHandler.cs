using Autobus;
using BeatTogether.DedicatedServer.Interface.Events;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Kernel.Abstractions;
using Microsoft.Extensions.Hosting;
using Serilog;
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

        public DedicatedServerEventHandler(
            IAutobus autobus,
            IServerRepository serverRepository,
            IMasterServerSessionService masterServerSessionService)
        {
            _autobus = autobus;
            _serverRepository = serverRepository;
            _masterServerSessionService = masterServerSessionService;
        }

        #region Public Methods

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _autobus.Subscribe<MatchmakingServerStoppedEvent>(HandleServerStop);
            _autobus.Subscribe<PlayerLeaveServerEvent>(HandlePlayerDisconnect);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _autobus.Unsubscribe<MatchmakingServerStoppedEvent>(HandleServerStop);
            _autobus.Unsubscribe<PlayerLeaveServerEvent>(HandlePlayerDisconnect);
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
        }

        private async Task HandlePlayerDisconnect(PlayerLeaveServerEvent integrationEvent)
        {
            await _masterServerSessionService.CloseSession(integrationEvent.endPoint);
        }

        #endregion
    }
}
