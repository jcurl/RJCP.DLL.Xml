# XmlTreeReader <!-- omit in toc -->

The `XmlTreeReader` reads XML files with a small memory footprint, based on a
tree structure in code that maps similarly to the tree structure of an XML being
parsed.

The core implementation uses the `System.Xml.XmlReader` class to do the reading,
so only the elements being read are in memory at the time, and not the entire
XML file.

## Table of Contents <!-- omit in toc -->

- [1. Getting Started](#1-getting-started)
  - [1.1. Simple Usage of the XmlTreeReader without Namespaces](#11-simple-usage-of-the-xmltreereader-without-namespaces)
- [2. Parsing a XML Tree](#2-parsing-a-xml-tree)
  - [2.1. Defining the Tree Structure](#21-defining-the-tree-structure)
  - [2.2. Information Provided in Callbacks](#22-information-provided-in-callbacks)
  - [2.3. Parsing Known Elements](#23-parsing-known-elements)
    - [2.3.1. Stack Behaviour of UserObject](#231-stack-behaviour-of-userobject)
  - [2.4. Parsing Unknown Elements](#24-parsing-unknown-elements)
  - [2.5. Parsing Text Elements](#25-parsing-text-elements)
  - [2.6. Parsing CDATA](#26-parsing-cdata)
  - [2.7. Parsing the End of an Element](#27-parsing-the-end-of-an-element)
- [3. XmlTreeSettings for Reader Behavior](#3-xmltreesettings-for-reader-behavior)
  - [3.1. Default Behavior](#31-default-behavior)
  - [3.2. Throwing an Exception on Unhandled Elements](#32-throwing-an-exception-on-unhandled-elements)
  - [3.3. Throwing an Exception on Unhandled Text](#33-throwing-an-exception-on-unhandled-text)
- [4. Parsing Sub-Trees](#4-parsing-sub-trees)
  - [4.1. Manually Skipping over a Section in the ProcessElement](#41-manually-skipping-over-a-section-in-the-processelement)
  - [4.2. Using XmlReader.ReadSubTree](#42-using-xmlreaderreadsubtree)
  - [4.3. Using XmlTreeReader.Read for Subtrees](#43-using-xmltreereaderread-for-subtrees)
  - [4.4. Reading Subtrees and Passing the XmlTreeSettings](#44-reading-subtrees-and-passing-the-xmltreesettings)
- [5. Managing Namespaces](#5-managing-namespaces)
  - [5.1. Defining and Using Namespaces in Code](#51-defining-and-using-namespaces-in-code)
  - [5.2. Prefixes without Namespaces](#52-prefixes-without-namespaces)
  - [5.3. The Default Namespace](#53-the-default-namespace)
    - [5.3.1. The Default Namespace as Empty](#531-the-default-namespace-as-empty)
  - [5.4. Undefined Namespaces](#54-undefined-namespaces)
    - [5.4.1. Prefix Collisions](#541-prefix-collisions)
  - [5.5. Namespaces and Sub-Trees](#55-namespaces-and-sub-trees)

## 1. Getting Started

### 1.1. Simple Usage of the XmlTreeReader without Namespaces

Let's start with a simple XML file:

```xml
<root>
  <sub>Line1</sub>
  <sub>Line2</sub>
</root>
```

The code set to read the XML file is simple:

```csharp
List<string> myData = null;

// Describes an XML format that only has a single root node.
XmlTreeReader reader = new XmlTreeReader() {
    Nodes = {
        new XmlTreeNode("root") {
            ProcessElement = (n, e) => { e.UserObject = new List<string>(); },
            Nodes = {
                new XmlTreeNode("sub") {
                    ProcessTextElement = (n, e) => { ((List<string>)e.UserObject).Add(e.Reader.Value); },
                }
            }
            ProcessEndElement = (n, e) => { myData = (List<string>)e.UserObject; },
        }
    }
};

reader.Read(file);

// Here, myData will contain two elements, Line1 and Line2.
```

The code defines the tree like structure of the original XML. As elements are
parsed, callbacks are executed as seen in the C# code.

When the `root` node is parsed, the List object is instantiated and assigned to
the `e.UserObject`. The `u.UserObject` is propagated through to the
sub-elements. This way, it is possible to use the recursive tree structure to
parse XML datastructures simply.

## 2. Parsing a XML Tree

### 2.1. Defining the Tree Structure

The root of the XML tree is defined by the `XmlTreeReader`. It derives from the
`XmlTreeNode`. The `XmlTreeReader` defines the ability to read XML, the
`XmlTreeNode` defines the structure of the XML tree.

From the root node, construct a list of all subnodes that should be parsed, by
adding to the `XmlTreeNode.Nodes` collection. This is a collection of other
`XmlTreeNode` objects. It is this datastructure that defines the tree.

Each node can have callbacks defined as delegates. Delegates are used instead of
events, to allow in-line in C# with the simplified list initialization.

The callbacks are:

- `XmlTreeNode.ProcessElement`: Used when an element is parsed, such as
  `<root>`. In this callback, one can parse the attributes associated with the
  element.
- `XmlTreeNode.ProcessEndElement`: Used when the end tag for the element is
  parsed, or called after the `ProcessElement` if that element is empty.
- `XmlTreeNode.ProcessUnknownElement`: Called where there's no `XmlTreeNode` for
  that element defined in the `Nodes` collection for the current node.
- `XmlTreeNode.ProcessTextElement`: Get the text content of the current element.

### 2.2. Information Provided in Callbacks

In all the callbacks, the object passed is the `XmlNodeEventArgs`. This object
contains the current `XmlReader` object `Reader` and any user data as
`UserObject`.

Only a single `XmlNodeEventArgs` is allocated while reading the tree. Storing a
copy of the reference of the `XmlNodeEventArgs` should not be done, as it's
contents generally change while the XML Tree structure is being traversed.
Specifically, the `UserObject` is automatically updated to be relevant for the
current depth while parsing the XML Tree structure. If the `UserObject` must be
stored, make a copy of that reference explicitly, and not the
`XmlNodeEventArgs`.

### 2.3. Parsing Known Elements

Known elements are those which are defined in the current node's `Nodes`
collection. These are generally the interesting nodes that your application
wants to read and know about.

Operations typically done in this callback are to:

- Read the attributes for the current element;
- Create new datastructures, and allow that to be passed to elements that are
  under this current element.

#### 2.3.1. Stack Behaviour of UserObject

Parsing the XML Tree structure is done using a depth first search algorithm,
which is stack based. At each Element of the XML tree, the
`XmlNodeEventArgs.UserObject` is the value of the parent element. The
`UserObject` can be replaced with something different to pass data to children
of the current element.

This can simplify user code by having to remove the need for maintaining cursor
variables for creating tree like structures when reading an XML file. For
example:

```csharp
ProcessElement = (n, e) => { e.UserObject = new List<string>(); },
```

The old value of `e.UserObject` is automatically placed on a stack and restored
when the current node is out of scope. Such a stack based approach ensures that
the correct datastructure is used for subelements, which is very important when
creating lists of lists for example.

Imagine the structure:

```xml
<root>
  <item name="Item1">
    <subitem>value1</subitem>
    <subitem>value2</subitem>
  </item>
  <item name="Item2">
    <subitem>value3</subitem>
    <subitem>value4</subitem>
  </item>
</root>
```

One can define a parser like so:

```csharp
Dictionary<string, List<string>> myData = null;

// Describes an XML format that only has a single root node.
XmlTreeReader reader = new XmlTreeReader() {
    Nodes = {
        new XmlTreeNode("root") {
            ProcessElement = (n, e) => { e.UserObject = new Dictionary<string, List<string>>(); },
            Nodes = {
                new XmlTreeNode("item") {
                    ProcessElement = (n, e) => {
                        string name = e.Reader["name"];
                        List<string> newList = new List<string>();
                        ((Dictionary<string, List<string>>)e.UserObject).Add(name, newList);
                        e.UserObject = newList;
                    },
                    Nodes = {
                        new XmlTreeNode("subitem") {
                            ProcessTextElement = (n, e) {
                                ((List<string>)e.UserObject).Add(e.Reader.Value);
                            }
                        }
                    }
                }
            }
            ProcessEndElement = (n, e) => { myData = (List<string>)e.UserObject; },
        }
    }
};

reader.Read(file);
```

Observe the recursive (stack like) nature of parsing. Parsing `e.UserObject`
through the tree allows for the object to be the correct value.

After processing an element, it is expected that:

- Nothing has been read, so the cursor has not changed; or
- If data was read, the cursor is at:
  - the beginning of the next Element at the same depth of the element when
    called; or
  - the end of the parent element; or
  - the end of the current element (and the name is the same); or
  - There is whitespace, which will be skipped over, until an element/end
    element is reached.

Stack checking is performed while processing the elements. An exception will be
raised if the current position of the `XmlReader` doesn't match what is
expected.

### 2.4. Parsing Unknown Elements

When parsing unknown elements, the callback `ProcessUnknownElement` for the
current node will be called.

If it is not allowed to have an unknown element, an exception should be thrown
within this callback to abort the read process.

If the unknown element should be ignored, simply do not provide the callback. It
will automatically be skipped.

Processing an unknown element is similar to processing an element. After
processing an unknown element, the same rules apply as for processing an
element.

### 2.5. Parsing Text Elements

The callback `ProcessTextElement` is provided to allow a simple parse of the
text element present. The text element is `e.Reader.Value`.

```csharp
ProcessTextElement = (n, e) {
    ((List<string>)e.UserObject).Add(e.Reader.Value);
}
```

As an alternative to parsing text elements with the `ProcessTextElement`
callback, one can parse the content within the `ProcessElement` callback:

```csharp
ProcessElement = (n, e) => {
    ((List<string>)e.UserObject).Add(e.Reader.ReadElementContentAsString());
}
```

### 2.6. Parsing CDATA

To read CDATA, one must use the `ProcessElement` callback.

Let's say there is the following XML

```xml
<script>
  <![CDATA[
    <message> Welcome to my world! </message>
  ]]>
</script>
```

Then the callback code would look like:

```csharp
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
```

Reading that file would result in the content of `value` to contain the string
(note that ¶ shown below, is only to highlight a paragraph)

```text
¶
  ¶
    <message> Welcome to my world! </message>¶
  ¶
```

### 2.7. Parsing the End of an Element

The end of an element is the end tag in XML, e.g. `</root>`. This element
indicates that the parsing is complete. The `XmlNodeEventArgs.UserObject` given
as part of `ProcessEndElement` is the value of the user object that was present
on the exit of the previous call to `ProcessElement`.

## 3. XmlTreeSettings for Reader Behavior

### 3.1. Default Behavior

The most common use case is to have an XML document without an associated XML
schema defining the structure of the input document. This makes it easy to use
XML for reading and writing configuration files. If it is required that the XML
document conforms strictly to a schema, a schema file must be provided with the
`XmlReaderSettings` when instantiating the `XmlReader`.

Otherwise, when reading an XML tree, the default behavior is to raise an
exception on unexpected text, but to skip over unknown elements without raising
an error. This is common behavior when reading configuration files, so that
unknown configuration sections are ignored, but it is not expected that the
structure of the configuration file changes.

The default behavior is defined with:

```csharp
new XmlTreeSettings() {
  ThrowOnUnknownElement = false,
  ThrowOnUnhandledText = true
}
```

### 3.2. Throwing an Exception on Unhandled Elements

If it is desired that an exception be automatically thrown when an unknown
element is raised, set the `ThrowOnUnknownElement` to `true`. If an
`XmlTreeNode` is parsed and a node is found which is not in the `Nodes` list of
that object, and the delegate `ProcessUnknownElement` is not defined, an
`XmlException` shall be raised.

This allows to throw an exception, and explicitly define the delegate where the
exception should not be raised.

### 3.3. Throwing an Exception on Unhandled Text

The default behavior is to throw an exception `XmlException` when an
`XmlNodeType.Text` is found where that node does not have a delegate for
`ProcessTextElement`. A common reason for not handling a text element explicitly
is because text is not expected at this node.

If the `ProcessElement` delegate reads the current nodes text with
`e.Reader.ReadContentElementAsString()` or similar, then this will skip over the
text element and an exception will not be raised as expected.

## 4. Parsing Sub-Trees

Each element of an XML document may be seen as the root of a new tree. The
recursive nature of XML makes it ideal for delegating functionality for reading
a subtree to other functions, or even other assemblies.

A design might require that another function be responsible for reading a
section of XML. That function may be in the same class, or in a different
assembly and may be implemented without the support of the `XmlTreeReader`. For
example, some other functions might require a call to load state from a subtree
section XML. The tree structure of XML easily lends to such a design pattern.

It could be that a portion of the XML is read, and that the programmer has no
control over the function that must read this code, so it is not possible to
implement that subtree inside the definition of the `XmlTreeReader`.

### 4.1. Manually Skipping over a Section in the ProcessElement

As the basics, when passing control to another function to continue reading the
`XmlReader` it is seen by the current `XmlTreeReader` as if the current element
is skipped.

If a section should be skipped, one can call either:

```csharp
ProcessElement = (n, e) => {
    e.Reader.Skip());
}
```

or the extension method

```csharp
ProcessElement = (n, e) => {
    e.Reader.SkipToEndElement();
}
```

There is a small difference between the two: The `Skip()` method will jump over
the current element, repositioning the reader to the start of the next element.
The `XmlTreeReader` uses the functionality of the `XmlReader.GetPosition()` to
know that the position has changed, and that the depth has not changed (or it is
now at the end element of the parent node).

The `SkipToEndElement()` extension method manually reads the structure and
repositions the `XmlReader` to the `EndElement` of the current node.

Both cases are handled in the `XmlTreeReader` and are equivalent in code, as the
case when `GetPosition()` is not supported raises an exception and parsing
cannot continue.

### 4.2. Using XmlReader.ReadSubTree

If an implementation already exists that takes an `XmlReader` to parse through
an XML document, it can be given the results of the `XmlReader.ReadSubTree()`
method.

```csharp
new XmlTreeNode("b") {
    ProcessElement = (n, e) => {
        OtherObj.OtherFunc(e.Reader.ReadSubtree());
    }
}
```

As that subtree is read, the cursor of the `e.Reader` object is automatically
updated also.

Let's say that the implementation of `OtherObj.OtherFunc` is trivial:

```csharp
public static void OtherFunc(XmlReader reader) {
    while (reader.Read()) {
        Console.WriteLine("Node: {0}; Name={1}; Value={2}", reader.NodeType, reader.Name, reader.Value);
    }
}
```

Then with the following XML:

```xml
<a>
  <b>
    <c>Text</c>
    <c>Text2</c>
  </b>
</a>
```

at the end of `ProcessElement` on node `<b>`, it will now be pointing to the end
element of node `</c>`. The output of the program would show that the XML
subtree given to `OtherFunc()` is node `<b>..</b>`.

The output of the program would show:

```text
Node: Element; Name=b; Value=
Node: Element; Name=c; Value=
Node: Text; Name=; Value=Text
Node: EndElement; Name=c; Value=
Node: Element; Name=c; Value=
Node: Text; Name=; Value=Text2
Node: EndElement; Name=c; Value=
Node: EndElement; Name=b; Value=
```

It is important that the `OtherFunc` read until the end element for the current
node, or one step immediately after (to the element of the next node, or the end
element of the parent node).

### 4.3. Using XmlTreeReader.Read for Subtrees

A design may split parsing a tree into multiple `XmlTreeReader` objects. There
are two mechanisms to parsing a subtree, with subtle differences in the way that
the `XmlReader` works.

The first is to pass the `e.Reader.ReadSubTree()` to a new instance
`XmlTreeReader.Read()`. The new instance of the `XmlReader` provided by
`ReadSubTree()` starts at the beginning like any other newly instantiated
`XmlReader` objects. Reading occurs until the `XmlReader.Read()` returns
`false`, in which the subtree is completely read. The original `XmlReader`
position is automatically advanced.

The second technique is to just pass the `e.Reader` object to a new instance of
`XmlTreeReader.Read()`. This is slightly different in that the new
`XmlReader.NodeType` remains as `XmlNodeType.Element` and the `XmlReader.Value`
is the name of the current element. This may be useful in knowing what the
current root is when reading.

The `XmlTreeReader.Read()` method checks the current state of the
`XmlReader.NodeType`. If it is `XmlNodeType.None`, it assumes that there is new
tree, otherwise, it assumes that it is continuing reading and will parse to the
end of the current subtree.

As an example of how reading subtrees using `XmlTreeReader` works:

```csharp
public void ReadTree() {
  bool gotSub = false;
  XmlTreeReader readerRoot = new XmlTreeReader() {
    Nodes = {
      new XmlTreeNode("root") {
        Nodes = {
          new XmlTreeNode("sub") {
            ProcessElement = (n, e) => { gotSub = ReadSubTree(e.Reader); }
          }
        }
      }
    }
  };

  readerRoot.Read(new StringReader(xml));
  if (gotSub) Console.WriteLine("Found 'x'");
}

public bool ReadSubTree(XmlReader reader) {
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

  readerSub.Read(reader);
  return readSub;
}
```

Interestingly, say that the `ReadSubTree` method should work no matter the name
of the current element. The example above will only work if the subtree element
is `sub`. The function `ReadSubTree` can be slightly modified so that:

```csharp
public void ReadTree() {
  bool gotSub1 = false;
  XmlTreeReader readerRoot = new XmlTreeReader() {
    Nodes = {
      new XmlTreeNode("root") {
        Nodes = {
          new XmlTreeNode("sub") {
            ProcessElement = (n, e) => { gotSub1 = ReadSubTree(e.Reader); }
          },
          new XmlTreeNode("sub2") {
            ProcessElement = (n, e) => { gotSub2 = ReadSubTree(e.Reader); }
          }
        }
      }
    }
  };

  readerRoot.Read(new StringReader(xml));
  if (gotSub) Console.WriteLine("Found 'x'");
}

public bool ReadSubTree(XmlReader reader) {
  bool readSub = false;
  XmlTreeReader readerSub = new XmlTreeReader() {
    Nodes = {
      // The root node in the subtree must be the same as the node which is being processed. If it isn't,
      // an exception will be raised that no known root can be found.
      new XmlTreeNode(reader.Value) {
        Nodes = {
          new XmlTreeNode("x") {
            ProcessElement = (n, e) => { readSub = true; }
          }
        }
      }
    }
  };

  readerSub.Read(reader);
  return readSub;
}
```

As on entry of the modified `ReadSubTree` method the current state of
`reader.NodeType` is `XmlNodeType.Element`, the actual node can be used to
dynamically define the root of the subtree. This allows reuse where an XML
fragment might have content with the encapsulating element changing. e.g.

```xml
<root>
  <sub>
    <x/>
  </sub>
  <sub2>
    <x/>
  </sub2>
</root>
```

Note, the example above wouldn't work if it were called with
`ReadSubTree(e.Reader.ReadSubTree())` because the method `ReadSubTree` won't be
able to get the current element name `reader.Value` as the type would not be
`XmlNodeType.Element`, but is instead `XmlNodeType.None`.

### 4.4. Reading Subtrees and Passing the XmlTreeSettings

When reading sub-trees, the current `XmlTreeSettings` are part of the event being handled.
A copy of the `XmlTreeSettings` is provided, that it can be given to other read methods.

```csharp
XmlTreeSettings xmlTreeSettings = new XmlTreeSettings() {
    ThrowOnUnhandledText = false
};

bool readSub = false;
XmlTreeReader readerSub = new XmlTreeReader() {
    Nodes = {
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
            ProcessElement = (n, e) => { readerSub.Read(e.Reader, e.TreeSettings); }
        }
    }
};

readerRoot.Read(new StringReader(xml), xmlTreeSettings);
```

In the example above, the root tree ignores unhandled text, and propagates these
settings to reading the subtree also through the `e.TreeSettings` property.

## 5. Managing Namespaces

XML has the concept of namespaces for XML elements by defining the `xmlns` tag
and the prefix `xmlns:prefix`, followed by a namespace string. Two XML documents
are identical if, for each XML element, the same XML element in the second
document has the same namespace, even if their prefix for that namespace is
different, as the next two documents are identical:

```xml
<fx:a xmlns:fx="urn:namespacetest">
  <fx:b>Text</fx:b>
</fx:a>
```

and

```xml
<a xmlns="urn:namespacetest">
  <b>Text</b>
</a>
```

but is different to the following because the elements now belong to a different
namespace:

```xml
<fx:a xmlns:fx="urn:other">
  <fx:b>Text</fx:b>
</fx:a>
```

A naive implementation parsing with the `XmlReader` based on the `Name` property
only would result in the first and second document being incompatible, and
possibly believing the first and third are identical, which would be considered
a violation of the XML specification.

### 5.1. Defining and Using Namespaces in Code

When creating an XML parser that uses namespaces, it is necessary to define the
namespace for each element in the `XmlTreeReader` tree structure. Not doing so
assumes that the default namespace should be used.

When reading an XML with the `XmlTreeReader`, provide a dictionary of mapping
prefixes to namespaces. The namespaces are actually the prefixes used with the
`XmlTreeNode` objects when being instantiated, and not necessarily the same
prefixes used in the document being read.

For example, to read the XML:

```xml
<fx:a xmlns:fx="urn:namespacetest">
  <fx:b>Text</fx:b>
</fx:a>
```

Then the structure definition and reader may look like:

```csharp
Dictionary<string, string> xmlns = new Dictionary<string, string>() {
    ["fx"] = "urn:namespacetest"
};

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

reader.Read(new StringReader(xml), xmlns);
```

Note, the actual namespaces to use are defined when reading the structure, not
when creating the tree.

Entirely equivalent code for successfully reading the exact same input XML could
be to change the namespace in code used:

```csharp
Dictionary<string, string> xmlns = new Dictionary<string, string>() {
    ["ho"] = "urn:namespacetest"
};

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

reader.Read(new StringReader(xml), xmlns);
```

When an XML file is read, the prefix `fx` in the first example, or `ho` in the
second example, in code maps to the namespace `urn:namespacetest`. It is
entirely irrelevant what the prefix is defined as in the XML document, so long
as the namespace in the XML document is identical. If the following XML document
were used to by read with either of the above code modules, it would fail, as
the namespace is not the same as required for reading:

```xml
<fx:a xmlns:fx="urn:other">
  <fx:b>Text</fx:b>
</fx:a>
```

### 5.2. Prefixes without Namespaces

If a dictionary is not provided to the `Read` function for reading the XML, all
comparisons are made using the `XmlReader.Name` field, which is a combination of
the prefix and the local name.

So if the following code were used to read the XML given in the previous section:

```csharp
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

reader.Read(new StringReader(xml));
```

it would only work if the original XML file also had a prefix with `fx`. But it
would not matter what the namespace is, it is the prefix that would have to
match here.

This isn't conformant when reading XML.

### 5.3. The Default Namespace

XML has the concept of a default namespace. The default namespace used in the
XML document is not the same as the default namespace used when reading the XML
document. The concept of the default namespace for the `XmlTreeReader` is
specifically what namespace to use when no namespace is given to the constructor
of an `XmlTreeNode` object.

So the following code defines a default namespace, which is able to read the XML
document provided earlier in this section:

```csharp
Dictionary<string, string> xmlns = new Dictionary<string, string>() {
    [""] = "urn:namespacetest"
};

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

reader.Read(new StringReader(xml), xmlns);
```

#### 5.3.1. The Default Namespace as Empty

Similarly, some code might require the default namespace being empty. When
providing a namespace mapping, it is required that the default empty namespace
must be provided if it is expected to be used:

```csharp
Dictionary<string, string> xmlns = new Dictionary<string, string>() {
    [""] = "",
    ["ho"] = "http://www.asam.net/xml",
    ["fx"] = "http://www.asam.net/xml/fbx",
};
```

### 5.4. Undefined Namespaces

It may be that an XML file is being read that contains elements with namespaces
that is not foreseen in the original code.

When the parser looks to the list of children nodes for the current node, it
tries to map the current namespace for the XML document being read to find the
prefix that was defined with the `xmlns` dictionary. If the `xmlns` contains
such a mapping, then this is what determines the equivalency between the prefix
in the document and the prefix defined in code. e.g.

```xml
<fx:a xmlns:fx="urn:namespacetest">
</fx:a>
```

is equivalent to

```csharp
Dictionary<string, string> xmlns = new Dictionary<string, string>() {
    ["ho"] = "urn:namespacetest"
};

XmlTreeReader reader = new XmlTreeReader() {
    Nodes = {
        new XmlTreeNode("ho", "a") {
            ...
        }
    }
};
```

So while the `XmlReader` has the namespace of `urn:Namespacetest`, the lookup
maps to `ho` (even if in the XML this is `fx`) and so equivalency between the
namespaces defined in the XML and parsing the XML are found.

If the namespace is not defined in `xmlns`, then such a reverse mapping doesn't
exist, and so the node being sought for is a combination of the prefix and local
name as defined in the XML file.

So if the XML being parsed is:

```xml
<fx:a xmlns:fx="urn:namespacetest">
  <baz:a xmlns:bas="urn:other">Text</baz:a>
</fx:a>
```

then `baz:a` will be the node sought for as a child to `fx:a`.

#### 5.4.1. Prefix Collisions

When a dictionary of namespaces are provided when parsing the XML document with
`XmlTreeReader`, any unknown namespace in the original document is necessarily
ignored.

This is necessarily so, as there is no prefix to map the unknown namespace to!
The prefix in the original XML cannot be used, as there may be a mapping from a
namespace to the same prefix in code. Thus two elements would be parsed as being
equivalent, when by the XML standard they differ.

For example, the following would fail with an unknown root node:

```xml
<fx:a xmlns:fx="urn:other">
  <fx:b>Text</fx:b>
</fx:a>
```

and the code:

```csharp
Dictionary<string, string> xmlns = new Dictionary<string, string>() {
    ["fx"] = "urn:namespacetest"
};

XmlTreeReader reader = new XmlTreeReader() {
    Nodes = {
        new XmlTreeNode("fx", "a") {
            ...
        }
    }
};
```

The code is looking for elements in the namespace `urn:namespacetest`, where the
elements in the original XML belong to the namespace `urn:other` and is thus
different.

### 5.5. Namespaces and Sub-Trees

When parsing subtrees, the namespace given for the root parsing is not automatically
passed to the subtree. The namespace must be explicitly provided to work.

For this purpose, the callback event arguments `XmlNodeEventArgs` contains a
property called `XmlNamespaceManager`, which can be used to pass to the subtree
`XmlTreeReader.Read(XmlReader, XmlNamespaceManager)` method overload. To ensure
that namespaces are properly passed in the subtree, use code similar to below:

```csharp
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
```
