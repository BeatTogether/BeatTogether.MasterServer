using System;
using System.Collections.Generic;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Abstractions.Registries;
using BeatTogether.MasterServer.Messaging.Enums;

namespace BeatTogether.MasterServer.Messaging.Implementations.Registries
{
    public abstract class BaseMessageRegistry : IMessageRegistry
    {
        public abstract MessageGroup MessageGroup { get; }

        private readonly Dictionary<int, Type> _typeByIdLookup;
        private readonly Dictionary<Type, int> _idByTypeLookup;
        private readonly Dictionary<int, Func<IMessage>> _factoryByIdLookup;

        public BaseMessageRegistry()
        {
            _typeByIdLookup = new Dictionary<int, Type>();
            _idByTypeLookup = new Dictionary<Type, int>();
            _factoryByIdLookup = new Dictionary<int, Func<IMessage>>();
        }

        #region Public Methods

        public Type GetMessageType(object id)
            => _typeByIdLookup[(int)id];

        public bool TryGetMessageType(object id, out Type type)
            => _typeByIdLookup.TryGetValue((int)id, out type);

        public int GetMessageId(Type type)
            => _idByTypeLookup[type];

        public bool TryGetMessageId(Type type, out int id)
            => _idByTypeLookup.TryGetValue(type, out id);

        public int GetMessageId<T>()
            where T : class, IMessage
            => GetMessageId(typeof(T));

        public bool TryGetMessageId<T>(out int id)
            where T : class, IMessage
            => TryGetMessageId(typeof(T), out id);

        public IMessage GetMessage(object id)
            => _factoryByIdLookup[(int)id]();

        public bool TryGetMessage(object id, out IMessage message)
        {
            if (_factoryByIdLookup.TryGetValue((int)id, out var factory))
            {
                message = factory();
                return true;
            }

            message = null;
            return false;
        }

        #endregion

        #region Protected Methods

        protected void Register<TMessage>(object id)
            where TMessage : class, IMessage, new()
        {
            var type = typeof(TMessage);
            _typeByIdLookup[(int)id] = type;
            _idByTypeLookup[type] = (int)id;
            _factoryByIdLookup[(int)id] = () => new TMessage();
        }

        #endregion
    }
}
