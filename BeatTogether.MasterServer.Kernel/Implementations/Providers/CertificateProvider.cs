using System.Security.Cryptography.X509Certificates;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Kernel.Configuration;

namespace BeatTogether.MasterServer.Kernel.Implementations.Providers
{
    public class CertificateProvider : ICertificateProvider
    {
        private readonly X509Certificate2 _x509Certificate;

        public CertificateProvider(MasterServerConfiguration configuration)
        {
            _x509Certificate = new X509Certificate2(configuration.CertificatePath);
        }

        public X509Certificate2 GetCertificate()
            => _x509Certificate;
    }
}
