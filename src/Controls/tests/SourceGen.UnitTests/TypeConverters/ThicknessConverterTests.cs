using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;


using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.SourceGen.TypeConverters;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;



/// <summary>
/// Unit tests for ThicknessConverter class.
/// </summary>
[TestFixture]
public partial class ThicknessConverterTests
{
	private ThicknessConverter _converter = null!;
	private Mock<BaseNode> _mockNode = null!;
	private Mock<ITypeSymbol> _mockToType = null!;
	private Mock<SourceGenContext> _mockContext = null!;
	private Mock<Compilation> _mockCompilation = null!;
	private Mock<INamedTypeSymbol> _mockThicknessType = null!;

	/// <summary>
	/// Tests Convert method with valid two-value thickness (horizontal, vertical).
	/// Should return proper Thickness constructor call with two parameters.
	/// </summary>
	[TestCase("5,10")]
	[TestCase("10.5,20.5")]
	[TestCase("0,0")]
	[TestCase("123.456,789.012")]
	public void Convert_ValidTwoValues_ReturnsThicknessConstructorWithTwoParameters(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.IsTrue(result.StartsWith("new Microsoft.Maui.Thickness("));
		Assert.IsTrue(result.EndsWith(")"));
		var commaCount = result.Split(',').Length - 1;
		Assert.AreEqual(1, commaCount, "Should have exactly one comma for two parameters");
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
	}

	/// <summary>
	/// Tests Convert method with valid four-value thickness (left, top, right, bottom).
	/// Should return proper Thickness constructor call with four parameters.
	/// </summary>
	[TestCase("5,10,15,20")]
	[TestCase("10.5,20.5,30.5,40.5")]
	[TestCase("0,0,0,0")]
	[TestCase("123.456,789.012,345.678,901.234")]
	public void Convert_ValidFourValues_ReturnsThicknessConstructorWithFourParameters(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.IsTrue(result.StartsWith("new Microsoft.Maui.Thickness("));
		Assert.IsTrue(result.EndsWith(")"));
		var commaCount = result.Split(',').Length - 1;
		Assert.AreEqual(3, commaCount, "Should have exactly three commas for four parameters");
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
	}

	/// <summary>
	/// Tests Convert method with values containing extra whitespace.
	/// Should trim values and return proper Thickness constructor call.
	/// </summary>
	[TestCase("  5  ")]
	[TestCase("  5  ,  10  ")]
	[TestCase("  5  ,  10  ,  15  ,  20  ")]
	public void Convert_ValuesWithWhitespace_TrimsAndReturnsThicknessConstructor(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.IsTrue(result.StartsWith("new Microsoft.Maui.Thickness("));
		Assert.IsTrue(result.EndsWith(")"));
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
	}

	/// <summary>
	/// Tests Convert method with special double values.
	/// Should handle special floating-point values correctly.
	/// </summary>
	[TestCase("NaN")]
	[TestCase("Infinity")]
	[TestCase("-Infinity")]
	public void Convert_SpecialDoubleValues_HandlesCorrectly(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
		{
			Assert.IsTrue(result.StartsWith("new Microsoft.Maui.Thickness("));
			_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
		}
		else
		{
			Assert.AreEqual("default", result);
			_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
		}
	}

	/// <summary>
	/// Tests Convert method with extreme numeric values.
	/// Should handle boundary values correctly.
	/// </summary>
	[TestCase("0")]
	[TestCase("-0")]
	[TestCase("1.7976931348623157E+308")] // double.MaxValue
	[TestCase("-1.7976931348623157E+308")] // double.MinValue
	[TestCase("4.94065645841247E-324")] // double.Epsilon
	public void Convert_ExtremeNumericValues_HandlesCorrectly(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
		{
			Assert.IsTrue(result.StartsWith("new Microsoft.Maui.Thickness("));
			_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
		}
		else
		{
			Assert.AreEqual("default", result);
			_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
		}
	}

