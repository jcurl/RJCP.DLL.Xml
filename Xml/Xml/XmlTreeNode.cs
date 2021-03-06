namespace RJCP.Core.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml;
    using Collections;

    /// <summary>
    /// XML Processing Delegate.
    /// </summary>
    /// <param name="node">The node for which the delegate is relevant.</param>
    /// <param name="args">The <see cref="XmlNodeEventArgs"/> instance containing the event data.</param>
    public delegate void XmlProcessing(XmlTreeNode node, XmlNodeEventArgs args);

    /// <summary>
    /// A node in the XML Tree, with the root given by <see cref="XmlTreeReader"/>.
    /// </summary>
    [DebuggerDisplay("{Name}, Nodes={Nodes.Count}")]
    public class XmlTreeNode : INamedItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlTreeNode"/> class. This is for a root tree only.
        /// </summary>
        /// <remarks>
        /// Using this constructor instantiates a node that has no name. This node cannot be assigned to the
        /// <see cref="XmlNodeList"/>, making it the root node.
        /// </remarks>
        protected XmlTreeNode()
        {
            // Indicates this is the root node. It can't be added to any list.
            Name = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlTreeNode"/> class for specific element name.
        /// </summary>
        /// <param name="nodeName">
        /// Name of the node which matches the <see cref="XmlNodeType.Element"/><see cref="XmlReader.Name"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="nodeName"/> may not be <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="nodeName"/> may not be an empty string, or is invalid.
        /// </exception>
        public XmlTreeNode(string nodeName)
        {
            if (nodeName == null) throw new ArgumentNullException(nameof(nodeName));
            if (string.IsNullOrWhiteSpace(nodeName)) throw new ArgumentException("May not be an empty string", nameof(nodeName));

            Name = nodeName;
            int prefixSep = nodeName.IndexOf(':');
            if (prefixSep == -1) {
                Prefix = string.Empty;
                LocalName = nodeName;
            } else if (prefixSep != 0 && nodeName.Length - prefixSep > 1) {
#if NETFRAMEWORK
                Prefix = nodeName.Substring(0, prefixSep);
                LocalName = nodeName.Substring(prefixSep + 1);
#else
                Prefix = nodeName[..prefixSep];
                LocalName = nodeName[(prefixSep + 1)..];
#endif
                if (LocalName.IndexOf(':') != -1) ThrowInvalidNodeName(nodeName, nameof(nodeName));
                if (string.IsNullOrWhiteSpace(Prefix)) ThrowInvalidNodeName(nodeName, nameof(nodeName));
                if (string.IsNullOrWhiteSpace(LocalName)) ThrowInvalidNodeName(nodeName, nameof(nodeName));
            } else {
                ThrowInvalidNodeName(nodeName, nameof(nodeName));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlTreeNode"/> class with a prefix and local name.
        /// </summary>
        /// <param name="prefix">The name space prefix alias.</param>
        /// <param name="localName">Name of the local XML element.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="prefix"/> or <paramref name="localName"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="localName"/> may not be an empty string, or is invalid;
        /// <para>- or -</para>
        /// <paramref name="prefix"/> may not be an empty string, or is invalid.
        /// </exception>
        public XmlTreeNode(string prefix, string localName)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            if (localName == null) throw new ArgumentNullException(nameof(localName));
            if (string.IsNullOrWhiteSpace(localName)) throw new ArgumentException("May not be an empty string", nameof(localName));

            if (string.IsNullOrEmpty(prefix)) {
                Name = localName;
                if (localName.IndexOf(':') != -1) ThrowInvalidNodeName(Name, nameof(localName));

                Prefix = string.Empty;
                LocalName = localName;
            } else {
                if (string.IsNullOrWhiteSpace(prefix)) throw new ArgumentException("May not be an empty string", nameof(prefix));

                Name = string.Format("{0}:{1}", prefix, localName);
                if (prefix.IndexOf(':') != -1) ThrowInvalidNodeName(Name, nameof(prefix));
                if (localName.IndexOf(':') != -1) ThrowInvalidNodeName(Name, nameof(localName));

                Prefix = prefix;
                LocalName = localName;
            }
        }

        private static void ThrowInvalidNodeName(string nodeName, string argument)
        {
            string message = string.Format("Invalid Node Name: '{0}'", nodeName);
            throw new ArgumentException(message, argument);
        }

        /// <summary>
        /// Gets the name of this node.
        /// </summary>
        /// <value>The name of this XML node.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the name space prefix alias, or <see cref="string.Empty"/> if not defined.
        /// </summary>
        /// <value>
        /// The XML name space prefix alias.
        /// </value>
        public string Prefix { get; private set; }

        /// <summary>
        /// Gets the local name of the XML element.
        /// </summary>
        /// <value>
        /// The local name of the XML element.
        /// </value>
        public string LocalName { get; private set; }

        /// <summary>
        /// A list of all nodes that may be found under this node.
        /// </summary>
        /// <value>The list of all nodes that may be found under this node.</value>
        /// <remarks>This property defines the tree structure of the XML tree.</remarks>
        public XmlNodeList Nodes { get; private set; } = new XmlNodeList();

        /// <summary>
        /// The delegate to call when processing a <see cref="XmlNodeType.Element"/>.
        /// </summary>
        /// <value>The delegate to call when processing a <see cref="XmlNodeType.Element"/>.</value>
        /// <remarks>
        /// When reading the <see cref="XmlReader"/> and finding the node type <see cref="XmlNodeType.Element"/>, the
        /// list of all nodes ( <see cref="Nodes"/>) is searched from parent node with a <see cref="XmlReader.Name"/>
        /// that matches. When a match is found, the <see cref="ProcessElement"/> delegate of that node is called.
        /// <para>
        /// The delegate passes the <see cref="XmlTreeNode"/> that matched, and a <see cref="XmlNodeEventArgs"/> which
        /// contains the current <see cref="XmlReader"/> and a user object <see cref="XmlNodeEventArgs.UserObject"/>
        /// as passed from the parent node.
        /// </para>
        /// <para>
        /// The delegate may replace the value of the <see cref="XmlNodeEventArgs.UserObject"/> with something else,
        /// that will then be given to all other delegates down in the XML Tree. When the processing is complete, the
        /// user object is replaced.
        /// </para>
        /// <para>This delegate is called from the root node in the case it is being read from a subtree.</para>
        /// <para>
        /// As XML trees are a recursive data structure, it might be required to call a function that has its own XML
        /// reading implementation for the current portion of the tree. There is support in <see cref="XmlTreeNode"/>
        /// XML parsing to allow this, so long on return of this delegate hasn't moved, or points to the end element
        /// when it returns. For example, calling <see cref="XmlReader.ReadElementContentAsString()"/> would move the
        /// reader to the end element, which is detected and then assumed that the node has been read.
        /// </para>
        /// <para>
        /// One must not call <see cref="XmlReader.Skip()"/> in this method, or advance the node to the beginning of
        /// the next element in the tree at the same level. If after the advancing of the
        /// <see cref="XmlNodeEventArgs.Reader"/> the names of the elements are the same (as often is for list type
        /// XML structures), the parser will enter into the node without calling <see cref="ProcessElement"/>, instead
        /// of calling this delegate <see cref="ProcessElement"/> for the next element that should normally be done,
        /// as it cannot identify that something has changed.
        /// </para>
        /// <para>
        /// If you must call a function with the subtree and that function advances to the beginning of the next
        /// element, you must instead provide it with a copy of the XML at the current node with a call to
        /// <see cref="XmlReader.ReadSubtree"/>. The function will receive a copy of the subtree, and the current
        /// <see cref="XmlNodeEventArgs.Reader"/> will advance to the next element as if <see cref="XmlReader.Read"/>
        /// was called, which a call to <see cref="XmlExtensions.SkipToEndElement(XmlReader)"/> can help by moving to
        /// the end element, putting the <see cref="XmlNodeEventArgs.Reader"/> at the correct position.
        /// </para>
        /// </remarks>
        public XmlProcessing ProcessElement { get; set; }

        private void InternalOnProcessElement(XmlNodeEventArgs args)
        {
            try {
                OnProcessElement(args);
            } catch (XmlException) {
                throw;
            } catch (Exception ex) {
                if (args?.Reader != null) args.Reader.Throw(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Handles the <see cref="ProcessElement" /> delegate.
        /// </summary>
        /// <param name="args">The <see cref="XmlNodeEventArgs"/> instance containing the event data.</param>
        protected virtual void OnProcessElement(XmlNodeEventArgs args)
        {
            XmlProcessing handler = ProcessElement;
            if (handler == null) return;

            handler(this, args);
        }

        /// <summary>
        /// The delegate to call when processing a <see cref="XmlNodeType.Element"/> for an element that is unknown.
        /// </summary>
        /// <value>
        /// The delegate to call when processing a <see cref="XmlNodeType.Element"/> when it is not found in the
        /// <see cref="Nodes"/> list.
        /// </value>
        /// <remarks>
        /// When reading the <see cref="XmlReader"/> and finding the node type <see cref="XmlNodeType.Element"/>, the
        /// list of all nodes ( <see cref="Nodes"/>) is searched from parent node with a <see cref="XmlReader.Name"/>
        /// that matches. When no match is found, the <see cref="ProcessUnknownElement"/> delegate of that parent is
        /// called.
        /// <para>
        /// If this delegate is specified, it must properly parse the unknown part of the tree to avoid a stack
        /// mismatch between the tree from the current parent node <see cref="Nodes"/>, and the
        /// <see cref="XmlReader"/>. All data must be parsed for the current node, so that the reader is positioned at
        /// the beginning of the next element parallel to this one, exactly as documented by the
        /// <see cref="XmlReader.Skip()"/> method. This normally means performing one additional read with
        /// <see cref="XmlReader.Read()"/> after finding the appropriate end element.
        /// </para>
        /// <para>
        /// If this delegate is not specified (it remains <see langword="null"/>), a call to
        /// <see cref="XmlReader.Skip"/> is made automatically.
        /// </para>
        /// <para>This delegate is never called from the root node.</para>
        /// </remarks>
        public XmlProcessing ProcessUnknownElement { get; set; }

        private bool InternalOnProcessUnknownElement(XmlNodeEventArgs args)
        {
            try {
                return OnProcessUnknownElement(args);
            } catch (XmlException) {
                throw;
            } catch (Exception ex) {
                if (args?.Reader != null) args.Reader.Throw(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Handles the <see cref="ProcessUnknownElement"/> delegate.
        /// </summary>
        /// <param name="args">The <see cref="XmlNodeEventArgs"/> instance containing the event data.</param>
        /// <returns>
        /// Returns <see langword="true"/> if the delegate was handled, <see langword="false"/> otherwise. When this
        /// method returns <see langword="false"/>, the reader will automatically call <see cref="XmlReader.Skip"/> to
        /// advance to the next element
        /// </returns>
        protected virtual bool OnProcessUnknownElement(XmlNodeEventArgs args)
        {
            XmlProcessing handler = ProcessUnknownElement;
            if (handler == null && args?.TreeSettings != null) {
                if (args.TreeSettings.ThrowOnUnknownElement)
                    args.Reader.Throw("Unhandled Element");
                return false;
            }

            handler(this, args);
            return true;
        }

        /// <summary>
        /// The delegate to call when processing a <see cref="XmlNodeType.Text"/> portion of the XML file.
        /// </summary>
        /// <value>
        /// The delegate to call when processing a <see cref="XmlNodeType.Text"/> portion of the XML file.
        /// </value>
        /// <remarks>
        /// Use this delegate when reading text blocks within the XML file. For example, to read an integer from the
        /// text block within the element, call <c>int.Parse(e.Reader.Value);</c>.
        /// <para>This delegate is never called from the root node.</para>
        /// </remarks>
        public XmlProcessing ProcessTextElement { get; set; }

        private void InternalOnProcessTextElement(XmlNodeEventArgs args)
        {
            try {
                OnProcessTextElement(args);
            } catch (XmlException) {
                throw;
            } catch (Exception ex) {
                if (args?.Reader != null) args.Reader.Throw(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Handles the <see cref="ProcessTextElement" /> delegate.
        /// </summary>
        /// <param name="args">The <see cref="XmlNodeEventArgs"/> instance containing the event data.</param>
        protected virtual void OnProcessTextElement(XmlNodeEventArgs args)
        {
            XmlProcessing handler = ProcessTextElement;
            if (handler == null && args?.TreeSettings != null) {
                if (args.TreeSettings.ThrowOnUnhandledText)
                    args.Reader.Throw("Unhandled Text Element");
                return;
            }

            handler(this, args);
        }

        /// <summary>
        /// The delegate to call when finished processing a <see cref="XmlNodeType.Element"/>.
        /// </summary>
        /// <value>The delegate to call when finished processing a <see cref="XmlNodeType.Element"/>.</value>
        /// <remarks>
        /// When a node is finished processing, either because it is empty ( <see cref="XmlReader.IsEmptyElement"/>)
        /// or the end element ( <see cref="XmlNodeType.EndElement"/>) is found. Upon parsing the node, the
        /// <see cref="XmlNodeEventArgs.UserObject"/> is reset to what it was before the call to
        /// <see cref="ProcessElement"/>.
        /// <para>This delegate is never called from the root node.</para>
        /// </remarks>
        public XmlProcessing ProcessEndElement { get; set; }

        private void InternalOnProcessEndElement(XmlNodeEventArgs args)
        {
            try {
                OnProcessEndElement(args);
            } catch (XmlException) {
                throw;
            } catch (Exception ex) {
                if (args?.Reader != null) args.Reader.Throw(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Handles the <see cref="ProcessEndElement" /> delegate.
        /// </summary>
        /// <param name="args">The <see cref="XmlNodeEventArgs"/> instance containing the event data.</param>
        protected virtual void OnProcessEndElement(XmlNodeEventArgs args)
        {
            XmlProcessing handler = ProcessEndElement;
            if (handler == null) return;

            handler(this, args);
        }

        private struct XmlStackEntry
        {
            public XmlStackEntry(XmlTreeNode node, string name, object userObject)
            {
                Node = node;
                Name = name;
                UserObject = userObject;
            }

            public XmlTreeNode Node { get; private set; }

            public string Name { get; private set; }

            public object UserObject { get; set; }
        }

        private struct XmlPosition : IXmlLineInfo
        {
            private readonly IXmlLineInfo m_NodePos;

            public XmlPosition(XmlReader reader)
            {
                NodeName = reader.Name;
                m_NodePos = reader.GetPosition();
                if (!m_NodePos.HasLineInfo())
                    reader.Throw("Parsing not supported on readers without IXmlLineInfo");
                Depth = reader.Depth;
            }

            public bool IsMoved(XmlReader reader)
            {
                IXmlLineInfo newPos = reader.GetPosition();
                if (!newPos.HasLineInfo())
                    reader.Throw("Parsing not supported on readers without IXmlLineInfo");

                if (m_NodePos.LineNumber != newPos.LineNumber) return true;
                if (m_NodePos.LinePosition != newPos.LinePosition) return true;
                return false;
            }

            public string NodeName { get; private set; }

            public int Depth { get; private set; }

            public bool HasLineInfo() { return true; }

            public int LineNumber { get { return m_NodePos.LineNumber; } }

            public int LinePosition { get { return m_NodePos.LinePosition; } }
        }

        /// <summary>
        /// Reads the XML file.
        /// </summary>
        /// <param name="reader">The reader to use when reading the XML file.</param>
        /// <param name="rootList">The root list defining the top level of the XML file.</param>
        /// <param name="xmlnsmgr">
        /// The namespace manager for mapping XML namespaces to prefixes which are used in the
        /// <paramref name="rootList"/>.
        /// </param>
        /// <param name="treeSettings">
        /// The <see cref="XmlTreeSettings"/> which configure the behavior while reading the XML tree.
        /// </param>
        /// <param name="skip">
        /// Skip reading the first element and process the current state of the <paramref name="reader"/>.
        /// </param>
        /// <param name="userObject">The initial user object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="reader"/> may not be <see langword="null"/>.
        /// <para>- or -</para>
        /// <paramref name="rootList"/> may not be <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// This method is called by the root node, <see cref="XmlTreeReader"/>, to read through the XML. When called,
        /// it will only process one top level XML element. So if called with a freshly created
        /// <see cref="XmlReader"/> on a file, it will read the entire file (as XML states there should not be more
        /// than one top level XML element). If this is called on sub-nodes within the XML tree, it will need to be
        /// called multiple times, and will return the XML reader in the state of the last read element.
        /// <para>
        /// If the <paramref name="xmlnsmgr"/> is not <see langword="null"/>, this will be used when mapping from a
        /// prefix defined in each <see cref="XmlTreeNode(string, string)"/> constructor, so that the application can
        /// define an XML namespace and the prefix expected, irrespective of what is actually in the XML file being
        /// read. This satisfies the requirement that two XML files are identical if two different prefixes for a name
        /// space are used, but are the same namespace themselves. The helper function
        /// <see cref="GetXmlNsMgr(IDictionary{string, string})"/> can be used to create a namespace manager.
        /// </para>
        /// </remarks>
        protected static void Read(XmlReader reader, XmlNodeList rootList, XmlNamespaceManager xmlnsmgr, XmlTreeSettings treeSettings, bool skip, object userObject)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (rootList == null) throw new ArgumentNullException(nameof(rootList));

            Stack<XmlStackEntry> xmlStack = new Stack<XmlStackEntry>();
            Read(reader, rootList, xmlnsmgr, treeSettings, xmlStack, skip, userObject);

            if (xmlStack.Count != 0)
                reader.Throw("Stack mismatch, {0} nodes still on the stack", xmlStack.Count);
        }

        private static void Read(XmlReader reader, XmlNodeList rootList, XmlNamespaceManager xmlnsmgr, XmlTreeSettings treeSettings, Stack<XmlStackEntry> xmlStack, bool skip, object userObject)
        {
            int initialDepth = reader.Depth;
            XmlNodeEventArgs xmlArgs = new XmlNodeEventArgs(reader, xmlnsmgr, treeSettings, userObject);
            XmlNodeList nodeList = rootList;
            XmlTreeNode node = null;
            while (skip || reader.Read()) {
                skip = false;
                switch (reader.NodeType) {
                case XmlNodeType.Element:
                    string nodeName = GetElementName(reader, xmlnsmgr);
                    if (nodeName != null && nodeList.TryGetValue(nodeName, out XmlTreeNode childNode)) {
                        node = ReadElement(reader, node, childNode, xmlStack, xmlArgs, out skip);
                        if (node != null) nodeList = node.Nodes;
                    } else {
                        ReadUnknownElement(reader, node, xmlStack, xmlArgs, out skip);
                    }
                    break;
                case XmlNodeType.Text:
                    if (node != null) node.InternalOnProcessTextElement(xmlArgs);
                    break;
                case XmlNodeType.EndElement:
                    if (node == null) {
                        // We've finished parsing the root node found of this tree. Return, so the user can continue
                        // parsing if this was a subtree.
                        return;
                    }

                    // Check the stack immediately, instead of only at the end, so we can raise an exception as soon as
                    // possible.
                    XmlStackEntry entry = xmlStack.Pop();
                    if (reader.Depth - initialDepth != xmlStack.Count)
                        reader.Throw("Stack mismatch, at depth {0} end node {1}, expected depth {2} with node {3}",
                            reader.Depth, reader.Name, xmlStack.Count + initialDepth, entry.Name);

                    if (reader.Name.Equals(entry.Name)) {
                        node.InternalOnProcessEndElement(xmlArgs);
                        node = entry.Node;
                        if (node == null) {
                            // We've finished parsing the root node found of this tree. Return, so the user can
                            // continue parsing if this was a subtree.
                            return;
                        }
                        nodeList = node.Nodes;
                        xmlArgs.UserObject = entry.UserObject;
                        break;
                    }
                    reader.Throw("Stack mismatch, found end tag <{0}/> when expected <{1}/>", reader.Name, entry.Name);
                    break;
                }
            }
        }

        private static string GetElementName(XmlReader reader, XmlNamespaceManager xmlnsmgr)
        {
            if (xmlnsmgr == null) return reader.Name;

            string prefix = xmlnsmgr.LookupPrefix(reader.NamespaceURI);

            if (string.IsNullOrEmpty(prefix)) {
                if (reader.NamespaceURI.Equals(xmlnsmgr.DefaultNamespace))
                    return reader.LocalName;

                // No match can be made with this, indicating that the namespace is unknown. Prevents misinterpreting a
                // prefix in the XML file from a prefix defined with xmlnsmgr having a different namespace.
                return null;
            }
            return string.Format("{0}:{1}", prefix, reader.LocalName);
        }

        private static XmlTreeNode ReadElement(XmlReader reader, XmlTreeNode node, XmlTreeNode childNode, Stack<XmlStackEntry> xmlStack, XmlNodeEventArgs xmlArgs, out bool skip)
        {
            XmlPosition xmlPos = new XmlPosition(reader);
            object currentObject = xmlArgs.UserObject;
            bool isEmpty = reader.IsEmptyElement;

            childNode.InternalOnProcessElement(xmlArgs);
            if (reader.NodeType == XmlNodeType.Attribute) reader.MoveToElement();

            if (xmlPos.IsMoved(reader)) {
                skip = PostProcessElement(reader, ref xmlPos, xmlStack);
                childNode.InternalOnProcessEndElement(xmlArgs);
                xmlArgs.UserObject = currentObject;
            } else {
                skip = false;
                if (isEmpty) {
                    childNode.InternalOnProcessEndElement(xmlArgs);
                    xmlArgs.UserObject = currentObject;
                } else {
                    // Traverse the next child node
                    xmlStack.Push(new XmlStackEntry(node, xmlPos.NodeName, currentObject));
                    node = childNode;
                }
            }

            return node;
        }

        private static void ReadUnknownElement(XmlReader reader, XmlTreeNode node, Stack<XmlStackEntry> xmlStack, XmlNodeEventArgs xmlArgs, out bool skip)
        {
            if (node == null) {
                reader.Throw("No known root found in XML stream");
                skip = false;
                return;
            }

            XmlPosition xmlPos = new XmlPosition(reader);
            object currentObject = xmlArgs.UserObject;

            if (node.InternalOnProcessUnknownElement(xmlArgs)) {
                if (reader.NodeType == XmlNodeType.Attribute) reader.MoveToElement();

                if (xmlPos.IsMoved(reader)) {
                    skip = PostProcessElement(reader, ref xmlPos, xmlStack);
                    xmlArgs.UserObject = currentObject;
                } else {
                    reader.Skip();
                    skip = true;
                }
            } else {
                reader.Skip();
                skip = true;
            }
        }

        /// <summary>
        /// Processes the XmlReader after it's determined the position of the XmlReader has moved.
        /// </summary>
        /// <param name="reader">The reader that should be processed.</param>
        /// <param name="xmlPos">The XML position.</param>
        /// <param name="xmlStack">The XML stack. Used for checking possible stack misalignment.</param>
        /// <returns>
        /// A value if the XmlReader is at the end of the current element as given by <paramref name="xmlPos"/>. If the
        /// return value is <see langword="true"/>, then parsing should not call <see cref="XmlReader.Read()"/>, but
        /// isntead parse the current node. If the result is <see langword="false"/>, then call
        /// <see cref="XmlReader.Read()"/> to advance to the next node before processing it.
        /// </returns>
        /// <remarks>
        /// This method should only be called if it's known that the position of the <paramref name="reader"/> has moved
        /// after processing the element. This can be determined by checking the <paramref name="xmlPos"/> parameter
        /// <see cref="XmlPosition.IsMoved(XmlReader)"/>. That check is not done here for performance.
        /// <para>
        /// The <paramref name="xmlPos"/> is a <see langword="ref"/> to a <see langword="struct"/> (value type), for
        /// performance reasons so it isn't copied when passed here (although its contents are not changed). The
        /// <see cref="XmlPosition"/> is a <see langword="struct"/> and not a <see langword="class"/> so that it is
        /// allocated on the stack and not on the global heap, thus being slightly faster.
        /// </para>
        /// </remarks>
        private static bool PostProcessElement(XmlReader reader, ref XmlPosition xmlPos, Stack<XmlStackEntry> xmlStack)
        {
            // --------------------------------------------------------------------------------------------------------
            // WARNING: The reader must implement IXmlLineInfo, so that if the reader position has changed, it is
            // assumed that the user code parsed the current node, and we don't need to push it on the stack. Else it
            // might be that this parser thinks it's going one level deeper when really the node was read and the depth
            // hasn't changed.
            // --------------------------------------------------------------------------------------------------------

            do {
                int delta = xmlPos.Depth - reader.Depth;
                switch (reader.NodeType) {
                case XmlNodeType.Element:
                    if (delta != 0)
                        reader.Throw(xmlPos, "Unexpected position after processing element <{0}>, stack depth changed at <{1}{2}>",
                            xmlPos.NodeName, reader.Name, reader.IsEmptyElement ? "/" : "");

                    // Assume that the user read the current node, and just moved over it with a method like
                    // reader.Skip() and came to the next node at the same depth.
                    return true;
                case XmlNodeType.EndElement:
                    if (delta == 0) {
                        // Assume the user called reader.SkipToEndElement() as we're at the same depth as before.
                        if (!reader.Name.Equals(xmlPos.NodeName))
                            reader.Throw(xmlPos, "Unexpected position after processing element <{0}>, got element </{1}>",
                                xmlPos.NodeName, reader.Name);
                        return false;
                    } else if (delta == 1) {
                        // Assume the user called reader.Skip() or similar functions and we're at the end element of the
                        // parent node.
                        if (!reader.Name.Equals(xmlStack.Peek().Name))
                            reader.Throw(xmlPos, "Unexpected position after processing element <{0}>, got element </{1}>, expected </{2}>",
                                xmlPos.NodeName, reader.Name, xmlStack.Peek().Name);
                        return true;
                    } else {
                        reader.Throw(xmlPos, "Unexpected position after processing element <{0}>, stack depth changed at </{1}>",
                            xmlPos.NodeName, reader.Name);
                    }
                    break;
                case XmlNodeType.Whitespace:
                    if (delta < 0 || delta > 1)
                        reader.Throw(xmlPos, "Unexpected position after processing element <{0}>, stack depth changed",
                            xmlPos.NodeName);

                    // If it's white space, then just continue consuming it until we get to an element.
                    break;
                default:
                    if (!reader.EOF)
                        reader.Throw(xmlPos, "Unsupported state of XmlReader after processing element <{0}> (node type {1})",
                            xmlPos.NodeName, reader.NodeType);
                    return true;
                }
            } while (reader.Read());
            return false;
        }

        /// <summary>
        /// Creates an XML Namespace Manager given a dictionary of namespaces.
        /// </summary>
        /// <param name="xmlns">The dictionary where the key is the prefix, and the value is the namespace.</param>
        /// <returns>An <see cref="XmlNamespaceManager"/> that can be used for mapping top level namespaces.</returns>
        protected static XmlNamespaceManager GetXmlNsMgr(IDictionary<string, string> xmlns)
        {
            return GetXmlNsMgr(null, xmlns);
        }

        /// <summary>
        /// Creates an XML Namespace Manager given a dictionary of namespaces.
        /// </summary>
        /// <param name="reader">
        /// The reader to share the name table with. If <see langword="null"/>, then a new nametable is generated.
        /// </param>
        /// <param name="xmlns">The dictionary where the key is the prefix, and the value is the namespace.</param>
        /// <returns>An <see cref="XmlNamespaceManager"/> that can be used for mapping top level namespaces.</returns>
        protected static XmlNamespaceManager GetXmlNsMgr(XmlReader reader, IDictionary<string, string> xmlns)
        {
            if (xmlns == null || xmlns.Count == 0) return null;

            XmlNamespaceManager xmlnsmgr;
            if (reader != null) {
                xmlnsmgr = new XmlNamespaceManager(reader.NameTable);
            } else {
                // Create a new NameTable, and don't use the one from the XmlReader, so that this name table remains in the
                // global scope.
                xmlnsmgr = new XmlNamespaceManager(new NameTable());
            }

            foreach (var ns in xmlns) {
                xmlnsmgr.AddNamespace(ns.Key, ns.Value);
            }
            return xmlnsmgr;
        }
    }
}
