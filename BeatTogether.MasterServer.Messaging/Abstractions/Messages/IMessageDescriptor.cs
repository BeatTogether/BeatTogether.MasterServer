namespace BeatTogether.MasterServer.Messaging.Abstractions.Messages
{
    public interface IMessageDescriptor
    {
        uint MessageGroup { get; }
        uint MessageId { get; }
    }
}