	/// <summary>
	/// Tests Convert method with invalid single values.
	/// Should report conversion failed and return default.
	/// </summary>
	[TestCase("abc")]
	[TestCase("5.5.5")]
	[TestCase("5x")]
	[TestCase("")]
	[TestCase("   ")]
	[TestCase("--5")]
	[TestCase("5..5")]
	public void Convert_InvalidSingleValues_ReportsConversionFailedAndReturnsDefault(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with invalid two-value combinations.
	/// Should report conversion failed and return default when any value is invalid.
	/// </summary>
	[TestCase("5,abc")]
	[TestCase("abc,5")]
	[TestCase("abc,def")]
	[TestCase("5,")]
	[TestCase(",5")]
	[TestCase("5,,5")]
	public void Convert_InvalidTwoValues_ReportsConversionFailedAndReturnsDefault(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with invalid four-value combinations.
	/// Should report conversion failed and return default when any value is invalid.
	/// </summary>
	[TestCase("5,10,15,abc")]
	[TestCase("abc,10,15,20")]
	[TestCase("5,abc,15,20")]
	[TestCase("5,10,abc,20")]
	[TestCase("5,10,15,")]
	[TestCase(",10,15,20")]
	[TestCase("5,,15,20")]
	public void Convert_InvalidFourValues_ReportsConversionFailedAndReturnsDefault(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with wrong number of comma-separated values.
	/// Should report conversion failed and return default for unsupported value counts.
	/// </summary>
	[TestCase("5,10,15")] // 3 values - not supported
	[TestCase("5,10,15,20,25")] // 5 values - not supported
	[TestCase("5,10,15,20,25,30")] // 6 values - not supported
	public void Convert_WrongNumberOfValues_ReportsConversionFailedAndReturnsDefault(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with negative values.
	/// Should handle negative numbers correctly.
	/// </summary>
	[TestCase("-5")]
	[TestCase("-5,-10")]
	[TestCase("-5,-10,-15,-20")]
	public void Convert_NegativeValues_HandlesCorrectly(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.IsTrue(result.StartsWith("new Microsoft.Maui.Thickness("));
		Assert.IsTrue(result.EndsWith(")"));
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
	}

	/// <summary>
	/// Tests Convert method with scientific notation values.
	/// Should handle scientific notation correctly if supported by double.TryParse.
	/// </summary>
	[TestCase("1e5")]
	[TestCase("1.5e-3")]
	[TestCase("1E10")]
	public void Convert_ScientificNotation_HandlesCorrectly(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
		{
			Assert.IsTrue(result.StartsWith("new Microsoft.Maui.Thickness("));
			_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
		}
		else
		{
			Assert.AreEqual("default", result);
			_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
		}
	}

	/// <summary>
	/// Tests Convert method with comma but only one value.
	/// Should report conversion failed for single value with comma.
	/// </summary>
	[TestCase("5,")]
	[TestCase(",")]
	[TestCase(" , ")]
	public void Convert_SingleValueWithComma_ReportsConversionFailedAndReturnsDefault(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}


	/// <summary>
	/// Sets up test dependencies and mock objects before each test.
	/// Configures the mock chain for proper dependency injection.
	/// </summary>
	[SetUp]
	public void SetUp()
	{
		// Arrange
		_converter = new ThicknessConverter();
		_mockNode = new Mock<BaseNode>();
		_mockToType = new Mock<ITypeSymbol>();
		_mockContext = new Mock<SourceGenContext>();
		_mockCompilation = new Mock<Compilation>();
		_mockThicknessType = new Mock<INamedTypeSymbol>();

		// Setup mock chain: context.Compilation returns compilation mock
		_mockContext.Setup(x => x.Compilation).Returns(_mockCompilation.Object);

		// Setup compilation to return thickness type mock
		_mockCompilation.Setup(x => x.GetTypeByMetadataName("Microsoft.Maui.Thickness"))
						.Returns(_mockThicknessType.Object);

		// Setup thickness type to return fully qualified name
		_mockThicknessType.Setup(x => x.ToFQDisplayString())
						 .Returns("Microsoft.Maui.Thickness");

		// Setup node to be castable to IXmlLineInfo
		_mockNode.As<IXmlLineInfo>();
	}

	/// <summary>
	/// Tests Convert method with null value input.
	/// Should report conversion failed and return default when value is null.
	/// </summary>
	[Test]
	public void Convert_NullValue_ReportsConversionFailedAndReturnsDefault()
	{
		// Arrange & Act
		var result = _converter.Convert(null!, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.AreEqual("default", result);
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Once);
	}

	/// <summary>
	/// Tests Convert method with valid single uniform thickness value.
	/// Should return proper Thickness constructor call with single parameter.
	/// </summary>
	[TestCase("5")]
	[TestCase("10.5")]
	[TestCase("0")]
	[TestCase("123.456")]
	public void Convert_ValidSingleValue_ReturnsThicknessConstructorWithSingleParameter(string value)
	{
		// Arrange & Act
		var result = _converter.Convert(value, _mockNode.Object, _mockToType.Object, _mockContext.Object);

		// Assert
		Assert.IsTrue(result.StartsWith("new Microsoft.Maui.Thickness("));
		Assert.IsTrue(result.EndsWith(")"));
		var commaCount = result.Split(',').Length - 1;
		Assert.AreEqual(0, commaCount, "Should have no commas for single parameter");
		_mockContext.Verify(x => x.ReportConversionFailed(It.IsAny<IXmlLineInfo>(), It.IsAny<string>(), It.IsAny<DiagnosticDescriptor>()), Times.Never);
	}

}
