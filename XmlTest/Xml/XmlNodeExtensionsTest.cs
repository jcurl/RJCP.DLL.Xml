namespace RJCP.Core.Xml
{
    using System.IO;
    using System.Xml;
    using NUnit.Framework;
    using RJCP.CodeQuality.NUnitExtensions;

    [TestFixture]
    public class XmlNodeExtensionsTest
    {
        private readonly static string XmlFile = Path.Combine(Deploy.TestDirectory, "TestResources", "SampleXml.xml");

        [Test]
        public void InsertAfter()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlFile);

            XmlElement node = (XmlElement)doc.SelectSingleNode("/rootElement/devices/device[@name='headunit']");
            XmlElement newNode = doc.CreateElement("device");
            newNode.AppendAttribute("name", "display");
            XmlElement added = node.InsertAfter(newNode);
            Assert.That(added, Is.EqualTo(newNode));

            Assert.That(doc.OuterXml, Is.EqualTo("<rootElement><devices><device name=\"headunit\"><element>Text</element></device><device name=\"display\" /></devices></rootElement>"));
        }

        [Test]
        public void InsertAfterNullDoc()
        {
                XmlDocument doc = new XmlDocument();
                doc.Load(XmlFile);

                XmlElement node = null;
                XmlElement newNode = doc.CreateElement("device");
                newNode.AppendAttribute("name", "display");
                Assert.That(() => {
                    _ = node.InsertAfter(newNode);
                }, Throws.ArgumentNullException);
        }

        [Test]
        public void InsertAfterNullNode()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlFile);

            XmlElement node = (XmlElement)doc.SelectSingleNode("/rootElement/devices/device[@name='headunit']");
            Assert.That(() => {
                _ = node.InsertAfter(null);
            }, Throws.ArgumentNullException);
        }

        [Test]
        public void InsertAfterNoParent()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlFile);

            XmlElement node = doc.CreateElement("newNode");
            XmlElement newNode = doc.CreateElement("device");

            // `node` doesn't have a parent. So nothing to add after.
            Assert.That(node.InsertAfter(newNode), Is.Null);
        }

        [Test]
        public void InsertBefore()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlFile);

            XmlElement node = (XmlElement)doc.SelectSingleNode("/rootElement/devices/device[@name='headunit']");
            XmlElement newNode = doc.CreateElement("device");
            newNode.AppendAttribute("name", "display");
            XmlElement added = node.InsertBefore(newNode);
            Assert.That(added, Is.EqualTo(newNode));

            Assert.That(doc.OuterXml, Is.EqualTo("<rootElement><devices><device name=\"display\" /><device name=\"headunit\"><element>Text</element></device></devices></rootElement>"));
        }

        [Test]
        public void InsertBeforeNullDoc()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlFile);

            XmlElement node = null;
            XmlElement newNode = doc.CreateElement("device");
            newNode.AppendAttribute("name", "display");
            Assert.That(() => {
                _ = node.InsertBefore(newNode);
            }, Throws.ArgumentNullException);
        }

        [Test]
        public void InsertBeforeNullNode()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlFile);

            XmlElement node = (XmlElement)doc.SelectSingleNode("/rootElement/devices/device[@name='headunit']");
            Assert.That(() => {
                _ = node.InsertBefore(null);
            }, Throws.ArgumentNullException);
        }

        [Test]
        public void InsertBeforeNoParent()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlFile);

            XmlElement node = doc.CreateElement("newNode");
            XmlElement newNode = doc.CreateElement("device");

            // `node` doesn't have a parent. So nothing to add before.
            Assert.That(node.InsertBefore(newNode), Is.Null);
        }

        [Test]
        public void RemoveElement()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlFile);

            XmlElement node = (XmlElement)doc.SelectSingleNode("/rootElement/devices/device[@name='headunit']/element");
            XmlElement delNode = node.RemoveElement();
            Assert.That(delNode.Name, Is.EqualTo("element"));

            Assert.That(doc.OuterXml, Is.EqualTo("<rootElement><devices><device name=\"headunit\"></device></devices></rootElement>"));
        }

        [Test]
        public void RemoveElementNull()
        {
            XmlElement node = null;
            Assert.That(() => {
                _ = node.RemoveElement();
            }, Throws.ArgumentNullException);
        }

        [Test]
        public void RemoveElementNoParent()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlFile);

            XmlElement node = doc.CreateElement("newNode");
            Assert.That(node.RemoveElement(), Is.Null);
        }

        [Test]
        public void AppendAttribute()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlFile);

            XmlElement node = (XmlElement)doc.SelectSingleNode("/rootElement/devices/device[@name='headunit']");
            XmlAttribute attr = node.AppendAttribute("baud", "9600");
            Assert.That(attr.Name, Is.EqualTo("baud"));
            Assert.That(attr.Value, Is.EqualTo("9600"));

            Assert.That(doc.OuterXml, Is.EqualTo("<rootElement><devices><device name=\"headunit\" baud=\"9600\"><element>Text</element></device></devices></rootElement>"));
        }

        [Test]
        public void AppendAttributeNullNode()
        {
            XmlElement node = null;
            Assert.That(() => {
                _ = node.AppendAttribute("baud", "9600");
            }, Throws.ArgumentNullException);
        }

        [Test]
        public void AppendAttributeNullName()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlFile);

            XmlElement node = (XmlElement)doc.SelectSingleNode("/rootElement/devices/device[@name='headunit']");
            Assert.That(() => {
                _ = node.AppendAttribute(null, "9600");
            }, Throws.ArgumentNullException);
        }

        [Test]
        public void AppendAttributeNullValue()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlFile);

            XmlElement node = (XmlElement)doc.SelectSingleNode("/rootElement/devices/device[@name='headunit']");
            XmlAttribute attr = node.AppendAttribute("baud", null);
            Assert.That(attr.Name, Is.EqualTo("baud"));
            Assert.That(attr.Value, Is.EqualTo(string.Empty));

            Assert.That(doc.OuterXml, Is.EqualTo("<rootElement><devices><device name=\"headunit\" baud=\"\"><element>Text</element></device></devices></rootElement>"));
        }
    }
}
