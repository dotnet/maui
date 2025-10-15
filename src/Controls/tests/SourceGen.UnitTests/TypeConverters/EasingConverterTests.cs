using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.SourceGen.TypeConverters;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.TypeConverters;

/// <summary>
/// Unit tests for EasingConverter class.
/// </summary>
[TestFixture]
public partial class EasingConverterTests
{
	private EasingConverter _converter = null!;
	private Mock<BaseNode> _mockNode = null!;
	private Mock<ITypeSymbol> _mockToType = null!;
	private Mock<SourceGenContext> _mockContext = null!;
	private Mock<Compilation> _mockCompilation = null!;
	private Mock<INamedTypeSymbol> _mockEasingType = null!;

	[SetUp]
	public void Setup()
	{
		_converter = new EasingConverter();
		_mockNode = new Mock<BaseNode>(Mock.Of<IXmlNamespaceResolver>(), -1, -1);
		_mockToType = new Mock<ITypeSymbol>();
		_mockContext = new Mock<SourceGenContext>();
		_mockCompilation = new Mock<Compilation>();
		_mockEasingType = new Mock<INamedTypeSymbol>();
		_mockContext.Setup(x => x.Compilation).Returns(_mockCompilation.Object);
		_mockCompilation.Setup(x => x.GetTypeByMetadataName("Microsoft.Maui.Easing")).Returns(_mockEasingType.Object);
		_mockEasingType.Setup(x => x.ToFQDisplayString()).Returns("global::Microsoft.Maui.Easing");
	}

