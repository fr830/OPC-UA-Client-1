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
using Node = Automa.Opc.Ua.Client.Models.Node;

namespace Automa.Opc.Ua.Client.Tests
{
    [TestFixture]
    public class ClientTests
    {
        private Mock<IDiscoveryClient> _mockDiscoveryClient;
        private Mock<ISession> _mockSession;

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

        [SetUp]
        public void SetUp()
        {
            _mockDiscoveryClient = new Mock<IDiscoveryClient>();
            _mockDiscoveryClient.Setup(x => x.Create(
                It.IsAny<Uri>(),
                It.IsAny<EndpointConfiguration>())).Returns(_mockDiscoveryClient.Object);
            // ReSharper disable once RedundantAssignment
            var expectedData = new EndpointDescriptionCollection
            {
                new EndpointDescription($"opc.tcp://{Environment.MachineName}:51210/UA/SampleServer")
            };
            _mockDiscoveryClient
                .Setup(x => x.GetEndpoints(
                    It.IsAny<RequestHeader>(),
                    It.IsAny<string>(),
                    It.IsAny<StringCollection>(),
                    It.IsAny<StringCollection>(),
                    out expectedData))
                .Returns(new ResponseHeader());

            _mockSession = new Mock<ISession>();
            _mockSession.Setup(x => x.Create(
                It.IsAny<ApplicationConfiguration>(),
                It.IsAny<ConfiguredEndpoint>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<uint>(),
                It.IsAny<IUserIdentity>(),
                It.IsAny<IList<string>>())).Returns(Task.FromResult(_mockSession.Object));
            _mockSession.Setup(x => x.GetDefaultSubscription()).Returns(new Subscription
            {
                DisplayName = "Subscription",
                PublishingInterval = 1000,
                KeepAliveCount = 10U,
                LifetimeCount = 1000U,
                Priority = byte.MaxValue,
                PublishingEnabled = true
            });

            Client.DiscoveryClientStub = _mockDiscoveryClient.Object;
            Client.SessionStub = _mockSession.Object;
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
                await client.Watch("tag", null);
                await client.Unwatch("tag");
            }
        }

        [Test]
        public void GetUriFromApplicationName()
        {
            Assert.That(Client.ConvertToCamelCase(Client.RemoveSpecialCharacters("hello*world...")), Is.EqualTo("Helloworld"));
        }

        [Test]
        public async Task ReadNode()
        {
            DiagnosticInfoCollection diagnosticInfos;
            var results = new DataValueCollection
            {
                new DataValue
                {
                    Value = "value"
                }
            };
            _mockSession.Setup(x => x.Read(
                It.IsAny<RequestHeader>(),
                It.IsAny<double>(),
                It.IsAny<TimestampsToReturn>(),
                It.IsAny<ReadValueIdCollection>(),
                out results,
                out diagnosticInfos
            )).Returns(new ResponseHeader());

            using (var client = await Client.Create(new ClientOptions
            {
                ApplicationName = "UA Core Sample Client",
                EndpointUrl = $"opc.tcp://{Environment.MachineName}:51210/UA/SampleServer",
                ApplicationCertificate = null,
                AutoAcceptUntrustedCertificates = true
            }))
            {
                var values = await client.ReadNode("node");
                var enumerable = values as object[] ?? values.ToArray();
                Assert.That(enumerable.Count, Is.EqualTo(results.Count));
                Assert.That(enumerable.Single(), Is.EqualTo(results.Single().Value));
            }
        }

