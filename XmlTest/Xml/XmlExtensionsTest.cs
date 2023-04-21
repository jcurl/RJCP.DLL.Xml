namespace RJCP.Core.Xml
{
    using NUnit.Framework;

    [TestFixture]
    public class XmlExtensionsTest
    {
        [TestCase(null, "")]
        [TestCase("", "")]
        [TestCase("0123456789@ABCDEFGHIJKLMNOPQRSTUVWXYZ €100.99.", "0123456789@ABCDEFGHIJKLMNOPQRSTUVWXYZ €100.99.")]
        [TestCase("1234567890abcdefghijklmnopqrstuvwxyz", "1234567890abcdefghijklmnopqrstuvwxyz")]
        [TestCase("\u0008ab", "[0x08]ab")]
        [TestCase("a\u0009z", "a\u0009z")]
        [TestCase("\u000B\u000C", "[0x0B][0x0C]")]
        [TestCase("\u000E\u000F\u001F\u0020", "[0x0E][0x0F][0x1F] ")]
        [TestCase("\uD800\uDFFF\uE000", "[0xD800][0xDFFF]\uE000")]
        [TestCase("\uFFFE\uFFFF", "[0xFFFE][0xFFFF]")]
        [TestCase("\uD840\uDC00", "[0xD840][0xDC00]",
            Description = "Unicode point 0x20000 (Chinese character) - each UTF-16 character is found in the invalid XML 1.0 interval [0xD800, 0xE000)")]
        public void SanitizeXml10(string input, string output)
        {
            Assert.That(XmlExtensions.SanitizeXml10(input), Is.EqualTo(output));
        }
    }
}
