namespace BeatTogether.MasterServer.Messaging.Abstractions.Messages
{
    public interface IReliableMessage : IMessage
    {
        uint RequestId { get; set; }
        uint ResponseId { get; set; }
    }
}
