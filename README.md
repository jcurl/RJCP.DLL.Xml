# XML <!-- omit in toc -->

This assembly contains functions which are useful for parsing XML trees.

- [1. Parsing XML using Tree-Like Lambdas](#1-parsing-xml-using-tree-like-lambdas)
- [2. Release History](#2-release-history)
  - [2.1. Version 0.2.1](#21-version-021)
  - [2.2. Version 0.2.0](#22-version-020)

## 1. Parsing XML using Tree-Like Lambdas

The `XmlTreeReader` allows for defining a datastructure, that is similar to the
structure of the XML that should be read, to be interpreted, by executing
lambda's within the datastructure.

This has the benefit that:

- No `XmlDocument` is needed. Convert an XML into your own datastructure while
  it is being parsed and potentially save memory.
- Define the structure for reading, that it is easily readable, similar to the
  structure of the XML file.

Detailed usage is given in the repository
[XmlTreeReader.md](docs/XmlTreeReader.md)

For example:

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

Lambdas are preferred over events, that keeps the code small and concise.

## 2. Release History

### 2.1. Version 0.2.1

Bugfixes:

- XML: Fix possible `NullReferenceException` on `OnProcessUnknownElement`
  (DOTNET-832)

Quality

- Add README.md reference to NuGet package (DOTNET-814, DOTNET-932)
- Upgrade from .NET Standard 2.1 to .NET 6.0 (keep .NET 4.0) (DOTNET-833,
  DOTNET-936, DOTNET-942, DOTNET-945, DOTNET-959, DOTNET-961, DOTNET-962)
- XmlTreeReader: Rework `null` constants (DOTNET-942)
- NET 6.0: Use the argument name as a string (DOTNET-965)

### 2.2. Version 0.2.0

- Initial Release
