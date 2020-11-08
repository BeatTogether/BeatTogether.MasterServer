using System.Security.Cryptography.X509Certificates;

namespace BeatTogether.MasterServer.Kernel.Abstractions.Providers
{
    public interface ICertificateProvider
    {
        X509Certificate2 GetCertificate();
    }
}
