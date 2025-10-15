using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.SourceGen.TypeConverters;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using Moq.Language;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.TypeConverters;

/// <summary>
/// Tests for GridLengthConverter type converter.
/// </summary>
[TestFixture]
public class GridLengthConverterTests
{
    private GridLengthConverter _converter = null!;
    private Mock<BaseNode> _mockNode = null!;
    private Mock<IXmlLineInfo> _mockXmlLineInfo = null!;
    private Mock<SourceGenContext> _mockContext = null!;
    private Mock<Compilation> _mockCompilation = null!;
    private Mock<INamedTypeSymbol> _mockGridLengthType = null!;
    private Mock<INamedTypeSymbol> _mockGridUnitType = null!;
    private Mock<ITypeSymbol> _mockToType = null!;

    /// <summary>
    /// Tests Convert method with star pattern ("*").
    /// Should return GridLength.Star.
    /// </summary>
    [TestCase("*")]
    [TestCase("*")]
    public void Convert_StarPattern_ReturnsGridLengthStar(string value)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.AreEqual("Microsoft.Maui.GridLength.Star", result);
        _mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with auto pattern ("Auto").
    /// Should return GridLength.Auto.
    /// </summary>
    [TestCase("Auto")]
    [TestCase("auto")]
    [TestCase("AUTO")]
    [TestCase("AuTo")]
    public void Convert_AutoPattern_ReturnsGridLengthAuto(string value)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.AreEqual("Microsoft.Maui.GridLength.Auto", result);
        _mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with valid numeric star patterns.
    /// Should return new GridLength with Star unit type.
    /// </summary>
    [TestCase("2*", "2")]
    [TestCase("0.5*", "0.5")]
    [TestCase("10*", "10")]
    [TestCase("0*", "0")]
    [TestCase("1.5*", "1.5")]
    [TestCase("100.25*", "100.25")]
    public void Convert_ValidNumericStarPattern_ReturnsGridLengthWithStarUnit(string value, string expectedNumber)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.AreEqual($"new Microsoft.Maui.GridLength({expectedNumber}, Microsoft.Maui.GridUnitType.Star)", result);
        _mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with invalid numeric star patterns.
    /// Should report conversion failed and return "default".
    /// </summary>
    [TestCase("abc*")]
    [TestCase("NaN*")]
    [TestCase("invalid*")]
    [TestCase("**")]
    [TestCase("*abc*")]
    [TestCase("")]
    public void Convert_InvalidNumericStarPattern_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        _mockContext.Verify(c => c.ReportConversionFailed(_mockXmlLineInfo.Object, value, It.IsAny<DiagnosticDescriptor>()), Times.Once);
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests Convert method with valid numeric values.
    /// Should return new GridLength with Absolute unit type.
    /// </summary>
    [TestCase("100", "100")]
    [TestCase("0", "0")]
    [TestCase("1.5", "1.5")]
    [TestCase("-5", "-5")]
    [TestCase("123.456", "123.456")]
    public void Convert_ValidNumericValues_ReturnsGridLengthWithAbsoluteUnit(string value, string expectedNumber)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.AreEqual($"new Microsoft.Maui.GridLength({expectedNumber}, Microsoft.Maui.GridUnitType.Absolute)", result);
        _mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with special double values.
    /// Should return new GridLength with Absolute unit type for valid special values.
    /// </summary>
    [TestCase("NaN", "NaN")]
    [TestCase("Infinity", "∞")]
    [TestCase("-Infinity", "-∞")]
    public void Convert_SpecialDoubleValues_ReturnsGridLengthWithAbsoluteUnit(string value, string expectedFormatted)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.AreEqual($"new Microsoft.Maui.GridLength({expectedFormatted}, Microsoft.Maui.GridUnitType.Absolute)", result);
        _mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with invalid numeric patterns.
    /// Should report conversion failed and return "default".
    /// </summary>
    [TestCase("abc")]
    [TestCase("invalid")]
    [TestCase("123abc")]
    [TestCase("auto123")]
    [TestCase("*123")]
    [TestCase("12.3.4")]
    [TestCase("")]
    public void Convert_InvalidNumericPatterns_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        _mockContext.Verify(c => c.ReportConversionFailed(_mockXmlLineInfo.Object, value, It.IsAny<DiagnosticDescriptor>()), Times.Once);
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests Convert method with values containing leading and trailing whitespace.
    /// Should trim values and process correctly.
    /// </summary>
    [TestCase("  *  ")]
    [TestCase("\t*\t")]
    [TestCase("\n*\n")]
    public void Convert_ValueWithWhitespace_TrimsAndProcessesCorrectly(string value)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        Assert.AreEqual("Microsoft.Maui.GridLength.Star", result);
        _mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with numeric values containing whitespace.
    /// Should trim and process correctly.
    /// </summary>
    [TestCase("  100  ", "100")]
    [TestCase("\t2*\t", "2")]
    [TestCase("  Auto  ")]
    public void Convert_NumericValueWithWhitespace_TrimsAndProcessesCorrectly(string value, string expectedNumber = null!)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        if (value.Trim() == "Auto")
        {
            Assert.AreEqual("Microsoft.Maui.GridLength.Auto", result);
        }
        else if (value.Trim().EndsWith("*"))
        {
            Assert.AreEqual($"new Microsoft.Maui.GridLength({expectedNumber}, Microsoft.Maui.GridUnitType.Star)", result);
        }
        else
        {
            Assert.AreEqual($"new Microsoft.Maui.GridLength({expectedNumber}, Microsoft.Maui.GridUnitType.Absolute)", result);
        }

