using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Text;

namespace ProvisionService.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly ProvisionService.ProvisionOptions _options;
        private readonly AsymmetricKeyParameter _caPrivateKey;
        private readonly X509Certificate _caCertificate;

        public CertificateService(IConfiguration configuration)
        {
            _options = new ProvisionService.ProvisionOptions();
            configuration.GetSection("ProvisionOptions").Bind(_options);

            if (!string.IsNullOrEmpty(_options.CaPrivateKeyPath) && File.Exists(_options.CaPrivateKeyPath)
                && !string.IsNullOrEmpty(_options.CaCertPath) && File.Exists(_options.CaCertPath))
            {
                _caPrivateKey = LoadPrivateKey(_options.CaPrivateKeyPath);
                _caCertificate = LoadCertificate(_options.CaCertPath);
            }
            else
            {
                var pair = GenerateEd25519KeyPair();
                _caPrivateKey = pair.Private;
                _caCertificate = GenerateSelfSignedCa(pair, "CN=Provisioning-CA");
                if (!string.IsNullOrEmpty(_options.CaPrivateKeyPath))
                    SavePrivateKey(_options.CaPrivateKeyPath, pair);
                if (!string.IsNullOrEmpty(_options.CaCertPath))
                    SaveCertificate(_options.CaCertPath, _caCertificate);
            }
        }

        public Task<string> SignCsrAsync(string csrPem, string deviceId)
        {
            var csr = ReadCsr(csrPem) ?? throw new InvalidOperationException("invalid csr");
            var publicKey = csr.GetPublicKey();

            var certGen = new X509V3CertificateGenerator();
            certGen.SetSerialNumber(Org.BouncyCastle.Math.BigInteger.ValueOf(DateTime.UtcNow.Ticks));
            certGen.SetIssuerDN(_caCertificate.SubjectDN);
            certGen.SetNotBefore(DateTime.UtcNow.AddMinutes(-5));
            certGen.SetNotAfter(DateTime.UtcNow.AddYears(10));
            certGen.SetSubjectDN(csr.GetCertificationRequestInfo().Subject);
            certGen.SetPublicKey(publicKey);

            if (!string.IsNullOrEmpty(deviceId))
            {
                var san = new GeneralNames(new GeneralName(GeneralName.Rfc822Name, deviceId));
                certGen.AddExtension(X509Extensions.SubjectAlternativeName, false, san);
            }

            ISignatureFactory sigFactory = new Asn1SignatureFactory("Ed25519", _caPrivateKey);
            var cert = certGen.Generate(sigFactory);

            var sb = new StringBuilder();
            using var sw = new StringWriter(sb);
            var pemWriter = new PemWriter(sw);
            pemWriter.WriteObject(cert);
            sw.Flush();
            return Task.FromResult(sb.ToString());
        }

        // helpers
        private Pkcs10CertificationRequest? ReadCsr(string pem)
        {
            using var sr = new StringReader(pem);
            var pr = new PemReader(sr).ReadObject();
            return pr as Pkcs10CertificationRequest;
        }

        private AsymmetricCipherKeyPair GenerateEd25519KeyPair()
        {
            var gen = GeneratorUtilities.GetKeyPairGenerator("Ed25519");
            gen.Init(new Org.BouncyCastle.Crypto.KeyGenerationParameters(new SecureRandom(), 256));
            return gen.GenerateKeyPair();
        }

        private X509Certificate GenerateSelfSignedCa(AsymmetricCipherKeyPair pair, string subjectDn)
        {
            var certGen = new X509V3CertificateGenerator();
            certGen.SetSerialNumber(Org.BouncyCastle.Math.BigInteger.ValueOf(DateTime.UtcNow.Ticks));
            var name = new X509Name(subjectDn);
            certGen.SetIssuerDN(name);
            certGen.SetSubjectDN(name);
            certGen.SetNotBefore(DateTime.UtcNow.AddDays(-1));
            certGen.SetNotAfter(DateTime.UtcNow.AddYears(20));
            certGen.SetPublicKey(pair.Public);
            ISignatureFactory sigFactory = new Asn1SignatureFactory("Ed25519", pair.Private);
            return certGen.Generate(sigFactory);
        }

        private AsymmetricKeyParameter LoadPrivateKey(string path)
        {
            using var sr = File.OpenText(path);
            var obj = new PemReader(sr).ReadObject();
            if (obj is AsymmetricCipherKeyPair kp) return kp.Private;
            if (obj is AsymmetricKeyParameter key) return key;
            throw new InvalidOperationException("unsupported private key format");
        }

        private X509Certificate LoadCertificate(string path)
        {
            var pem = File.ReadAllText(path);
            var parser = new X509CertificateParser();
            return parser.ReadCertificate(Encoding.UTF8.GetBytes(pem));
        }

        private void SavePrivateKey(string path, AsymmetricCipherKeyPair pair)
        {
            using var sw = new StreamWriter(path);
            var pw = new PemWriter(sw);
            pw.WriteObject(pair.Private);
            sw.Flush();
        }

        private void SaveCertificate(string path, X509Certificate cert)
        {
            using var sw = new StreamWriter(path);
            var pw = new PemWriter(sw);
            pw.WriteObject(cert);
            sw.Flush();
        }
    }
}
