using StackExchange.Redis;

namespace BeatTogether.MasterServer.Data.Abstractions
{
    public interface IConnectionMultiplexerPool
    {
        IConnectionMultiplexer GetConnection();
    }
}
