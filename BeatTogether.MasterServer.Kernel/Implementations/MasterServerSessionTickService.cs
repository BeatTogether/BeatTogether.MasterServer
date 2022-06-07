using System;
using System.Threading;
using System.Threading.Tasks;
using Autobus;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Interface.Events;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using StackExchange.Redis;

namespace BeatTogether.MasterServer.Kernel.Implementations.Sessions
{
    public class MasterServerSessionTickService : IHostedService
    {
        private readonly MasterServerConfiguration _configuration;
        private readonly IMasterServerSessionService _sessionService;
        private readonly ILogger _logger;
        private readonly IAutobus _autobus;
        private readonly INodeRepository _nodeRepository;

        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;

        public MasterServerSessionTickService(
            MasterServerConfiguration configuration,
            IMasterServerSessionService sessionService,
            IAutobus autobus,
            INodeRepository nodeRepository)
        {
            _configuration = configuration;
            _sessionService = sessionService;
            _logger = Log.ForContext<MasterServerSessionTickService>();
            _autobus = autobus;
            _nodeRepository = nodeRepository;
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
            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Remove servers that are hosted on a node if that node is not responsive every 10 seconds
                    if (!_nodeRepository.WaitingForResponses)
                    {
                        _nodeRepository.StartWaitForAllNodesTask();
                        _nodeRepository.WaitingForResponses = true;
                        _autobus.Publish(new CheckNodesEvent());
                    }
                    // Prune inactive sessions every 10 seconds, a session must be over 4 min old to be removed
                    var inactiveSessions = _sessionService
                        .GetInactiveSessions(_configuration.SessionTimeToLive);
                    foreach (var session in inactiveSessions)
                        _sessionService.CloseSession(session);
                }
                catch (Exception e) when ((e is RedisException) || (e is RedisTimeoutException))
                {
                    _logger.Warning(e, "Error while pruning inactive sessions.");
                }
                finally
                {
                    await Task.Delay(10000, cancellationToken);
                }
            }
        }

        #endregion
    }
}
