using System.Linq;
using System.Security.Cryptography;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;

namespace BeatTogether.MasterServer.Kernel.Implementations.Providers
{
    public class ServerCodeProvider : IServerCodeProvider
    {
        private static readonly string _alphanumeric = "ABCDEFGHIJKLMNPQRSTUVWXYZ1245789";

        private readonly RNGCryptoServiceProvider _rngCryptoServiceProvider;

        public ServerCodeProvider(RNGCryptoServiceProvider rngCryptoServiceProvider)
        {
            _rngCryptoServiceProvider = rngCryptoServiceProvider;
        }

        public string Generate(int length = 5)
        {
            var randomBytes = new byte[length];
            _rngCryptoServiceProvider.GetBytes(randomBytes);
            return new string(
                randomBytes
                    .Select(b => _alphanumeric[b % _alphanumeric.Length])
                    .ToArray()
            );
        }
    }
}
