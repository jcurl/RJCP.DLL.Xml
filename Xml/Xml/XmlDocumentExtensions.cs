namespace RJCP.Core.Xml
{
    using System;
    using System.Xml;

    /// <summary>
    /// Extension methods for common use cases within the <see cref="System.Xml"/> namespace.
    /// </summary>
    public static class XmlDocumentExtensions
    {
        /// <summary>
        /// Creates an <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="doc">The XML Document.</param>
        /// <param name="prefix">The prefix of the new element (if any). <see cref="string.Empty"/> and <see langword="null"/> are equivalent.</param>
        /// <param name="localName">The local name of the new element.</param>
        /// <param name="nsMgr">The <see cref="XmlNamespaceManager"/> to resolve the prefix.</param>
        /// <returns>The new <see cref="XmlElement"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="doc"/> is <see langword="null"/>;
        /// <para>- or -</para>
        /// <paramref name="localName"/> is <see langword="null"/>;
        /// <para>- or -</para>
        /// <paramref name="nsMgr"/> is <see langword="null"/>.
        /// </exception>
        public static XmlElement CreateElement(this XmlDocument doc, string prefix, string localName, XmlNamespaceManager nsMgr)
        {
            ThrowHelper.ThrowIfNull(doc);
            ThrowHelper.ThrowIfNull(localName);
            ThrowHelper.ThrowIfNull(nsMgr);

            return doc.CreateElement(prefix, localName, nsMgr.LookupNamespace(prefix ?? string.Empty));
        }
    }
}
