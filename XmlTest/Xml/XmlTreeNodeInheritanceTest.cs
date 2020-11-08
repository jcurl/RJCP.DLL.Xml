namespace RJCP.Core.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using NUnit.Framework;
    using CodeQuality.NUnitExtensions;

    [TestFixture]
    public class XmlTreeNodeInheritanceTest
    {
        private static DateTime GetDateTime(string value)
        {
            return DateTime.ParseExact(value, "u", CultureInfo.InvariantCulture);
        }

        private static long GetLong(string value)
        {
            if (value.StartsWith("0x")) {
                return long.Parse(value.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            }

            return long.Parse(value, CultureInfo.InvariantCulture);
        }

        private class Volume
        {
            public DateTime RecordedTime { get; set; }

            public IList<VolumeInfo> VolumeInfo { get; } = new List<VolumeInfo>();
        }


        private class VolumeInfo
        {
            public DateTime RecordedTime { get; set; }

            public IList<VolumeExtent> VolumeExtent { get; } = new List<VolumeExtent>();
        }

        private class VolumeExtent
        {
            public string Device { get; set; }

            public long Offset { get; set; }

            public long Length { get; set; }
        }

        private class VolumeXmlTreeNode : XmlTreeNode
        {
            private readonly IList<Volume> m_Volumes;

            public VolumeXmlTreeNode(IList<Volume> volumes) : base("volume")
            {
                m_Volumes = volumes;
                Nodes.AddRange(new XmlNodeList() {
                    new XmlTreeNode("recordedtime") {
                        ProcessTextElement = (n, e) => { Volume.RecordedTime = GetDateTime(e.Reader.Value); }
                    },
                    new VolumeInfoXmlTreeNode(this)
                });
            }

            public Volume Volume { get; private set; }

            protected override void OnProcessElement(XmlNodeEventArgs args)
            {
                Volume = new Volume();
            }

            protected override void OnProcessEndElement(XmlNodeEventArgs args)
            {
                if (Volume.RecordedTime.Ticks == 0) {
                    args.Reader.Throw("Element 'recordedtime' missing");
                    return;
                }

                m_Volumes.Add(Volume);
            }
        }

        private class VolumeInfoXmlTreeNode : XmlTreeNode
        {
            private readonly VolumeXmlTreeNode m_Parent;

            public VolumeInfoXmlTreeNode(VolumeXmlTreeNode parent) : base("volumeinfo")
            {
                m_Parent = parent;
                Nodes.AddRange(new XmlNodeList() {
                    new XmlTreeNode("recordedtime") {
                        ProcessTextElement = (n, e) => { VolumeInfo.RecordedTime = GetDateTime(e.Reader.Value); }
                    },
                    new VolumeExtentXmlTreeNode(this)
                });
            }

            public VolumeInfo VolumeInfo { get; private set; }

            protected override void OnProcessElement(XmlNodeEventArgs args)
            {
                VolumeInfo = new VolumeInfo();
            }

            protected override void OnProcessEndElement(XmlNodeEventArgs args)
            {
                if (VolumeInfo.RecordedTime.Ticks == 0) {
                    args.Reader.Throw("Element 'recordedtime' missing");
                    return;
                }

                m_Parent.Volume.VolumeInfo.Add(VolumeInfo);
            }
        }

        private class VolumeExtentXmlTreeNode : XmlTreeNode
        {
            private readonly VolumeInfoXmlTreeNode m_Parent;

            public VolumeExtentXmlTreeNode(VolumeInfoXmlTreeNode parent) : base("volumeextent")
            {
                m_Parent = parent;
                Nodes.AddRange(new XmlNodeList() {
                    new XmlTreeNode("device") {
                        ProcessTextElement = (n, e) => { VolumeExtent.Device = e.Reader.Value; }
                    },
                    new XmlTreeNode("offset") {
                        ProcessTextElement = (n, e) => { VolumeExtent.Offset = GetLong(e.Reader.Value); }
                    },
                    new XmlTreeNode("length") {
                        ProcessTextElement = (n, e) => { VolumeExtent.Length = GetLong(e.Reader.Value); }
                    }
                });
            }

            public VolumeExtent VolumeExtent { get; private set; }

            protected override void OnProcessElement(XmlNodeEventArgs args)
            {
                VolumeExtent = new VolumeExtent();
            }

            protected override void OnProcessEndElement(XmlNodeEventArgs args)
            {
                if (VolumeExtent.Device == null) {
                    args.Reader.Throw("Element 'device' missing");
                    return;
                }

                m_Parent.VolumeInfo.VolumeExtent.Add(VolumeExtent);
            }
        }

        private static IList<Volume> LoadDisks(string fileName)
        {
            List<Volume> volumes = new List<Volume>();

            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("disks") {
                        Nodes = {
                            new VolumeXmlTreeNode(volumes)
                        }
                    }
                }
            };

            reader.Read(fileName);
            return volumes;
        }

        [TestCase("ComplexStructureP1.xml")]
        [TestCase("ComplexStructureP2.xml")]
        [TestCase("ComplexStructureP3.xml")]
        public void ReadComplexXmlWithInheritance(string inputXml)
        {
            string fileName = Path.Combine(Deploy.TestDirectory, "TestResources", inputXml);

            IList<Volume> volumes = LoadDisks(fileName);
            Assert.That(volumes.Count, Is.EqualTo(1));
            Assert.That(volumes[0].VolumeInfo.Count, Is.EqualTo(1));
            Assert.That(volumes[0].VolumeInfo[0].VolumeExtent.Count, Is.EqualTo(2));
            Assert.That(volumes[0].VolumeInfo[0].VolumeExtent[0].Device, Is.EqualTo(@"\\.\PhysicalDrive2"));
            Assert.That(volumes[0].VolumeInfo[0].VolumeExtent[1].Device, Is.EqualTo(@"\\.\PhysicalDrive1"));
        }

        [TestCase("ComplexStructureE1.xml")]
        [TestCase("ComplexStructureE2.xml")]
        [TestCase("ComplexStructureE3.xml")]
        public void ReadComplexXmlWithException(string inputXml)
        {
            string fileName = Path.Combine(Deploy.TestDirectory, "TestResources", inputXml);

            Assert.That(() => { LoadDisks(fileName); }, Throws.TypeOf<XmlException>());
        }
    }
}
