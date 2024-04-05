using System;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernal.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations.Sessions
{
    public class MasterServerSessionTickService : IHostedService
    {
        private readonly MasterServerConfiguration _configuration;
        private readonly IMasterServerSessionService _sessionService;
        private readonly ILogger _logger;
        private readonly INodeRepository _nodeRepository;

        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;

        public MasterServerSessionTickService(
            MasterServerConfiguration configuration,
            IMasterServerSessionService sessionService,
            INodeRepository nodeRepository)
        {
            _configuration = configuration;
            _sessionService = sessionService;
            _logger = Log.ForContext<MasterServerSessionTickService>();
            _nodeRepository = nodeRepository;
        }

        #region Public Methods

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Starting master server, Version: {_configuration.MasterServerVersion} (EndPoint='{_configuration.EndPoint}').");
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
            catch (Exception ex)
            {
                _logger?.Error(ex.Message);
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
                //cancellationToken.ThrowIfCancellationRequested();

                // Removes servers that are hosted on a node if that node is not responsive every 10 seconds
                _nodeRepository.StartWaitForAllNodesTask();
                // Prune inactive sessions every 10 seconds, a session must be over 3 min old to be removed
                var inactiveSessions = _sessionService
                    .GetInactiveSessions(_configuration.SessionTimeToLive);
                foreach (var session in inactiveSessions)
                    _sessionService.CloseSession(session);

                await Task.Delay(10000, cancellationToken);//waits 10 seconds
            }
        }

        #endregion
    }
}
