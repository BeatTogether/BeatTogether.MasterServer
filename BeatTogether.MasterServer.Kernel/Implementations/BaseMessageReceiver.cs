using System;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Models;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public abstract class BaseMessageReceiver : IMessageReceiver
    {
        private readonly IServiceProvider _serviceProvider;

        public BaseMessageReceiver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        #region Public Methods

        public Task OnReceived(Session session, ReadOnlySpan<byte> data)
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
