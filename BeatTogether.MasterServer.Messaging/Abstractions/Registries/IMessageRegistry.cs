using System;
using BeatTogether.MasterServer.Messaging.Enums;

namespace BeatTogether.MasterServer.Messaging.Abstractions.Registries
{
    public interface IMessageRegistry
    {
        MessageGroup MessageGroup { get; }

        Type GetMessageType(object id);
        bool TryGetMessageType(object id, out Type type);

        object GetMessageId(Type type);
        object GetMessageId<T>()
            where T : class, IMessage;
        bool TryGetMessageId(Type type, out object id);
        bool TryGetMessageId<T>(out object id)
            where T : class, IMessage;

        IMessage GetMessage(object id);
        bool TryGetMessage(object id, out IMessage message);
    }
}
