using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;
using System.IO;

namespace TinifyAPI.Internal
{
    internal static class SSL
    {
        public static bool ValidationCallback(HttpRequestMessage req, X509Certificate2 cert, X509Chain chain,
            SslPolicyErrors errors)
        {
            if (errors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable)) return false;
            if (errors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch)) return false;
            return new X509Chain() {ChainPolicy = policy}.Build(cert);
        }

        private static X509ChainPolicy policy = createSSLChainPolicy();

        private static X509ChainPolicy createSSLChainPolicy()
        {
            var policy = new X509ChainPolicy()
            {
                VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority
            };

            var header = "-----BEGIN CERTIFICATE-----";
            var footer = "-----END CERTIFICATE-----";

            using (var stream = getBundleStream())
            using (var reader = new StreamReader(stream))
            {
                var pem = reader.ReadToEnd();
                var start = 0;
                var end = 0;
                while (true)
                {
                    start = pem.IndexOf(header, start, StringComparison.Ordinal);
                    if (start < 0) break;

                    start += header.Length;
                    end = pem.IndexOf(footer, start, StringComparison.Ordinal);
                    if (end < 0) break;

                    var bytes = Convert.FromBase64String(pem.Substring(start, end - start));
                    policy.ExtraStore.Add(new X509Certificate2(bytes));
                }
            }

            return policy;
        }

        private static Stream getBundleStream()
        {
            var assembly = typeof(SSL).GetTypeInfo().Assembly;
            return assembly.GetManifestResourceStream("TinifyAPI.data.cacert.pem");
        }
    }
}