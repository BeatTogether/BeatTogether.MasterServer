using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Configuration;
using Krypton.Buffers;
using Microsoft.Extensions.Hosting;
using NetCoreServer;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class MasterServer : UdpServer, IHostedService
    {
        private readonly MasterServerConfiguration _configuration;
        private readonly ILogger _logger;

        public MasterServer(MasterServerConfiguration configuration)
            : base(IPEndPoint.Parse(configuration.Endpoint))
        {
            _configuration = configuration;
            _logger = Log.ForContext<MasterServer>();
        }

        protected override void OnStarted() => ReceiveAsync();

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            _logger.Verbose($"Handling OnReceived (Endpoint='{endpoint}', Offset={offset}, Size={size}).");
            ReceiveAsync();
        }

        protected override void OnError(SocketError error)
            => _logger.Error($"Handling socket error (Error={error}).");

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Starting Master Server (Endpoint='{_configuration.Endpoint}').");
            Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Stopping Master Server (Endpoint='{_configuration.Endpoint}').");
            Stop();
            return Task.CompletedTask;
        }
    }
}
