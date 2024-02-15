namespace RJCP.Core.Xml
{
    using System.Xml;

    internal class XmlLineInfo : IXmlLineInfo
    {
        public XmlLineInfo(IXmlLineInfo xmlObject)
        {
            if (xmlObject is null) return;
            m_HasLineInfo = xmlObject.HasLineInfo();
            if (m_HasLineInfo) {
                LineNumber = xmlObject.LineNumber;
                LinePosition = xmlObject.LinePosition;
            }
        }

        public int LineNumber { get; private set; }

        public int LinePosition { get; private set; }

        private readonly bool m_HasLineInfo;

        public bool HasLineInfo() { return m_HasLineInfo; }
    }
}
