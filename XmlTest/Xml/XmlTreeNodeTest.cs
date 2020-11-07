namespace RJCP.Core.Xml
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class XmlTreeNodeTest
    {
        [Test]
        public void XmlNodeNull()
        {
            Assert.That(() => { _ = new XmlTreeNode(null); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void XmlNodeNullPrefix()
        {
            Assert.That(() => { _ = new XmlTreeNode(null, "node"); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void XmlNodeNullLocalName()
        {
            Assert.That(() => { _ = new XmlTreeNode("fx", null); }, Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(":")]
        [TestCase("fx:")]
        [TestCase(":name")]
        [TestCase(" : ")]
        [TestCase(": ")]
        [TestCase(" :")]
        [TestCase("fx:name:2")]
        [TestCase("fx:name:")]
        [TestCase(":fx:name:")]
        [TestCase(":fx:name")]
        public void XmlNodeInvalid(string nodeName)
        {
            Assert.That(() => { _ = new XmlTreeNode(nodeName); }, Throws.TypeOf<ArgumentException>());
        }

        [TestCase("", "")]
        [TestCase(" ", "local")]
        [TestCase("fx", " ")]
        [TestCase(" ", " ")]
        [TestCase(":", "")]
        [TestCase(":", "local")]
        [TestCase("", ":")]
        [TestCase("prefix", ":")]
        [TestCase("fx:", "")]
        [TestCase("fx:", "local")]
        [TestCase("fx", "local:")]
        [TestCase("fx", ":local")]
        [TestCase("fx", "lo:cal")]
        public void XmlNodePrefixInvalid(string prefix, string localName)
        {
            Assert.That(() => { _ = new XmlTreeNode(prefix, localName); }, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void XmlNodeNormal()
        {
            XmlTreeNode node = new XmlTreeNode("node");
            Assert.That(node.Name, Is.EqualTo("node"));
            Assert.That(node.Prefix, Is.Empty);
            Assert.That(node.LocalName, Is.EqualTo("node"));
        }

        [Test]
        public void XmlNodeNormalWithPrefix2Char()
        {
            XmlTreeNode node = new XmlTreeNode("fx:node");
            Assert.That(node.Name, Is.EqualTo("fx:node"));
            Assert.That(node.Prefix, Is.EqualTo("fx"));
            Assert.That(node.LocalName, Is.EqualTo("node"));
        }

        [Test]
        public void XmlNodeNormalWithPrefix1Char()
        {
            XmlTreeNode node = new XmlTreeNode("x:node");
            Assert.That(node.Name, Is.EqualTo("x:node"));
            Assert.That(node.Prefix, Is.EqualTo("x"));
            Assert.That(node.LocalName, Is.EqualTo("node"));
        }

        [Test]
        public void XmlNodeNormalWithLocal2Char()
        {
            XmlTreeNode node = new XmlTreeNode("x:nd");
            Assert.That(node.Name, Is.EqualTo("x:nd"));
            Assert.That(node.Prefix, Is.EqualTo("x"));
            Assert.That(node.LocalName, Is.EqualTo("nd"));
        }

        [Test]
        public void XmlNodeNormalWithLocal1Char()
        {
            XmlTreeNode node = new XmlTreeNode("x:n");
            Assert.That(node.Name, Is.EqualTo("x:n"));
            Assert.That(node.Prefix, Is.EqualTo("x"));
            Assert.That(node.LocalName, Is.EqualTo("n"));
        }

        [Test]
        public void XmlNodePrefix()
        {
            XmlTreeNode node = new XmlTreeNode("fx", "node");
            Assert.That(node.Name, Is.EqualTo("fx:node"));
            Assert.That(node.Prefix, Is.EqualTo("fx"));
            Assert.That(node.LocalName, Is.EqualTo("node"));
        }
    }
}
