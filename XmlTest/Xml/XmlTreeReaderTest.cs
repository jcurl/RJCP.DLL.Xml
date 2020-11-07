namespace RJCP.Core.Xml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using NUnit.Framework;

    [TestFixture]
    public class XmlTreeReaderTest
    {
        [Test]
        public void Constructor()
        {
            XmlTreeReader reader = new XmlTreeReader();
            Assert.That(reader.Name, Is.Null.Or.Empty);
            Assert.That(reader.Nodes.Count, Is.EqualTo(0));
        }

        [Test]
        public void ReadEmptyXmlNoRoot()
        {
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("root")
                }
            };
            Assert.That(() => { reader.Read(new StringReader("")); }, Throws.TypeOf<XmlException>());
        }

        [TestCase("<root/>", TestName = "ReadSingleElementRootEmpty")]
        [TestCase("<root></root>", TestName = "ReadSingleElementRootNonEmpty")]
        [TestCase("<root>\n</root>", TestName = "ReadSingleElementRootNonEmptyLine")]
        public void ReadSingleElementRoot(string xml)
        {
            int rootRead = 0;
            int rootFinished = 0;
            int rootText = 0;
            int rootUnknown = 0;

            // Describes an XML format that only has a single root node.
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("root") {
                        ProcessElement = (n, e) => { rootRead++; },
                        ProcessTextElement = (n, e) => { rootText++; },
                        ProcessEndElement = (n, e) => { rootFinished++; },
                        ProcessUnknownElement = (n, e) => { rootUnknown++; e.Reader.Skip(); }
                    }
                }
            };

            reader.Read(new StringReader(xml));
            Assert.That(rootRead, Is.EqualTo(1));
            Assert.That(rootText, Is.EqualTo(0));
            Assert.That(rootFinished, Is.EqualTo(1));
            Assert.That(rootUnknown, Is.EqualTo(0));
        }

        [TestCase("<root><sub/></root>", TestName = "ReadElementsSubEmpty")]
        [TestCase("<root>\n  <sub/>\n</root>", TestName = "ReadElementsSubEmptyLine")]
        [TestCase("<root><sub></sub></root>", TestName = "ReadElementsSubNonEmpty")]
        [TestCase("<root>\n  <sub>\n  \n  </sub>\n</root>", TestName = "ReadElementsSubNonEmptyLine")]
        public void ReadElementSub(string xml)
        {
            int rootRead = 0;
            int rootFinished = 0;
            int rootText = 0;
            int rootUnknown = 0;
            int subRead = 0;
            int subFinished = 0;
            int subText = 0;
            int subUnknown = 0;

            // Describes an XML format that only has a single root node.
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("root") {
                        ProcessElement = (n, e) => { rootRead++; },
                        ProcessTextElement = (n, e) => { rootText++; },
                        ProcessEndElement = (n, e) => { rootFinished++; },
                        ProcessUnknownElement = (n, e) => { rootUnknown++; e.Reader.Skip(); },
                        Nodes = {
                            new XmlTreeNode("sub") {
                                ProcessElement = (n, e) => { subRead++; },
                                ProcessTextElement = (n, e) => { subText++; },
                                ProcessEndElement = (n, e) => { subFinished++; },
                                ProcessUnknownElement = (n, e) => { subUnknown++; e.Reader.Skip(); }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(xml));
            Assert.That(rootRead, Is.EqualTo(1));
            Assert.That(rootText, Is.EqualTo(0));
            Assert.That(rootFinished, Is.EqualTo(1));
            Assert.That(rootUnknown, Is.EqualTo(0));
            Assert.That(subRead, Is.EqualTo(1));
            Assert.That(subText, Is.EqualTo(0));
            Assert.That(subFinished, Is.EqualTo(1));
            Assert.That(subUnknown, Is.EqualTo(0));
        }

        [Test]
        public void UserObjectRoot()
        {
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("root") {
                        ProcessElement = (n, e) => { e.UserObject = 1; },
                        ProcessTextElement = (n, e) => { Assert.That(e.UserObject, Is.TypeOf<int>().And.EqualTo(1)); },
                        ProcessEndElement = (n, e) => { Assert.That(e.UserObject, Is.TypeOf<int>().And.EqualTo(1)); }
                    }
                }
            };
            reader.Read(new StringReader("<root>value</root>"));
        }

        [TestCase("<root><sub>value</sub></root>", TestName = "UserObjectSubNodesText")]
        [TestCase("<root><sub></sub></root>", TestName = "UserObjectSubNodesNoText")]
        [TestCase("<root><sub/></root>", TestName = "UserObjectSubNodesEmpty")]
        public void UserObjectSubNodes(string xml)
        {
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("root") {
                        ProcessElement = (n, e) => { e.UserObject = 1; },
                        ProcessTextElement = (n, e) => { Assert.That(e.UserObject, Is.TypeOf<int>().And.EqualTo(1)); },
                        ProcessEndElement = (n, e) => { Assert.That(e.UserObject, Is.TypeOf<int>().And.EqualTo(1)); },
                        Nodes = {
                            new XmlTreeNode("sub") {
                                ProcessElement = (n, e) => {
                                    // The user object is inherited by the level above and is replaced here, so that
                                    // this level and sub-levels have the new value. When ProcessEndElement is called,
                                    // it is restored, so that root.ProcessEndElement sees the old value, not the value
                                    // replaced here.
                                    Assert.That(e.UserObject, Is.TypeOf<int>().And.EqualTo(1));
                                    e.UserObject = 2;
                                },
                                ProcessTextElement = (n, e) => { Assert.That(e.UserObject, Is.TypeOf<int>().And.EqualTo(2)); },
                                ProcessEndElement = (n, e) => { Assert.That(e.UserObject, Is.TypeOf<int>().And.EqualTo(2)); }
                            }
                        }
                    }
                }
            };
            reader.Read(new StringReader(xml));
        }

        [TestCase("<a><b/><c/></a>", TestName = "IgnoreUnknownElementEmptyLevel1")]
        [TestCase("<a><b/><c></c></a>", TestName = "IgnoreUnknownElementLevel1")]
        [TestCase("<a><b/><c>Text</c></a>", TestName = "IgnoreUnknownElementLevel1Text")]
        [TestCase("<a><b/><c><d></d></c></a>", TestName = "IgnoreUnknownElementLevel1Sub")]
        [TestCase("<a><b><c/></b></a>", TestName = "IgnoreUnknownElementEmptyLevel2")]
        [TestCase("<a><b><c></c></b></a>", TestName = "IgnoreUnknownElementLevel2")]
        [TestCase("<a><b><c>Text</c></b></a>", TestName = "IgnoreUnknownElementLevel2Text")]
        [TestCase("<a><b><c><d/></c></b></a>", TestName = "IgnoreUnknownElementLevel2Sub")]
        [TestCase("<a><b><d/><c/></b></a>", TestName = "IgnoreUnknownElementLevel2SubFirst1")]
        [TestCase("<a><b><d></d><c/></b></a>", TestName = "IgnoreUnknownElementLevel2SubFirst2")]
        [TestCase("<a><b><c/><d/></b></a>", TestName = "IgnoreUnknownElementLevel2SubSecond1")]
        [TestCase("<a><b><c/><d></d></b></a>", TestName = "IgnoreUnknownElementLevel2SubSecond2")]
        public void IgnoreUnknownElement(string xml)
        {
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") { }
                        }
                    }
                }
            };

            // Should read and do nothing. If an exception occurs, there was an error parsing a valid XML file.
            Assert.That(() => { reader.Read(new StringReader(xml)); }, Throws.Nothing);
        }

        [TestCase("<a><b><d>TEXT</d></b></a>", TestName = "UnknownElementDefinedWithText")]
        [TestCase("<a><b><c><d/></c></b></a>", TestName = "UnknownElementDefinedEmpty")]
        [TestCase("<a><b><d/><c/></b></a>", TestName = "UnknownElementDefinedLeftEmpty")]
        [TestCase("<a><b><d></d><c/></b></a>", TestName = "UnknownElementDefinedLeft")]
        [TestCase("<a><b><c/><d/></b></a>", TestName = "UnknownElementDefinedRightEmpty")]
        [TestCase("<a><b><c/><d></d></b></a>", TestName = "UnknownElementDefinedRight")]
        public void UnknownElementDefinedAndEmpty(string xml)
        {
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                ProcessUnknownElement = (n, e) => { Console.WriteLine("{0}", e.Reader.Name); },
                                Nodes = {
                                    new XmlTreeNode("c") { }
                                }
                            }
                        },
                    }
                }
            };

            // Should read and do nothing. If an exception occurs, there was an error parsing a valid XML file.
            Assert.That(() => { reader.Read(new StringReader(xml)); }, Throws.Nothing);
        }

        [TestCase("<a><b><d>TEXT</d></b></a>", TestName = "UnknownElementSkipWithText")]
        [TestCase("<a><b><c><d/></c></b></a>", TestName = "UnknownElementSkipEmpty")]
        [TestCase("<a><b><d/><c/></b></a>", TestName = "UnknownElementSkipLeftEmpty")]
        [TestCase("<a><b><d></d><c/></b></a>", TestName = "UnknownElementSkipLeft")]
        [TestCase("<a><b><c/><d/></b></a>", TestName = "UnknownElementSkipRightEmpty")]
        [TestCase("<a><b><c/><d></d></b></a>", TestName = "UnknownElementSkipRight")]
        public void UnknownElementSkip(string xml)
        {
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                ProcessUnknownElement = (n, e) => { e.Reader.Skip(); },
                                Nodes = {
                                    new XmlTreeNode("c") { }
                                }
                            }
                        },
                    }
                }
            };

            // Should read and do nothing. If an exception occurs, there was an error parsing a valid XML file.
            Assert.That(() => { reader.Read(new StringReader(xml)); }, Throws.Nothing);
        }

        [TestCase("<a><b><d>TEXT</d></b></a>", TestName = "UnknownElementSkipEndElementWithText")]
        [TestCase("<a><b><c><d/></c></b></a>", TestName = "UnknownElementSkipEndElementEmpty")]
        [TestCase("<a><b><d/><c/></b></a>", TestName = "UnknownElementSkipEndElementLeftEmpty")]
        [TestCase("<a><b><d></d><c/></b></a>", TestName = "UnknownElementSkipEndElementLeft")]
        [TestCase("<a><b><c/><d/></b></a>", TestName = "UnknownElementSkipEndElementRightEmpty")]
        [TestCase("<a><b><c/><d></d></b></a>", TestName = "UnknownElementSkipEndElementRight")]
        public void UnknownElementSkipEndElement(string xml)
        {
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                ProcessUnknownElement = (n, e) => { e.Reader.SkipToEndElement(); },
                                Nodes = {
                                    new XmlTreeNode("c") { }
                                }
                            }
                        },
                    }
                }
            };

            // Should read and do nothing. If an exception occurs, there was an error parsing a valid XML file.
            Assert.That(() => { reader.Read(new StringReader(xml)); }, Throws.Nothing);
        }

        [TestCase("<a></a>", 0, TestName = "MultipleRootsANonEmpty")]
        [TestCase("<a/>", 0, TestName = "MultipleRootsAEmpty")]
        [TestCase("<a><b></b></a>", 1, TestName = "MultipleRootsAFoundB")]
        [TestCase("<a><b/></a>", 1, TestName = "MultipleRootsAFoundBEmpty")]
        [TestCase("<c></c>", 0, TestName = "MultipleRootsCNonEmpty")]
        [TestCase("<c/>", 0, TestName = "MultipleRootCAEmpty")]
        [TestCase("<c><d></d></c>", 2, TestName = "MultipleRootsCFoundD")]
        [TestCase("<c><d/></c>", 2, TestName = "MultipleRootsCFoundDEmpty")]
        public void MultipleRoots(string xml, int path)
        {
            int foundPath = 0;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    // Even though an XML file may only have one root, the XmlTreeReader implementation can allow to
                    // handle multiple roots (e.g. multiple versions of the same file can be read at once, possibly
                    // simplifying implementations).
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                ProcessElement = (n, e) => { foundPath = 1; }
                            }
                        }
                    },
                    new XmlTreeNode("c") {
                        Nodes = {
                            new XmlTreeNode("d") {
                                ProcessElement = (n, e) => { foundPath = 2; }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(xml));
            Assert.That(foundPath, Is.EqualTo(path));
        }

        [TestCase("<root></root>", false, TestName = "ReadFromSub2None")]
        [TestCase("<root><sub/></root>", true, TestName = "ReadFromSub2Empty")]
        [TestCase("<root><sub></sub></root>", true, TestName = "ReadFromSub2")]
        [TestCase("<root><sub>Text</sub></root>", true, TestName = "ReadFromSub2Text")]
        [TestCase("<root><sub><x></x></sub></root>", true, TestName = "ReadFromSub2X")]
        [TestCase("<root><sub><x/></sub></root>", true, TestName = "ReadFromSub2XEmpty")]
        [TestCase("<root><sub><x>Text</x></sub></root>", true, TestName = "ReadFromSub2XText")]
        public void ReadFromSub2(string xml, bool found)
        {
            XmlTreeSettings xmlTreeSettings = new XmlTreeSettings() {
                ThrowOnUnhandledText = false
            };

            bool readSub = false;
            XmlTreeReader readerSub = new XmlTreeReader() {
                Nodes = {
                    // The root node in the subtree must be the same as the node which is being processed. If it isn't,
                    // an exception will be raised that no known root can be found.
                    new XmlTreeNode("root") {
                        Nodes = {
                            new XmlTreeNode("sub") {
                                ProcessElement = (n, e) => { readSub = true; }
                            }
                        }
                    }
                }
            };

            XmlTreeReader readerRoot = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("root") {
                        // A typical use case might be to split reading different sections of the XML into different
                        // functions. So while this root doesn't know about the 'sub' element, the next will continue
                        // reading that portion of the tree, and return, allowing this tree to walk up the stack.
                        //
                        // Doing this from the root node looks silly, because the subtree must have the parent node
                        // listed, thus making this example look redundant, so refer to the next test cases
                        // ReadFromSub3, where this code makes more sense.
                        ProcessElement = (n, e) => { readerSub.Read(e.Reader, e.TreeSettings); }
                    }
                }
            };

            readerRoot.Read(new StringReader(xml), xmlTreeSettings);
            Assert.That(readSub, Is.EqualTo(found));
        }

        [TestCase("<root></root>", false, TestName = "ReadFromSub3None")]
        [TestCase("<root><sub/></root>", false, TestName = "ReadFromSub3Empty")]
        [TestCase("<root><sub></sub></root>", false, TestName = "ReadFromSub3")]
        [TestCase("<root><sub>Text</sub></root>", false, TestName = "ReadFromSub3Text")]
        [TestCase("<root><sub><x></x></sub></root>", true, TestName = "ReadFromSub3X")]
        [TestCase("<root><sub><x/></sub></root>", true, TestName = "ReadFromSub3XEmpty")]
        [TestCase("<root><sub><x>Text</x></sub></root>", true, TestName = "ReadFromSub3XText")]
        public void ReadFromSub3(string xml, bool found)
        {
            XmlTreeSettings xmlTreeSettings = new XmlTreeSettings() {
                ThrowOnUnhandledText = false
            };

            bool readSub = false;
            XmlTreeReader readerSub = new XmlTreeReader() {
                Nodes = {
                    // The root node in the subtree must be the same as the node which is being processed. If it isn't,
                    // an exception will be raised that no known root can be found.
                    new XmlTreeNode("sub") {
                        Nodes = {
                            new XmlTreeNode("x") {
                                ProcessElement = (n, e) => { readSub = true; }
                            }
                        }
                    }
                }
            };

            XmlTreeReader readerRoot = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("root") {
                        Nodes = {
                            new XmlTreeNode("sub") {
                                ProcessElement = (n, e) => { readerSub.Read(e.Reader, e.TreeSettings); }
                            }
                        }
                    }
                }
            };

            readerRoot.Read(new StringReader(xml), xmlTreeSettings);
            Assert.That(readSub, Is.EqualTo(found));
        }

        [Test]
        public void ReadFromSub3List()
        {
            const string Xml = "<root><sub><x>Text1</x></sub><sub><x>Text2</x></sub></root>";
            List<string> list = new List<string>();

            int subProcessElement = 0;
            int subProcessEndElement = 0;
            int subXProcessElement = 0;
            int subXProcessEndElement = 0;
            XmlTreeReader readerSub = new XmlTreeReader() {
                Nodes = {
                    // The root node in the subtree must be the same as the node which is being processed. If it isn't,
                    // an exception will be raised that no known root can be found.
                    new XmlTreeNode("sub") {
                        ProcessElement = (n, e) => {subProcessElement++; },
                        ProcessEndElement = (n, e) => { subProcessEndElement++; },
                        Nodes = {
                            new XmlTreeNode("x") {
                                ProcessElement = (n, e) => { subXProcessElement++; },
                                ProcessEndElement = (n, e) => { subXProcessEndElement++; },
                                ProcessTextElement = (n, e) => { list.Add(e.Reader.Value); }
                            }
                        }
                    }
                }
            };

            int rootProcessElement = 0;
            int rootProcessEndElement = 0;
            int rootSubProcessElement = 0;
            int rootSubProcessEndElement = 0;
            int rootSubXProcessElement = 0;
            int rootSubXProcessEndElement = 0;
            XmlTreeReader readerRoot = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("root") {
                        ProcessElement = (n, e) => { rootProcessElement++; },
                        ProcessEndElement = (n, e) => { rootProcessEndElement++; },
                        Nodes = {
                            new XmlTreeNode("sub") {
                                ProcessElement = (n, e) => { rootSubProcessElement++; readerSub.Read(e.Reader); },
                                ProcessEndElement = (n, e) => { rootSubProcessEndElement++; },
                                Nodes = {
                                    new XmlTreeNode("x") {
                                        ProcessElement = (n, e) => { rootSubXProcessElement++; },
                                        ProcessEndElement = (n, e) => { rootSubXProcessEndElement++; }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            readerRoot.Read(new StringReader(Xml));

            // Reading the list ensures that the subtrees are handled properly.
            Assert.That(list, Is.EqualTo(new[] { "Text1", "Text2" }));

            // And that each event was properly called. Note, that readerRoot[root].Nodes[sub].Nodes[x] won't be called
            Assert.That(rootProcessElement, Is.EqualTo(1));
            Assert.That(rootProcessEndElement, Is.EqualTo(1));
            Assert.That(rootSubProcessElement, Is.EqualTo(2));
            Assert.That(rootSubProcessEndElement, Is.EqualTo(2));
            Assert.That(rootSubXProcessElement, Is.EqualTo(0));        // Skipped by readerSub.ReadSubTree()
            Assert.That(rootSubXProcessEndElement, Is.EqualTo(0));     // Skipped by readerSub.ReadSubTree()
            Assert.That(subProcessElement, Is.EqualTo(2));             // Tag is processed once per tree
            Assert.That(subProcessEndElement, Is.EqualTo(2));          // Tag is processed once per tree
            Assert.That(subXProcessElement, Is.EqualTo(2));
            Assert.That(subXProcessEndElement, Is.EqualTo(2));
        }

        [TestCase("<a>", TestName = "IncompleteXml(a)")]
        [TestCase("<a><b>", TestName = "IncompleteXml(ab)")]
        [TestCase("<a><b/>", TestName = "IncompleteXml(ab/)")]
        [TestCase("<a><b></b>", TestName = "IncompleteXml(ab/b)")]
        [TestCase("<a><b><c>", TestName = "IncompleteXml(abc)")]
        [TestCase("<a><b><c/>", TestName = "IncompleteXml(abc/)")]
        [TestCase("<a><b><c></c>", TestName = "IncompleteXml(abc/c)")]
        [TestCase("<a><b><c></c></b>", TestName = "IncompleteXml(abc/c/b)")]
        [TestCase("<a><b><c/></b></a><foo></foo>", TestName = "TooMuchXml")]
        [TestCase("<a><b></c></a>", TestName = "MismatchXml(ab/c/a)")]
        public void InvalidXml(string xml)
        {
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                Nodes = {
                                    new XmlTreeNode("c") { }
                                }
                            }
                        }
                    }
                }
            };
            Assert.That(() => { reader.Read(new StringReader(xml)); }, Throws.TypeOf<XmlException>());
        }

        [Test]
        public void WrapProcessTextException()
        {
            const string Xml = "<a><b>foo</b></a>";

            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                ProcessTextElement = (n, e) => { _ = int.Parse(e.Reader.Value); }
                            }
                        }
                    }
                }
            };
            Assert.That(() => {
                reader.Read(new StringReader(Xml));
            }, Throws.TypeOf<XmlException>().With.InnerException.TypeOf<FormatException>());
        }

        [Test]
        public void WrapProcessException()
        {
            const string Xml = "<a><b>foo</b></a>";

            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                ProcessElement = (n, e) => { throw new InvalidOperationException(); }
                            }
                        }
                    }
                }
            };
            Assert.That(() => {
                reader.Read(new StringReader(Xml));
            }, Throws.TypeOf<XmlException>().With.InnerException.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void WrapProcessEndException()
        {
            const string Xml = "<a><b>foo</b></a>";

            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                ProcessEndElement = (n, e) => { throw new InvalidOperationException(); }
                            }
                        }
                    }
                }
            };

            XmlTreeSettings treeSettings = new XmlTreeSettings() {
                ThrowOnUnhandledText = false
            };

            Assert.That(() => {
                reader.Read(new StringReader(Xml), treeSettings);
            }, Throws.TypeOf<XmlException>().With.InnerException.TypeOf<InvalidOperationException>());
        }

        [TestCase("<a><b>foo</b></a>", "foo", TestName = "ReadElementContentAsStringNoWhitespace")]
        [TestCase("<a><b>\nfoo\n</b></a>", "\nfoo\n", TestName = "ReadElementContentAsStringNewLine")]
        [TestCase("<a><b>  foo  </b></a>", "  foo  ", TestName = "ReadElementContentAsStringSpaces")]
        [TestCase("<a><b>\n\t  foo\n  </b></a>", "\n\t  foo\n  ", TestName = "ReadElementContentAsStringTabs")]
        public void ReadElementContentAsString(string xml, string expectedValue)
        {
            string value = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                ProcessElement = (n, e) => { value = e.Reader.ReadElementContentAsString(); }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(xml));
            Assert.That(value, Is.EqualTo(expectedValue));
        }

        [TestCase("<a><b><c>Text</c></b></a>", TestName = "ProcessElementReadSubTree")]
        [TestCase("<a><b/></a>", TestName = "ProcessElementReadSubTreeEmpty")]
        [TestCase("<a><b></b></a>", TestName = "ProcessElementReadSubTreeNonEmpty")]
        [TestCase("<a><b>Text</b></a>", TestName = "ProcessElementReadSubTreeText")]
        public void ProcessElementReadSubTree(string xml)
        {
            string value = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                ProcessElement = (n, e) => { e.Reader.ReadSubtree(); e.Reader.SkipToEndElement(); },
                                Nodes = {
                                    new XmlTreeNode("c") {
                                        ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(xml));
            Assert.That(value, Is.Null);
        }

        [Test]
        public void ProcessElementReadSubTreeTwice()
        {
            const string Xml = "<a><b><c>Text</c><c>Text2</c></b></a>";

            string value = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                ProcessElement = (n, e) => {
                                    XmlReader subtree = e.Reader.ReadSubtree();
                                    while (subtree.Read()) {
                                        Console.WriteLine("Node: {0}; Name={1}; Value={2}", subtree.NodeType, subtree.Name, subtree.Value);
                                    }
                                    Console.WriteLine("Reader: {0}; Name={1}; Value={2}", e.Reader.NodeType, e.Reader.Name, e.Reader.Value);
                                },
                                Nodes = {
                                    new XmlTreeNode("c") {
                                        ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(Xml));
            Assert.That(value, Is.Null);
        }

        [TestCase("<a><b><c>Text</c><d>Text2</d></b></a>", TestName = "ProcessElementReadString")]
        [TestCase("<a>\n  <b>\n    <c>Text</c>\n    <d>Text2</d>\n  </b>\n</a>", TestName = "ProcessElementReadStringWhiteSpace")]
        public void ProcessElementReadString(string xml)
        {
            string value1 = null;
            string value2 = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                Nodes = {
                                    new XmlTreeNode("c") {
                                        ProcessElement = (n, e) => { value1 = e.Reader.ReadElementContentAsString(); }
                                    },
                                    new XmlTreeNode("d") {
                                        ProcessElement = (n, e) => { value2 = e.Reader.ReadElementContentAsString(); }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(xml));
            Assert.That(value1, Is.EqualTo("Text"));
            Assert.That(value2, Is.EqualTo("Text2"));
        }

        [TestCase("<a><b><c>Text</c></b></a>", TestName = "ProcessElementSkipTree")]
        [TestCase("<a><b/></a>", TestName = "ProcessElementSkipTreeEmpty")]
        [TestCase("<a><b></b></a>", TestName = "ProcessElementSkipTreeNonEmpty")]
        [TestCase("<a><b>Text</b></a>", TestName = "ProcessElementSkipTreeText")]
        [TestCase("<a><b>Text</b><c></c></a>", TestName = "ProcessElementSkipLeft")]
        [TestCase("<a><c>Text</c><b></b></a>", TestName = "ProcessElementSkipRight")]
        public void ProcessElementSkip(string xml)
        {
            string value = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                // Skipping will only work if the reader supports IXmlLineInfo, so we can detect it is
                                // skipped. Otherwise, one must use e.Reader.SkipToEndElement().
                                ProcessElement = (n, e) => { e.Reader.Skip(); },
                                Nodes = {
                                    new XmlTreeNode("c") {
                                        ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(xml));
            Assert.That(value, Is.Null);
        }

        [TestCase("<a><b><c>Text</c></b></a>", TestName = "ProcessElementSkipToEndElementTree")]
        [TestCase("<a><b/></a>", TestName = "ProcessElementSkipToEndElementTreeEmpty")]
        [TestCase("<a><b></b></a>", TestName = "ProcessElementSkipToEndElementTreeNonEmpty")]
        [TestCase("<a><b>Text</b></a>", TestName = "ProcessElementSkipToEndElementTreeText")]
        [TestCase("<a><b>Text</b><c></c></a>", TestName = "ProcessElementSkipToEndElementLeft")]
        [TestCase("<a><c>Text</c><b></b></a>", TestName = "ProcessElementSkipToEndElementRight")]
        public void ProcessElementSkipToEndElement(string xml)
        {
            string value = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                // Skipping will only work if the reader supports IXmlLineInfo, so we can detect it is
                                // skipped. Otherwise, one must use e.Reader.SkipToEndElement().
                                ProcessElement = (n, e) => { e.Reader.SkipToEndElement(); },
                                Nodes = {
                                    new XmlTreeNode("c") {
                                        ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(xml));
            Assert.That(value, Is.Null);
        }

        [TestCase("<a><b attr='y'><c></c></b></a>", TestName = "ProcessElementInAttributeNonEmptySub")]
        [TestCase("<a><b attr='y'><c/></b></a>", TestName = "ProcessElementInAttributeEmptySub")]
        public void ProcessElementInAttribute(string xml)
        {
            int processedC = 0;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                ProcessElement = (n, e) => {
                                    e.Reader.MoveToAttribute(0);
                                    Assert.That(e.Reader.Name, Is.EqualTo("attr"));
                                    Assert.That(e.Reader.Value, Is.EqualTo("y"));
                                },
                                Nodes = {
                                    new XmlTreeNode("c") {
                                        ProcessElement = (n, e) => { processedC++; }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(xml));
            Assert.That(processedC, Is.EqualTo(1));
        }

        [TestCase("<a><b><c>Content</c></b></a>", TestName = "ProcessElementContent")]
        public void ProcessElementContent(string xml)
        {
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                Nodes = {
                                    new XmlTreeNode("c") {
                                        ProcessElement = (n, e) => {
                                            e.Reader.MoveToContent();
                                            Assert.That(e.Reader.Name, Is.EqualTo("c"));
                                            Assert.That(e.Reader.ReadString(), Is.EqualTo("Content"));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(xml));
        }

        [Test]
        public void ProcessCData()
        {
            const string Xml = "<script>\n  <![CDATA[\n    <message> Welcome to my world! </message>\n  ]]>\n</script>";
            const string expectedValue = "\n  \n    <message> Welcome to my world! </message>\n  \n";
            string value = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("script") {
                        ProcessElement = (n, e) => {
                            value = e.Reader.ReadElementContentAsString();
                        }
                    }
                }
            };

            reader.Read(new StringReader(Xml));
            Assert.That(value, Is.EqualTo(expectedValue));
        }

        [Test]
        public void ProcessXmlWithNamespaceNoMgr()
        {
            const string Xml = "<fx:a xmlns:fx=\"urn:namespacetest\"><fx:b>Text</fx:b></fx:a>";

            string value = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("fx", "a") {
                        Nodes = {
                            new XmlTreeNode("fx", "b") {
                                ProcessTextElement = (n, e) => {
                                    Assert.That(e.XmlNamespaceManager, Is.Null);
                                    value = e.Reader.Value;
                                }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(Xml));
            Assert.That(value, Is.EqualTo("Text"));
        }

        [Test]
        public void ProcessXmlWithNamespaceWithMgrDefault()
        {
            const string Xml = "<fx:a xmlns:fx=\"urn:namespacetest\"><fx:b>Text</fx:b></fx:a>";

            Dictionary<string, string> xmlns = new Dictionary<string, string>() {
                ["fx"] = "urn:namespacetest"
            };

            string value = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("fx", "a") {
                        Nodes = {
                            new XmlTreeNode("fx", "b") {
                                ProcessTextElement = (n, e) => {
                                    Assert.That(e.XmlNamespaceManager, Is.Not.Null);
                                    value = e.Reader.Value;
                                }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(Xml), xmlns);
            Assert.That(value, Is.EqualTo("Text"));
        }

        [Test]
        public void ProcessXmlWithNamespaceWithMgrAliased()
        {
            const string Xml = "<fx:a xmlns:fx=\"urn:namespacetest\"><fx:b>Text</fx:b></fx:a>";

            // XML contains the prefix fx = urn:namespacetest. Our tree has the name space prefix 'ho' for the same URI,
            // which should also work (name spaces are resolved as equal, not the prefix).
            Dictionary<string, string> xmlns = new Dictionary<string, string>() {
                ["ho"] = "urn:namespacetest"
            };

            string value = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("ho", "a") {
                        Nodes = {
                            new XmlTreeNode("ho", "b") {
                                ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(Xml), xmlns);
            Assert.That(value, Is.EqualTo("Text"));
        }

        [Test]
        public void ProcessXmlWithNamespaceWithMgrDifferent()
        {
            const string Xml = "<fx:a xmlns:fx=\"urn:namespacetest2\"><fx:b>Text</fx:b></fx:a>";

            // What happens if we have defined a namespace which the original XML uses something different? The
            // namespace used in the original document will be expected, as there is nothing else to reference to.
            Dictionary<string, string> xmlns = new Dictionary<string, string>() {
                ["ho"] = "urn:namespacetest"
            };

            string value = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("fx", "a") {
                        Nodes = {
                            new XmlTreeNode("fx", "b") {
                                ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                            }
                        }
                    }
                }
            };

            // No known root, because 'fx' is not defined in xmlns.
            Assert.That(() => { reader.Read(new StringReader(Xml), xmlns); }, Throws.TypeOf<XmlException>());
            Assert.That(value, Is.Null);
        }

        [Test]
        public void ProcessXmlWithNamespaceWithMgrOverlapped()
        {
            const string Xml = "<fx:a xmlns:fx=\"urn:namespacetest2\"><fx:b>Text</fx:b></fx:a>";

            // What happens if a namespace is defined, but not in the original document, but the prefixes are the same?
            // We will use the namespace in the original XML document, even though there's a namespace collision.
            Dictionary<string, string> xmlns = new Dictionary<string, string>() {
                ["fx"] = "urn:namespacetest"
            };

            string value = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("fx", "a") {
                        Nodes = {
                            new XmlTreeNode("fx", "b") {
                                ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                            }
                        }
                    }
                }
            };

            // No known root, because 'fx' namespace is not the same as 'fx' in the XML file.
            Assert.That(() => { reader.Read(new StringReader(Xml), xmlns); }, Throws.TypeOf<XmlException>());
            Assert.That(value, Is.Null);
        }

        [Test]
        public void ProcessXmlWithNamespaceDefault()
        {
            const string Xml = "<a xmlns=\"urn:namespacetest\"><b>Text</b></a>";

            // The original XML has a default namespace, which will be considered something else while parsing.
            Dictionary<string, string> xmlns = new Dictionary<string, string>() {
                ["fx"] = "urn:namespacetest"
            };

            string value = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("fx", "a") {
                        Nodes = {
                            new XmlTreeNode("fx", "b") {
                                ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(Xml), xmlns);
            Assert.That(value, Is.EqualTo("Text"));
        }

        [Test]
        public void ProcessXmlWithNamespaceDefaultAlias1()
        {
            const string Xml = "<fx:a xmlns:fx=\"urn:namespacetest\"><fx:b>Text</fx:b></fx:a>";

            // The XML uses a prefix for the namespace. Parsing defined what the default namespace should be.
            Dictionary<string, string> xmlns = new Dictionary<string, string>() {
                [""] = "urn:namespacetest"
            };

            string value = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("", "a") {
                        Nodes = {
                            new XmlTreeNode("", "b") {
                                ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(Xml), xmlns);
            Assert.That(value, Is.EqualTo("Text"));
        }

        [Test]
        public void ProcessXmlWithNamespaceDefaultAlias2()
        {
            const string Xml = "<fx:a xmlns:fx=\"urn:namespacetest\"><fx:b>Text</fx:b></fx:a>";

            // The XML uses a prefix for the namespace. Parsing defined what the default namespace should be.
            Dictionary<string, string> xmlns = new Dictionary<string, string>() {
                [""] = "urn:namespacetest"
            };

            string value = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(Xml), xmlns);
            Assert.That(value, Is.EqualTo("Text"));
        }

        [Test]
        public void ProcessXmlSubTreeWithNamespace()
        {
            const string Xml = "<fx:a xmlns:fx=\"urn:namespacetest\"><fx:b><fx:c>Text</fx:c></fx:b></fx:a>";
            string value = null;

            Dictionary<string, string> xmlns = new Dictionary<string, string>() {
                ["fx"] = "urn:namespacetest"
            };

            XmlTreeReader subReader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("fx", "b") {
                        Nodes = {
                            new XmlTreeNode("fx", "c") {
                                ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                            }
                        }
                    }
                }
            };

            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("fx", "a") {
                        Nodes = {
                            new XmlTreeNode("fx", "b") {
                                ProcessElement = (n, e) => { subReader.Read(e.Reader, e.XmlNamespaceManager); }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(Xml), xmlns);
            Assert.That(value, Is.EqualTo("Text"));
        }

        [Test]
        public void ProcessXmlSubTreeWithNoNamespace()
        {
            const string Xml = "<a><b><c>Text</c></b></a>";
            string value = null;

            XmlTreeReader subReader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("b") {
                        Nodes = {
                            new XmlTreeNode("c") {
                                ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                            }
                        }
                    }
                }
            };

            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                ProcessElement = (n, e) => { subReader.Read(e.Reader, e.XmlNamespaceManager); }
                            }
                        }
                    }
                }
            };

            reader.Read(new StringReader(Xml));
            Assert.That(value, Is.EqualTo("Text"));
        }

        [Test]
        public void ExceptionOnUnexpectedText()
        {
            const string Xml = "<a><b><c>Text</c><d>Text2</d></b></a>";

            string value = null;
            bool dnode = false;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                Nodes = {
                                    new XmlTreeNode("c") {
                                        ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                                    },
                                    new XmlTreeNode("d") {
                                        ProcessElement = (n, e) => { dnode = true; }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            Assert.That(() => { reader.Read(new StringReader(Xml)); }, Throws.TypeOf<XmlException>());
            Assert.That(value, Is.EqualTo("Text"));
            Assert.That(dnode, Is.True);
        }

        [Test]
        public void ExceptionOnUnexpectedTextHandledInElement()
        {
            const string Xml = "<a><b><c>Text</c><d>Text2</d></b></a>";

            string value = null;
            string valued = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                Nodes = {
                                    new XmlTreeNode("c") {
                                        ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                                    },
                                    new XmlTreeNode("d") {
                                        ProcessElement = (n, e) => { valued = e.Reader.ReadElementContentAsString(); }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // As the 'ProcessElement' handles the text, no exception should occur.
            Assert.That(() => { reader.Read(new StringReader(Xml)); }, Throws.Nothing);
            Assert.That(value, Is.EqualTo("Text"));
            Assert.That(valued, Is.EqualTo("Text2"));
        }

        [Test]
        public void ExceptionSuppressedOnUnexpectedText()
        {
            const string Xml = "<a><b><c>Text</c><d>Text2</d></b></a>";

            string value = null;
            bool dnode = false;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                Nodes = {
                                    new XmlTreeNode("c") {
                                        ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                                    },
                                    new XmlTreeNode("d") {
                                        ProcessElement = (n, e) => { dnode = true; }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            XmlTreeSettings treeSettings = new XmlTreeSettings() {
                ThrowOnUnhandledText = false
            };

            Assert.That(() => { reader.Read(new StringReader(Xml), treeSettings); }, Throws.Nothing);
            Assert.That(value, Is.EqualTo("Text"));
            Assert.That(dnode, Is.True);
        }

        [Test]
        public void ExceptionOnUnexpectedElement()
        {
            const string Xml = "<a><b><c>Text</c><d>Text2</d></b></a>";

            string value = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                Nodes = {
                                    new XmlTreeNode("d") {
                                        ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            XmlTreeSettings treeSettings = new XmlTreeSettings() {
                ThrowOnUnknownElement = true
            };

            Assert.That(() => { reader.Read(new StringReader(Xml), treeSettings); }, Throws.TypeOf<XmlException>());
            Assert.That(value, Is.Null);
        }

        [Test]
        public void ExceptionOnUnexpectedElementHandled()
        {
            const string Xml = "<a><b><c>Text</c><d>Text2</d></b></a>";

            string value = null;
            string unknown = null;
            string valued = null;
            XmlTreeReader reader = new XmlTreeReader() {
                Nodes = {
                    new XmlTreeNode("a") {
                        Nodes = {
                            new XmlTreeNode("b") {
                                Nodes = {
                                    new XmlTreeNode("c") {
                                        ProcessTextElement = (n, e) => { value = e.Reader.Value; }
                                    },
                                },
                                ProcessUnknownElement = (n, e) => {
                                    if (unknown != null) throw new InvalidOperationException();
                                    unknown = e.Reader.Name;
                                    valued = e.Reader.ReadElementContentAsString();
                                }
                            }
                        }
                    }
                }
            };

            XmlTreeSettings treeSettings = new XmlTreeSettings() {
                ThrowOnUnknownElement = true
            };

            // As the 'ProcessElement' handles the text, no exception should occur.
            Assert.That(() => { reader.Read(new StringReader(Xml), treeSettings); }, Throws.Nothing);
            Assert.That(value, Is.EqualTo("Text"));
            Assert.That(unknown, Is.EqualTo("d"));
            Assert.That(valued, Is.EqualTo("Text2"));
        }
    }
}
