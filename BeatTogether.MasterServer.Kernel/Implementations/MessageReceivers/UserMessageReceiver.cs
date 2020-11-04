using System;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Messaging.Implementations;
using BeatTogether.MasterServer.Messaging.Implementations.Registries;

namespace BeatTogether.MasterServer.Kernel.Implementations.MessageReceivers
{
    public class UserMessageReceiver : BaseMessageReceiver<UserMessageRegistry, IUserService>
    {
        public UserMessageReceiver(
            IServiceProvider serviceProvider,
            MessageReader<UserMessageRegistry> messageReader,
            MessageWriter<UserMessageRegistry> messageWriter)
            : base(serviceProvider, messageReader, messageWriter)
        {
        }
    }
}
