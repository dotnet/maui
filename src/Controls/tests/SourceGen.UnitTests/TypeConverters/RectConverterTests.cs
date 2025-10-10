using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.SourceGen.TypeConverters;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.TypeConverters;
/// <summary>
/// Tests for RectConverter class.
/// </summary>
[TestFixture]
[Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
[Category("auto-generated")]
public class RectConverterTests
{
    private RectConverter _converter = null !;
    private Mock<BaseNode> _mockNode = null !;
    private Mock<ITypeSymbol> _mockToType = null !;
    private Mock<SourceGenContext> _mockContext = null !;
    private Mock<IXmlLineInfo> _mockXmlLineInfo = null !;
    private Mock<Compilation> _mockCompilation = null !;
    private Mock<INamedTypeSymbol> _mockRectType = null !;
    [SetUp]
    public void SetUp()
    {
        _converter = new RectConverter();
        _mockNode = new Mock<BaseNode>();
        _mockToType = new Mock<ITypeSymbol>();
        _mockContext = new Mock<SourceGenContext>();
        _mockXmlLineInfo = new Mock<IXmlLineInfo>();
        _mockCompilation = new Mock<Compilation>();
        _mockRectType = new Mock<INamedTypeSymbol>();
        // Setup node to be castable to IXmlLineInfo
        _mockNode.As<IXmlLineInfo>();
        _mockNode.As<IXmlLineInfo>().Setup(x => x.LineNumber).Returns(1);
        _mockNode.As<IXmlLineInfo>().Setup(x => x.LinePosition).Returns(1);
        // Setup context
        _mockContext.Setup(x => x.Compilation).Returns(_mockCompilation.Object);
        _mockCompilation.Setup(x => x.GetTypeByMetadataName("Microsoft.Maui.Graphics.Rect")).Returns(_mockRectType.Object);
        _mockRectType.Setup(x => x.ToFQDisplayString()).Returns("Microsoft.Maui.Graphics.Rect");
    }

    /// <summary>
    /// Tests Convert method with valid comma-separated rectangle values.
    /// Should return formatted Rect constructor string with parsed values.
    /// </summary>
    [TestCase("0,0,100,50", 0.0, 0.0, 100.0, 50.0)]
    [TestCase("10.5,20.5,30.5,40.5", 10.5, 20.5, 30.5, 40.5)]
    [TestCase("-10,-20,100,200", -10.0, -20.0, 100.0, 200.0)]
    [TestCase("1.7976931348623157E+308,1.7976931348623157E+308,1.7976931348623157E+308,1.7976931348623157E+308", double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue)]
    [TestCase("-1.7976931348623157E+308,-1.7976931348623157E+308,1.7976931348623157E+308,1.7976931348623157E+308", double.MinValue, double.MinValue, double.MaxValue, double.MaxValue)]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void Convert_ValidRectangleValues_ReturnsFormattedRectConstructor(string value, double expectedX, double expectedY, double expectedW, double expectedH)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Is.EqualTo($"new Microsoft.Maui.Graphics.Rect({expectedX}, {expectedY}, {expectedW}, {expectedH})"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with whitespace-only string value.
    /// Should report conversion failure and return default.
    /// </summary>
    [TestCase("   ")]
    [TestCase("\t")]
    [TestCase("\n")]
    [TestCase("\r\n")]
    [TestCase("  \t  \n  ")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void Convert_WhitespaceOnlyString_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Is.EqualTo("default"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.Is<string>(v => v == value), It.IsAny<DiagnosticDescriptor>()), Times.Once);
    }

    /// <summary>
    /// Tests Convert method with incorrect number of comma-separated values.
    /// Should report conversion failure and return default.
    /// </summary>
    [TestCase("")]
    [TestCase("10")]
    [TestCase("10,20")]
    [TestCase("10,20,30")]
    [TestCase("10,20,30,40,50")]
    [TestCase("10,20,30,40,50,60")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void Convert_IncorrectNumberOfValues_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Is.EqualTo("default"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.Is<string>(v => v == value), It.IsAny<DiagnosticDescriptor>()), Times.Once);
    }

    /// <summary>
    /// Tests Convert method with non-numeric values in comma-separated string.
    /// Should report conversion failure and return default.
    /// </summary>
    [TestCase("abc,20,30,40")]
    [TestCase("10,abc,30,40")]
    [TestCase("10,20,abc,40")]
    [TestCase("10,20,30,abc")]
    [TestCase("abc,def,ghi,jkl")]
    [TestCase("10.5.5,20,30,40")]
    [TestCase("10,20.5.5,30,40")]
    [TestCase("10,20,30.5.5,40")]
    [TestCase("10,20,30,40.5.5")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void Convert_NonNumericValues_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Is.EqualTo("default"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.Is<string>(v => v == value), It.IsAny<DiagnosticDescriptor>()), Times.Once);
    }

    /// <summary>
    /// Tests Convert method with special double values.
    /// Should handle NaN and Infinity values appropriately.
    /// </summary>
    [TestCase("NaN,0,100,50")]
    [TestCase("0,NaN,100,50")]
    [TestCase("0,0,NaN,50")]
    [TestCase("0,0,100,NaN")]
    [TestCase("Infinity,0,100,50")]
    [TestCase("0,Infinity,100,50")]
    [TestCase("0,0,Infinity,50")]
    [TestCase("0,0,100,Infinity")]
    [TestCase("-Infinity,0,100,50")]
    [TestCase("0,-Infinity,100,50")]
    [TestCase("0,0,-Infinity,50")]
    [TestCase("0,0,100,-Infinity")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void Convert_SpecialDoubleValues_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Is.EqualTo("default"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.Is<string>(v => v == value), It.IsAny<DiagnosticDescriptor>()), Times.Once);
    }

    /// <summary>
    /// Tests Convert method with values containing extra whitespace.
    /// Should report conversion failure and return default as Split uses exact comma separation.
    /// </summary>
    [TestCase(" 10 , 20 , 30 , 40 ")]
    [TestCase("10, 20, 30, 40")]
    [TestCase("10 ,20, 30, 40")]
    [TestCase("10,20 ,30, 40")]
    [TestCase("10,20, 30 ,40")]
    [TestCase("10,20, 30, 40 ")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void Convert_ValuesWithWhitespace_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Is.EqualTo("default"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.Is<string>(v => v == value), It.IsAny<DiagnosticDescriptor>()), Times.Once);
    }

    /// <summary>
    /// Tests Convert method with culture-specific decimal separators.
    /// Should report conversion failure as method uses InvariantCulture which expects dots.
    /// </summary>
    [TestCase("10,5,20,5,30,5,40,5")] // Too many values due to comma as decimal separator
    [TestCase("10;20;30;40")] // Semicolon separator
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void Convert_CultureSpecificFormat_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Is.EqualTo("default"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.Is<string>(v => v == value), It.IsAny<DiagnosticDescriptor>()), Times.Once);
    }

    /// <summary>
    /// Tests Convert method with string containing control characters.
    /// Should report conversion failure for strings with control characters.
    /// </summary>
    [TestCase("10\0,20,30,40")]
    [TestCase("10,20\u0001,30,40")]
    [TestCase("10,20,30\u0002,40")]
    [TestCase("10,20,30,40\u0003")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void Convert_StringWithControlCharacters_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Is.EqualTo("default"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.Is<string>(v => v == value), It.IsAny<DiagnosticDescriptor>()), Times.Once);
    }

    /// <summary>
    /// Tests Convert method with scientific notation values.
    /// Should successfully parse and return formatted constructor.
    /// </summary>
    [TestCase("1E+2,2E+3,3E+4,4E+5", 100.0, 2000.0, 30000.0, 400000.0)]
    [TestCase("1e-2,2e-3,3e-4,4e-5", 0.01, 0.002, 0.0003, 0.00004)]
    [TestCase("1.5E+10,2.5E-10,3.5E+20,4.5E-20", 1.5E+10, 2.5E-10, 3.5E+20, 4.5E-20)]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void Convert_ScientificNotationValues_ReturnsFormattedRectConstructor(string value, double expectedX, double expectedY, double expectedW, double expectedH)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Is.EqualTo($"new Microsoft.Maui.Graphics.Rect({expectedX}, {expectedY}, {expectedW}, {expectedH})"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with values that have trailing zeros.
    /// Should successfully parse and return formatted constructor.
    /// </summary>
    [TestCase("10.0,20.00,30.000,40.0000")]
    [TestCase("10.10,20.20,30.30,40.40")]
    [TestCase("0.0,0.00,0.000,0.0000")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void Convert_TrailingZeroValues_ReturnsFormattedRectConstructor(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Does.StartWith("new Microsoft.Maui.Graphics.Rect("));
        Assert.That(result, Does.EndWith(")"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with string containing only commas.
    /// Should report conversion failure due to invalid parsing.
    /// </summary>
    [TestCase(",,,")]
    [TestCase(",,")]
    [TestCase(",")]
    [TestCase(",,,,,")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void Convert_OnlyCommas_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Is.EqualTo("default"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.Is<string>(v => v == value), It.IsAny<DiagnosticDescriptor>()), Times.Once);
    }

    /// <summary>
    /// Tests Convert method with mixed valid and invalid values.
    /// Should report conversion failure when any value cannot be parsed.
    /// </summary>
    [TestCase("10,20,30,")] // Empty fourth value
    [TestCase("10,20,,40")] // Empty third value
    [TestCase("10,,30,40")] // Empty second value
    [TestCase(",20,30,40")] // Empty first value
    [TestCase("10,20,30,40,")] // Too many values with empty
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void Convert_MixedValidInvalidValues_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.That(result, Is.EqualTo("default"));
        _mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.Is<string>(v => v == value), It.IsAny<DiagnosticDescriptor>()), Times.Once);
    }
}