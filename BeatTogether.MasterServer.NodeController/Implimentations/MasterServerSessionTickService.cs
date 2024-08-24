using BeatTogether.Core.Extensions;
using BeatTogether.Core.ServerMessaging.Models;
using BeatTogether.MasterServer.NodeController.Abstractions;
using BeatTogether.MasterServer.NodeController.Configuration;
using BinaryRecords;
using Serilog;

namespace BeatTogether.MasterServer.NodeController.Implementations.Sessions
{
    public class MasterServerSessionTickService : IHostedService
    {
        private readonly NodeControllerConfiguration _configuration;
        private readonly Serilog.ILogger _logger;
        private readonly INodeRepository _nodeRepository;

        private Task? _task;
        private CancellationTokenSource? _cancellationTokenSource;

        public MasterServerSessionTickService(
            NodeControllerConfiguration configuration,
            INodeRepository nodeRepository)
        {
            _configuration = configuration;
            _logger = Log.ForContext<MasterServerSessionTickService>();
            _nodeRepository = nodeRepository;

            BinarySerializer.AddGeneratorProvider(
                (Player value, ref BinaryBufferWriter buffer) => BinaryBufferWriterExtensions.WritePlayer(ref buffer, value),
                (ref BinaryBufferReader bufferReader) => BinaryBufferReaderExtensions.ReadPlayer(ref bufferReader)
            );
            BinarySerializer.AddGeneratorProvider(
                (Server value, ref BinaryBufferWriter buffer) => BinaryBufferWriterExtensions.WriteServer(ref buffer, value),
                (ref BinaryBufferReader bufferReader) => BinaryBufferReaderExtensions.ReadServer(ref bufferReader)
            );
        }

        #region Public Methods

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            
            _logger.Information($"Starting node repository, Version: {_configuration.MasterServerVersion}.");
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

                await Task.Delay(10000, cancellationToken);//waits 10 seconds
            }
        }

        #endregion
    }
}
