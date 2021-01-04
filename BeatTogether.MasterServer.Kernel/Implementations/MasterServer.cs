using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.Core.Data.Abstractions;
using BeatTogether.Core.Data.Configuration;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Implementations;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Obvs;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class MasterServer : BaseUdpServer, IHostedService
    {
        private readonly IMasterServerSessionService _sessionService;
        private readonly ILogger _logger;

        public MasterServer(
            IServiceProvider serviceProvider,
            MasterServerConfiguration configuration,
            RedisConfiguration redisConfiguration,
            MasterServerMessageSource messageSource,
            MasterServerMessageDispatcher messageDispatcher,
            IMasterServerSessionService sessionService)
            : base(IPEndPoint.Parse(configuration.EndPoint), messageSource, messageDispatcher)
        {
            _sessionService = sessionService;
            _logger = Log.ForContext<MasterServer>();

            // Warm up service bus/Redis connection pool
            serviceProvider.GetRequiredService<IServiceBus>();
            if (redisConfiguration.Enabled)
                serviceProvider.GetRequiredService<IConnectionMultiplexerPool>();
        }

        protected override ISession GetSession(EndPoint endPoint) =>
            _sessionService.GetOrAddSession(endPoint);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Starting master server (EndPoint='{Endpoint}').");
            Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Stopping master server (EndPoint='{Endpoint}').");
            Stop();
            return Task.CompletedTask;
        }
    }
}
