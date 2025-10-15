#nullable disable
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;


using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.SourceGen.TypeConverters;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;


/// <summary>
/// Unit tests for PointCollectionConverter class.
/// </summary>
[TestFixture]
public partial class PointCollectionConverterTests
{
    private PointCollectionConverter _converter = null!;
    private Mock<BaseNode> _mockNode = null!;
    private Mock<ITypeSymbol> _mockToType = null!;
    private Mock<SourceGenContext> _mockContext = null!;
    private Mock<Compilation> _mockCompilation = null!;
    private Mock<INamedTypeSymbol> _mockPointCollectionType = null!;

    [SetUp]
    public void SetUp()
    {
        _converter = new PointCollectionConverter();
        _mockNode = new Mock<BaseNode>();
        _mockToType = new Mock<ITypeSymbol>();
        _mockContext = new Mock<SourceGenContext>();
        _mockCompilation = new Mock<Compilation>();
        _mockPointCollectionType = new Mock<INamedTypeSymbol>();
        // Setup the node to be castable to IXmlLineInfo
        var mockXmlLineInfo = _mockNode.As<IXmlLineInfo>();
        mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
        mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
        // Setup compilation chain
        _mockContext.Setup(x => x.Compilation).Returns(_mockCompilation.Object);
        _mockCompilation.Setup(x => x.GetTypeByMetadataName("Microsoft.Maui.Controls.PointCollection")).Returns(_mockPointCollectionType.Object);
        _mockPointCollectionType.Setup(x => x.ToFQDisplayString()).Returns("Microsoft.Maui.Controls.PointCollection");
    }

