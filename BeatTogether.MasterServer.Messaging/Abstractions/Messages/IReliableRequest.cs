namespace BeatTogether.MasterServer.Messaging.Abstractions.Messages
{
    public interface IReliableRequest : IMessage
    {
        uint RequestId { get; set; }
    }
}
