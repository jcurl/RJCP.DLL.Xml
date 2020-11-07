namespace RJCP.Core.Xml
{
    using System;
    using Collections.Generic;

    /// <summary>
    /// Provides a list of <see cref="XmlTreeNode"/>.
    /// </summary>
    /// <remarks>
    /// When adding nodes to this list, the <see cref="ArgumentException"/> shall be raised if the name of the tag
    /// through the <see cref="XmlTreeNode.Name"/> is whitespace or <see cref="String.Empty"/>.
    /// </remarks>
    public class XmlNodeList : NamedItemCollection<XmlTreeNode>
    {
        /// <summary>
        /// A derived class can override this method to perform additional checks on the item before it is added.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <exception cref="ArgumentException">Item name may not be empty or whitespace</exception>
        protected override void OnAdd(XmlTreeNode item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrWhiteSpace(item.Name))
                throw new ArgumentException("Item name may not be empty or whitespace");
        }
    }
}
