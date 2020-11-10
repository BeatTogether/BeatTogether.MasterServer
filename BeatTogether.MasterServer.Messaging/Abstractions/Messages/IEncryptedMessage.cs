namespace BeatTogether.MasterServer.Messaging.Abstractions.Messages
{
    public interface IEncryptedMessage : IMessage
    {
        uint SequenceId { get; set; }
    }
}
