namespace BeatTogether.MasterServer.Messaging.Abstractions.Messages
{
    public interface IReliableResponse : IMessage
    {
        uint ResponseId { get; set; }
    }
}
