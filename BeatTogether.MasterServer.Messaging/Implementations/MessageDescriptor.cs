using BeatTogether.MasterServer.Messaging.Abstractions.Messages;

namespace BeatTogether.MasterServer.Messaging.Implementations
{
    public class MessageDescriptor : IMessageDescriptor
    {
        public uint MessageGroup { get; set; }
        public uint MessageId { get; set; }
    }
}
