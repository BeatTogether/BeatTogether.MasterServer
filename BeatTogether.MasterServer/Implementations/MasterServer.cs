using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BeatTogether.MasterServer.Implementations
{
    public class MasterServer : IHostedService
    {
        private readonly MasterServerConfiguration _configuration;
        private readonly ILogger _logger;

        public MasterServer(MasterServerConfiguration configuration)
        {
            _configuration = configuration;
            _logger = Log.ForContext<MasterServer>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Starting Master Server (Endpoint='{_configuration.Endpoint}').");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Stopping Master Server (Endpoint='{_configuration.Endpoint}').");
            return Task.CompletedTask;
        }
    }
}
