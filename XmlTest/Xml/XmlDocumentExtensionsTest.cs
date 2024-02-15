namespace RJCP.Core.Xml
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using NUnit.Framework;
    using RJCP.CodeQuality.NUnitExtensions;

    [TestFixture]
    public class XmlDocumentExtensionsTest
    {
        private readonly static string XmlFile = Path.Combine(Deploy.TestDirectory, "TestResources", "SampleXmlNs.xml");

        [Test]
        public void CreateElementWithNs()
        {
            XmlDocument doc = new();
            doc.Load(XmlFile);

            XmlNamespaceManager nsmgr = new(doc.NameTable);
            nsmgr.AddNamespace("", "");
            nsmgr.AddNamespace("a", "urn:one");
            nsmgr.AddNamespace("b", "urn:two");

            XmlElement node = (XmlElement)doc.SelectSingleNode("/rootElement/a:devices/a:device[@name='headunit']/b:element", nsmgr);
            XmlElement newNode = doc.CreateElement("b", "extension", nsmgr);
            node.InsertAfter(newNode);

            Assert.That(doc.OuterXml, Is.EqualTo("<rootElement xmlns:a=\"urn:one\" xmlns:b=\"urn:two\"><a:devices><a:device name=\"headunit\"><b:element>Text</b:element><b:extension /></a:device></a:devices></rootElement>"));
        }

        [Test]
        public void CreateElementWithNsOther()
        {
            XmlDocument doc = new();
            doc.Load(XmlFile);

            XmlNamespaceManager nsmgr = new(doc.NameTable);
            nsmgr.AddNamespace("", "");
            nsmgr.AddNamespace("x", "urn:one");
            nsmgr.AddNamespace("y", "urn:two");

            XmlElement node = (XmlElement)doc.SelectSingleNode("/rootElement/x:devices/x:device[@name='headunit']/y:element", nsmgr);
            XmlElement newNode = doc.CreateElement("y", "extension", nsmgr);
            node.InsertAfter(newNode);

            Assert.That(doc.OuterXml, Is.EqualTo("<rootElement xmlns:a=\"urn:one\" xmlns:b=\"urn:two\"><a:devices><a:device name=\"headunit\"><b:element>Text</b:element><y:extension xmlns:y=\"urn:two\" /></a:device></a:devices></rootElement>"));
        }

        [Test]
        public void CreateElementWithNsDefault()
        {
            XmlDocument doc = new();
            doc.Load(XmlFile);

            // Assume we don't know what the namespace is when we search, just the URNs. This is good enough to search.
            XmlNamespaceManager nsmgr1 = new(doc.NameTable);
            nsmgr1.AddNamespace("", "");
            nsmgr1.AddNamespace("x", "urn:one");
            nsmgr1.AddNamespace("y", "urn:two");
            XmlElement node = (XmlElement)doc.SelectSingleNode("/rootElement/x:devices/x:device[@name='headunit']/y:element", nsmgr1);

            // Add the new element, using the same prefix as in the document. The available namespace is dependent on
            // the location in the tree.
            string a = node.GetPrefixOfNamespace("urn:one");
            string b = node.GetPrefixOfNamespace("urn:two");
            XmlNamespaceManager nsmgr2 = new(doc.NameTable);
            nsmgr2.AddNamespace("", "");
            nsmgr2.AddNamespace(a, "urn:one");
            nsmgr2.AddNamespace(b, "urn:two");
            XmlElement newNode = doc.CreateElement(b, "extension", nsmgr2);
            node.InsertAfter(newNode);

            Assert.That(doc.OuterXml, Is.EqualTo("<rootElement xmlns:a=\"urn:one\" xmlns:b=\"urn:two\"><a:devices><a:device name=\"headunit\"><b:element>Text</b:element><b:extension /></a:device></a:devices></rootElement>"));
        }

        [Test]
        public void CreateElementNullDoc()
        {
            XmlDocument doc = null;
            XmlNamespaceManager nsmgr = new(new NameTable());
            nsmgr.AddNamespace("", "");
            nsmgr.AddNamespace("a", "urn:one");
            nsmgr.AddNamespace("b", "urn:two");

            Assert.That(() => {
                _ = doc.CreateElement("b", "extension", nsmgr);
            }, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateElementNullLocalName()
        {
            XmlDocument doc = new();
            doc.Load(XmlFile);

            XmlNamespaceManager nsmgr = new(doc.NameTable);
            nsmgr.AddNamespace("", "");
            nsmgr.AddNamespace("a", "urn:one");
            nsmgr.AddNamespace("b", "urn:two");

            Assert.That(() => {
                _ = doc.CreateElement("b", null, nsmgr);
            }, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateElementNullNsMgr()
        {
            XmlDocument doc = new();
            doc.Load(XmlFile);

            Assert.That(() => {
                XmlNamespaceManager nsmgr = null;
                _ = doc.CreateElement("b", "extension", nsmgr);
            }, Throws.ArgumentNullException);
        }

        [TestCase(null)]
        [TestCase("")]
        public void CreateElementNullPrefix(string prefix)
        {
            XmlDocument doc = new();
            doc.Load(XmlFile);

            XmlNamespaceManager nsmgr = new(doc.NameTable);
            nsmgr.AddNamespace("", "");
            nsmgr.AddNamespace("a", "urn:one");
            nsmgr.AddNamespace("b", "urn:two");

            XmlElement node = (XmlElement)doc.SelectSingleNode("/rootElement/a:devices/a:device[@name='headunit']/b:element", nsmgr);
            XmlElement newNode = doc.CreateElement(prefix, "extension", nsmgr);
            node.InsertAfter(newNode);

            Assert.That(doc.OuterXml, Is.EqualTo("<rootElement xmlns:a=\"urn:one\" xmlns:b=\"urn:two\"><a:devices><a:device name=\"headunit\"><b:element>Text</b:element><extension /></a:device></a:devices></rootElement>"));
        }
    }
}
