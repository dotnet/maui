using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.SourceGen.TypeConverters;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using Moq.Language;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.TypeConverters;
/// <summary>
/// Tests for GridLengthConverter type converter.
/// </summary>
[TestFixture]
[Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
[Category("auto-generated")]
public class GridLengthConverterTests
{
    private GridLengthConverter _converter = null !;
    private Mock<BaseNode> _mockNode = null !;
    private Mock<IXmlLineInfo> _mockXmlLineInfo = null !;
    private Mock<SourceGenContext> _mockContext = null !;
    private Mock<Compilation> _mockCompilation = null !;
    private Mock<INamedTypeSymbol> _mockGridLengthType = null !;
    private Mock<INamedTypeSymbol> _mockGridUnitType = null !;
    private Mock<ITypeSymbol> _mockToType = null !;
    /// <summary>
    /// Tests Convert method with star pattern ("*").
    /// Should return GridLength.Star.
    /// </summary>
    [TestCase("*")]
    [TestCase("*")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
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
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
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
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
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
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
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
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
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
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
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
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
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
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
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
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void Convert_NumericValueWithWhitespace_TrimsAndProcessesCorrectly(string value, string expectedNumber = null !)
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
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void Convert_SpecialCharacters_ReportsConversionFailedAndReturnsDefault(string value)
    {
        // Arrange & Act
        var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
        // Assert
        _mockContext.Verify(c => c.ReportConversionFailed(_mockXmlLineInfo.Object, value, It.IsAny<DiagnosticDescriptor>()), Times.Once);
        Assert.AreEqual("default", result);
    }
}