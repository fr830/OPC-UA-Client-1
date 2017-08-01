using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Automa.Opc.Ua.Client.Interfaces;
using Moq;
using NUnit.Framework;
using Opc.Ua;
using Opc.Ua.Client;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Automa.Opc.Ua.Client.Tests
{
    [TestFixture]
    public class ClientTests
    {
        private Mock<IDiscoveryClient> mockDiscoveryClient;
        private Mock<ISession> mockSession;

        [SetUp]
        public void SetUp()
        {
            mockDiscoveryClient = new Mock<IDiscoveryClient>();
            mockDiscoveryClient.Setup(x => x.Create(
                It.IsAny<Uri>(),
                It.IsAny<EndpointConfiguration>())).Returns(mockDiscoveryClient.Object);
            var expectedData = new EndpointDescriptionCollection();
            expectedData.Add(new EndpointDescription($"opc.tcp://{Environment.MachineName}:51210/UA/SampleServer"));
            mockDiscoveryClient
                .Setup(x => x.GetEndpoints(
                    It.IsAny<RequestHeader>(),
                    It.IsAny<string>(),
                    It.IsAny<StringCollection>(),
                    It.IsAny<StringCollection>(),
                    out expectedData))
                .Returns(new ResponseHeader());

            mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Create(
                It.IsAny<ApplicationConfiguration>(),
                It.IsAny<ConfiguredEndpoint>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<uint>(),
                It.IsAny<IUserIdentity>(),
                It.IsAny<IList<string>>())).Returns(Task.FromResult(mockSession.Object));
            mockSession.Setup(x => x.GetDefaultSubscription()).Returns(new Subscription
            {
                DisplayName = "Subscription",
                PublishingInterval = 1000,
                KeepAliveCount = 10U,
                LifetimeCount = 1000U,
                Priority = byte.MaxValue,
                PublishingEnabled = true
            });

            Automa.Opc.Ua.Client.Client.discoveryClientStub = mockDiscoveryClient.Object;
            Automa.Opc.Ua.Client.Client.sessionStub = mockSession.Object;
        }

        [Test]
        public async Task Create()
        {
            using (var client = await Client.Create(new ClientOptions
            {
                ApplicationName = "UA Core Sample Client",
                EndpointUrl = $"opc.tcp://{Environment.MachineName}:51210/UA/SampleServer",
                ApplicationCertificate = null,
                AutoAcceptUntrustedCertificates = true
            }))
            {
                Assert.NotNull(client);
            }
        }

        [Test]
        public async Task Create2()
        {
            using (var client = await Client.Create(new ClientOptions
            {
                ApplicationName = "UA Core Sample Client",
                EndpointUrl = $"opc.tcp://{Environment.MachineName}:51210/UA/SampleServer",
                ApplicationCertificate = GenerateCertificate("UA Core Sample Client"),
                AutoAcceptUntrustedCertificates = false
            }))
            {
                Assert.NotNull(client);
            }
        }

        [Test]
        public async Task Watch()
        {
            using (var client = await Client.Create(new ClientOptions
            {
                ApplicationName = "UA Core Sample Client",
                EndpointUrl = $"opc.tcp://{Environment.MachineName}:51210/UA/SampleServer",
                ApplicationCertificate = null,
                AutoAcceptUntrustedCertificates = true
            }))
            {
                await client.Watch("tag", null, 1000);
            }
        }
        [Test]
        public async Task Unwatch()
        {
            using (var client = await Client.Create(new ClientOptions
            {
                ApplicationName = "UA Core Sample Client",
                EndpointUrl = $"opc.tcp://{Environment.MachineName}:51210/UA/SampleServer",
                ApplicationCertificate = null,
                AutoAcceptUntrustedCertificates = true
            }))
            {
                await client.Unwatch("tag");
            }
        }

        [Test]
        public void GetURIFromApplicationName()
        {
            Assert.That(Automa.Opc.Ua.Client.Client.ConvertToCamelCase(Automa.Opc.Ua.Client.Client.RemoveSpecialCharacters("hello*world...")), Is.EqualTo("Helloworld"));
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
