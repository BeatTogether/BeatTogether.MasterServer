using System;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Kernel.Abstractions.Sessions;
using BeatTogether.MasterServer.Kernel.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using StackExchange.Redis;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    /*
    public class ServerTickService : IHostedService
    {
        private readonly SessionLifetimeConfiguration _sessionLifetimeConfiguration;
        private readonly IServerRepository _serverRepository;
        private readonly ILogger _logger;

        private Task _task;

        public ServerTickService(
            SessionLifetimeConfiguration sessionLifetimeConfiguration,
            IServerRepository serverRepository)
        {
            _sessionLifetimeConfiguration = sessionLifetimeConfiguration;
            _serverRepository = serverRepository;
            _logger = Log.ForContext<ServerTickService>();
        }

        #region Public Methods

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_task != null)
                await StopAsync(cancellationToken);

            _task = Task.Run(() => Tick(cancellationToken));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _task;
            _task = null;
        }

        #endregion

        #region Private Methods

        private async Task Tick(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Prune inactive sessions
                    var inactiveServerSecrets = await _serverRepository
                        .GetInactiveServers(_sessionLifetimeConfiguration.TimeToLive);
                    foreach (var secret in inactiveServerSecrets)
                        _serverRepository.RemoveServer(secret);
                }
                catch (Exception e) when ((e is RedisException) || (e is RedisTimeoutException))
                {
                    _logger.Warning(e, "Error while pruning inactive servers.");
                }
                finally
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        #endregion
    }
    */
}
