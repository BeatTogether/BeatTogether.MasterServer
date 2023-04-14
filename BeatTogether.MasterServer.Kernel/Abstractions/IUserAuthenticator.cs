using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Implementations;
using BeatTogether.MasterServer.Messaging.Models;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IUserAuthenticator
    {
        /// <summary>
        /// Will attempt to verify a user's authentication token with their platform.
        /// </summary>
        /// <param name="session">The session to update if authentication is successful</param>
        /// <param name="token">The authentication token to verify</param>
        /// <returns>True on auth success or if skipped, false on explicit auth failure.</returns>
        Task<bool> TryAuthenticateUserWithPlatform(MasterServerSession session, AuthenticationToken token);
    }
}