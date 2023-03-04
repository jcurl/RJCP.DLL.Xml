namespace RJCP.Core.Xml
{
    using System;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Extension methods for common use cases within the <see cref="System.Xml"/> namespace.
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Skip XML to move the cursor to the end element of the current node.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <remarks>
        /// This method is similar to <see cref="XmlReader.Skip()"/>, but instead of advancing to the next
        /// XmlReader.NodeType == XmlNodeType.Element, it advances to the XmlNodeType.EndElement for the current
        /// depth.
        /// </remarks>
        public static void SkipToEndElement(this XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                xmlReader.Throw("Can only skip to end element beginning from a XmlNodeType.Element");
            if (xmlReader.IsEmptyElement) return;

            int depth = 0;
            while (xmlReader.Read()) {
                switch (xmlReader.NodeType) {
                case XmlNodeType.Element:
                    depth++;
                    break;
                case XmlNodeType.EndElement:
                    if (depth == 0) return;
                    depth--;
                    break;
                }
            }
        }

        /// <summary>
        /// Returns an object describing position information for an XML node.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <returns>An object describing position information for the current XML node.</returns>
        public static IXmlLineInfo GetPosition(this XmlReader xmlReader)
        {
            return new XmlLineInfo(xmlReader as IXmlLineInfo);
        }

        /// <summary>
        /// Throws the specified message.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="message">The message to provide in the exception.</param>
        /// <exception cref="XmlException">The user exception.</exception>
        /// <remarks>
        /// If the <see cref="XmlReader"/> provided supports the <see cref="IXmlLineInfo"/>, line information is added to
        /// the exception allowing for better identification of the error location.
        /// </remarks>
        public static void Throw(this XmlReader xmlReader, string message)
        {
            if (xmlReader is IXmlLineInfo position && position.HasLineInfo())
                throw new XmlException(message, null, position.LineNumber, position.LinePosition);
            throw new XmlException(message);
        }

        /// <summary>
        /// Throws the specified message.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="format">The formatting string for generating the message.</param>
        /// <param name="args">Arguments for the <paramref name="format"/>.</param>
        /// <exception cref="XmlException">The user exception.</exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="format"/> or <paramref name="args"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="FormatException">
        /// <paramref name="format"/> is invalid.
        /// <para>- or -</para>
        /// The index of a format item is less than zero, or greater than or equal to the length of the
        /// <paramref name="args"/> array.
        /// </exception>
        /// <remarks>
        /// If the <see cref="XmlReader"/> provided supports the <see cref="IXmlLineInfo"/>, line information is added to
        /// the exception allowing for better identification of the error location.
        /// </remarks>
        public static void Throw(this XmlReader xmlReader, string format, params object[] args)
        {
            string message = string.Format(format, args);
            Throw(xmlReader, message);
        }

        /// <summary>
        /// Throws the specified message.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="message">The message to provide in the exception.</param>
        /// <param name="innerException">The inner exception which caused this exception.</param>
        /// <exception cref="XmlException">The user exception.</exception>
        /// <remarks>
        /// If the <see cref="XmlReader"/> provided supports the <see cref="IXmlLineInfo"/>, line information is added to
        /// the exception allowing for better identification of the error location.
        /// </remarks>
        public static void Throw(this XmlReader xmlReader, string message, Exception innerException)
        {
            if (xmlReader is IXmlLineInfo position && position.HasLineInfo())
                throw new XmlException(message, innerException, position.LineNumber, position.LinePosition);
            throw new XmlException(message, innerException);
        }

        /// <summary>
        /// Throws the specified message.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="position">
        /// An optional parameter describing the position at when the exception occurred. If <see langword="null"/>, no
        /// position information is used.
        /// </param>
        /// <param name="message">The message to provide in the exception.</param>
        /// <exception cref="XmlException">The user exception.</exception>
        /// <remarks>
        /// If the <see cref="XmlReader"/> provided supports the <see cref="IXmlLineInfo"/>, line information is added to
        /// the exception allowing for better identification of the error location.
        /// </remarks>
        public static void Throw(this XmlReader xmlReader, IXmlLineInfo position, string message)
        {
            if (position == null) {
                xmlReader.Throw(message);
                return;
            }

            if (position.HasLineInfo())
                throw new XmlException(message, null, position.LineNumber, position.LinePosition);

            throw new XmlException(message);
        }

        /// <summary>
        /// Throws the specified message.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="position">
        /// An optional parameter describing the position at when the exception occurred. If <see langword="null"/>, no
        /// position information is used.
        /// </param>
        /// <param name="format">The formatting string for generating the message.</param>
        /// <param name="args">Arguments for the <paramref name="format"/>.</param>
        /// <exception cref="XmlException">The user exception.</exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="format"/> or <paramref name="args"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="FormatException">
        /// <paramref name="format"/> is invalid.
        /// <para>- or -</para>
        /// The index of a format item is less than zero, or greater than or equal to the length of the
        /// <paramref name="args"/> array.
        /// </exception>
        /// <remarks>
        /// If the <see cref="XmlReader"/> provided supports the <see cref="IXmlLineInfo"/>, line information is added to
        /// the exception allowing for better identification of the error location.
        /// </remarks>
        public static void Throw(this XmlReader xmlReader, IXmlLineInfo position, string format, params object[] args)
        {
            string message = string.Format(format, args);
            Throw(xmlReader, position, message);
        }

        /// <summary>
        /// Throws the specified message.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="position">
        /// An optional parameter describing the position at when the exception occurred. If <see langword="null"/>, no
        /// position information is used.
        /// </param>
        /// <param name="message">The message to provide in the exception.</param>
        /// <param name="innerException">The inner exception which caused this exception.</param>
        /// <exception cref="XmlException">The user exception.</exception>
        /// <remarks>
        /// If the <see cref="XmlReader"/> provided supports the <see cref="IXmlLineInfo"/>, line information is added to
        /// the exception allowing for better identification of the error location.
        /// </remarks>
        public static void Throw(this XmlReader xmlReader, IXmlLineInfo position, string message, Exception innerException)
        {
            if (position == null) {
                xmlReader.Throw(message);
                return;
            }

            if (position.HasLineInfo())
                throw new XmlException(message, innerException, position.LineNumber, position.LinePosition);

            throw new XmlException(message, innerException);
        }

        /// <summary>
        /// Throws the specified position.
        /// </summary>
        /// <param name="position">
        /// An optional parameter describing the position at when the exception occurred. If <see langword="null"/>, no
        /// position information is used.
        /// </param>
        /// <param name="message">The message to provide in the exception.</param>
        /// <exception cref="XmlException">The user exception.</exception>
        public static void Throw(IXmlLineInfo position, string message)
        {
            Throw(position, message, null);
        }

        /// <summary>
        /// Throws the specified position.
        /// </summary>
        /// <param name="position">
        /// An optional parameter describing the position at when the exception occurred. If <see langword="null"/>, no
        /// position information is used.
        /// </param>
        /// <param name="message">The message to provide in the exception.</param>
        /// <param name="innerException">The inner exception which caused this exception.</param>
        /// <exception cref="XmlException">The user exception.</exception>
        public static void Throw(IXmlLineInfo position, string message, Exception innerException)
        {
            if (position == null || !position.HasLineInfo()) throw new XmlException(message);
            throw new XmlException(message, innerException, position.LineNumber, position.LinePosition);
        }

        /// <summary>
        /// Converts any character which is not allowed according to XML 1.0 to a textual representation.
        /// </summary>
        /// <param name="input">The input that should be sanitized.</param>
        /// <returns>The sanitized input string.</returns>
        /// <remarks>
        /// This method is used to sanitize output which is intended for human readable input. It is not intended for
        /// storing information that might need to be reverted back - for that you should use a different encoding, such
        /// as Base64 encoding for general binary input data.
        /// <para>See XML Recommendation 1.0 Section 2.2 Characters.</para>
        /// <para>Char ::= #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]</para>
        /// </remarks>
        public static string SanitizeXml10(string input)
        {
            if (input == null) return string.Empty;
            StringBuilder sb = null;
            int pos = 0;
            int cp = 0;
            foreach (char c in input) {
                if (!IsValidXml10(c)) {
#if NETFRAMEWORK
                    if (sb == null) sb = new StringBuilder(input.Length + 128);
                    if (pos > cp) sb.Append(input.Substring(cp, pos - cp));
#else
                    sb ??= new StringBuilder(input.Length + 128);
                    if (pos > cp) sb.Append(input.AsSpan(cp, pos - cp));
#endif
                    cp = pos + 1;
                    sb.Append(EncodeChar(c));
                }
                pos++;
            }
            if (sb == null) return input;
#if NETFRAMEWORK
            if (pos > cp) sb.Append(input.Substring(cp, pos - cp));
#else
            if (pos > cp) sb.Append(input.AsSpan(cp, pos - cp));
#endif
            return sb.ToString();
        }

        private static bool IsValidXml10(char c)
        {
            if (c <= 8) return false;
            if (c == 11 || c == 12) return false;
            if (c >= 14 && c < 32) return false;
            if (c >= 0xD800 && c < 0xE000) return false;
            if (c == 0xFFFE || c == 0xFFFF) return false;
            return true;
        }

        private static string EncodeChar(char c)
        {
            return string.Format("[0x{0:X2}]", (int)c);
        }
    }
}
