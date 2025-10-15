using System;
using System.CodeDom.Compiler;
using System.Collections;
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

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.TypeConverters;


[TestFixture]
public class ConstraintConverterTests
{
	private ConstraintConverter _converter = null!;
	private Mock<BaseNode> _mockNode = null!;
	private Mock<ITypeSymbol> _mockToType = null!;
	private Mock<SourceGenContext> _mockContext = null!;
	private Mock<Compilation> _mockCompilation = null!;
	private Mock<INamedTypeSymbol> _mockConstraintType = null!;

	[SetUp]
	public void SetUp()
	{
		_converter = new ConstraintConverter();
		_mockNode = new Mock<BaseNode>();
		_mockToType = new Mock<ITypeSymbol>();
		_mockContext = new Mock<SourceGenContext>();
		_mockCompilation = new Mock<Compilation>();
		_mockConstraintType = new Mock<INamedTypeSymbol>();
		_mockContext.Setup(c => c.Compilation).Returns(_mockCompilation.Object);
		_mockCompilation.Setup(c => c.GetTypeByMetadataName("Microsoft.Maui.Controls.Compatibility.Constraint")).Returns(_mockConstraintType.Object);
		_mockConstraintType.Setup(ct => ct.ToFQDisplayString()).Returns("Microsoft.Maui.Controls.Compatibility.Constraint");
	}

