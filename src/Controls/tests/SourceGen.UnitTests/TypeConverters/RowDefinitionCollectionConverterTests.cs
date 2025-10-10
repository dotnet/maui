using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.SourceGen.TypeConverters;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
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

    /// <summary>
    /// Tests that Convert returns "default" and reports conversion failed when value is whitespace only.
    /// Should call ReportConversionFailed and return "default" for whitespace-only input.
    /// </summary>
    [TestCase("   ")]
    [TestCase("\t")]
    [TestCase("\n")]
    [TestCase("\r\n")]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void Convert_WhitespaceValue_ReturnsDefaultAndReportsFailure(string value)
    {
        // Arrange & Act
        string result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.AreEqual("default", result);
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), value, It.IsAny<DiagnosticDescriptor>()), Times.Once);
    }

    /// <summary>
    /// Tests that Convert processes multiple valid values correctly.
    /// Should create multiple RowDefinitions with the converted GridLength values.
    /// </summary>
    [TestCase("100,*")]
    [TestCase("Auto,100,*")]
    [TestCase("100,2*,Auto")]
    [TestCase("*,*,*")]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void Convert_MultipleValidValues_ReturnsCorrectRowDefinitionCollection(string value)
    {
        // Arrange
        string[] expectedSegments = value.Split(',');
        // Act
        string result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.IsTrue(result.StartsWith("new Microsoft.Maui.Controls.RowDefinitionCollection(["));
        Assert.IsTrue(result.EndsWith("])"));
        // Count occurrences of "new RowDefinition(" should match number of segments
        int rowDefinitionCount = result.Split("new RowDefinition(").Length - 1;
        Assert.AreEqual(expectedSegments.Length, rowDefinitionCount);
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests that Convert handles values with empty segments from commas correctly.
    /// Should process only non-empty segments and create corresponding RowDefinitions.
    /// </summary>
    [TestCase(",100")]
    [TestCase("100,")]
    [TestCase(",100,")]
    [TestCase("100,,*")]
    [TestCase(",,,")]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void Convert_ValueWithEmptySegments_HandlesCorrectly(string value)
    {
        // Arrange
        string[] segments = value.Split(',');
        int nonEmptySegments = 0;
        foreach (string segment in segments)
        {
            if (!string.IsNullOrEmpty(segment))
                nonEmptySegments++;
        }

        // Act
        string result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.IsTrue(result.StartsWith("new Microsoft.Maui.Controls.RowDefinitionCollection(["));
        Assert.IsTrue(result.EndsWith("])"));
        // Count occurrences of "new RowDefinition(" should match number of non-empty segments
        int rowDefinitionCount = result.Split("new RowDefinition(").Length - 1;
        Assert.AreEqual(nonEmptySegments, rowDefinitionCount);
    }

    /// <summary>
    /// Tests that Convert handles special characters and invalid values correctly.
    /// Should process invalid segments that GridLengthConverter cannot handle.
    /// </summary>
    [TestCase("100,invalid,*")]
    [TestCase("@#$,%^&")]
    [TestCase("100,null,*")]
    [TestCase("true,false")]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void Convert_ValueWithInvalidSegments_HandlesCorrectly(string value)
    {
        // Arrange
        string[] segments = value.Split(',');
        // Act
        string result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.IsTrue(result.StartsWith("new Microsoft.Maui.Controls.RowDefinitionCollection(["));
        Assert.IsTrue(result.EndsWith("])"));
        // Should have same number of RowDefinitions as segments (GridLengthConverter handles invalid values)
        int rowDefinitionCount = result.Split("new RowDefinition(").Length - 1;
        Assert.AreEqual(segments.Length, rowDefinitionCount);
    }

    /// <summary>
    /// Tests that Convert handles strings with special characters correctly.
    /// Should process special characters that may be valid or invalid for GridLength conversion.
    /// </summary>
    [TestCase("100,@#$,*")]
    [TestCase("Auto,unicode\u00A0test,100")]
    [TestCase("100,\t\r\n,*")]
    [TestCase("*,control\x01chars,Auto")]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void Convert_StringsWithSpecialCharacters_HandlesCorrectly(string value)
    {
        // Arrange & Act
        string result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.IsTrue(result.StartsWith("new Microsoft.Maui.Controls.RowDefinitionCollection(["));
        Assert.IsTrue(result.EndsWith("])"));
        // Should create RowDefinitions for all segments (some may be default)
        string[] segments = value.Split(',');
        int rowDefinitionCount = result.Split("new RowDefinition(").Length - 1;
        Assert.AreEqual(segments.Length, rowDefinitionCount);
    }

    /// <summary>
    /// Tests that Convert handles collections with duplicate values correctly.
    /// Should create separate RowDefinitions for each duplicate value.
    /// </summary>
    [TestCase("100,100,100")]
    [TestCase("*,*")]
    [TestCase("Auto,Auto,Auto,Auto")]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void Convert_CollectionWithDuplicates_CreatesAllRowDefinitions(string value)
    {
        // Arrange
        string[] segments = value.Split(',');
        // Act
        string result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.IsTrue(result.StartsWith("new Microsoft.Maui.Controls.RowDefinitionCollection(["));
        Assert.IsTrue(result.EndsWith("])"));
        // Should create RowDefinitions for all segments, even duplicates
        int rowDefinitionCount = result.Split("new RowDefinition(").Length - 1;
        Assert.AreEqual(segments.Length, rowDefinitionCount);
    }

}