using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

// ReSharper disable AccessToDisposedClosure

namespace Automa.Opc.Ua.Client.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        public static async Task MainAsync(string[] args)
        {
            var endpointUrl = args.Length == 0 ? $"opc.tcp://{Environment.MachineName}:51210/UA/SampleServer" : args[0];

            using (var client = await Client.Create(new ClientOptions
            {
                ApplicationName = "UA Core Sample Client",
                EndpointUrl = endpointUrl,
                ApplicationCertificate = GenerateCertificate("UA Core Sample Client")
            }))
            {
                Console.WriteLine("read 1st level nodes under root");
                var nodes = await client.BrowseNode();
                foreach (var node in nodes)
                {
                    Console.WriteLine($"Tag: {node.Tag}, DisplayName: {node.DisplayName}");
                    Console.WriteLine($"read {node.DisplayName} child nodes");
                    var childNodes = await client.BrowseNode(node.Tag);
                    foreach (var childNode in childNodes)
                    {
                        Console.WriteLine($"Tag: {childNode.Tag}, DisplayName: {childNode.DisplayName}");
                    }
                }
                const string tag = "i=2258";
                Console.WriteLine($"read node information for node with tag {tag}");
                var currentTime = await client.GetNode(tag);
                Console.WriteLine($"Tag: {currentTime.Tag}, DisplayName: {currentTime.DisplayName}");
                Console.WriteLine($"read current value for {currentTime.DisplayName}");
                var values = await client.ReadNode(tag);
                foreach (var value in values)
                {
                    Console.WriteLine($"{currentTime.DisplayName}: {value}");
                }
                Console.WriteLine($"start watching {currentTime.DisplayName}, just first 5 changes");
                var counter = 0;
                await client.Watch(tag, async (sender, e) =>
                {
                    foreach (var value in e.Values)
                    {
                        Console.WriteLine($"{currentTime.DisplayName}: {value}");
                    }
                    counter++;
                    if (counter != 5) return;
                    await client.Unwatch(tag);
                    Console.WriteLine($"stopped watching {currentTime.DisplayName}");
                });

                Console.ReadKey(true);
            }
        }

        private static X509Certificate2 GenerateCertificate(string certName)
        {
            var keypairgen = new RsaKeyPairGenerator();
            keypairgen.Init(new KeyGenerationParameters(new SecureRandom(new DigestRandomGenerator(new MD5Digest())), 1024));

            var keypair = keypairgen.GenerateKeyPair();
            var gen = new X509V3CertificateGenerator();
            var cn = new X509Name("CN=" + certName);
            var sn = BigInteger.ProbablePrime(120, new Random());

            gen.SetSerialNumber(sn);
            gen.SetSubjectDN(cn);
            gen.SetIssuerDN(cn);
            gen.SetNotAfter(DateTime.MaxValue);
            gen.SetNotBefore(DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0)));
            gen.SetPublicKey(keypair.Public);
            var newCert = gen.Generate(new Asn1SignatureFactory("MD5WithRSA", keypair.Private));

            return new X509Certificate2(newCert.GetEncoded());
        }
    }
}