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
            XmlNodeList list = new XmlNodeList();
            XmlTreeReader reader = new XmlTreeReader();

            // The Root node is also empty, which can't be added to the list
            Assert.That(() => { list.Add(reader); }, Throws.TypeOf<ArgumentException>());
            Assert.That(list.Count, Is.EqualTo(0));
        }

        [Test]
        public void FailOnAddNull()
        {
            XmlNodeList list = new XmlNodeList();
            Assert.That(() => { list.Add(null); }, Throws.TypeOf<ArgumentNullException>());
        }
    }
}
