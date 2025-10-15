using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml;

#nullable disable
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Unit tests for the CreateValuesVisitor class.
/// </summary>
[TestFixture]
public partial class CreateValuesVisitorTests
{
    /// <summary>
    /// Tests that IsResourceDictionary throws NullReferenceException when node parameter is null.
    /// This test verifies proper null parameter handling for the delegation to the extension method.
    /// Expected result: NullReferenceException should be thrown.
    /// </summary>
    [Test]
    public void IsResourceDictionary_NullNode_ThrowsNullReferenceException()
    {
        // Arrange
        var visitor = new CreateValuesVisitor(null!);
        // Act & Assert
        Assert.Throws<NullReferenceException>(() => visitor.IsResourceDictionary(null!));
    }

    /// <summary>
    /// Tests that Visit method with MarkupNode does not throw exception when parentNode parameter is null.
    /// This test verifies that the empty method implementation handles null parent node gracefully.
    /// Expected result: No exception should be thrown.
    /// </summary>
    [Test]
    public void Visit_MarkupNode_NullParentNode_DoesNotThrow()
    {
        // Arrange
        var visitor = new CreateValuesVisitor(null!);
        var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();
        var markupNode = new MarkupNode("test markup", mockNamespaceResolver.Object);
        // Act & Assert
        Assert.DoesNotThrow(() => visitor.Visit(markupNode, (INode)null!));
    }

    /// <summary>
    /// Tests that Visit method with MarkupNode does not throw exception when both parameters are null.
    /// This test verifies that the empty method implementation handles null inputs gracefully.
    /// Expected result: No exception should be thrown.
    /// </summary>
    [Test]
    public void Visit_MarkupNode_BothParametersNull_DoesNotThrow()
    {
        // Arrange
        var visitor = new CreateValuesVisitor(null!);
        // Act & Assert
        Assert.DoesNotThrow(() => visitor.Visit((MarkupNode)null!, (INode)null!));
    }

    /// <summary>
    /// Tests that Visit method with MarkupNode handles extreme line position values.
    /// This test verifies behavior with MarkupNode created with boundary line number and position values.
    /// Expected result: Method executes without throwing exceptions.
    /// </summary>
    [TestCase(int.MinValue, int.MinValue)]
    [TestCase(int.MaxValue, int.MaxValue)]
    [TestCase(0, 0)]
    [TestCase(-1, -1)]
    public void Visit_MarkupNode_ExtremeLineValues_ExecutesSuccessfully(int lineNumber, int linePosition)
    {
        // Arrange
        var mockContext = new Mock<SourceGenContext>();
        var visitor = new CreateValuesVisitor(mockContext.Object);
        var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();
        var mockParentNode = new Mock<INode>();
        var markupNode = new MarkupNode("test markup", mockNamespaceResolver.Object, lineNumber, linePosition);
        // Act & Assert
        Assert.DoesNotThrow(() => visitor.Visit(markupNode, mockParentNode.Object));
    }
}

/// <summary>
/// Unit tests for the CreateValuesVisitor.Visit(ElementNode, INode) method.
/// </summary>
[TestFixture]
public partial class CreateValuesVisitorVisitElementNodeTests
{
}