        [Test]
        public async Task BrowseNode()
        {
            byte[] continuationPoint;
            var results = new ReferenceDescriptionCollection
            {
                new ReferenceDescription
                {
                    NodeId = "NodeId",
                    DisplayName = "DisplayName"
                }
            };
            _mockSession.Setup(x => x.Browse(
                It.IsAny<RequestHeader>(),
                It.IsAny<ViewDescription>(),
                It.IsAny<NodeId>(),
                It.IsAny<uint>(),
                It.IsAny<BrowseDirection>(),
                It.IsAny<NodeId>(),
                It.IsAny<bool>(),
                It.IsAny<uint>(),
                out continuationPoint,
                out results
            )).Returns(new ResponseHeader());

            using (var client = await Client.Create(new ClientOptions
            {
                ApplicationName = "UA Core Sample Client",
                EndpointUrl = $"opc.tcp://{Environment.MachineName}:51210/UA/SampleServer",
                ApplicationCertificate = null,
                AutoAcceptUntrustedCertificates = true
            }))
            {
                var nodes = await client.BrowseNode("node");
                var enumerable = nodes as Node[] ?? nodes.ToArray();
                Assert.That(enumerable.Count, Is.EqualTo(results.Count));
                Assert.That(enumerable.Single().Tag, Is.EqualTo(results.Single().NodeId.ToString()));
                Assert.That(enumerable.Single().DisplayName, Is.EqualTo(results.Single().DisplayName.Text));
            }
        }

        [Test]
        public async Task BrowseNode2()
        {
            byte[] continuationPoint;
            var results = new ReferenceDescriptionCollection
            {
                new ReferenceDescription
                {
                    NodeId = "NodeId",
                    DisplayName = "DisplayName"
                }
            };
            _mockSession.Setup(x => x.Browse(
                It.IsAny<RequestHeader>(),
                It.IsAny<ViewDescription>(),
                It.IsAny<NodeId>(),
                It.IsAny<uint>(),
                It.IsAny<BrowseDirection>(),
                It.IsAny<NodeId>(),
                It.IsAny<bool>(),
                It.IsAny<uint>(),
                out continuationPoint,
                out results
            )).Returns(new ResponseHeader());

            using (var client = await Client.Create(new ClientOptions
            {
                ApplicationName = "UA Core Sample Client",
                EndpointUrl = $"opc.tcp://{Environment.MachineName}:51210/UA/SampleServer",
                ApplicationCertificate = null,
                AutoAcceptUntrustedCertificates = true
            }))
            {
                var nodes = await client.BrowseNode();
                var enumerable = nodes as Node[] ?? nodes.ToArray();
                Assert.That(enumerable.Count, Is.EqualTo(results.Count));
                Assert.That(enumerable.Single().Tag, Is.EqualTo(results.Single().NodeId.ToString()));
                Assert.That(enumerable.Single().DisplayName, Is.EqualTo(results.Single().DisplayName.Text));
            }
        }

        [Test]
        public async Task GetNode()
        {
            _mockSession.Setup(x => x.Find(
                "s=NodeId"
            )).Returns(new global::Opc.Ua.Node
            {
                NodeId = "NodeId",
                DisplayName = "DisplayName"
            });

            using (var client = await Client.Create(new ClientOptions
            {
                ApplicationName = "UA Core Sample Client",
                EndpointUrl = $"opc.tcp://{Environment.MachineName}:51210/UA/SampleServer",
                ApplicationCertificate = null,
                AutoAcceptUntrustedCertificates = true
            }))
            {
                var node = await client.GetNode("s=NodeId");
                Assert.That(node, Is.Not.Null);
                Assert.That(node.Tag, Is.EqualTo("s=NodeId"));
                Assert.That(node.DisplayName, Is.EqualTo("DisplayName"));
            }
        }

        [Test]
        public async Task GetNode2()
        {
            _mockSession.Setup(x => x.Find(
                "s=NodeId"
            )).Returns((INode) null);

            using (var client = await Client.Create(new ClientOptions
            {
                ApplicationName = "UA Core Sample Client",
                EndpointUrl = $"opc.tcp://{Environment.MachineName}:51210/UA/SampleServer",
                ApplicationCertificate = null,
                AutoAcceptUntrustedCertificates = true
            }))
            {
                var node = await client.GetNode("node");
                Assert.That(node, Is.Null);
            }
        }
    }
}
