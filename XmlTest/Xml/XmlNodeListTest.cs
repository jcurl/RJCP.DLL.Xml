namespace RJCP.Core.Xml
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class XmlNodeListTest
    {
        [Test]
        public void FailOnAddRootNode()
        {
            XmlNodeList list = new();
            XmlTreeReader reader = new();

            // The Root node is also empty, which can't be added to the list
            Assert.That(() => { list.Add(reader); }, Throws.TypeOf<ArgumentException>());
            Assert.That(list, Is.Empty);
        }

        [Test]
        public void FailOnAddNull()
        {
            XmlNodeList list = new();
            Assert.That(() => { list.Add(null); }, Throws.TypeOf<ArgumentNullException>());
        }
    }
}
