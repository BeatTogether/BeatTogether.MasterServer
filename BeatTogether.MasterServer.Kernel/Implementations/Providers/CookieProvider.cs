using System.Security.Cryptography;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;

namespace BeatTogether.MasterServer.Kernel.Implementations.Providers
{
    public class CookieProvider : ICookieProvider
    {
        private const int _cookieLength = 32;

        private readonly RandomNumberGenerator _rngCryptoServiceProvider;

        public CookieProvider(RandomNumberGenerator rngCryptoServiceProvider)
        {
            _rngCryptoServiceProvider = rngCryptoServiceProvider;
        }

        public byte[] GetCookie()
        {
            var cookie = new byte[_cookieLength];
            _rngCryptoServiceProvider.GetBytes(cookie);
            return cookie;
        }
    }
}
