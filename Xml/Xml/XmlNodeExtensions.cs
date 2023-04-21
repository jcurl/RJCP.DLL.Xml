namespace RJCP.Core.Xml
{
    using System;
    using System.Xml;

    /// <summary>
    /// Extension methods for common use cases within the <see cref="System.Xml"/> namespace.
    /// </summary>
    public static class XmlNodeExtensions
    {
        /// <summary>
        /// Inserts the child immediately after the reference node.
        /// </summary>
        /// <param name="refChild">The reference child.</param>
        /// <param name="newChild">The new child.</param>
        /// <returns>The node being inserted.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="refChild"/> is <see langword="null"/>;
        /// <para>- or -</para>
        /// <paramref name="newChild"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// This node is of a type that does not allow child nodes of the type of the <paramref name="newChild"/> node;
        /// <para>- or -</para>
        /// The <paramref name="newChild"/> is an ancestor of this node.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="newChild"/> was created from a different document than the one that created this node.
        /// <para>- or -</para>
        ///This node is read-only.
        /// </exception>
        public static XmlElement InsertAfter(this XmlElement refChild, XmlElement newChild)
        {
            if (refChild == null) throw new ArgumentNullException(nameof(refChild));
            if (newChild == null) throw new ArgumentNullException(nameof(newChild));
            if (refChild.ParentNode == null) return null;

            return (XmlElement)refChild.ParentNode.InsertAfter(newChild, refChild);
        }

        /// <summary>
        /// Inserts the child immediately before the reference node.
        /// </summary>
        /// <param name="refChild">The reference child.</param>
        /// <param name="newChild">The new child.</param>
        /// <returns>The node being inserted.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="refChild"/> is <see langword="null"/>;
        /// <para>- or -</para>
        /// <paramref name="newChild"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// This node is of a type that does not allow child nodes of the type of the <paramref name="newChild"/> node;
        /// <para>- or -</para>
        /// The <paramref name="newChild"/> is an ancestor of this node.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="newChild"/> was created from a different document than the one that created this node.
        /// <para>- or -</para>
        /// This node is read-only.
        /// </exception>
        public static XmlElement InsertBefore(this XmlElement refChild, XmlElement newChild)
        {
            if (refChild == null) throw new ArgumentNullException(nameof(refChild));
            if (newChild == null) throw new ArgumentNullException(nameof(newChild));
            if (refChild.ParentNode == null) return null;

            return (XmlElement)refChild.ParentNode.InsertBefore(newChild, refChild);
        }

        /// <summary>
        /// Removes the specified old child.
        /// </summary>
        /// <param name="oldChild">The old child node.</param>
        /// <returns>The node removed. Or <see langword="null"/> if the node has no parent.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="oldChild"/> is <see langword="null"/>.</exception>
        public static XmlElement RemoveElement(this XmlElement oldChild)
        {
            if (oldChild == null) throw new ArgumentNullException(nameof(oldChild));
            if (oldChild.ParentNode == null) return null;

            return (XmlElement)oldChild.ParentNode.RemoveChild(oldChild);
        }

        /// <summary>
        /// Appends the attribute to the end of the attributes for the current node..
        /// </summary>
        /// <param name="node">The node to append the attribute to.</param>
        /// <param name="attribute">The attribute name that should be appended.</param>
        /// <param name="value">The value of the attribute that should be assigned.</param>
        /// <returns>The <see cref="XmlAttribute"/> that was created and appended.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="node"/> is <see langword="null"/>;
        /// <para>- or -</para>
        /// <paramref name="attribute"/> is <see langword="null"/>.
        /// </exception>
        public static XmlAttribute AppendAttribute(this XmlElement node, string attribute, string value)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (attribute == null) throw new ArgumentNullException(nameof(attribute));

            XmlAttribute xmlAttr = node.OwnerDocument.CreateAttribute(attribute);
            xmlAttr.Value = value ?? string.Empty;
            node.Attributes.Append(xmlAttr);
            return xmlAttr;
        }
    }
}
