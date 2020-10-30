using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Data.Abstractions;
using BeatTogether.MasterServer.Data.Configuration;
using Serilog;
using StackExchange.Redis;

namespace BeatTogether.MasterServer.Data.Implementations
{
    public class ConnectionMultiplexerPool : IConnectionMultiplexerPool, IDisposable
    {
        private long _getConnectionFailureCount;
        public long GetConnectionFailureCount => _getConnectionFailureCount;

        private readonly RedisConfiguration _configuration;
        private readonly ILogger _logger;

        private readonly ConcurrentQueue<Lazy<Task<ConnectionMultiplexer>>> _connectionQueue;

        public ConnectionMultiplexerPool(RedisConfiguration configuration)
        {
            _configuration = configuration;
            _logger = Log.ForContext<ConnectionMultiplexerPool>();

            if (configuration.ConnectionPoolSize <= 0)
                throw new Exception("Redis connection pool size must be greater than 0");

            // Build the connection pool
            _logger.Debug(
                "Initializing Redis connection pool " +
                $"(Endpoint={configuration.Endpoint}, " +
                $"ConnectionPoolSize={configuration.ConnectionPoolSize})."
            );
            _connectionQueue = new ConcurrentQueue<Lazy<Task<ConnectionMultiplexer>>>();

            var connectionMultiplexerConfiguration = new ConfigurationOptions()
            {
                AbortOnConnectFail = false
            };
            connectionMultiplexerConfiguration.EndPoints.Add(configuration.Endpoint);

            while (_connectionQueue.Count < _configuration.ConnectionPoolSize)
                _connectionQueue.Enqueue(new Lazy<Task<ConnectionMultiplexer>>(
                    () => ConnectionMultiplexer.ConnectAsync(connectionMultiplexerConfiguration)
                ));
        }

        #region Public Methods

        public IConnectionMultiplexer GetConnection()
        {
            Lazy<Task<ConnectionMultiplexer>> connection;
            while (!_connectionQueue.TryDequeue(out connection))
                Interlocked.Increment(ref _getConnectionFailureCount);
            _connectionQueue.Enqueue(connection);
            return connection.Value.Result;
        }

        #endregion

        #region IDisposable Methods

        public void Dispose()
        {
            for (var i = 0; i < _configuration.ConnectionPoolSize; i++)
            {
                Lazy<Task<ConnectionMultiplexer>> connection;
                while (!_connectionQueue.TryDequeue(out connection))
                    Interlocked.Increment(ref _getConnectionFailureCount);
                connection.Value.Result.Dispose();
            }
        }

        #endregion
    }
}
