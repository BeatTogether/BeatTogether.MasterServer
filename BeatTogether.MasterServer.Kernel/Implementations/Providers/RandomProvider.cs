using System.Security.Cryptography;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;

namespace BeatTogether.MasterServer.Kernel.Implementations.Providers
{
    public class RandomProvider : IRandomProvider
    {
        private const int _randomLength = 32;

        private readonly RNGCryptoServiceProvider _rngCryptoServiceProvider;

        public RandomProvider(RNGCryptoServiceProvider rngCryptoServiceProvider)
        {
            _rngCryptoServiceProvider = rngCryptoServiceProvider;
        }

        public byte[] GetRandom()
        {
            var random = new byte[_randomLength];
            _rngCryptoServiceProvider.GetBytes(random);
            return random;
        }
    }
}
