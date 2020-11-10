using System;
using System.Security.Cryptography;
using BeatTogether.MasterServer.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations
{
    public class EncryptedMessageWriter : IEncryptedMessageWriter
    {
        private readonly RNGCryptoServiceProvider _rngCryptoServiceProvider;
        private readonly AesCryptoServiceProvider _aesCryptoServiceProvider;
        private readonly IMessageWriter _messageWriter;

        public EncryptedMessageWriter(
            RNGCryptoServiceProvider rngCryptoServiceProvider,
            AesCryptoServiceProvider aesCryptoServiceProvider,
            IMessageWriter messageWriter)
        {
            _rngCryptoServiceProvider = rngCryptoServiceProvider;
            _aesCryptoServiceProvider = aesCryptoServiceProvider;
            _messageWriter = messageWriter;
        }

        /// <inheritdoc cref="IEncryptedMessageWriter.WriteTo"/>
        public void WriteTo<T>(ref GrowingSpanBuffer buffer, T message, byte[] key, HMAC hmac)
            where T : class, IMessage
        {
            if (message is not IEncryptedMessage)
                throw new Exception($"Message of type '{typeof(T).Name}' cannot be encrypted.");

            var unencryptedBuffer = new GrowingSpanBuffer(stackalloc byte[412]);
            _messageWriter.WriteTo(ref unencryptedBuffer, message);

            var hashBuffer = new GrowingSpanBuffer(stackalloc byte[unencryptedBuffer.Size + 4]);
            hashBuffer.WriteBytes(unencryptedBuffer.Data);
            hashBuffer.WriteUInt32(((IEncryptedMessage)message).SequenceId);
            Span<byte> hash = stackalloc byte[32];
            if (!hmac.TryComputeHash(hashBuffer.Data, hash, out _))
                throw new Exception("Failed to compute message hash.");
            unencryptedBuffer.WriteBytes(hash.Slice(0, 10));

            var iv = new byte[16];
            _rngCryptoServiceProvider.GetBytes(iv);

            var paddingByteCount = (byte)((16 - ((unencryptedBuffer.Size + 1) & 15)) & 15);
            for (var i = 0; i < paddingByteCount + 1; i++)
                unencryptedBuffer.WriteUInt8(paddingByteCount);

            var encryptedBuffer = unencryptedBuffer.Data.ToArray();
            using (var cryptoTransform = _aesCryptoServiceProvider.CreateEncryptor(key, iv))
            {
                var bytesWritten = 0;
                for (var i = encryptedBuffer.Length; i >= cryptoTransform.InputBlockSize; i -= bytesWritten)
                {
                    var inputCount = cryptoTransform.CanTransformMultipleBlocks
                        ? (i / cryptoTransform.InputBlockSize * cryptoTransform.InputBlockSize)
                        : cryptoTransform.InputBlockSize;
                    bytesWritten = cryptoTransform.TransformBlock(
                        encryptedBuffer, bytesWritten, inputCount,
                        encryptedBuffer, bytesWritten
                    );
                }
            }

            buffer.WriteUInt32(((IEncryptedMessage)message).SequenceId);
            buffer.WriteBytes(iv);
            buffer.WriteBytes(encryptedBuffer);
        }
    }
}
