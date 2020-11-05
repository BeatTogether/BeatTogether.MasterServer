using System.Security.Cryptography;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;

namespace BeatTogether.MasterServer.Kernel.Implementations.Providers
{
    public class CookieProvider : ICookieProvider
    {
        private readonly RNGCryptoServiceProvider _rngCryptoServiceProvider;

        public CookieProvider(RNGCryptoServiceProvider rngCryptoServiceProvider)
        {
            _rngCryptoServiceProvider = rngCryptoServiceProvider;
        }

        public byte[] GetCookie()
        {
            var cookie = new byte[32];
            _rngCryptoServiceProvider.GetBytes(cookie);
            return cookie;
        }
    }
}
