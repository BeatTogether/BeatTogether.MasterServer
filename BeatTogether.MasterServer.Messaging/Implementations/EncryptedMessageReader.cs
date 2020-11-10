using System;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using BeatTogether.MasterServer.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations
{
    public class EncryptedMessageReader : IEncryptedMessageReader
    {
        private readonly RNGCryptoServiceProvider _rngCryptoServiceProvider;
        private readonly AesCryptoServiceProvider _aesCryptoServiceProvider;
        private readonly IMessageReader _messageReader;

        public EncryptedMessageReader(
            RNGCryptoServiceProvider rngCryptoServiceProvider,
            AesCryptoServiceProvider aesCryptoServiceProvider,
            IMessageReader messageReader)
        {
            _rngCryptoServiceProvider = rngCryptoServiceProvider;
            _aesCryptoServiceProvider = aesCryptoServiceProvider;
            _messageReader = messageReader;
        }

        /// <inheritdoc cref="IEncryptedMessageReader.ReadFrom"/>
        public IEncryptedMessage ReadFrom(ref SpanBufferReader bufferReader, byte[] key, HMAC hmac)
        {
            var sequenceId = bufferReader.ReadUInt32();
            var iv = bufferReader.ReadBytes(16).ToArray();
            var decryptedBuffer = bufferReader.RemainingData.ToArray();
            using (var cryptoTransform = _aesCryptoServiceProvider.CreateDecryptor(key, iv))
            {
                var bytesWritten = 0;
                for (var i = decryptedBuffer.Length; i >= cryptoTransform.InputBlockSize; i -= bytesWritten)
                {
                    var inputCount = cryptoTransform.CanTransformMultipleBlocks
                        ? (i / cryptoTransform.InputBlockSize * cryptoTransform.InputBlockSize)
                        : cryptoTransform.InputBlockSize;
                    bytesWritten = cryptoTransform.TransformBlock(
                        decryptedBuffer, bytesWritten, inputCount,
                        decryptedBuffer, bytesWritten
                    );
                }
            }

            var paddingByteCount = decryptedBuffer[decryptedBuffer.Length - 1] + 1;
            var hmacStart = decryptedBuffer.Length - paddingByteCount - 10;
            var decryptedBufferSpan = decryptedBuffer.AsSpan();
            var hash = decryptedBufferSpan.Slice(hmacStart, 10);
            var hashBuffer = new GrowingSpanBuffer(stackalloc byte[decryptedBuffer.Length + 4]);
            hashBuffer.WriteBytes(decryptedBufferSpan.Slice(0, hmacStart));
            hashBuffer.WriteUInt32(sequenceId);
            Span<byte> computedHash = stackalloc byte[32];
            if (!hmac.TryComputeHash(hashBuffer.Data, computedHash, out _))
                throw new Exception("Failed to compute message hash.");
            if (!hash.SequenceEqual(computedHash.Slice(0, 10)))
                throw new Exception("Message hash does not match the computed hash.");

            bufferReader = new SpanBufferReader(decryptedBuffer);
            if (_messageReader.ReadFrom(ref bufferReader) is not IEncryptedMessage message)
                throw new Exception(
                    "Successfully decrypted message but failed to cast to type " +
                    $"'{nameof(IEncryptedMessage)}'."
                );

            message.SequenceId = sequenceId;
            return message;
        }
    }
}