	/// <summary>
	/// Tests Convert method with valid double value.
	/// Should return formatted Constraint.Constant call with parsed value.
	/// </summary>
	[TestCase("42", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(42)")]
	[TestCase("0", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(0)")]
	[TestCase("-123.45", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(-123.45)")]
	[TestCase("3.14159", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(3.14159)")]
	[TestCase("1.7976931348623157E+308", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(1.7976931348623157E+308)")]
	[TestCase("-1.7976931348623157E+308", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(-1.7976931348623157E+308)")]
	[TestCase("4.9406564584124654E-324", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(4.9406564584124654E-324)")]
	public void Convert_ValidDoubleValue_ReturnsConstraintConstant(string value, string expected)
	{
		// Arrange
		// SetUp already configures mocks
		// Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual(expected, result);
		_mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<ITypeSymbol>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
	}

	/// <summary>
	/// Tests Convert method with valid double value containing whitespace.
	/// Should trim whitespace and return formatted Constraint.Constant call.
	/// </summary>
	[TestCase("  42  ", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(42)")]
	[TestCase("\t123.45\n", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(123.45)")]
	[TestCase(" \r\n -99.9 \t ", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(-99.9)")]
	public void Convert_ValidDoubleValueWithWhitespace_TrimsAndReturnsConstraintConstant(string value, string expected)
	{
		// Arrange
		// SetUp already configures mocks
		// Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual(expected, result);
		_mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<ITypeSymbol>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
	}

	/// <summary>
	/// Tests Convert method with whitespace-only string value.
	/// Should report conversion failure and return "default".
	/// </summary>
	[TestCase("   ")]
	[TestCase("\t")]
	[TestCase("\n")]
	[TestCase("\r\n")]
	[TestCase(" \t \n \r ")]
	public void Convert_WhitespaceOnlyValue_ReportsFailureAndReturnsDefault(string value)
	{
		// Arrange
		// SetUp already configures mocks
		// Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), value, _mockToType.Object, It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with invalid numeric string values.
	/// Should report conversion failure and return "default".
	/// </summary>
	[TestCase("abc")]
	[TestCase("12.34.56")]
	[TestCase("1e")]
	[TestCase("infinity")]
	[TestCase("nan")]
	[TestCase("++123")]
	[TestCase("--456")]
	[TestCase("12a34")]
	[TestCase("text123")]
	[TestCase("123text")]
	[TestCase("")]
	[TestCase("1,234")]
	[TestCase("$123")]
	[TestCase("123%")]
	public void Convert_InvalidNumericValue_ReportsFailureAndReturnsDefault(string value)
	{
		// Arrange
		// SetUp already configures mocks
		// Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), value, _mockToType.Object, It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with special double values that cannot be parsed.
	/// Should report conversion failure and return "default".
	/// </summary>
	[TestCase("NaN")]
	[TestCase("Infinity")]
	[TestCase("-Infinity")]
	[TestCase("PositiveInfinity")]
	[TestCase("NegativeInfinity")]
	public void Convert_SpecialDoubleStringValues_ReportsFailureAndReturnsDefault(string value)
	{
		// Arrange
		// SetUp already configures mocks
		// Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), value, _mockToType.Object, It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method when GetTypeByMetadataName returns null.
	/// Should handle null constraint type gracefully.
	/// </summary>
	[Test]
	[Category("ProductionBugSuspected")]
	public void Convert_GetTypeByMetadataNameReturnsNull_ThrowsNullReferenceException()
	{
		// Arrange
		var value = "42";
		_mockCompilation.Setup(c => c.GetTypeByMetadataName("Microsoft.Maui.Controls.Compatibility.Constraint")).Returns((INamedTypeSymbol?)null);
		// Act & Assert
		Assert.Throws<NullReferenceException>(() => _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object));
	}

	/// <summary>
	/// Tests Convert method with null node parameter.
	/// Should throw InvalidCastException when casting null to IXmlLineInfo.
	/// </summary>
	[Test]
	[Category("ProductionBugSuspected")]
	public void Convert_NullNode_ThrowsInvalidCastException()
	{
		// Arrange
		var value = "invalid";
		BaseNode? node = null;
		// Act & Assert
		Assert.Throws<InvalidCastException>(() => _converter.Convert(value, node!, _mockToType.Object, _mockContext.Object));
	}

	/// <summary>
	/// Tests Convert method with null context parameter.
	/// Should throw NullReferenceException when accessing context.Compilation.
	/// </summary>
	[Test]
	[Category("ProductionBugSuspected")]
	public void Convert_NullContext_ThrowsNullReferenceException()
	{
		// Arrange
		var value = "42";
		SourceGenContext? context = null;
		// Act & Assert
		Assert.Throws<NullReferenceException>(() => _converter.Convert(value, _mockNode.Object, _mockToType.Object, context!));
	}

	/// <summary>
	/// Tests Convert method with boundary double values.
	/// Should successfully parse and return constraint constant for extreme values.
	/// </summary>
	[TestCase("1.7976931348623157E+308")] // double.MaxValue
	[TestCase("-1.7976931348623157E+308")] // double.MinValue
	[TestCase("4.9406564584124654E-324")] // double.Epsilon
	[TestCase("0")]
	[TestCase("-0")]
	public void Convert_BoundaryDoubleValues_ReturnsConstraintConstant(string value)
	{
		// Arrange
		// SetUp already configures mocks
		// Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);
		// Assert
		Assert.That(result, Does.StartWith("Microsoft.Maui.Controls.Compatibility.Constraint.Constant("));
		Assert.That(result, Does.EndWith(")"));
		_mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<ITypeSymbol>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
	}

	/// <summary>
	/// Tests Convert method with scientific notation values.
	/// Should successfully parse scientific notation and return constraint constant.
	/// </summary>
	[TestCase("1e10", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(10000000000)")]
	[TestCase("1.5e-10", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(1.5E-10)")]
	[TestCase("2.5E+5", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(250000)")]
	[TestCase("-3.14e2", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(-314)")]
	public void Convert_ScientificNotation_ReturnsConstraintConstant(string value, string expected)
	{
		// Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.That(result, Is.EqualTo(expected));
		_mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<ITypeSymbol>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
	}

	/// <summary>
	/// Tests Convert method with various decimal formats.
	/// Should successfully parse different decimal formats and return constraint constant.
	/// </summary>
	[TestCase("0.0")]
	[TestCase("1.")]
	[TestCase(".5")]
	[TestCase("123.456789")]
	[TestCase("-0.001")]
	public void Convert_DecimalFormats_ReturnsConstraintConstant(string value)
	{
		// Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.That(result, Does.StartWith("Microsoft.Maui.Controls.Compatibility.Constraint.Constant("));
		Assert.That(result, Does.EndWith(")"));
		_mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<ITypeSymbol>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
	}

	/// <summary>
	/// Tests Convert method with special characters and unicode in strings.
	/// Should report conversion failure for non-numeric strings with special characters.
	/// </summary>
	[TestCase("123\u0000")]
	[TestCase("\u2013123")] // en dash
	[TestCase("123\u00A0")] // non-breaking space
	[TestCase("12\u20093")] // thin space
	[TestCase("±123")]
	[TestCase("×123")]
	public void Convert_SpecialCharactersInStrings_ReportsFailureAndReturnsDefault(string value)
	{
		// Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.That(result, Is.EqualTo("default"));
		_mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), value, _mockToType.Object, It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with string containing only unicode whitespace characters.
	/// Should trim to empty and report conversion failure.
	/// </summary>
	[TestCase("\u00A0")]    // Non-breaking space
	[TestCase("\u2000")]    // En quad
	[TestCase("\u2001")]    // Em quad  
	[TestCase("\u2002")]    // En space
	[TestCase("\u2003")]    // Em space
	[TestCase("\u2004")]    // Three-per-em space
	[TestCase("\u2005")]    // Four-per-em space
	[TestCase("\u2006")]    // Six-per-em space
	[TestCase("\u2007")]    // Figure space
	[TestCase("\u2008")]    // Punctuation space
	[TestCase("\u2009")]    // Thin space
	[TestCase("\u200A")]    // Hair space
	[TestCase("\u3000")]    // Ideographic space
	public void Convert_UnicodeWhitespaceOnly_TrimsToEmptyAndReportsFailure(string value)
	{
		// Arrange
		// SetUp already configures mocks

		// Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), value, _mockToType.Object, It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with values that become empty after trimming unicode whitespace.
	/// Should report conversion failure after trimming.
	/// </summary>
	[TestCase("\u00A0\u00A0")]        // Multiple non-breaking spaces
	[TestCase("\u2000\u2001\u2002")]  // Mixed unicode spaces
	[TestCase("\u3000\u3000")]        // Multiple ideographic spaces
	public void Convert_MultipleUnicodeWhitespace_TrimsToEmptyAndReportsFailure(string value)
	{
		// Arrange
		// SetUp already configures mocks

		// Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), value, _mockToType.Object, It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with valid numeric values surrounded by unicode whitespace.
	/// Should successfully trim unicode whitespace and parse the number.
	/// </summary>
	[TestCase("\u00A042\u00A0", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(42)")]
	[TestCase("\u200042\u2000", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(42)")]
	[TestCase("\u3000123.45\u3000", "Microsoft.Maui.Controls.Compatibility.Constraint.Constant(123.45)")]
	public void Convert_ValidNumberWithUnicodeWhitespace_TrimsAndReturnsConstraintConstant(string value, string expected)
	{
		// Arrange
		// SetUp already configures mocks

		// Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.AreEqual(expected, result);
		_mockContext.Verify(c => c.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<ITypeSymbol>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
	}
}