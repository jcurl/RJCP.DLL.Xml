namespace RJCP.Core.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// Read an XML file described by a tree structure.
    /// </summary>
    /// <remarks>
    /// The <see cref="XmlTreeReader"/> is a class that reads an XML file, matching the elements found with the tree
    /// structure exposed of the property <see cref="XmlTreeNode.Nodes"/>. The <see cref="XmlTreeReader"/> describes
    /// the root node of the XML tree.
    /// <para>
    /// Using this class requires instantiation and then creating the tree structure and assigning it to the
    /// <see cref="XmlTreeNode.Nodes"/> property. The root node (this class) can have multiple definitions of what the
    /// first element should be, but XML standards allow only a single element. That allows code to accommodate
    /// multiple file formats by doing a single read.
    /// </para>
    /// <para>
    /// As the XML is read, each <see cref="XmlNodeType"/> is interpreted and delegates are called, which can be used
    /// to interpret the contents of the XML. The delegates are listed below. Each delegate gets the reference to the
    /// <see cref="XmlTreeNode"/> being parsed, and an <see cref="XmlNodeEventArgs"/> object that contains the
    /// reference to the <see cref="XmlReader"/> and its current state, as well as a user defined object which is kept
    /// within the tree parsing.
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <see cref="XmlTreeNode.ProcessElement"/>: Executed when an element of the node is found in the tree. The
    /// reader is at the <see cref="XmlNodeType.Element"/>. It is possible to read the attributes of the element,
    /// instantiate new objects and assign to the <see cref="XmlNodeEventArgs.UserObject"/> which will be given to
    /// further delegates as the tree is parsed.
    /// </item>
    /// <item>
    /// <see cref="XmlTreeNode.ProcessTextElement"/>: Executed for the node when the <see cref="XmlReader"/> returns
    /// the node of <see cref="XmlNodeType.Text"/>. This can be used to read the content of the element, convert from
    /// integers, by using <see cref="XmlNode.Value"/> from the <see cref="XmlNodeEventArgs.Reader"/>.
    /// </item>
    /// <item>
    /// <see cref="XmlTreeNode.ProcessEndElement"/>: Always called just before the current node stack entry is exited,
    /// i.e. when the end element is read ( <see cref="XmlNodeType.EndElement"/>) or if the current node is empty (
    /// <see cref="XmlReader.IsEmptyElement"/>). This is where data structures for the read XML are usually appended
    /// to the final value.
    /// </item>
    /// <item>
    /// <see cref="XmlTreeNode.ProcessUnknownElement"/>: Called when the reader is at the
    /// <see cref="XmlNodeType.Element"/>, but there is no node in the current <see cref="XmlTreeNode.Nodes"/> list
    /// with a matching tag. If this delegate is not assigned, the tag is automatically skipped. If assigned, one must
    /// read through the XML <see cref="XmlReader"/> manually, or call <see cref="XmlReader.Skip"/> to advance to the
    /// next element to parse.
    /// </item>
    /// </list>
    /// <para>
    /// While parsing, the reader is compared against the current position in the stack. If there is a mismatch, an
    /// <see cref="XmlException"/> will be raised. This prevents common programming errors when parsing XML, to ensure
    /// that only XML files expected (in the absence of a validating schema file) are read. Any exceptions that occur
    /// in any of the delegates are captured, and a new <see cref="XmlException"/> is rethrown with the original
    /// exception as the <see cref="Exception.InnerException"/>. The raised exception will provide the location in the
    /// XML file where the error occurred, if the <see cref="XmlReader"/> supports this by implementing the
    /// <see cref="IXmlLineInfo"/> interface.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// As an example of reading the following XML snippet:
    /// </para>
    /// code might look like:
    /// <code language="xml"><![CDATA[
    /// <root>
    ///   <sub>Line1</sub>
    ///   <sub>Line2</sub>
    /// </root>
    /// ]]></code>
    /// Then the code to read the XML and place it in a list, and assign that list when complete, could look like:
    /// <code language="csharp"><![CDATA[
    /// List<string> myData = null;
    ///
    /// // Describes an XML format that only has a single root node.
    /// XmlTreeReader reader = new XmlTreeReader() {
    ///     Nodes = {
    ///         new XmlTreeNode("root") {
    ///             ProcessElement = (n, e) => { e.UserObject = new List<string>(); },
    ///             Nodes = {
    ///                 new XmlTreeNode("sub") {
    ///                     ProcessTextElement = (n, e) => { ((List<string>)e.UserObject).Add(e.Text); },
    ///                 }
    ///             }
    ///             ProcessEndElement = (n, e) => { myData = (List<string>)e.UserObject; },
    ///         }
    ///     }
    /// };
    /// ]]></code>
    /// </example>
    [DebuggerDisplay("ROOT, Nodes={Nodes.Count}")]
    public class XmlTreeReader : XmlTreeNode
    {
        private const XmlReaderSettings DefaultReaderSettings = null;
        private const XmlTreeSettings DefaultTreeSettings = null;
        private const XmlNamespaceManager DefaultNamespaceManager = null;
        private const IDictionary<string, string> EmptyNamespace = null;

        /// <summary>
        /// Reads the XML file name given, using default reader settings.
        /// </summary>
        /// <param name="fileName">Name of the file to read.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="fileName"/> is an empty string, contains only whitespace , or contains one or more invalid
        /// characters.
        /// <para>- or -</para>
        /// <paramref name="fileName"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="fileName"/> refers to a non-file device,such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="FileNotFoundException">The file cannot be found.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// THe path specified is invalid, such as being on an unmapped drive.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The access (read) requested is not permitted by the operating system for the specified
        /// <paramref name="fileName"/>.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, filename or both exceed the system defined maximum length.
        /// </exception>
        public void Read(string fileName)
        {
            Read(fileName, DefaultReaderSettings, DefaultTreeSettings, EmptyNamespace);
        }

        /// <summary>
        /// Reads the XML file name given, using default reader settings and a namespace mapping.
        /// </summary>
        /// <param name="fileName">Name of the file to read.</param>
        /// <param name="xmlns">The namespace mapping. The key is the prefix, the value is the URI namespace.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="fileName"/> is an empty string, contains only whitespace , or contains one or more invalid
        /// characters.
        /// <para>- or -</para>
        /// <paramref name="fileName"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="fileName"/> refers to a non-file device,such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="FileNotFoundException">The file cannot be found.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// THe path specified is invalid, such as being on an unmapped drive.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The access (read) requested is not permitted by the operating system for the specified
        /// <paramref name="fileName"/>.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, filename or both exceed the system defined maximum length.
        /// </exception>
        public void Read(string fileName, IDictionary<string, string> xmlns)
        {
            Read(fileName, DefaultReaderSettings, DefaultTreeSettings, xmlns);
        }

        /// <summary>
        /// Reads the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file to read.</param>
        /// <param name="readerSettings">The XML reader settings to use.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="fileName"/> is an empty string, contains only whitespace , or contains one or more invalid
        /// characters.
        /// <para>- or -</para>
        /// <paramref name="fileName"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="fileName"/> refers to a non-file device,such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="FileNotFoundException">The file cannot be found.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// THe path specified is invalid, such as being on an unmapped drive.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The access (read) requested is not permitted by the operating system for the specified
        /// <paramref name="fileName"/>.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, filename or both exceed the system defined maximum length.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The <paramref name="readerSettings"/> specifies a conformance level that is not consistent with the
        /// conformance level of the underlying reader.
        /// </exception>
        public void Read(string fileName, XmlReaderSettings readerSettings)
        {
            Read(fileName, readerSettings, DefaultTreeSettings, EmptyNamespace);
        }

        /// <summary>
        /// Reads the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file to read.</param>
        /// <param name="readerSettings">The XML reader settings to use.</param>
        /// <param name="treeSettings">
        /// Extra settings controlling the behavior of the <see cref="XmlTreeReader"/>.
        /// </param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="fileName"/> is an empty string, contains only whitespace , or contains one or more invalid
        /// characters.
        /// <para>- or -</para>
        /// <paramref name="fileName"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="fileName"/> refers to a non-file device,such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="FileNotFoundException">The file cannot be found.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// THe path specified is invalid, such as being on an unmapped drive.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The access (read) requested is not permitted by the operating system for the specified
        /// <paramref name="fileName"/>.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, filename or both exceed the system defined maximum length.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The <paramref name="readerSettings"/> specifies a conformance level that is not consistent with the
        /// conformance level of the underlying reader.
        /// </exception>
        public void Read(string fileName, XmlReaderSettings readerSettings, XmlTreeSettings treeSettings)
        {
            Read(fileName, readerSettings, treeSettings, EmptyNamespace);
        }

        /// <summary>
        /// Reads the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file to read.</param>
        /// <param name="treeSettings">
        /// Extra settings controlling the behavior of the <see cref="XmlTreeReader"/>.
        /// </param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="fileName"/> is an empty string, contains only whitespace , or contains one or more invalid
        /// characters.
        /// <para>- or -</para>
        /// <paramref name="fileName"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="fileName"/> refers to a non-file device,such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="FileNotFoundException">The file cannot be found.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// THe path specified is invalid, such as being on an unmapped drive.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The access (read) requested is not permitted by the operating system for the specified
        /// <paramref name="fileName"/>.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, filename or both exceed the system defined maximum length.
        /// </exception>
        public void Read(string fileName, XmlTreeSettings treeSettings)
        {
            Read(fileName, DefaultReaderSettings, treeSettings, EmptyNamespace);
        }

        /// <summary>
        /// Reads the specified file name with a namespace mapping.
        /// </summary>
        /// <param name="fileName">Name of the file to read.</param>
        /// <param name="readerSettings">The XML reader settings to use.</param>
        /// <param name="xmlns">The namespace mapping. The key is the prefix, the value is the URI namespace.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="fileName"/> is an empty string, contains only whitespace , or contains one or more invalid
        /// characters.
        /// <para>- or -</para>
        /// <paramref name="fileName"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="fileName"/> refers to a non-file device,such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="FileNotFoundException">The file cannot be found.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// THe path specified is invalid, such as being on an unmapped drive.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The access (read) requested is not permitted by the operating system for the specified
        /// <paramref name="fileName"/>.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, filename or both exceed the system defined maximum length.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The <paramref name="readerSettings"/> specifies a conformance level that is not consistent with the
        /// conformance level of the underlying reader.
        /// </exception>
        public void Read(string fileName, XmlReaderSettings readerSettings, IDictionary<string, string> xmlns)
        {
            Read(fileName, readerSettings, DefaultTreeSettings, xmlns);
        }

        /// <summary>
        /// Reads the specified file name with a namespace mapping.
        /// </summary>
        /// <param name="fileName">Name of the file to read.</param>
        /// <param name="treeSettings">
        /// Extra settings controlling the behavior of the <see cref="XmlTreeReader"/>.
        /// </param>
        /// <param name="xmlns">The namespace mapping. The key is the prefix, the value is the URI namespace.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="fileName"/> is an empty string, contains only whitespace , or contains one or more invalid
        /// characters.
        /// <para>- or -</para>
        /// <paramref name="fileName"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="fileName"/> refers to a non-file device,such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="FileNotFoundException">The file cannot be found.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// THe path specified is invalid, such as being on an unmapped drive.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The access (read) requested is not permitted by the operating system for the specified
        /// <paramref name="fileName"/>.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, filename or both exceed the system defined maximum length.
        /// </exception>
        public void Read(string fileName, XmlTreeSettings treeSettings, IDictionary<string, string> xmlns)
        {
            Read(fileName, DefaultReaderSettings, treeSettings, xmlns);
        }

        /// <summary>
        /// Reads the specified file name with a namespace mapping.
        /// </summary>
        /// <param name="fileName">Name of the file to read.</param>
        /// <param name="readerSettings">The XML reader settings to use.</param>
        /// <param name="treeSettings">
        /// Extra settings controlling the behavior of the <see cref="XmlTreeReader"/>.
        /// </param>
        /// <param name="xmlns">The namespace mapping. The key is the prefix, the value is the URI namespace.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="fileName"/> is an empty string, contains only whitespace , or contains one or more invalid
        /// characters.
        /// <para>- or -</para>
        /// <paramref name="fileName"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="fileName"/> refers to a non-file device,such as "con:", "com1:", "lpt1:", etc. in an NTFS
        /// environment.
        /// </exception>
        /// <exception cref="FileNotFoundException">The file cannot be found.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// THe path specified is invalid, such as being on an unmapped drive.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The access (read) requested is not permitted by the operating system for the specified
        /// <paramref name="fileName"/>.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, filename or both exceed the system defined maximum length.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The <paramref name="readerSettings"/> specifies a conformance level that is not consistent with the
        /// conformance level of the underlying reader.
        /// </exception>
        public void Read(string fileName, XmlReaderSettings readerSettings, XmlTreeSettings treeSettings, IDictionary<string, string> xmlns)
        {
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                Read(fileStream, readerSettings, treeSettings, xmlns);
            }
        }

        /// <summary>
        /// Reads XML from the specified stream, using default reader settings.
        /// </summary>
        /// <param name="stream">The stream to read the XML from.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public void Read(Stream stream)
        {
            Read(stream, DefaultReaderSettings, DefaultTreeSettings, EmptyNamespace);
        }

        /// <summary>
        /// Reads XML from the specified stream, using default reader settings and a namespace mapping.
        /// </summary>
        /// <param name="stream">The stream to read the XML from.</param>
        /// <param name="xmlns">The namespace mapping. The key is the prefix, the value is the URI namespace.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public void Read(Stream stream, IDictionary<string, string> xmlns)
        {
            Read(stream, DefaultReaderSettings, DefaultTreeSettings, xmlns);
        }

        /// <summary>
        /// Reads XML from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to read the XML from.</param>
        /// <param name="readerSettings">The XML reader settings to use.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// The <paramref name="readerSettings"/> specifies a conformance level that is not consistent with the
        /// conformance level of the underlying reader.
        /// </exception>
        public void Read(Stream stream, XmlReaderSettings readerSettings)
        {
            Read(stream, readerSettings, DefaultTreeSettings, EmptyNamespace);
        }

        /// <summary>
        /// Reads XML from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to read the XML from.</param>
        /// <param name="treeSettings">
        /// Extra settings controlling the behavior of the <see cref="XmlTreeReader"/>.
        /// </param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public void Read(Stream stream, XmlTreeSettings treeSettings)
        {
            Read(stream, DefaultReaderSettings, treeSettings, EmptyNamespace);
        }

        /// <summary>
        /// Reads XML from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to read the XML from.</param>
        /// <param name="readerSettings">The XML reader settings to use.</param>
        /// <param name="treeSettings">
        /// Extra settings controlling the behavior of the <see cref="XmlTreeReader"/>.
        /// </param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// The <paramref name="readerSettings"/> specifies a conformance level that is not consistent with the
        /// conformance level of the underlying reader.
        /// </exception>
        public void Read(Stream stream, XmlReaderSettings readerSettings, XmlTreeSettings treeSettings)
        {
            Read(stream, readerSettings, treeSettings, EmptyNamespace);
        }

        /// <summary>
        /// Reads XML from the specified stream with a namespace mapping.
        /// </summary>
        /// <param name="stream">The stream to read the XML from.</param>
        /// <param name="readerSettings">The XML reader settings to use.</param>
        /// <param name="xmlns">The namespace mapping. The key is the prefix, the value is the URI namespace.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// The <paramref name="readerSettings"/> specifies a conformance level that is not consistent with the
        /// conformance level of the underlying reader.
        /// </exception>
        public void Read(Stream stream, XmlReaderSettings readerSettings, IDictionary<string, string> xmlns)
        {
            Read(stream, readerSettings, DefaultTreeSettings, xmlns);
        }

        /// <summary>
        /// Reads XML from the specified stream with a namespace mapping.
        /// </summary>
        /// <param name="stream">The stream to read the XML from.</param>
        /// <param name="treeSettings">
        /// Extra settings controlling the behavior of the <see cref="XmlTreeReader"/>.
        /// </param>
        /// <param name="xmlns">The namespace mapping. The key is the prefix, the value is the URI namespace.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        public void Read(Stream stream, XmlTreeSettings treeSettings, IDictionary<string, string> xmlns)
        {
            Read(stream, DefaultReaderSettings, treeSettings, xmlns);
        }

        /// <summary>
        /// Reads XML from the specified stream with a namespace mapping.
        /// </summary>
        /// <param name="stream">The stream to read the XML from.</param>
        /// <param name="readerSettings">The XML reader settings to use.</param>
        /// <param name="treeSettings">
        /// Extra settings controlling the behavior of the <see cref="XmlTreeReader"/>.
        /// </param>
        /// <param name="xmlns">The namespace mapping. The key is the prefix, the value is the URI namespace.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// The <paramref name="readerSettings"/> specifies a conformance level that is not consistent with the
        /// conformance level of the underlying reader.
        /// </exception>
        public void Read(Stream stream, XmlReaderSettings readerSettings, XmlTreeSettings treeSettings, IDictionary<string, string> xmlns)
        {
            if (readerSettings == DefaultReaderSettings) readerSettings = DefaultSafeSettings();
            using (XmlReader reader = XmlReader.Create(stream, readerSettings)) {
                Read(reader, GetXmlNsMgr(null, xmlns), treeSettings);
            }
        }

        /// <summary>
        /// Reads XML from the specified <see cref="TextReader"/>, using default reader settings.
        /// </summary>
        /// <param name="textReader">The <see cref="TextReader"/> to read the XML from.</param>
        /// <remarks>
        /// The <paramref name="textReader"/> will be automatically closed when this method returns (when the underlying
        /// <see cref="XmlTextReader"/> is closed).
        /// </remarks>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="textReader"/> is <see langword="null"/>.</exception>
        public void Read(TextReader textReader)
        {
            Read(textReader, DefaultReaderSettings, DefaultTreeSettings, EmptyNamespace);
        }

        /// <summary>
        /// Reads XML from the specified <see cref="TextReader"/>, using default reader settings and a namespace
        /// mapping.
        /// </summary>
        /// <param name="textReader">The <see cref="TextReader"/> to read the XML from.</param>
        /// <param name="xmlns">The namespace mapping. The key is the prefix, the value is the URI namespace.</param>
        /// <remarks>
        /// The <paramref name="textReader"/> will be automatically closed when this method returns (when the underlying
        /// <see cref="XmlTextReader"/> is closed).
        /// </remarks>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="textReader"/> is <see langword="null"/>.</exception>
        public void Read(TextReader textReader, IDictionary<string, string> xmlns)
        {
            Read(textReader, DefaultReaderSettings, DefaultTreeSettings, xmlns);
        }

        /// <summary>
        /// Reads XML from the specified <see cref="TextReader"/>.
        /// </summary>
        /// <param name="textReader">The <see cref="TextReader"/> to read the XML from.</param>
        /// <param name="readerSettings">The XML reader settings to use.</param>
        /// <remarks>
        /// The <paramref name="textReader"/> will be automatically closed when this method returns (when the
        /// underlying <see cref="XmlTextReader"/> is closed).
        /// </remarks>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="textReader"/> is <see langword="null"/>.
        /// </exception>
        public void Read(TextReader textReader, XmlReaderSettings readerSettings)
        {
            Read(textReader, readerSettings, DefaultTreeSettings, EmptyNamespace);
        }

        /// <summary>
        /// Reads XML from the specified <see cref="TextReader"/>.
        /// </summary>
        /// <param name="textReader">The <see cref="TextReader"/> to read the XML from.</param>
        /// <param name="treeSettings">
        /// Extra settings controlling the behavior of the <see cref="XmlTreeReader"/>.
        /// </param>
        /// <remarks>
        /// The <paramref name="textReader"/> will be automatically closed when this method returns (when the
        /// underlying <see cref="XmlTextReader"/> is closed).
        /// </remarks>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="textReader"/> is <see langword="null"/>.
        /// </exception>
        public void Read(TextReader textReader, XmlTreeSettings treeSettings)
        {
            Read(textReader, DefaultReaderSettings, treeSettings, EmptyNamespace);
        }

        /// <summary>
        /// Reads XML from the specified <see cref="TextReader"/> and a namespace mapping.
        /// </summary>
        /// <param name="textReader">The <see cref="TextReader"/> to read the XML from.</param>
        /// <param name="readerSettings">The XML reader settings to use.</param>
        /// <param name="xmlns">The namespace mapping. The key is the prefix, the value is the URI namespace.</param>
        /// <remarks>
        /// The <paramref name="textReader"/> will be automatically closed when this method returns (when the underlying
        /// <see cref="XmlTextReader"/> is closed).
        /// </remarks>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="textReader"/> is <see langword="null"/>.</exception>
        public void Read(TextReader textReader, XmlReaderSettings readerSettings, IDictionary<string, string> xmlns)
        {
            Read(textReader, readerSettings, DefaultTreeSettings, xmlns);
        }

        /// <summary>
        /// Reads XML from the specified <see cref="TextReader"/> and a namespace mapping.
        /// </summary>
        /// <param name="textReader">The <see cref="TextReader"/> to read the XML from.</param>
        /// <param name="treeSettings">
        /// Extra settings controlling the behavior of the <see cref="XmlTreeReader"/>.
        /// </param>
        /// <param name="xmlns">The namespace mapping. The key is the prefix, the value is the URI namespace.</param>
        /// <remarks>
        /// The <paramref name="textReader"/> will be automatically closed when this method returns (when the underlying
        /// <see cref="XmlTextReader"/> is closed).
        /// </remarks>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="textReader"/> is <see langword="null"/>.</exception>
        public void Read(TextReader textReader, XmlTreeSettings treeSettings, IDictionary<string, string> xmlns)
        {
            Read(textReader, DefaultReaderSettings, treeSettings, xmlns);
        }

        /// <summary>
        /// Reads XML from the specified <see cref="TextReader"/> and a namespace mapping.
        /// </summary>
        /// <param name="textReader">The <see cref="TextReader"/> to read the XML from.</param>
        /// <param name="readerSettings">The XML reader settings to use.</param>
        /// <param name="treeSettings">
        /// Extra settings controlling the behavior of the <see cref="XmlTreeReader"/>.
        /// </param>
        /// <remarks>
        /// The <paramref name="textReader"/> will be automatically closed when this method returns (when the underlying
        /// <see cref="XmlTextReader"/> is closed).
        /// </remarks>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="textReader"/> is <see langword="null"/>.</exception>
        public void Read(TextReader textReader, XmlReaderSettings readerSettings, XmlTreeSettings treeSettings)
        {
            Read(textReader, readerSettings, treeSettings, EmptyNamespace);
        }

        /// <summary>
        /// Reads XML from the specified <see cref="TextReader"/> and a namespace mapping.
        /// </summary>
        /// <param name="textReader">The <see cref="TextReader"/> to read the XML from.</param>
        /// <param name="readerSettings">The XML reader settings to use.</param>
        /// <param name="treeSettings">
        /// Extra settings controlling the behavior of the <see cref="XmlTreeReader"/>.
        /// </param>
        /// <param name="xmlns">The namespace mapping. The key is the prefix, the value is the URI namespace.</param>
        /// <remarks>
        /// The <paramref name="textReader"/> will be automatically closed when this method returns (when the underlying
        /// <see cref="XmlTextReader"/> is closed).
        /// </remarks>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="textReader"/> is <see langword="null"/>.</exception>
        public void Read(TextReader textReader, XmlReaderSettings readerSettings, XmlTreeSettings treeSettings, IDictionary<string, string> xmlns)
        {
            if (readerSettings == null) readerSettings = DefaultSafeSettings();
            using (XmlReader reader = XmlReader.Create(textReader, readerSettings)) {
                Read(reader, GetXmlNsMgr(null, xmlns), treeSettings);
            }
        }

        /// <summary>
        /// Reads XML using the given reader.
        /// </summary>
        /// <param name="reader">The XML reader.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <see langword="null"/>.</exception>
        public void Read(XmlReader reader)
        {
            Read(reader, DefaultNamespaceManager, DefaultTreeSettings);
        }

        /// <summary>
        /// Reads XML using the given reader and a namespace mapping.
        /// </summary>
        /// <param name="reader">The XML reader.</param>
        /// <param name="xmlns">The namespace mapping. The key is the prefix, the value is the URI namespace.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <see langword="null"/>.</exception>
        public void Read(XmlReader reader, IDictionary<string, string> xmlns)
        {
            Read(reader, GetXmlNsMgr(null, xmlns), DefaultTreeSettings);
        }

        /// <summary>
        /// Reads XML using the given reader and a namespace mapping.
        /// </summary>
        /// <param name="reader">The XML reader.</param>
        /// <param name="treeSettings">
        /// Extra settings controlling the behavior of the <see cref="XmlTreeReader"/>.
        /// </param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <see langword="null"/>.</exception>
        public void Read(XmlReader reader, XmlTreeSettings treeSettings)
        {
            Read(reader, DefaultNamespaceManager, treeSettings);
        }

        /// <summary>
        /// Reads XML using the given reader and a namespace mapping.
        /// </summary>
        /// <param name="reader">The XML reader.</param>
        /// <param name="treeSettings">
        /// Extra settings controlling the behavior of the <see cref="XmlTreeReader"/>.
        /// </param>
        /// <param name="xmlns">The namespace mapping. The key is the prefix, the value is the URI namespace.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <see langword="null"/>.</exception>
        public void Read(XmlReader reader, XmlTreeSettings treeSettings, IDictionary<string, string> xmlns)
        {
            Read(reader, GetXmlNsMgr(null, xmlns), treeSettings);
        }

        /// <summary>
        /// Reads XML using the given reader and a namespace mapping.
        /// </summary>
        /// <param name="reader">The XML reader.</param>
        /// <param name="xmlnsmgr">The namespace mapping.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <see langword="null"/>.</exception>
        public void Read(XmlReader reader, XmlNamespaceManager xmlnsmgr)
        {
            Read(reader, xmlnsmgr, DefaultTreeSettings);
        }

        /// <summary>
        /// Reads XML using the given reader and a namespace mapping.
        /// </summary>
        /// <param name="reader">The XML reader.</param>
        /// <param name="xmlnsmgr">The namespace mapping.</param>
        /// <param name="treeSettings">Settings on how the reader should behave.</param>
        /// <exception cref="XmlException">
        /// An error occurred when reading the XML file, either the file is corrupted, a delegate raised an exception,
        /// or there was a mismatch when parsing the tree.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <see langword="null"/>.</exception>
        public void Read(XmlReader reader, XmlNamespaceManager xmlnsmgr, XmlTreeSettings treeSettings)
        {
            bool isSubTree = reader.NodeType != XmlNodeType.None;
            if (treeSettings == DefaultTreeSettings) treeSettings = new XmlTreeSettings();
            Read(reader, Nodes, xmlnsmgr, treeSettings, isSubTree, null);

            if (!isSubTree) {
                // Final step, read until there is no more data. If we do find data, then there was a mismatch in the
                // stack, or simply invalid XML because data was attached. Normally, the validating Xml Reader will throw
                // its own exception before ours here are thrown.
                while (reader.Read()) {
                    switch (reader.NodeType) {
                    case XmlNodeType.Element:
                        reader.Throw("Stack mismatch, found start tag <{0}/> when end of file expected", reader.Name);
                        break;
                    case XmlNodeType.EndElement:
                        reader.Throw("Stack mismatch, found end tag <{0}/> when end of file expected", reader.Name);
                        break;
                    }
                }
            }
        }

        private static XmlReaderSettings DefaultSafeSettings()
        {
            return new XmlReaderSettings() {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };
        }
    }
}
