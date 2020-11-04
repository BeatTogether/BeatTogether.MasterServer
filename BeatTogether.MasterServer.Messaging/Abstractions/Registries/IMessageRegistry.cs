using System;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Enums;

namespace BeatTogether.MasterServer.Messaging.Abstractions.Registries
{
    public interface IMessageRegistry
    {
        MessageGroup MessageGroup { get; }
        uint ProtocolVersion { get; }

        Type GetMessageType(object id);
        bool TryGetMessageType(object id, out Type type);

        int GetMessageId(Type type);
        int GetMessageId<T>()
            where T : class, IMessage;
        bool TryGetMessageId(Type type, out int id);
        bool TryGetMessageId<T>(out int id)
            where T : class, IMessage;

        IMessage GetMessage(object id);
        bool TryGetMessage(object id, out IMessage message);
    }
}
