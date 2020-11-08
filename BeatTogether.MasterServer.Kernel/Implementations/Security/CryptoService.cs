using System;
using System.Security.Cryptography;
using BeatTogether.MasterServer.Kernel.Abstractions.Security;

namespace BeatTogether.MasterServer.Kernel.Implementations.Security
{
    public class CryptoService : ICryptoService
    {
        public int IvLength => 16;

        private readonly RNGCryptoServiceProvider _rngCryptoServiceProvider;
        private readonly AesCryptoServiceProvider _aesCryptoServiceProvider;

        public CryptoService(
            RNGCryptoServiceProvider rngCryptoServiceProvider,
            AesCryptoServiceProvider aesCryptoServiceProvider)
        {
            _rngCryptoServiceProvider = rngCryptoServiceProvider;
            _aesCryptoServiceProvider = aesCryptoServiceProvider;
        }

        public byte[] GetIv()
        {
            var iv = new byte[IvLength];
            _rngCryptoServiceProvider.GetBytes(iv);
            return iv;
        }

        public void Encrypt(byte[] buffer, byte[] key, byte[] iv)
        {
            using var cryptoTransform = _aesCryptoServiceProvider.CreateEncryptor(key, iv);
            var bytesWritten = 0;
            for (var i = buffer.Length; i >= cryptoTransform.InputBlockSize; i -= bytesWritten)
            {
                var inputCount = cryptoTransform.CanTransformMultipleBlocks
                    ? (i / cryptoTransform.InputBlockSize * cryptoTransform.InputBlockSize)
                    : cryptoTransform.InputBlockSize;
                bytesWritten = cryptoTransform.TransformBlock(buffer, bytesWritten, inputCount, buffer, bytesWritten);
            }
        }

        public void Decrypt(byte[] buffer, byte[] key, byte[] iv)
        {
            using var cryptoTransform = _aesCryptoServiceProvider.CreateDecryptor(key, iv);
            var bytesWritten = 0;
            for (var i = buffer.Length; i >= cryptoTransform.InputBlockSize; i -= bytesWritten)
            {
                var inputCount = cryptoTransform.CanTransformMultipleBlocks
                    ? (i / cryptoTransform.InputBlockSize * cryptoTransform.InputBlockSize)
                    : cryptoTransform.InputBlockSize;
                bytesWritten = cryptoTransform.TransformBlock(buffer, bytesWritten, inputCount, buffer, bytesWritten);
            }
        }

        public byte[] MakeSeed(byte[] baseSeed, byte[] serverSeed, byte[] clientSeed)
        {
            var seed = new byte[baseSeed.Length + serverSeed.Length + clientSeed.Length];
            Array.Copy(baseSeed, 0, seed, 0, baseSeed.Length);
            Array.Copy(serverSeed, 0, seed, baseSeed.Length, serverSeed.Length);
            Array.Copy(clientSeed, 0, seed, baseSeed.Length + serverSeed.Length, clientSeed.Length);
            return seed;
        }

        public byte[] PRF(byte[] key, byte[] seed, int length)
        {
            var i = 0;
            var array = new byte[length + seed.Length];
            while (i < length)
            {
                Array.Copy(seed, 0, array, i, seed.Length);
                PRFHash(key, array, ref i);
            }
            var array2 = new byte[length];
            Array.Copy(array, 0, array2, 0, length);
            return array2;
        }

        public void PRFHash(byte[] key, byte[] seed, ref int length)
        {
            using var hmacsha256 = new HMACSHA256(key);
            var array = hmacsha256.ComputeHash(seed, 0, length);
            int num = Math.Min(length + array.Length, seed.Length);
            Array.Copy(array, 0, seed, length, num - length);
            length = num;
        }
    }
}
