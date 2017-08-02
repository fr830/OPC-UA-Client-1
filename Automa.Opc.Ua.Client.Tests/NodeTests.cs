using Automa.Opc.Ua.Client.Models;
using NUnit.Framework;

namespace Automa.Opc.Ua.Client.Tests
{
    [TestFixture]
    public class NodeTests
    {
        [Test]
        public void Tag()
        {
            var obj = new Node();
            const string val = "Tag";
            obj.Tag = val;
            Assert.That(obj.Tag, Is.EqualTo(val));
        }

        [Test]
        public void DisplayName()
        {
            var obj = new Node();
            const string val = "DisplayName";
            obj.DisplayName = val;
            Assert.That(obj.DisplayName, Is.EqualTo(val));
        }
    }
}
