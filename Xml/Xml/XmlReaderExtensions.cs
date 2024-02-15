namespace RJCP.Core.Xml
{
    using System;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Extension methods for common use cases within the <see cref="System.Xml"/> namespace.
    /// </summary>
    public static class XmlReaderExtensions
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
            if (position is null) {
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
            if (position is null) {
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
            if (position is null || !position.HasLineInfo()) throw new XmlException(message);
            throw new XmlException(message, innerException, position.LineNumber, position.LinePosition);
        }
    }
}
