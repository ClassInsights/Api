using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Api.Utils;

public static class CertificateUtils
{
    // https://stackoverflow.com/a/17225510/16871250
    public static bool ValidateClientCertificate(X509Certificate2 clientCertificate)
    {
        if (clientCertificate.NotBefore > DateTime.UtcNow || clientCertificate.NotAfter < DateTime.UtcNow)
            return false;

        if (!File.Exists("rootCertificate.pfx"))
            SaveCertificate(CreateRootCertificate("CI-Authority"), "rootCertificate.pfx");
        
        var rootCertificate = new X509Certificate2("rootCertificate.pfx");
        var chain = new X509Chain();
        
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
        chain.ChainPolicy.VerificationTime = DateTime.Now;
        
        // Add the root certificate to the chain's trusted root
        chain.ChainPolicy.ExtraStore.Add(rootCertificate);

        var isChainValid = chain.Build(clientCertificate);

        if (!isChainValid)
            return false;

        // Check if Thumbprints of Authority match
        var valid = chain.ChainElements.Any(x => x.Certificate.Thumbprint == rootCertificate.Thumbprint);
        return valid;
    }

    private static X509Certificate2 CreateRootCertificate(string rootCertificateName)
    {
        // Define a distinguished name for the certificate
        var distinguishedName = new X500DistinguishedName($"CN={rootCertificateName}");

        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        // Specify certificate extensions
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        // Create the self-signed root certificate
        var rootCertificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(10));
        return rootCertificate;
    }
    
    public static X509Certificate2 CreateClientCertificate(string clientCertificateName)
    {
        if (!File.Exists("rootCertificate.pfx"))
            SaveCertificate(CreateRootCertificate("CI-Authority"), "rootCertificate.pfx");
        
        var rootCertificate = new X509Certificate2("rootCertificate.pfx");
        var distinguishedName = new X500DistinguishedName($"CN={clientCertificateName}");

        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

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

    private static void SaveCertificate(X509Certificate2 certificate, string filePath)
    {
        var pfxBytes = certificate.Export(X509ContentType.Pfx);
        File.WriteAllBytes(filePath, pfxBytes);
    }
}