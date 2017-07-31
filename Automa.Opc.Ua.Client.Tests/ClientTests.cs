using System.Security.Cryptography.X509Certificates;
using Moq;
using NUnit.Framework;
using OPCUA = Opc.Ua;

namespace Automa.Opc.Ua.Client.Tests
{
    [TestFixture]
    public class ClientTests
    {
        [Test]
        public void Client()
        {
            var mockDiscoveryClient = new Mock<OPCUA.DiscoveryClient>(MockBehavior.Strict, new object[] {
                new Mock<OPCUA.ITransportChannel>().Object});
            /* var mockSession = new Mock<OPCUA.Client.Session>(MockBehavior.Strict, new object[] {
                new Mock<OPCUA.ISessionChannel>().Object,
                new OPCUA.ApplicationConfiguration(),
                new Mock<OPCUA.ConfiguredEndpoint>().Object,
                new X509Certificate2()}); 
            var ses = new OPCUA.Client.Session(
                new Mock<OPCUA.ITransportChannel>().Object,
                new OPCUA.ApplicationConfiguration(),
                new OPCUA.ConfiguredEndpoint(),
                new X509Certificate2());*/
           /*  Automa.Opc.Ua.Client.Client.MoqDiscoveryClient = mockDiscoveryClient.Object;
            Automa.Opc.Ua.Client.Client.MoqSession = mockSession.Object; */
        }
    }
}
