using System.Security.Cryptography;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;

namespace BeatTogether.MasterServer.Kernel.Implementations.Providers
{
    public class ServerCodeProvider : IServerCodeProvider
    {
        private static readonly string _alphanumeric = "ABCEFGHJKLMNPQRSTUVWXYZ01234579";

        private readonly RNGCryptoServiceProvider _rngCryptoServiceProvider;

        public ServerCodeProvider(RNGCryptoServiceProvider rngCryptoServiceProvider)
        {
            _rngCryptoServiceProvider = rngCryptoServiceProvider;
        }

        public string Generate(int length = 5)
        {
            var randomBytes = new byte[length];
            _rngCryptoServiceProvider.GetBytes(randomBytes);
            return string.Create(length, randomBytes, (str, randomBytes) => {
                for (var i = 0; i < str.Length; i++)
                {
                    str[i] = _alphanumeric[randomBytes[i] % _alphanumeric.Length];
                }
            });
        }
    }
}
