using Autobus;
using BeatTogether.DedicatedServer.Interface.Events;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
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

        public DedicatedServerEventHandler(
            IAutobus autobus,
            IServerRepository serverRepository)
        {
            _autobus = autobus;
            _serverRepository = serverRepository;
        }

        #region Public Methods

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _autobus.Subscribe<MatchmakingServerStoppedEvent>(Handle);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _autobus.Unsubscribe<MatchmakingServerStoppedEvent>(Handle);
            return Task.CompletedTask;
        }

        #endregion

        #region Private Methods

        private async Task Handle(MatchmakingServerStoppedEvent integrationEvent)
        {
            var secret = integrationEvent.Secret;
            _logger.Debug(
                $"Handling {nameof(MatchmakingServerStoppedEvent)} " +
                $"(Secret='{secret}').");
            await _serverRepository.RemoveServer(secret);
        }

        #endregion
    }
}
