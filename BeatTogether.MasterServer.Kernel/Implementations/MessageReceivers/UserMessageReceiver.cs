using System;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Messaging.Implementations;
using BeatTogether.MasterServer.Messaging.Implementations.Registries;

namespace BeatTogether.MasterServer.Kernel.Implementations.MessageReceivers
{
    public class UserMessageReceiver : BaseMessageReceiver<IUserService>
    {
        public UserMessageReceiver(
            IServiceProvider serviceProvider,
            IRequestIdProvider requestIdProvider,
            MessageReader<UserMessageRegistry> messageReader,
            MessageWriter<UserMessageRegistry> messageWriter)
            : base(serviceProvider, requestIdProvider, messageReader, messageWriter)
        {
        }
    }
}
