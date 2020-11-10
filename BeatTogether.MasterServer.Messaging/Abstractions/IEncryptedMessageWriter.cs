using System.Security.Cryptography;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Abstractions
{
    public interface IEncryptedMessageWriter
    {
        /// <summary>
        /// Writes an encrypted message to the given buffer.
        /// This will include message headers.
        /// </summary>
        /// <typeparam name="T">The type of message to serialize.</typeparam>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="message">The message to serialize.</param>
        /// <param name="key">The encryption key.</param>
        /// <param name="hmac">HMAC hasher.</param>
        void WriteTo<T>(ref GrowingSpanBuffer buffer, T message, byte[] key, HMAC hmac)
            where T : class, IMessage;
    }
}
