using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Automa.Opc.Ua.Client.Tests
{
    [TestFixture]
    public class ChangeEventArgsTests
    {

        [Test]
        public void Tag()
        {
            var obj = new ChangeEventArgs();
            const string val = "Tag";
            obj.Tag = val;
            Assert.That(obj.Tag, Is.EqualTo(val));
        }

        [Test]
        public void Values()
        {
            var obj = new ChangeEventArgs();
            var val = new List<object>
            {
                "obj1",
                2
            };
            obj.Values = val;
            Assert.That(obj.Values, Is.EqualTo(val));
        }
    }
}
