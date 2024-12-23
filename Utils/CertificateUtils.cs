using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Api.Utils;

public static class CertificateUtils
{
    public static X509Certificate2 CreateRootCertificate(string rootCertificateName)
    {
        // Define a distinguished name for the certificate
        var distinguishedName = new X500DistinguishedName($"CN={rootCertificateName}");

        using var rsa = RSA.Create(2048);
        var request =
            new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        // Specify certificate extensions
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        // Create the self-signed root certificate
        var rootCertificate =
            request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(10));
        return rootCertificate;
    }

    public static X509Certificate2 CreateClientCertificate(string clientCertificateName)
    {
        if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "root.pfx")))
            SaveCertificate(CreateRootCertificate("CI-Authority"), "root.pfx");

        var rootCertificate = new X509Certificate2("root.pfx");
        var distinguishedName = new X500DistinguishedName($"CN={clientCertificateName}");

        using var rsa = RSA.Create(2048);
        var request =
            new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        // Specify certificate extensions
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));

        // Create the client certificate signed by the root certificate
        var clientCertificate = request.Create(
            rootCertificate,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(5),
            new byte[] { 1, 2, 3, 4 });

        return clientCertificate.CopyWithPrivateKey(rsa);
    }

    public static void SaveCertificate(X509Certificate2 certificate, string filePath)
    {
        var pfxBytes = certificate.Export(X509ContentType.Pfx);
        File.WriteAllBytes(filePath, pfxBytes);
    }
}