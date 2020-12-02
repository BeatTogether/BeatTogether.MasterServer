using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Implementations;
using BeatTogether.MasterServer.Client.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BeatTogether.MasterServer.Client.Implementations
{
    public class MasterServerClient : BaseUdpClient, IHostedService
    {
        private readonly ILogger _logger;

        public MasterServerClient(
            MasterServerClientConfiguration configuration,
            MasterServerClientMessageSource messageSource,
            MasterServerClientMessageDispatcher messageDispatcher)
            : base(IPEndPoint.Parse(configuration.EndPoint), messageSource, messageDispatcher)
        {
            _logger = Log.ForContext<MasterServerClient>();
        }

        protected override ISession GetSession(EndPoint endPoint) =>
            new MasterServerClientSession()
            {
                EndPoint = endPoint
            };

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Connecting to Master Server (EndPoint='{Endpoint}').");
            Connect();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information($"Disconnecting from Master Server (EndPoint='{Endpoint}').");
            Disconnect();
            return Task.CompletedTask;
        }
    }
}
