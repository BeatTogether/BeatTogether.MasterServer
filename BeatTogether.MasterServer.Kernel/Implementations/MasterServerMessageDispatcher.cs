using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Configuration;
using BeatTogether.Core.Messaging.Implementations;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class MasterServerMessageDispatcher : BaseMessageDispatcher
    {
        protected override byte PacketProperty => 0x08;  // LiteNetLib.PacketProperty.UnconnectedMessage

        public MasterServerMessageDispatcher(
            MessagingConfiguration configuration,
            MasterServerMessageSource messageSource,
            IMessageWriter messageWriter,
            IEncryptedMessageWriter encryptedMessageWriter)
            : base(configuration, messageSource, messageWriter, encryptedMessageWriter)
        {
        }
    }
}
