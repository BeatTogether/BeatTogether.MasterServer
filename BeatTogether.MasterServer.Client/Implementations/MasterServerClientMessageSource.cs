using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Configuration;
using BeatTogether.Core.Messaging.Implementations;

namespace BeatTogether.MasterServer.Client.Implementations
{
    public class MasterServerClientMessageSource : BaseMessageSource
    {
        protected override byte PacketProperty => 0x08;  // LiteNetLib.PacketProperty.UnconnectedMessage

        public MasterServerClientMessageSource(
            MessagingConfiguration configuration,
            IMessageReader messageReader,
            IEncryptedMessageReader encryptedMessageReader)
            : base(configuration, messageReader, encryptedMessageReader)
        {
        }
    }
}
