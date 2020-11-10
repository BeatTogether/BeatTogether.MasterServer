using System;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;

namespace BeatTogether.MasterServer.Messaging.Abstractions.Registries
{
    public interface IMessageRegistry
    {
        /// <summary>
        /// An identifier for the group of messages in this registry.
        /// </summary>
        uint MessageGroup { get; }

        /// <summary>
        /// Retrieves the <see cref="Type"/> of the message
        /// associated with the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        /// The identifier associated with the message.
        /// This must be castable to a <see cref="uint"/>.
        /// </param>
        /// <returns>The <see cref="Type"/> object of the message.</returns>
        Type GetMessageType(object id);

        /// <summary>
        /// Retrieves the <see cref="Type"/> of the message
        /// associated with the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        /// The identifier associated with the message.
        /// This must be castable to a <see cref="uint"/>.
        /// </param>
        /// <param name="type">The <see cref="Type"/> object of the message.</param>
        /// <returns>
        /// <see langword="true"/> when the <paramref name="type"/> was retrieved successfully;
        /// <see langword="false"/> otherwise.</returns>
        bool TryGetMessageType(object id, out Type type);

        /// <summary>
        /// Retrieves the identifier associated with the message
        /// of the given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of message.</param>
        /// <returns>The identifier associated with the message.</returns>
        uint GetMessageId(Type type);

        /// <summary>
        /// Retrieves the identifier associated with the message
        /// of the given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of message.</param>
        /// <param name="id">The identifier associated with the message.</param>
        /// <returns>
        /// <see langword="true"/> when the <paramref name="id"/> was retrieved successfully;
        /// <see langword="false"/> otherwise.
        /// </returns>
        bool TryGetMessageId(Type type, out uint id);

        /// <summary>
        /// Retrieves the identifier associated with the message of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of message.</typeparam>
        /// <returns>The identifier associated with the message.</returns>
        uint GetMessageId<T>()
            where T : class, IMessage;

        /// <summary>
        /// Retrieves the identifier associated with the message of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of message.</typeparam>
        /// <param name="id">The identifier associated with the message.</param>
        /// <returns>
        /// <see langword="true"/> when the <paramref name="id"/> was retrieved successfully;
        /// <see langword="false"/> otherwise.
        /// </returns>
        bool TryGetMessageId<T>(out uint id)
            where T : class, IMessage;

        /// <summary>
        /// Creates a new instance of the message associated with the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        /// The identifier associated with the message.
        /// This must be castable to a <see cref="uint"/>.
        /// </param>
        /// <returns>The message instance.</returns>
        IMessage CreateMessage(object id);

        /// <summary>
        /// Creates a new instance of the message associated with the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">
        /// The identifier associated with the message.
        /// This must be castable to a <see cref="uint"/>.
        /// </param>
        /// <param name="message">The message instance.</param>
        /// <returns>
        /// <see langword="true"/> when the <paramref name="message"/> was created successfully;
        /// <see langword="false"/> otherwise.
        /// </returns>
        bool TryCreateMessage(object id, out IMessage message);
    }
}
