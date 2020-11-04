using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Models;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public abstract class BaseMessageReceiver<TService> : IMessageReceiver
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly Dictionary<object, Func<Task>> _handlersByMessageIdLookup;

        public BaseMessageReceiver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _handlersByMessageIdLookup = new Dictionary<object, Func<Task>>();
        }

        #region Public Methods

        public Task OnReceived(Session session, ReadOnlySpan<byte> data)
        {
        }

        #endregion

        #region Protected Methods

        protected void AddMessageHandler<TRequest, TResponse>(Func<Task> handler)
        {
        }

        #endregion
    }
}
