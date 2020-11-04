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
        public abstract uint ProtocolVersion { get; }

        private readonly Dictionary<object, Type> _typeByIdLookup;
        private readonly Dictionary<Type, object> _idByTypeLookup;
        private readonly Dictionary<object, Func<IMessage>> _factoryByIdLookup;

        public BaseMessageRegistry()
        {
            _typeByIdLookup = new Dictionary<object, Type>();
            _idByTypeLookup = new Dictionary<Type, object>();
            _factoryByIdLookup = new Dictionary<object, Func<IMessage>>();
        }

        #region Public Methods

        public Type GetMessageType(object id)
            => _typeByIdLookup[id];

        public bool TryGetMessageType(object id, out Type type)
            => _typeByIdLookup.TryGetValue(id, out type);

        public object GetMessageId(Type type)
            => _idByTypeLookup[type];

        public bool TryGetMessageId(Type type, out object id)
            => _idByTypeLookup.TryGetValue(type, out id);

        public object GetMessageId<T>()
            where T : class, IMessage
            => GetMessageId(typeof(T));

        public bool TryGetMessageId<T>(out object id)
            where T : class, IMessage
            => TryGetMessageId(typeof(T), out id);

        public IMessage GetMessage(object id)
            => _factoryByIdLookup[id]();

        public bool TryGetMessage(object id, out IMessage message)
        {
            if (_factoryByIdLookup.TryGetValue(id, out var factory))
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
            _typeByIdLookup[id] = type;
            _idByTypeLookup[type] = id;
            _factoryByIdLookup[id] = () => new TMessage();
        }

        #endregion
    }
}
