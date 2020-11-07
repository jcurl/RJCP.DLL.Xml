namespace RJCP.Core.Xml
{
    using System;
    using System.Xml;

    /// <summary>
    /// Object data when reading XML, called by delegates defined in <see cref="XmlTreeNode"/>.
    /// </summary>
    public class XmlNodeEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlNodeEventArgs"/> class.
        /// </summary>
        /// <param name="reader">The reader being used to read the XML.</param>
        /// <param name="xmlnsmgr">
        /// The namespace manager for global prefixes, or <see langword="null"/> if none defined.
        /// </param>
        /// <param name="treeSettings">The tree settings.</param>
        /// <param name="userObject">The user object data.</param>
        public XmlNodeEventArgs(XmlReader reader, XmlNamespaceManager xmlnsmgr, XmlTreeSettings treeSettings, object userObject)
        {
            Reader = reader;
            XmlNamespaceManager = xmlnsmgr;
            TreeSettings = treeSettings;
            UserObject = userObject;
        }

        /// <summary>
        /// Gets the reader being used to read the XML.
        /// </summary>
        /// <value>The reader being used to read the XML.</value>
        public XmlReader Reader { get; private set; }

        /// <summary>
        /// Gets the XML namespace manager for global prefixes.
        /// </summary>
        /// <value>
        /// The XML namespace manager for global prefixes. May be <see langword="null"/>, which indicates no namespaces
        /// in the global context is defined.
        /// </value>
        public XmlNamespaceManager XmlNamespaceManager { get; private set; }

        /// <summary>
        /// Gets the tree settings used when reading with <see cref="XmlTreeReader"/>.
        /// </summary>
        /// <value>The tree settings.</value>
        public XmlTreeSettings TreeSettings { get; private set; }

        /// <summary>
        /// User object data, that can be used to pass information along to other delegates in the tree.
        /// </summary>
        /// <value>User object data, that can be used to pass information along to other delegates in the tree.</value>
        public object UserObject { get; set; }
    }
}
