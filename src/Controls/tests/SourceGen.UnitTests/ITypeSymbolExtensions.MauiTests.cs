using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Moq;
using NUnit.Framework;


namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Unit tests for the GetBPTypeAndConverter method in ITypeSymbolExtensions.
/// </summary>
[TestFixture]
public partial class ITypeSymbolExtensionsTests
{
    /// <summary>
    /// Tests that GetBPTypeAndConverter returns null when field name does not end with "Property".
    /// This test targets the early return condition on line 34.
    /// </summary>
    [TestCase("TestField")]
    [TestCase("SomeProperty2")]
    [TestCase("PropertyTest")]
    [TestCase("Proper")]
    [TestCase("")]
    [TestCase("P")]
    [TestCase("Propert")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void GetBPTypeAndConverter_FieldNameDoesNotEndWithProperty_ReturnsNull(string fieldName)
    {
        // Arrange
        var mockFieldSymbol = new Mock<IFieldSymbol>();
        var mockContext = new Mock<SourceGenContext>();

        mockFieldSymbol.Setup(f => f.Name).Returns(fieldName);

        // Act
        var result = ITypeSymbolExtensions.GetBPTypeAndConverter(mockFieldSymbol.Object, mockContext.Object);

        // Assert
        Assert.IsNull(result);
    }

}
