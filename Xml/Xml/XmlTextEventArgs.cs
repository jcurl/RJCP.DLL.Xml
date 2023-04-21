namespace RJCP.Core.Xml
{
    using System.Xml;

    /// <summary>
    /// Object data when reading XML, called by delegates defined in <see cref="XmlTreeNode"/>.
    /// </summary>
    public class XmlTextEventArgs : XmlNodeEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlTextEventArgs"/> class.
        /// </summary>
        /// <param name="reader">The reader being used to read the XML.</param>
        /// <param name="xmlnsmgr">
        /// The namespace manager for global prefixes, or <see langword="null"/> if none defined.
        /// </param>
        /// <param name="treeSettings">The tree settings.</param>
        /// <param name="userObject">The user object data.</param>
        /// <param name="text">The text that was read from the reader.</param>
        public XmlTextEventArgs(XmlReader reader, XmlNamespaceManager xmlnsmgr, XmlTreeSettings treeSettings, object userObject, string text)
            : base(reader, xmlnsmgr, treeSettings, userObject)
        {
            Text = text;
        }

        /// <summary>
        /// Gets the text that is read from the reader.
        /// </summary>
        /// <value>The text that is read from the reader.</value>
        public string Text { get; private set; }
    }
}