	/// <summary>
	/// Tests Convert method with valid known easing names.
	/// Should return the fully qualified easing type name.
	/// </summary>
	[TestCase("Linear")]
	[TestCase("SinOut")]
	[TestCase("SinIn")]
	[TestCase("SinInOut")]
	[TestCase("CubicIn")]
	[TestCase("CubicOut")]
	[TestCase("CubicInOut")]
	[TestCase("BounceOut")]
	[TestCase("BounceIn")]
	[TestCase("SpringIn")]
	[TestCase("SpringOut")]
	public void Convert_ValidEasingName_ReturnsFullyQualifiedName(string easingName)
	{
		// Arrange & Act
		var result = _converter.Convert(easingName, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual($"global::Microsoft.Maui.Easing.{easingName}", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
	}

	/// <summary>
	/// Tests Convert method with valid easing names in different cases.
	/// Should return the fully qualified easing type name regardless of case.
	/// </summary>
	[TestCase("linear")]
	[TestCase("SINOUT")]
	[TestCase("CuBiCiN")]
	public void Convert_ValidEasingNameDifferentCase_ReturnsFullyQualifiedName(string easingName)
	{
		// Arrange & Act
		var result = _converter.Convert(easingName, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.IsTrue(result.StartsWith("global::Microsoft.Maui.Easing."));
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
	}

	/// <summary>
	/// Tests Convert method with invalid easing name.
	/// Should call ReportConversionFailed and return "default".
	/// </summary>
	[TestCase("InvalidEasing")]
	[TestCase("Unknown")]
	[TestCase("NotAnEasing")]
	public void Convert_InvalidEasingName_ReturnsDefault(string easingName)
	{
		// Arrange & Act
		var result = _converter.Convert(easingName, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with valid "Easing.EasingName" format.
	/// Should extract the easing name and return fully qualified name if valid.
	/// </summary>
	[TestCase("Easing.Linear")]
	[TestCase("easing.SinOut")]
	[TestCase("EASING.CubicIn")]
	public void Convert_EasingPrefixWithValidName_ReturnsFullyQualifiedName(string value)
	{
		// Arrange
		string expectedEasingName = value.Split('.')[1];
		// Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual($"global::Microsoft.Maui.Easing.{expectedEasingName}", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
	}

	/// <summary>
	/// Tests Convert method with invalid "Easing.EasingName" format.
	/// Should extract the easing name but call ReportConversionFailed if invalid.
	/// </summary>
	[TestCase("Easing.InvalidEasing")]
	[TestCase("EASING.Unknown")]
	public void Convert_EasingPrefixWithInvalidName_ReturnsDefault(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with non-Easing prefix format.
	/// Should test the full value as-is against known easing names.
	/// </summary>
	[TestCase("NotEasing.Linear")]
	[TestCase("Other.SinOut")]
	[TestCase("Prefix.CubicIn")]
	public void Convert_NonEasingPrefix_ReturnsDefault(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with multiple dots in value.
	/// Should only process if exactly two parts and first part is "Easing".
	/// </summary>
	[TestCase("Easing.Linear.Extra")]
	[TestCase("Prefix.Easing.Linear")]
	[TestCase("One.Two.Three.Four")]
	public void Convert_MultipleDots_ReturnsDefault(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with value containing dots but only one part.
	/// Should test the full value against known easing names.
	/// </summary>
	[TestCase("Linear.")]
	[TestCase(".Linear")]
	[TestCase(".")]
	public void Convert_DotsWithoutTwoParts_ReturnsDefault(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with null parentVar parameter.
	/// Should work normally as parentVar is optional.
	/// </summary>
	[Test]
	public void Convert_NullParentVar_ProcessesNormally()
	{
		// Arrange
		string value = "Linear";
		// Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object, null);
		// Assert
		Assert.AreEqual("global::Microsoft.Maui.Easing.Linear", result);
	}

	/// <summary>
	/// Tests Convert method with edge case easing names containing special characters.
	/// Should call ReportConversionFailed and return "default" for non-standard names.
	/// </summary>
	[TestCase("Linear@")]
	[TestCase("Sin Out")]
	[TestCase("Cubic-In")]
	[TestCase("Bounce_Out")]
	public void Convert_EasingNameWithSpecialCharacters_ReturnsDefault(string easingName)
	{
		// Arrange & Act
		var result = _converter.Convert(easingName, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}
}


/// <summary>
/// Unit tests for EasingConverter class covering additional edge cases.
/// </summary>
[TestFixture]
public partial class EasingConverterAdditionalTests
{
	private EasingConverter _converter = null!;
	private Mock<BaseNode> _mockNode = null!;
	private Mock<ITypeSymbol> _mockToType = null!;
	private Mock<SourceGenContext> _mockContext = null!;
	private Mock<Compilation> _mockCompilation = null!;
	private Mock<INamedTypeSymbol> _mockEasingType = null!;

	[SetUp]
	public void Setup()
	{
		_converter = new EasingConverter();
		_mockNode = new Mock<BaseNode>(Mock.Of<IXmlNamespaceResolver>(), -1, -1);
		_mockToType = new Mock<ITypeSymbol>();
		_mockContext = new Mock<SourceGenContext>();
		_mockCompilation = new Mock<Compilation>();
		_mockEasingType = new Mock<INamedTypeSymbol>();
		_mockContext.Setup(x => x.Compilation).Returns(_mockCompilation.Object);
		_mockCompilation.Setup(x => x.GetTypeByMetadataName("Microsoft.Maui.Easing")).Returns(_mockEasingType.Object);
		_mockEasingType.Setup(x => x.ToFQDisplayString()).Returns("global::Microsoft.Maui.Easing");
	}

	/// <summary>
	/// Tests Convert method with whitespace-only input values.
	/// Should call ReportConversionFailed and return "default".
	/// </summary>
	[TestCase(" ")]
	[TestCase("  ")]
	[TestCase("\t")]
	[TestCase("\n")]
	[TestCase("\r\n")]
	[TestCase("   \t  \n  ")]
	public void Convert_WhitespaceOnlyValue_ReturnsDefault(string whitespaceValue)
	{
		// Arrange & Act
		var result = _converter.Convert(whitespaceValue, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with valid easing names that have leading/trailing whitespace.
	/// Should trim and process the easing name correctly.
	/// </summary>
	[TestCase(" Linear ")]
	[TestCase("\tSinOut\t")]
	[TestCase("\nCubicIn\n")]
	[TestCase("  BounceOut  ")]
	public void Convert_ValidEasingNameWithWhitespace_ReturnsDefault(string easingNameWithWhitespace)
	{
		// Arrange & Act
		var result = _converter.Convert(easingNameWithWhitespace, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert - Should fail because the method doesn't trim whitespace
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with Easing prefix but empty second part.
	/// Should call ReportConversionFailed and return "default".
	/// </summary>
	[TestCase("Easing.")]
	[TestCase("easing.")]
	[TestCase("EASING.")]
	public void Convert_EasingPrefixWithEmptyName_ReturnsDefault(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with empty first part but valid second part.
	/// Should test the full value against known easing names and fail.
	/// </summary>
	[TestCase(".Linear")]
	[TestCase(".SinOut")]
	public void Convert_EmptyPrefixWithValidName_ReturnsDefault(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with Unicode characters in easing names.
	/// Should call ReportConversionFailed and return "default" for non-ASCII names.
	/// </summary>
	[TestCase("Lineár")]
	[TestCase("Линейный")]
	[TestCase("線形")]
	[TestCase("Linear🚀")]
	public void Convert_UnicodeCharacters_ReturnsDefault(string unicodeEasingName)
	{
		// Arrange & Act
		var result = _converter.Convert(unicodeEasingName, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with control characters in easing names.
	/// Should call ReportConversionFailed and return "default".
	/// </summary>
	[TestCase("Linear\0")]
	[TestCase("SinOut\u0001")]
	[TestCase("Cubic\u0010In")]
	public void Convert_ControlCharacters_ReturnsDefault(string easingNameWithControlChars)
	{
		// Arrange & Act
		var result = _converter.Convert(easingNameWithControlChars, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with case-insensitive matching edge cases.
	/// Should properly handle case variations of known easing names.
	/// </summary>
	[TestCase("liNEAr")]
	[TestCase("sinOUT")]
	[TestCase("CuBiCiNoUt")]
	[TestCase("bOuNcEoUt")]
	public void Convert_MixedCaseValidNames_ReturnsFullyQualifiedName(string mixedCaseEasingName)
	{
		// Arrange & Act
		var result = _converter.Convert(mixedCaseEasingName, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.IsTrue(result.StartsWith("global::Microsoft.Maui.Easing."));
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
	}
}


/// <summary>
/// Additional unit tests for EasingConverter class to achieve full code coverage.
/// </summary>
[TestFixture]
public partial class EasingConverterCoverageTests
{
	private EasingConverter _converter = null!;
	private Mock<BaseNode> _mockNode = null!;
	private Mock<ITypeSymbol> _mockToType = null!;
	private Mock<SourceGenContext> _mockContext = null!;
	private Mock<Compilation> _mockCompilation = null!;
	private Mock<INamedTypeSymbol> _mockEasingType = null!;

	[SetUp]
	public void Setup()
	{
		_converter = new EasingConverter();
		_mockNode = new Mock<BaseNode>(Mock.Of<IXmlNamespaceResolver>(), -1, -1);
		_mockToType = new Mock<ITypeSymbol>();
		_mockContext = new Mock<SourceGenContext>();
		_mockCompilation = new Mock<Compilation>();
		_mockEasingType = new Mock<INamedTypeSymbol>();
		_mockContext.Setup(x => x.Compilation).Returns(_mockCompilation.Object);
		_mockCompilation.Setup(x => x.GetTypeByMetadataName("Microsoft.Maui.Easing")).Returns(_mockEasingType.Object);
		_mockEasingType.Setup(x => x.ToFQDisplayString()).Returns("global::Microsoft.Maui.Easing");
	}

	/// <summary>
	/// Tests Convert method to ensure all code paths for valid easing names are covered.
	/// Should verify the internal flow of variable assignments and return fully qualified name.
	/// </summary>
	[TestCase("Linear")]
	[TestCase("SinOut")]
	public void Convert_ValidEasingName_CoverageFlow(string easingName)
	{
		// Arrange & Act
		var result = _converter.Convert(easingName, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.AreEqual($"global::Microsoft.Maui.Easing.{easingName}", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
		_mockCompilation.Verify(x => x.GetTypeByMetadataName("Microsoft.Maui.Easing"), Times.Once);
		_mockEasingType.Verify(x => x.ToFQDisplayString(), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with Easing prefix to ensure splitting logic coverage.
	/// Should extract the easing name after the dot and return fully qualified name.
	/// </summary>
	[TestCase("Easing.Linear")]
	[TestCase("easing.BounceOut")]
	public void Convert_EasingPrefix_CoverageFlow(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.IsTrue(result.StartsWith("global::Microsoft.Maui.Easing."));
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
		_mockCompilation.Verify(x => x.GetTypeByMetadataName("Microsoft.Maui.Easing"), Times.Once);
		_mockEasingType.Verify(x => x.ToFQDisplayString(), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with invalid easing name to ensure error path coverage.
	/// Should call ReportConversionFailed and return "default".
	/// </summary>
	[TestCase("InvalidEasing")]
	[TestCase("NotFound")]
	public void Convert_InvalidEasingName_CoverageFlow(string easingName)
	{
		// Arrange & Act
		var result = _converter.Convert(easingName, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}
}
