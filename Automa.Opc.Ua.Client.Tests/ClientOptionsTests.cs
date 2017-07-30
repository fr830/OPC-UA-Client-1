using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

namespace Automa.Opc.Ua.Client.Tests
{
    [TestFixture]
    public class ClientOptionsTests
    {
        [Test]
        public void ClientOptions()
        {
            var co = new ClientOptions();
            Assert.That(co.AutoAcceptUntrustedCertificates, Is.EqualTo(true));
            Assert.That(co.SessionTimeout, Is.EqualTo(60000));
            Assert.That(co.DefaultPublishingInterval, Is.EqualTo(1000));
        }

        [Test]
        public void ApplicationName()
        {
            var co = new ClientOptions();
            const string applicationName = "MyApplicationName";
            co.ApplicationName = applicationName;
            Assert.That(co.ApplicationName, Is.EqualTo(applicationName));
        }

        [Test]
        public void EndpointUrl()
        {
            var co = new ClientOptions();
            const string endpointurl = "MyEndpointUrl";
            co.EndpointUrl = endpointurl;
            Assert.That(co.EndpointUrl, Is.EqualTo(endpointurl));
        }

        [Test]
        public void ApplicationCertificate()
        {
            var co = new ClientOptions();
            var applicationcert = new X509Certificate2();
            co.ApplicationCertificate = applicationcert;
            Assert.That(co.ApplicationCertificate, Is.EqualTo(applicationcert));
        }

        [Test]
        public void SessionTimeout()
        {
            var co = new ClientOptions();
            const uint sessiontimeout = 999;
            co.SessionTimeout = sessiontimeout;
            Assert.That(co.SessionTimeout, Is.EqualTo(sessiontimeout));
        }

        [Test]
        public void DefaultPublishingInterval()
        {
            var co = new ClientOptions();
            const int interval = 999;
            co.DefaultPublishingInterval = interval;
            Assert.That(co.DefaultPublishingInterval, Is.EqualTo(interval));
        }

        [Test]
        public void AutoAcceptUntrustedCertificates()
        {
            var co = new ClientOptions();
            const bool val = false;
            co.AutoAcceptUntrustedCertificates = val;
            Assert.That(co.AutoAcceptUntrustedCertificates, Is.EqualTo(val));
        }
    }
}
