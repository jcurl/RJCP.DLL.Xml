namespace RJCP.Core.Xml
{
    using System.Xml;

    /// <summary>
    /// Settings for modifying behavior of reading with <see cref="XmlTreeReader"/>.
    /// </summary>
    /// <remarks>
    /// The settings provided is applicable for reading the tree.
    /// </remarks>
    public class XmlTreeSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlTreeSettings"/> class with default settings.
        /// </summary>
        public XmlTreeSettings() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlTreeSettings"/> class.
        /// </summary>
        /// <param name="settings">The settings to copy from.</param>
        public XmlTreeSettings(XmlTreeSettings settings)
        {
            ThrowOnUnknownElement = settings.ThrowOnUnknownElement;
            ThrowOnUnhandledText = settings.ThrowOnUnhandledText;
        }

        /// <summary>
        /// Configure if an exception should be thrown if an unknown element is parsed.
        /// </summary>
        /// <value>
        /// Set to <see langword="true"/> if an <see cref="XmlException"/> should be raised if an unknown
        /// element is found and the <see cref="XmlTreeNode.ProcessUnknownElement"/> is not defined. Set to
        /// <see langword="false"/> for default behavior where the element is ignored.
        /// </value>
        public bool ThrowOnUnknownElement { get; set; }

        /// <summary>
        /// Configure if an exception should be thrown if a text element is parsed and not handled.
        /// </summary>
        /// <value>
        /// Set to <see langword="true"/> if an <see cref="XmlException"/> should be raised if a node has a text
        /// element and there is no <see cref="XmlTreeNode.ProcessTextElement"/> handler defined. Set to
        /// <see langword="false"/> to ignore the text element which is default behavior.
        /// </value>
        public bool ThrowOnUnhandledText { get; set; } = true;
    }
}
