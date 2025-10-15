using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;


using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;


/// <summary>
/// Tests for the KnownMarkups class methods.
/// </summary>
[TestFixture]
public partial class KnownMarkupsTests
{
    private ElementNode CreateMockElementNode()
    {
        var mockXmlType = new Mock<XmlType>();
        var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();

        return new ElementNode(mockXmlType.Object, "", mockNamespaceResolver.Object);
    }

    #region Helper Methods

    private ITypeSymbol CreateMockTypeSymbol()
    {
        return new Mock<ITypeSymbol>().Object;
    }

    private LocalVariable CreateMockLocalVariable(string name, ITypeSymbol type)
    {
        var mockLocalVariable = new Mock<LocalVariable>();
        mockLocalVariable.Setup(x => x.Name).Returns(name);
        mockLocalVariable.Setup(x => x.Type).Returns(type);
        return mockLocalVariable.Object;
    }

    private void SetupPropertiesWithoutName(ElementNode markupNode)
    {
        var properties = new Dictionary<XmlName, INode>();
        Mock.Get(markupNode).Setup(x => x.Properties).Returns(properties);
    }

    private void SetupPropertiesWithName(ElementNode markupNode, string? namespaceUri, ValueNode valueNode)
    {
        var properties = new Dictionary<XmlName, INode>();
        var xmlName = new XmlName(namespaceUri, "Name");
        properties[xmlName] = valueNode;
        Mock.Get(markupNode).Setup(x => x.Properties).Returns(properties);
    }

    private void SetupEmptyCollectionItems(ElementNode markupNode)
    {
        Mock.Get(markupNode).Setup(x => x.CollectionItems).Returns(new List<INode>());
    }

    private void SetupCollectionItemsWithNode(ElementNode markupNode, INode node)
    {
        var collectionItems = new List<INode> { node };
        Mock.Get(markupNode).Setup(x => x.CollectionItems).Returns(collectionItems);
    }

    private void SetupParentContext(SourceGenContext context, SourceGenContext parentContext)
    {
        Mock.Get(context).Setup(x => x.ParentContext).Returns(parentContext);
    }

    private void SetupParentElementNode(ElementNode node, ElementNode parentNode)
    {
        Mock.Get(node).Setup(x => x.Parent).Returns(parentNode);
    }

    private void VerifyDiagnosticReported(SourceGenContext context)
    {
        Mock.Get(context).Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
    }

    #endregion
    private void SetupGetResourceNodeMock(ElementNode? resourceToReturn)
    {
        // Since GetResourceNode is a private static method, we can't easily mock it.
        // The test setup ensures that when it's called with our test parameters,
        // it will follow the expected code path based on the node structure we've created.
    }

    /// <summary>
    /// Helper class for creating test LocalVariable instances.
    /// </summary>
    private class LocalVariable
    {
        public ITypeSymbol Type { get; set; } = null!;
        public string Name { get; set; } = "";
    }

}