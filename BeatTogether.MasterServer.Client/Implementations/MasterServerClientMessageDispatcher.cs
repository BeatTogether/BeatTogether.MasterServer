using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Configuration;
using BeatTogether.Core.Messaging.Implementations;

namespace BeatTogether.MasterServer.Client.Implementations
{
    public class MasterServerClientMessageDispatcher : BaseMessageDispatcher
    {
        protected override byte PacketProperty => 0x08;  // LiteNetLib.PacketProperty.UnconnectedMessage

        public MasterServerClientMessageDispatcher(
            MessagingConfiguration configuration,
            MasterServerClientMessageSource messageSource,
            IMessageWriter messageWriter,
            IEncryptedMessageWriter encryptedMessageWriter)
            : base(configuration, messageSource, messageWriter, encryptedMessageWriter)
        {
        }
    }
}
