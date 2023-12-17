using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using System.Security.Cryptography;

namespace BeatTogether.MasterServer.Kernel.Implementations.Providers
{
    public class SecretProvider : ISecretProvider
    {
        private const int _secretLength = 22;

        private static readonly string _alphanumeric = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz12345789";

        private readonly RandomNumberGenerator _rngCryptoServiceProvider;

        public SecretProvider(RandomNumberGenerator rngCryptoServiceProvider)
        {
            _rngCryptoServiceProvider = rngCryptoServiceProvider;
        }

        public string GetSecret()
        {
            var randomBytes = new byte[_secretLength];
            _rngCryptoServiceProvider.GetBytes(randomBytes);
            return string.Create(_secretLength, randomBytes, (str, randomBytes) => {
                for (var i = 0; i < str.Length; i++)
                {
                    str[i] = _alphanumeric[randomBytes[i] % _alphanumeric.Length];
                }
            });
        }
    }
}