        _mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with special characters.
    /// Should report conversion failed and return "default".
    /// </summary>
    [TestCase("@#$%")]
    [TestCase("µ∑∂")]
    [TestCase("🚀")]
    [TestCase("\0\r\n")]
    public void Convert_SpecialCharacters_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        _mockContext.Verify(c => c.ReportConversionFailed(_mockXmlLineInfo.Object, value, It.IsAny<DiagnosticDescriptor>()), Times.Once);
        Assert.AreEqual("default", result);
    }
    [SetUp]
    public void SetUp()
    {
        _converter = new GridLengthConverter();
        _mockNode = new Mock<BaseNode>();
        _mockXmlLineInfo = new Mock<IXmlLineInfo>();
        _mockContext = new Mock<SourceGenContext>();
        _mockCompilation = new Mock<Compilation>();
        _mockGridLengthType = new Mock<INamedTypeSymbol>();
        _mockGridUnitType = new Mock<INamedTypeSymbol>();
        _mockToType = new Mock<ITypeSymbol>();

        // Setup the node to implement IXmlLineInfo
        _mockNode.As<IXmlLineInfo>().Setup(x => x.LineNumber).Returns(1);
        _mockNode.As<IXmlLineInfo>().Setup(x => x.LinePosition).Returns(1);
        _mockNode.As<IXmlLineInfo>().Setup(x => x.HasLineInfo()).Returns(true);

        // Setup context compilation
        _mockContext.Setup(c => c.Compilation).Returns(_mockCompilation.Object);

        // Setup GetTypeByMetadataName for GridLength and GridUnitType
        _mockCompilation.Setup(c => c.GetTypeByMetadataName("Microsoft.Maui.GridLength"))
            .Returns(_mockGridLengthType.Object);
        _mockCompilation.Setup(c => c.GetTypeByMetadataName("Microsoft.Maui.GridUnitType"))
            .Returns(_mockGridUnitType.Object);

        // Setup ToFQDisplayString for the type symbols
        _mockGridLengthType.Setup(t => t.ToFQDisplayString()).Returns("Microsoft.Maui.GridLength");
        _mockGridUnitType.Setup(t => t.ToFQDisplayString()).Returns("Microsoft.Maui.GridUnitType");
    }

    /// <summary>
    /// Tests Convert method with whitespace-only value.
    /// Should report conversion failed and return "default" after trimming to empty.
    /// </summary>
    [TestCase("   ")]
    [TestCase("\t")]
    [TestCase("\n")]
    [TestCase("\r\n")]
    [TestCase("  \t  \n  ")]
    public void Convert_WhitespaceOnlyValue_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

        // Assert
        _mockContext.Verify(c => c.ReportConversionFailed(
            It.IsAny<IXmlLineInfo>(),
            value,
            It.IsAny<DiagnosticDescriptor>()), Times.Once);
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests Convert method with star patterns that fail numeric parsing.
    /// Should report conversion failed and return "default".
    /// </summary>
    [TestCase("abc*")]
    [TestCase("invalid*")]
    [TestCase("**")]
    [TestCase("*abc*")]
    [TestCase("NaN*")]
    [TestCase("Infinity*")]
    [TestCase("-Infinity*")]
    [TestCase("1.2.3*")]
    [TestCase("*")]
    public void Convert_InvalidStarPatterns_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

        // Assert
        _mockContext.Verify(c => c.ReportConversionFailed(
            It.IsAny<IXmlLineInfo>(),
            value,
            It.IsAny<DiagnosticDescriptor>()), Times.Once);
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests Convert method with values that don't match any pattern.
    /// Should report conversion failed and return "default".
    /// </summary>
    [TestCase("invalid")]
    [TestCase("abc")]
    [TestCase("123abc")]
    [TestCase("auto123")]
    [TestCase("*123")]
    [TestCase("12.3.4")]
    [TestCase("@#$%")]
    [TestCase("µ∑∂")]
    [TestCase("🚀")]
    [TestCase("\0\r\n")]
    public void Convert_InvalidPatterns_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

        // Assert
        _mockContext.Verify(c => c.ReportConversionFailed(
            It.IsAny<IXmlLineInfo>(),
            value,
            It.IsAny<DiagnosticDescriptor>()), Times.Once);
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests Convert method with boundary double values.
    /// Should handle extreme values correctly.
    /// </summary>
    [TestCase("1.7976931348623157E+308")]   // double.MaxValue
    [TestCase("-1.7976931348623157E+308")]  // double.MinValue
    [TestCase("4.94065645841247E-324")]     // double.Epsilon
    [TestCase("0.0")]
    [TestCase("-0.0")]
    public void Convert_BoundaryDoubleValues_HandlesCorrectly(string value)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

        // Assert
        Assert.That(result, Does.StartWith("new Microsoft.Maui.GridLength("));
        Assert.That(result, Does.EndWith(", Microsoft.Maui.GridUnitType.Absolute)"));
        _mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
    }

    /// <summary>
    /// Tests Convert method with edge case star patterns.
    /// Should handle various malformed star patterns.
    /// </summary>
    [TestCase("*abc")]
    [TestCase("123*456")]
    [TestCase("*.5")]
    [TestCase(".*")]
    [TestCase("-*")]
    [TestCase("+*")]
    public void Convert_MalformedStarPatterns_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

        // Assert
        _mockContext.Verify(c => c.ReportConversionFailed(
            It.IsAny<IXmlLineInfo>(),
            value,
            It.IsAny<DiagnosticDescriptor>()), Times.Once);
        Assert.AreEqual("default", result);
    }
}