using System.Security.Cryptography;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;

namespace BeatTogether.MasterServer.Kernel.Implementations.Providers
{
    public class RandomProvider : IRandomProvider
    {
        private const int _randomLength = 32;

        private readonly RandomNumberGenerator _rngCryptoServiceProvider;

        public RandomProvider(RandomNumberGenerator rngCryptoServiceProvider)
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
