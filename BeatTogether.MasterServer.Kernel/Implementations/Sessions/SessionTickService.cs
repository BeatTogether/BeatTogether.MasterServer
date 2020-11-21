using System;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Kernel.Abstractions.Sessions;
using BeatTogether.MasterServer.Kernel.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using StackExchange.Redis;

namespace BeatTogether.MasterServer.Kernel.Implementations.Sessions
{
    public class SessionTickService : IHostedService
    {
        private readonly SessionLifetimeConfiguration _sessionLifetimeConfiguration;
        private readonly ISessionRepository _sessionRepository;
        private readonly ISessionService _sessionService;
        private readonly ILogger _logger;

        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;

        public SessionTickService(
            SessionLifetimeConfiguration sessionLifetimeConfiguration,
            ISessionRepository sessionRepository,
            ISessionService sessionService)
        {
            _sessionLifetimeConfiguration = sessionLifetimeConfiguration;
            _sessionRepository = sessionRepository;
            _sessionService = sessionService;
            _logger = Log.ForContext<SessionTickService>();
        }

        #region Public Methods

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_task != null)
                await StopAsync(cancellationToken);

            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Run(() => Tick(_cancellationTokenSource.Token));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_task == null)
                return;

            _cancellationTokenSource?.Cancel();
            try
            {
                await _task;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                _task = null;
            }
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
                    var inactiveSessionEndPoints = await _sessionRepository
                        .GetInactiveSessions(_sessionLifetimeConfiguration.TimeToLive);
                    foreach (var endPoint in inactiveSessionEndPoints)
                    {
                        if (_sessionService.TryGetSession(endPoint, out var session))
                            _sessionService.CloseSession(session);
                    }
                }
                catch (Exception e) when ((e is RedisException) || (e is RedisTimeoutException))
                {
                    _logger.Warning(e, "Error while pruning inactive sessions.");
                }
                finally
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        #endregion
    }
}
