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
[Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
[Category("auto-generated")]
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
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
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
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
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
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
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
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
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
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
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
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void Convert_OddNumberOfCoordinates_ReportsFailure(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

        // Assert
        Assert.AreEqual("default", result);
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), value, _mockToType.Object, Descriptors.ConversionFailed), Times.Once);
    }

}