    /// <summary>
    /// Tests Convert method with extreme double values.
    /// Should successfully convert boundary values.
    /// </summary>
    [TestCase("1.7976931348623157E+308", "1.7976931348623157E+308", TestName = "MaxValue")]
    [TestCase("-1.7976931348623157E+308", "-1.7976931348623157E+308", TestName = "MinValue")]
    public void Convert_ExtremeDoubleValues_ConvertsSuccessfully(string x, string y)
    {
        // Arrange
        var value = $"{x},{y}";
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Does.StartWith("new Microsoft.Maui.Controls.PointCollection(new[] {"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<ITypeSymbol>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with special double values like NaN and Infinity.
    /// Should report conversion failure for these values as they are not valid in NumberStyles.Number format.
    /// </summary>
    [TestCase("NaN,1", TestName = "NaN_X")]
    [TestCase("1,NaN", TestName = "NaN_Y")]
    [TestCase("Infinity,1", TestName = "Infinity_X")]
    [TestCase("1,Infinity", TestName = "Infinity_Y")]
    [TestCase("-Infinity,1", TestName = "NegativeInfinity_X")]
    [TestCase("1,-Infinity", TestName = "NegativeInfinity_Y")]
    public void Convert_SpecialDoubleValues_ReportsFailure(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.AreEqual("default", result);
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), value, _mockToType.Object, Descriptors.ConversionFailed), Times.Once);
    }

    /// <summary>
    /// Tests Convert method with single point.
    /// Should successfully convert single point.
    /// </summary>
    [Test]
    public void Convert_SinglePoint_ConvertsSuccessfully()
    {
        // Arrange
        var value = "5,10";
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Does.StartWith("new Microsoft.Maui.Controls.PointCollection(new[] {"));
        Assert.That(result, Does.Contain("new Microsoft.Maui.Graphics.Point(5, 10)"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<ITypeSymbol>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with whitespace-only value.
    /// Should report conversion failure and return default.
    /// </summary>
    [TestCase("   ", TestName = "Spaces")]
    [TestCase("\t", TestName = "Tab")]
    [TestCase("\n", TestName = "Newline")]
    [TestCase("\r\n", TestName = "CarriageReturnNewline")]
    public void Convert_WhitespaceOnlyValue_ReportsFailure(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

        // Assert
        Assert.AreEqual("default", result);
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), value, _mockToType.Object, Descriptors.ConversionFailed), Times.Once);
    }

    /// <summary>
    /// Tests Convert method with invalid number formats.
    /// Should report conversion failure and return default.
    /// </summary>
    [TestCase("abc,1", TestName = "InvalidX")]
    [TestCase("1,xyz", TestName = "InvalidY")]
    [TestCase("1.2.3,4", TestName = "InvalidDecimalX")]
    [TestCase("1,2.3.4", TestName = "InvalidDecimalY")]
    [TestCase("1e,2", TestName = "InvalidScientificX")]
    [TestCase("1,2e", TestName = "InvalidScientificY")]
    public void Convert_InvalidNumberFormat_ReportsFailure(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

        // Assert
        Assert.AreEqual("default", result);
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), value, _mockToType.Object, Descriptors.ConversionFailed), Times.Once);
    }

    /// <summary>
    /// Tests Convert method with odd number of coordinates (incomplete points).
    /// Should report conversion failure and return default.
    /// </summary>
    [TestCase("1", TestName = "SingleCoordinate")]
    [TestCase("1,2,3", TestName = "ThreeCoordinates")]
    [TestCase("1,2,3,4,5", TestName = "FiveCoordinates")]
    [TestCase("1 2 3", TestName = "ThreeSpaceSeparated")]
    public void Convert_OddNumberOfCoordinates_ReportsFailure(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

        // Assert
        Assert.AreEqual("default", result);
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), value, _mockToType.Object, Descriptors.ConversionFailed), Times.Once);
    }


    /// <summary>
    /// Tests Convert method with empty string value.
    /// Should report conversion failure and return default.
    /// </summary>
    [Test]
    public void Convert_EmptyString_ReportsFailure()
    {
        // Act
        var result = _converter.Convert("", _mockNode.Object, _mockToType.Object, _mockContext.Object);

        // Assert
        Assert.AreEqual("default", result);
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), "", _mockToType.Object, Descriptors.ConversionFailed), Times.Once);
    }

    /// <summary>
    /// Tests Convert method with multiple points.
    /// Should successfully convert all points.
    /// </summary>
    [TestCase("1,2,3,4", TestName = "TwoPoints")]
    [TestCase("0,0,10,10,20,20", TestName = "ThreePoints")]
    [TestCase("1.5,2.5,3.7,4.8", TestName = "DecimalPoints")]
    public void Convert_MultiplePoints_ConvertsSuccessfully(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Does.StartWith("new Microsoft.Maui.Controls.PointCollection(new[] {"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<ITypeSymbol>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with space-separated coordinates.
    /// Should successfully convert space-separated points.
    /// </summary>
    [TestCase("1 2", TestName = "SinglePointSpaceSeparated")]
    [TestCase("1 2 3 4", TestName = "TwoPointsSpaceSeparated")]
    [TestCase("0 0 10 10", TestName = "ZeroAndNonZeroSpaceSeparated")]
    public void Convert_SpaceSeparatedCoordinates_ConvertsSuccessfully(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Does.StartWith("new Microsoft.Maui.Controls.PointCollection(new[] {"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<ITypeSymbol>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with mixed separators (spaces and commas).
    /// Should successfully convert with mixed separators.
    /// </summary>
    [TestCase("1,2 3,4", TestName = "CommaThenSpace")]
    [TestCase("1 2,3 4", TestName = "SpaceThenComma")]
    [TestCase("1, 2 , 3 , 4", TestName = "CommasWithSpaces")]
    public void Convert_MixedSeparators_ConvertsSuccessfully(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Does.StartWith("new Microsoft.Maui.Controls.PointCollection(new[] {"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<ITypeSymbol>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with zero values.
    /// Should successfully convert zero coordinates.
    /// </summary>
    [TestCase("0,0", TestName = "BothZero")]
    [TestCase("0,5", TestName = "XZero")]
    [TestCase("5,0", TestName = "YZero")]
    public void Convert_ZeroValues_ConvertsSuccessfully(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Does.StartWith("new Microsoft.Maui.Controls.PointCollection(new[] {"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<ITypeSymbol>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with negative values.
    /// Should successfully convert negative coordinates.
    /// </summary>
    [TestCase("-1,-2", TestName = "BothNegative")]
    [TestCase("-5,10", TestName = "XNegative")]
    [TestCase("5,-10", TestName = "YNegative")]
    public void Convert_NegativeValues_ConvertsSuccessfully(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Does.StartWith("new Microsoft.Maui.Controls.PointCollection(new[] {"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<ITypeSymbol>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with extra whitespace around valid coordinates.
    /// Should successfully parse coordinates ignoring whitespace.
    /// </summary>
    [TestCase("  1  ,  2  ", TestName = "SpacesAroundComma")]
    [TestCase("\t1\t,\t2\t", TestName = "TabsAroundComma")]
    [TestCase("  1   2  ", TestName = "SpacesAroundSpace")]
    public void Convert_ExtraWhitespace_ConvertsSuccessfully(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Does.StartWith("new Microsoft.Maui.Controls.PointCollection(new[] {"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<ITypeSymbol>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with scientific notation.
    /// Should successfully convert scientific notation numbers.
    /// </summary>
    [TestCase("1E+2,2E+3", TestName = "PositiveExponent")]
    [TestCase("1E-2,2E-3", TestName = "NegativeExponent")]
    [TestCase("1.5E+10,2.7E-5", TestName = "MixedExponents")]
    public void Convert_ScientificNotation_ConvertsSuccessfully(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Does.StartWith("new Microsoft.Maui.Controls.PointCollection(new[] {"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<ITypeSymbol>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with consecutive separators.
    /// Should handle consecutive separators by skipping empty entries.
    /// </summary>
    [TestCase("1,,2", TestName = "ConsecutiveCommas")]
    [TestCase("1  2", TestName = "MultipleSpaces")]
    [TestCase("1, ,2", TestName = "CommaSpaceComma")]
    public void Convert_ConsecutiveSeparators_HandlesGracefully(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Does.StartWith("new Microsoft.Maui.Controls.PointCollection(new[] {"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<ITypeSymbol>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }
}