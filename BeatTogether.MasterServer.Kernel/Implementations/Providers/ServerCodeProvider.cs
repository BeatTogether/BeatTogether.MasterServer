using System.Linq;
using System.Security.Cryptography;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;

namespace BeatTogether.MasterServer.Kernel.Implementations.Providers
{
    public class ServerCodeProvider : IServerCodeProvider
    {
        private static readonly string _alphanumeric = "ABCDEFGHIJKLMNPQRSTUVWXYZ1245789";

        private readonly RNGCryptoServiceProvider _cryptoServiceProvider;

        public ServerCodeProvider(RNGCryptoServiceProvider rngCryptoServiceProvider)
        {
            _cryptoServiceProvider = rngCryptoServiceProvider;
        }

        #region Public Methods

        public string Generate(int length = 5)
        {
            byte[] randomBytes = GenerateRandomBytes(length);
            return new string(randomBytes
                .Select(b => _alphanumeric[b % _alphanumeric.Length])
                .ToArray()
            );
        }

        #endregion

        #region Private Methods

        private byte[] GenerateRandomBytes(int length)
        {
            var randomBytes = new byte[length];
            _cryptoServiceProvider.GetBytes(randomBytes);
            return randomBytes;
        }

        #endregion
    }
}
