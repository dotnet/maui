using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.SourceGen.TypeConverters;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.TypeConverters;
/// <summary>
/// Tests for the RowDefinitionCollectionConverter class.
/// </summary>
public partial class RowDefinitionCollectionConverterTests
{
    private RowDefinitionCollectionConverter _converter = null !;
    private Mock<BaseNode> _mockNode = null !;
    private Mock<ITypeSymbol> _mockToType = null !;
    private Mock<SourceGenContext> _mockContext = null !;
    private Mock<Compilation> _mockCompilation = null !;
    private Mock<INamedTypeSymbol> _mockRowDefinitionCollectionType = null !;
    private Mock<GridLengthConverter> _mockGridLengthConverter = null !;
    [SetUp]
    public void SetUp()
    {
        _converter = new RowDefinitionCollectionConverter();
        _mockNode = new Mock<BaseNode>();
        _mockToType = new Mock<ITypeSymbol>();
        _mockContext = new Mock<SourceGenContext>();
        _mockCompilation = new Mock<Compilation>();
        _mockRowDefinitionCollectionType = new Mock<INamedTypeSymbol>();
        _mockGridLengthConverter = new Mock<GridLengthConverter>();
        // Setup basic mocks
        _mockContext.Setup(x => x.Compilation).Returns(_mockCompilation.Object);
        _mockCompilation.Setup(x => x.GetTypeByMetadataName("Microsoft.Maui.Controls.RowDefinitionCollection")).Returns(_mockRowDefinitionCollectionType.Object);
        _mockRowDefinitionCollectionType.Setup(x => x.ToFQDisplayString()).Returns("Microsoft.Maui.Controls.RowDefinitionCollection");
    }

    /// <summary>
    /// Tests that Convert handles values with leading and trailing commas.
    /// </summary>
    [TestCase(",100,*,")]
    [TestCase(",,,")]
    [TestCase("100,")]
    [TestCase(",100")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void Convert_ValueWithExtraCommas_HandlesCorrectly(string value)
    {
        // Arrange
        var gridLengthConverter = new GridLengthConverter();
        string[] lengths = value.Split([',']);
        foreach (string length in lengths)
        {
            SetupValidGridLengthConversion(gridLengthConverter, length, string.IsNullOrEmpty(length) ? "default" : $"converted_{length}");
        }

        // Act
        string result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.IsTrue(result.StartsWith("new Microsoft.Maui.Controls.RowDefinitionCollection(["));
        Assert.IsTrue(result.EndsWith("])"));
    }

    private void SetupValidGridLengthConversion(GridLengthConverter converter, string input, string output)
    {
        // Since GridLengthConverter is a concrete class and we can't easily mock it,
        // we'll use the real implementation and setup the context appropriately
        var mockGridLengthType = new Mock<INamedTypeSymbol>();
        var mockGridUnitType = new Mock<INamedTypeSymbol>();
        mockGridLengthType.Setup(x => x.ToFQDisplayString()).Returns("Microsoft.Maui.GridLength");
        mockGridUnitType.Setup(x => x.ToFQDisplayString()).Returns("Microsoft.Maui.GridUnitType");
        _mockCompilation.Setup(x => x.GetTypeByMetadataName("Microsoft.Maui.GridLength")).Returns(mockGridLengthType.Object);
        _mockCompilation.Setup(x => x.GetTypeByMetadataName("Microsoft.Maui.GridUnitType")).Returns(mockGridUnitType.Object);
    }
}