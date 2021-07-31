namespace RJCP.Core.Xml
{
    using System;
    using System.IO;
    using System.Xml;
    using CodeQuality.NUnitExtensions;
    using NUnit.Framework;

    [TestFixture]
    public class XmlExtensionsTest
    {
        private class TestXmlLineInfo : IXmlLineInfo
        {
            public TestXmlLineInfo() { }

            public TestXmlLineInfo(int lineNumber, int linePosition)
            {
                m_HasLineInfo = true;
                LineNumber = lineNumber;
                LinePosition = linePosition;
            }

            public int LineNumber { get; private set; }

            public int LinePosition { get; private set; }

            private readonly bool m_HasLineInfo;

            public bool HasLineInfo() { return m_HasLineInfo; }
        }

        private void ReadElement(Action<XmlReader> testAction)
        {
            string path = Path.Combine(Deploy.TestDirectory, "TestResources", "SampleXml.xml");

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (XmlTextReader xmlReader = new XmlTextReader(fs)) {
                while (xmlReader.Read()) {
                    switch (xmlReader.NodeType) {
                    case XmlNodeType.Element:
                        if (xmlReader.Name.Equals("element")) {
                            testAction(xmlReader);
                        }
                        break;
                    }
                }
            }
        }

        [Test]
        public void GetPosition()
        {
            IXmlLineInfo line = null;
            IXmlLineInfo node = null;
            ReadElement((r) => {
                line = r.GetPosition();
                node = (IXmlLineInfo)r;
            });

            Assert.That(line.HasLineInfo(), Is.True);
            Assert.That(line.LineNumber, Is.EqualTo(4));
            Assert.That(line.LinePosition, Is.EqualTo(8));

            // Just to show that if 'node' is read, it will change during the enumeration, but 'line' doesn't.
            Console.WriteLine("Node after loop: HasLineInfo: {0}; LineNumber: {1}; LinePosition: {2}",
                node.HasLineInfo(), node.LineNumber, node.LinePosition);
            Console.WriteLine("Line after loop: HasLineInfo: {0}; LineNumber: {1}; LinePosition: {2}",
                line.HasLineInfo(), line.LineNumber, line.LinePosition);
        }

        [Test]
        public void ThrowSimpleMessage()
        {
            Assert.That(
                () => {
                    ReadElement((r) => {
                        r.Throw("Test Message");
                    });
                },
                Throws.TypeOf<XmlException>()
                    .And.Message.StartsWith("Test Message")
                    .And.Message.Length.GreaterThan(20)
                    .And.With.Property("LineNumber").EqualTo(4)
                    .And.With.Property("LinePosition").EqualTo(8)
            );
        }

        [Test]
        public void ThrowFormatMessage()
        {
            Assert.That(
                () => {
                    ReadElement((r) => {
                        r.Throw("Test Message {0}", "foo");
                    });
                },
                Throws.TypeOf<XmlException>()
                    .And.Message.StartsWith("Test Message foo")
                    .And.Message.Length.GreaterThan(20)
                    .And.With.Property("LineNumber").EqualTo(4)
                    .And.With.Property("LinePosition").EqualTo(8)
            );
        }

        [Test]
        public void ThrowSimpleMessageWithInnerException()
        {
            Assert.That(
                () => {
                    ReadElement((r) => {
                        try {
                            throw new InvalidOperationException();
                        } catch (Exception ex) {
                            r.Throw("Test Message", ex);
                        }
                    });
                },
                Throws.TypeOf<XmlException>()
                    .And.Message.StartsWith("Test Message")
                    .And.Message.Length.GreaterThan(20)
                    .And.With.Property("LineNumber").EqualTo(4)
                    .And.With.Property("LinePosition").EqualTo(8)
                    .And.InnerException.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void ThrowLinePosSimpleMessage()
        {
            Assert.That(
                () => {
                    ReadElement((r) => {
                        r.Throw(r.GetPosition(), "Test Message");
                    });
                },
                Throws.TypeOf<XmlException>()
                    .And.Message.StartsWith("Test Message")
                    .And.Message.Length.GreaterThan(20)
                    .And.With.Property("LineNumber").EqualTo(4)
                    .And.With.Property("LinePosition").EqualTo(8)
            );
        }

        [Test]
        public void ThrowLinePosFormatMessage()
        {
            Assert.That(
                () => {
                    ReadElement((r) => {
                        r.Throw(r.GetPosition(), "Test Message {0}", "foo");
                    });
                },
                Throws.TypeOf<XmlException>()
                    .And.Message.StartsWith("Test Message foo")
                    .And.Message.Length.GreaterThan(20)
                    .And.With.Property("LineNumber").EqualTo(4)
                    .And.With.Property("LinePosition").EqualTo(8)
            );
        }

        [Test]
        public void ThrowLinePosWithInnerException()
        {
            Assert.That(
                () => {
                    ReadElement((r) => {
                        try {
                            throw new InvalidOperationException();
                        } catch (Exception ex) {
                            r.Throw(r.GetPosition(), "Test Message", ex);
                        }
                    });
                },
                Throws.TypeOf<XmlException>()
                    .And.Message.StartsWith("Test Message")
                    .And.Message.Length.GreaterThan(20)
                    .And.With.Property("LineNumber").EqualTo(4)
                    .And.With.Property("LinePosition").EqualTo(8)
                    .And.InnerException.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void ThrowNoLinePosSimpleMessage()
        {
            Assert.That(
                () => {
                    ReadElement((r) => {
                        r.Throw(new TestXmlLineInfo(), "Test Message");
                    });
                },
                Throws.TypeOf<XmlException>()
                    .And.Message.StartsWith("Test Message")
                    .And.Message.Length.EqualTo(12)
                    .And.With.Property("LineNumber").EqualTo(0)
                    .And.With.Property("LinePosition").EqualTo(0)
            );
        }

        [Test]
        public void ThrowNoLinePosFormatMessage()
        {
            Assert.That(
                () => {
                    ReadElement((r) => {
                        r.Throw(new TestXmlLineInfo(), "Test Message {0}", "foo");
                    });
                },
                Throws.TypeOf<XmlException>()
                    .And.Message.StartsWith("Test Message foo")
                    .And.Message.Length.EqualTo(16)
                    .And.With.Property("LineNumber").EqualTo(0)
                    .And.With.Property("LinePosition").EqualTo(0)
            );
        }

        [Test]
        public void ThrowNoLinePosWithInnerException()
        {
            Assert.That(
                () => {
                    ReadElement((r) => {
                        try {
                            throw new InvalidOperationException();
                        } catch (Exception ex) {
                            r.Throw(new TestXmlLineInfo(), "Test Message", ex);
                        }
                    });
                },
                Throws.TypeOf<XmlException>()
                    .And.Message.StartsWith("Test Message")
                    .And.Message.Length.EqualTo(12)
                    .And.With.Property("LineNumber").EqualTo(0)
                    .And.With.Property("LinePosition").EqualTo(0)
                    .And.InnerException.TypeOf<InvalidOperationException>()
            );
        }

        [Test]
        public void ThrowDifferentLinePosSimpleMessage()
        {
            Assert.That(
                () => {
                    ReadElement((r) => {
                        r.Throw(new TestXmlLineInfo(5, 6), "Test Message");
                    });
                },
                Throws.TypeOf<XmlException>()
                    .And.Message.StartsWith("Test Message")
                    .And.Message.Length.GreaterThan(20)
                    .And.With.Property("LineNumber").EqualTo(5)
                    .And.With.Property("LinePosition").EqualTo(6)
            );
        }

        [Test]
        public void ThrowDifferentLinePosFormatMessage()
        {
            Assert.That(
                () => {
                    ReadElement((r) => {
                        r.Throw(new TestXmlLineInfo(5, 6), "Test Message {0}", "foo");
                    });
                },
                Throws.TypeOf<XmlException>()
                    .And.Message.StartsWith("Test Message foo")
                    .And.Message.Length.GreaterThan(20)
                    .And.With.Property("LineNumber").EqualTo(5)
                    .And.With.Property("LinePosition").EqualTo(6)
            );
        }

        [Test]
        public void ThrowDifferentLinePosWithInnerException()
        {
            Assert.That(
                () => {
                    ReadElement((r) => {
                        try {
                            throw new InvalidOperationException();
                        } catch (Exception ex) {
                            r.Throw(new TestXmlLineInfo(5, 6), "Test Message", ex);
                        }
                    });
                },
                Throws.TypeOf<XmlException>()
                    .And.Message.StartsWith("Test Message")
                    .And.Message.Length.GreaterThan(20)
                    .And.With.Property("LineNumber").EqualTo(5)
                    .And.With.Property("LinePosition").EqualTo(6)
                    .And.InnerException.TypeOf<InvalidOperationException>()
            );
        }

        [TestCase(null, "")]
        [TestCase("", "")]
        [TestCase("0123456789@ABCDEFGHIJKLMNOPQRSTUVWXYZ €100.99.", "0123456789@ABCDEFGHIJKLMNOPQRSTUVWXYZ €100.99.")]
        [TestCase("1234567890abcdefghijklmnopqrstuvwxyz", "1234567890abcdefghijklmnopqrstuvwxyz")]
        [TestCase("\u0008ab", "[0x08]ab")]
        [TestCase("a\u0009z", "a\u0009z")]
        [TestCase("\u000B\u000C", "[0x0B][0x0C]")]
        [TestCase("\u000E\u000F\u001F\u0020", "[0x0E][0x0F][0x1F] ")]
        [TestCase("\uD800\uDFFF\uE000", "[0xD800][0xDFFF]\uE000")]
        [TestCase("\uFFFE\uFFFF", "[0xFFFE][0xFFFF]")]
        [TestCase("\uD840\uDC00", "[0xD840][0xDC00]",
            Description = "Unicode point 0x20000 (Chinese character) - each UTF-16 character is found in the invalid XML 1.0 interval [0xD800, 0xE000)")]
        public void SanitizeXml10(string input, string output)
        {
            Assert.That(XmlExtensions.SanitizeXml10(input), Is.EqualTo(output));
        }
    }
}
