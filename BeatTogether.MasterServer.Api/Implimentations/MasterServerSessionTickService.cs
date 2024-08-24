using System;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Api.Abstractions;
using BeatTogether.MasterServer.Api.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BeatTogether.MasterServer.Api.Implimentations
{
    public class MasterServerSessionTickService : IHostedService
    {
        private readonly ApiServerConfiguration _configuration;
        private readonly IConfiguration _rootConfig;
        private readonly IMasterServerSessionService _sessionService;
        private readonly ILogger _logger;

        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;

        public MasterServerSessionTickService(
            ApiServerConfiguration configuration,
            IConfiguration rootConfig,
            IMasterServerSessionService sessionService)
        {
            _configuration = configuration;
            _sessionService = sessionService;
            _rootConfig = rootConfig;
            _logger = Log.ForContext<MasterServerSessionTickService>();
        }

        #region Public Methods

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Starting api server. Url: {_rootConfig.GetValue<string>("Urls")}");
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
