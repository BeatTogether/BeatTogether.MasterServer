using System;
using System.Collections.Generic;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Abstractions.Registries;
using BeatTogether.MasterServer.Messaging.Implementations.Messages;

namespace BeatTogether.MasterServer.Messaging.Implementations.Registries
{
    public abstract class BaseMessageRegistry : IMessageRegistry
    {
        /// <inheritdoc cref="IMessageRegistry.MessageGroup"/>
        public abstract uint MessageGroup { get; }

        private readonly Dictionary<uint, Type> _typeByIdLookup;
        private readonly Dictionary<Type, uint> _idByTypeLookup;
        private readonly Dictionary<uint, Func<IMessage>> _factoryByIdLookup;

        public BaseMessageRegistry()
        {
            _typeByIdLookup = new Dictionary<uint, Type>();
            _idByTypeLookup = new Dictionary<Type, uint>();
            _factoryByIdLookup = new Dictionary<uint, Func<IMessage>>();
        }

        #region Public Methods

        /// <inheritdoc cref="IMessageRegistry.GetMessageType"/>
        public Type GetMessageType(object id)
            => _typeByIdLookup[(uint)id];

        /// <inheritdoc cref="IMessageRegistry.TryGetMessageType"/>
        public bool TryGetMessageType(object id, out Type type)
            => _typeByIdLookup.TryGetValue((uint)id, out type);

        /// <inheritdoc cref="IMessageRegistry.GetMessageId"/>
        public uint GetMessageId(Type type)
            => _idByTypeLookup[type];

        /// <inheritdoc cref="IMessageRegistry.TryGetMessageId"/>
        public bool TryGetMessageId(Type type, out uint id)
            => _idByTypeLookup.TryGetValue(type, out id);

        /// <inheritdoc cref="IMessageRegistry.GetMessageId{T}"/>
        public uint GetMessageId<T>()
            where T : class, IMessage
            => GetMessageId(typeof(T));

        /// <inheritdoc cref="IMessageRegistry.TryGetMessageId{T}"/>
        public bool TryGetMessageId<T>(out uint id)
            where T : class, IMessage
            => TryGetMessageId(typeof(T), out id);

        /// <inheritdoc cref="IMessageRegistry.CreateMessage"/>
        public IMessage CreateMessage(object id)
            => _factoryByIdLookup[(uint)id]();

        /// <inheritdoc cref="IMessageRegistry.TryCreateMessage"/>
        public bool TryCreateMessage(object id, out IMessage message)
        {
            if (_factoryByIdLookup.TryGetValue((uint)id, out var factory))
            {
                message = factory();
                return true;
            }

            message = null;
            return false;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Associates the message of type <typeparamref name="T"/>
        /// with the given <paramref name="id"/>.
        /// </summary>
        /// <typeparam name="T">The type of message.</typeparam>
        /// <param name="id">
        /// The identifier to associate with the message.
        /// This must be castable to a <see cref="uint"/>.
        /// </param>
        protected void Register<T>(object id)
            where T : BaseMessage, new()
        {
            var type = typeof(T);
            _typeByIdLookup[(uint)id] = type;
            _idByTypeLookup[type] = (uint)id;
            _factoryByIdLookup[(uint)id] = () => new T()
            {
                Descriptor = new MessageDescriptor()
                {
                    MessageGroup = MessageGroup,
                    MessageId = (uint)id
                }
            };
        }

        #endregion
    }
}
