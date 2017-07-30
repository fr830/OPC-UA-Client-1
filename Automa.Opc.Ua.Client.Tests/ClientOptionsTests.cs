using NUnit.Framework;

namespace Automa.Opc.Ua.Client.Tests
{
    [TestFixture]
    public class ClientOptionsTests
    {
        [Test]
        public void TestMethod1()
        {
            var co = new ClientOptions();
            const string applicationName = "MyApplicationName";
            co.ApplicationName = applicationName;
            Assert.That(co.ApplicationName, Is.EqualTo(applicationName));
        }
    }
}
