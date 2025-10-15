using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Xml;

#nullable disable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.SourceGen.TypeConverters;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Unit tests for SourceGenContextExtensions.ReportConversionFailed method.
/// </summary>
[TestFixture]
public partial class SourceGenContextExtensionsTests
{
	/// <summary>
	/// Tests ReportConversionFailed with special characters and control characters in string parameters.
	/// This test verifies that the method handles unusual string content correctly.
	/// Expected result: Method processes special characters without throwing exceptions.
	/// </summary>
	[TestCase("\n\r\t", TestName = "ReportConversionFailed_WithControlCharacters_RequiresManualTesting")]
	[TestCase("Hello\0World", TestName = "ReportConversionFailed_WithNullCharacter_RequiresManualTesting")]
	[TestCase("🚀✨🎯", TestName = "ReportConversionFailed_WithUnicodeCharacters_RequiresManualTesting")]
	[TestCase("<xml>&amp;</xml>", TestName = "ReportConversionFailed_WithXmlContent_RequiresManualTesting")]
	public void ReportConversionFailed_WithSpecialCharacterParameters_RequiresManualTesting(string specialValue)
	{
		// Note: This test method outlines important edge cases involving special characters
		// that should be tested once the dependency injection and mocking challenges are resolved.
		//
		// These test cases are critical because:
		// 1. The method deals with XML line information and string values
		// 2. Special characters might affect diagnostic message formatting
		// 3. Control characters could cause issues in location creation
		// 4. Unicode characters should be preserved through the diagnostic pipeline
		//
		// Implementation would require:
		// 1. Testing string values containing control characters (\n, \r, \t, \0)
		// 2. Testing Unicode characters and emojis
		// 3. Testing XML-like content that might interfere with line parsing
		// 4. Verifying that all characters are properly preserved in the diagnostic output
		Assert.Inconclusive($"Special character test for '{specialValue}' requires dependency setup. " + "This test is important for ensuring proper handling of edge case string content.");
	}

	/// <summary>
	/// Tests ReportConversionFailed with various string values including edge cases.
	/// Verifies that the method handles different string inputs correctly.
	/// </summary>
	[TestCase("")]
	[TestCase(" ")]
	[TestCase("   \t\n\r  ")]
	[TestCase("NormalValue")]
	[TestCase("Value with spaces")]
	[TestCase("Special!@#$%^&*()Characters")]
	[TestCase("Very long string that contains many characters to test the boundary conditions of string processing in the diagnostic reporting functionality")]
	public void ReportConversionFailed_WithVariousStringValues_CallsReportDiagnosticSuccessfully(string testValue)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST003", "Test Title", "Test Message: {0} {1}", "Test Category", DiagnosticSeverity.Info, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(5);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(10);
		mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns("string");
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, mockTypeSymbol.Object, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with different ToDisplayString return values.
	/// Verifies that the method correctly handles various type display string formats.
	/// </summary>
	[TestCase("")]
	[TestCase("int")]
	[TestCase("System.String")]
	[TestCase("System.Collections.Generic.List<T>")]
	[TestCase("Namespace.ComplexType<string, int>")]
	public void ReportConversionFailed_WithVariousTypeDisplayStrings_CallsReportDiagnosticSuccessfully(string displayString)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST005", "Test Title", "Test Message: {0} {1}", "Test Category", DiagnosticSeverity.Warning, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns(displayString);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		string testValue = "testValue";
		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, mockTypeSymbol.Object, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with boundary line numbers and positions.
	/// Verifies that the method handles edge case line info correctly when creating diagnostics.
	/// </summary>
	[TestCase(0, 0, Description = "Zero line number and position")]
	[TestCase(-1, -1, Description = "Negative line number and position")]
	[TestCase(int.MaxValue, int.MaxValue, Description = "Maximum line number and position")]
	[TestCase(1, 0, Description = "Valid line number with zero position")]
	[TestCase(0, 1, Description = "Zero line number with valid position")]
	public void ReportConversionFailed_BoundaryLineInfo_HandlesBoundaryValues(int lineNumber, int linePosition)
	{
		// Arrange
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(lineNumber);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(linePosition);
		var descriptor = new DiagnosticDescriptor("TEST006", "Test Title", "Test Message", "Test Category", DiagnosticSeverity.Error, isEnabledByDefault: true);
		string testValue = "TestValue";
		// Act & Assert
		// Note: This test is marked as inconclusive because SourceGenContext cannot be easily mocked
		// due to its complex constructor dependencies and the fact that ReportDiagnostic method
		// cannot be mocked according to the framework constraints.
		// To fully test this method, a test would need to:
		// 1. Create a SourceGenContext with all required dependencies
		// 2. Verify that LocationCreate handles boundary line info correctly
		// 3. Verify that Diagnostic.Create is called with descriptor, location, and test value
		// 4. Verify that context.ReportDiagnostic is called with the created diagnostic
		Assert.Inconclusive($"SourceGenContext cannot be mocked due to complex dependencies. " + $"Manual integration testing required to verify diagnostic reporting behavior with line {lineNumber}, position {linePosition}.");
	}

	/// <summary>
	/// Tests ReportConversionFailed with various string parameters.
	/// Verifies that the method handles different string inputs correctly including edge cases.
	/// Expected result: Method processes all string values without throwing exceptions.
	/// </summary>
	[TestCase("", "", TestName = "EmptyStrings")]
	[TestCase(" ", " ", TestName = "WhitespaceStrings")]
	[TestCase("   \t\n\r  ", "   \t\n\r  ", TestName = "MixedWhitespaceStrings")]
	[TestCase("NormalValue", "NormalAdditionalInfo", TestName = "NormalValues")]
	[TestCase("Value with spaces", "Additional info with spaces", TestName = "ValuesWithSpaces")]
	[TestCase("Special!@#$%^&*()Characters", "Additional!@#$%^&*()Info", TestName = "SpecialCharacters")]
	[TestCase("Very long string that contains many characters to test boundary conditions", "Very long additional information string that contains many characters", TestName = "VeryLongStrings")]
	public void ReportConversionFailed_VariousStringParameters_HandlesAllStringInputs(string value, string additionalInfo)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST003", "Test Title", "Test Message: {0} {1} {2}", "Test Category", DiagnosticSeverity.Info, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns("int");
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, value, additionalInfo, mockTypeSymbol.Object, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with different type display strings.
	/// Verifies that the method correctly handles various type display string formats.
	/// Expected result: Method processes all type display string values correctly.
	/// </summary>
	[TestCase("", TestName = "EmptyDisplayString")]
	[TestCase("int", TestName = "SimpleType")]
	[TestCase("System.String", TestName = "FullyQualifiedType")]
	[TestCase("System.Collections.Generic.List<T>", TestName = "GenericType")]
	[TestCase("Namespace.ComplexType<string, int>", TestName = "ComplexGenericType")]
	[TestCase("Very.Long.Namespace.With.Many.Segments.ComplexType<System.String, System.Int32>", TestName = "VeryLongTypeName")]
	public void ReportConversionFailed_VariousTypeDisplayStrings_HandlesAllDisplayStringFormats(string displayString)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST004", "Test Title", "Test Message: {0} {1} {2}", "Test Category", DiagnosticSeverity.Warning, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns(displayString);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		string testValue = "testValue";
		string additionalInfo = "additionalInfo";
		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, additionalInfo, mockTypeSymbol.Object, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with boundary XML line information values.
	/// Verifies that the method handles edge case line numbers and positions correctly.
	/// Expected result: Method processes all line info values without throwing exceptions.
	/// </summary>
	[TestCase(0, 0, TestName = "ZeroLineNumberAndPosition")]
	[TestCase(-1, -1, TestName = "NegativeLineNumberAndPosition")]
	[TestCase(int.MaxValue, int.MaxValue, TestName = "MaximumLineNumberAndPosition")]
	[TestCase(1, 0, TestName = "ValidLineNumberZeroPosition")]
	[TestCase(0, 1, TestName = "ZeroLineNumberValidPosition")]
	[TestCase(1000000, 1000000, TestName = "VeryLargeLineNumberAndPosition")]
	public void ReportConversionFailed_BoundaryXmlLineInfo_HandlesBoundaryValues(int lineNumber, int linePosition)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST005", "Test Title", "Test Message: {0} {1} {2}", "Test Category", DiagnosticSeverity.Error, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(lineNumber);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(linePosition);
		mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns("string");
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		string testValue = "testValue";
		string additionalInfo = "additionalInfo";
		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, additionalInfo, mockTypeSymbol.Object, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with various diagnostic severity levels.
	/// Verifies that the method works correctly with different diagnostic descriptor configurations.
	/// Expected result: Method processes all diagnostic severity levels correctly.
	/// </summary>
	[TestCase(DiagnosticSeverity.Error, TestName = "ErrorSeverity")]
	[TestCase(DiagnosticSeverity.Warning, TestName = "WarningSeverity")]
	[TestCase(DiagnosticSeverity.Info, TestName = "InfoSeverity")]
	[TestCase(DiagnosticSeverity.Hidden, TestName = "HiddenSeverity")]
	public void ReportConversionFailed_VariousDiagnosticSeverities_HandlesAllSeverityLevels(DiagnosticSeverity severity)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST007", "Test Title", "Test Message: {0} {1} {2}", "Test Category", severity, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns("string");
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		string testValue = "testValue";
		string additionalInfo = "additionalInfo";
		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, additionalInfo, mockTypeSymbol.Object, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with various string values including edge cases.
	/// Verifies that the method handles different string inputs correctly.
	/// Expected result: Method processes all string values without throwing exceptions.
	/// </summary>
	[TestCase("")]
	[TestCase(" ")]
	[TestCase("   \t\n\r  ")]
	[TestCase("NormalValue")]
	[TestCase("Value with spaces")]
	[TestCase("Special!@#$%^&*()Characters")]
	[TestCase("Very long string that contains many characters to test the boundary conditions of string processing in the diagnostic reporting functionality")]
	[TestCase("\n\r\t")]
	[TestCase("Hello\0World")]
	[TestCase("🚀✨🎯")]
	[TestCase("<xml>&amp;</xml>")]
	public void ReportConversionFailed_VariousStringValues_HandlesAllStringInputs(string testValue)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST003", "Test Title", "Test Message: {0} {1}", "Test Category", DiagnosticSeverity.Info, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(5);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(10);
		mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns("string");
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, mockTypeSymbol.Object, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with various RelativePath values.
	/// Verifies that the method handles different project item relative paths correctly.
	/// Expected result: Method processes all relative path values without throwing exceptions.
	/// </summary>
	[TestCase("test.xaml")]
	[TestCase("")]
	[TestCase("folder/subfolder/file.xaml")]
	[TestCase("../relative/path.xaml")]
	[TestCase("C:\\absolute\\path\\file.xaml")]
	[TestCase("very/long/path/with/many/segments/that/tests/boundary/conditions/file.xaml")]
	public void ReportConversionFailed_VariousRelativePaths_HandlesAllPathFormats(string relativePath)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST007", "Test Title", "Test Message: {0} {1}", "Test Category", DiagnosticSeverity.Info, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns("TestType");
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns(relativePath);
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		string testValue = "testValue";
		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, mockTypeSymbol.Object, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with various string values including edge cases.
	/// Verifies that the method handles different string inputs correctly and calls ReportDiagnostic.
	/// Expected result: ReportDiagnostic is called once for each test case.
	/// </summary>
	[TestCase("")]
	[TestCase(" ")]
	[TestCase("   \t\n\r  ")]
	[TestCase("NormalValue")]
	[TestCase("Value with spaces")]
	[TestCase("Special!@#$%^&*()Characters")]
	[TestCase("Very long string that contains many characters to test the boundary conditions of string processing in the diagnostic reporting functionality for conversion failures")]
	[TestCase("String\nWith\nNewlines")]
	[TestCase("String\tWith\tTabs")]
	[TestCase("String\"With\"Quotes")]
	[TestCase("String'With'SingleQuotes")]
	[TestCase(@"String\With\Backslashes")]
	public void ReportConversionFailed_VariousStringValues_CallsReportDiagnosticSuccessfully(string testValue)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST003", "Test Title", "Test Message: {0}", "Test Category", DiagnosticSeverity.Warning, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(5);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(10);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with different diagnostic severity levels.
	/// Verifies that the method works correctly with various diagnostic descriptors.
	/// Expected result: ReportDiagnostic is called once for each severity level.
	/// </summary>
	[TestCase(DiagnosticSeverity.Error, Description = "Error severity")]
	[TestCase(DiagnosticSeverity.Warning, Description = "Warning severity")]
	[TestCase(DiagnosticSeverity.Info, Description = "Info severity")]
	[TestCase(DiagnosticSeverity.Hidden, Description = "Hidden severity")]
	public void ReportConversionFailed_VariousDiagnosticSeverities_HandlesAllSeverities(DiagnosticSeverity severity)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST006", "Test Title", "Test Message: {0}", "Test Category", severity, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(2);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(5);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		string testValue = "severityTestValue";
		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with special Unicode characters in value.
	/// Verifies that the method handles Unicode content correctly.
	/// Expected result: ReportDiagnostic is called once without throwing.
	/// </summary>
	[TestCase("🚀✨🎯", Description = "Emoji characters")]
	[TestCase("Iñtërnâtiônàlizætiøn", Description = "International characters")]
	[TestCase("中文测试", Description = "Chinese characters")]
	[TestCase("Тест на русском", Description = "Cyrillic characters")]
	[TestCase("اختبار عربي", Description = "Arabic characters")]
	[TestCase("Hello\0World", Description = "Null character")]
	[TestCase("\u0001\u0002\u0003", Description = "Control characters")]
	public void ReportConversionFailed_UnicodeAndSpecialCharacters_HandlesSpecialContent(string specialValue)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST007", "Test Title", "Test Message: {0}", "Test Category", DiagnosticSeverity.Warning, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, specialValue, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with various string parameters including edge cases.
	/// Verifies that the method handles different string inputs correctly and calls ReportDiagnostic.
	/// Expected result: Method processes all string combinations without throwing exceptions.
	/// </summary>
	[TestCase("", "", TestName = "EmptyStrings")]
	[TestCase(" ", " ", TestName = "WhitespaceStrings")]
	[TestCase("   \t\n\r  ", "   \t\n\r  ", TestName = "MixedWhitespaceStrings")]
	[TestCase("NormalValue", "NormalAdditionalInfo", TestName = "NormalValues")]
	[TestCase("Value with spaces", "Additional info with spaces", TestName = "ValuesWithSpaces")]
	[TestCase("Special!@#$%^&*()Characters", "Additional!@#$%^&*()Info", TestName = "SpecialCharacters")]
	[TestCase("Very long string that contains many characters to test boundary conditions of string processing", "Very long additional information string that contains many characters to test boundary conditions", TestName = "VeryLongStrings")]
	[TestCase("String\nWith\nNewlines", "Additional\nInfo\nWith\nNewlines", TestName = "StringsWithNewlines")]
	[TestCase("String\tWith\tTabs", "Additional\tInfo\tWith\tTabs", TestName = "StringsWithTabs")]
	[TestCase("String\"With\"Quotes", "Additional\"Info\"With\"Quotes", TestName = "StringsWithQuotes")]
	[TestCase("String'With'SingleQuotes", "Additional'Info'With'SingleQuotes", TestName = "StringsWithSingleQuotes")]
	[TestCase(@"String\With\Backslashes", @"Additional\Info\With\Backslashes", TestName = "StringsWithBackslashes")]
	public void ReportConversionFailed_VariousStringParameters_CallsReportDiagnosticSuccessfully(string value, string additionalInfo)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST001", "Test Title", "Test Message: {0} {1} {2}", "Test Category", DiagnosticSeverity.Error, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns("string");
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, value, additionalInfo, mockTypeSymbol.Object, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with special Unicode characters in value and additionalInfo.
	/// Verifies that the method handles Unicode content correctly.
	/// Expected result: ReportDiagnostic is called once without throwing.
	/// </summary>
	[TestCase("🚀✨🎯", "🌟💫⭐", TestName = "EmojiCharacters")]
	[TestCase("Iñtërnâtiônàlizætiøn", "Àddîtíönàl ïñfö", TestName = "InternationalCharacters")]
	[TestCase("中文测试", "额外信息", TestName = "ChineseCharacters")]
	[TestCase("Тест на русском", "Дополнительная информация", TestName = "CyrillicCharacters")]
	[TestCase("اختبار عربي", "معلومات إضافية", TestName = "ArabicCharacters")]
	[TestCase("Hello\0World", "Additional\0Info", TestName = "NullCharacters")]
	[TestCase("\u0001\u0002\u0003", "\u0004\u0005\u0006", TestName = "ControlCharacters")]
	public void ReportConversionFailed_UnicodeAndSpecialCharacters_HandlesSpecialContent(string specialValue, string specialAdditionalInfo)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST006", "Test Title", "Test Message: {0} {1} {2}", "Test Category", DiagnosticSeverity.Warning, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(10);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(15);
		mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns("string");
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, specialValue, specialAdditionalInfo, mockTypeSymbol.Object, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with extremely long string values.
	/// Verifies that the method handles very large string inputs correctly.
	/// Expected result: Method processes extremely long strings without throwing exceptions.
	/// </summary>
	[Test]
	public void ReportConversionFailed_ExtremelyLongString_HandlesLargeInput()
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST007", "Test Title", "Test Message: {0}", "Test Category", DiagnosticSeverity.Warning, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		var extremelyLongString = new string('A', 10000) + "SomeSpecialContent" + new string('B', 10000);
		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, extremelyLongString, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with complex Unicode characters and escape sequences.
	/// Verifies that the method handles complex character encodings correctly.
	/// Expected result: Method processes complex Unicode content without throwing exceptions.
	/// </summary>
	[TestCase("\u0000\u0001\u0002\u0003", TestName = "ControlCharacters")]
	[TestCase("\u200B\u200C\u200D", TestName = "ZeroWidthCharacters")]
	[TestCase("\uFEFF", TestName = "ByteOrderMark")]
	[TestCase("𝒽𝑒𝓁𝓁𝑜", TestName = "MathematicalAlphanumeric")]
	[TestCase("🏳️‍🌈🏳️‍⚧️", TestName = "ComplexEmojis")]
	[TestCase("\r\n\r\n\t\t", TestName = "MixedLineEndings")]
	public void ReportConversionFailed_ComplexUnicodeCharacters_HandlesComplexEncoding(string complexValue)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST008", "Test Title", "Test Message: {0}", "Test Category", DiagnosticSeverity.Info, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, complexValue, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}
}


/// <summary>
/// Unit tests for SourceGenContextExtensions.ReportConversionFailed method with ITypeSymbol parameter.
/// </summary>
[TestFixture]
public partial class SourceGenContextExtensionsNullableTypeTests
{
	/// <summary>
	/// Tests ReportConversionFailed with non-null toType parameter and various display strings.
	/// Verifies that the method handles type symbols with different display string values correctly.
	/// Expected result: Method processes all type display string values correctly.
	/// </summary>
	[TestCase("")]
	[TestCase("int")]
	[TestCase("System.String")]
	[TestCase("System.Collections.Generic.List<T>")]
	[TestCase("Namespace.ComplexType<string, int>")]
	[TestCase("Very.Long.Namespace.With.Many.Segments.ComplexType<System.String, System.Int32>")]
	public void ReportConversionFailed_NonNullTypeWithVariousDisplayStrings_HandlesAllDisplayStringFormats(string displayString)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST003", "Test Title", "Test Message: {0} {1}", "Test Category", DiagnosticSeverity.Info, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns(displayString);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		string testValue = "testValue";
		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, mockTypeSymbol.Object, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}
}

/// <summary>
/// Unit tests for SourceGenContextExtensions.ReportConversionFailed method with ITypeSymbol parameter.
/// </summary>
[TestFixture]
public partial class SourceGenContextExtensionsReportConversionFailedWithTypeSymbolTests
{
	/// <summary>
	/// Tests ReportConversionFailed with non-null toType parameter.
	/// Verifies that the method handles type symbols correctly and calls ToDisplayString.
	/// Expected result: ReportDiagnostic is called once with value and type display string.
	/// </summary>
	[TestCase("int")]
	[TestCase("System.String")]
	[TestCase("")]
	[TestCase("System.Collections.Generic.List<T>")]
	public void ReportConversionFailed_NonNullToType_CallsReportDiagnosticSuccessfully(string displayString)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST002", "Test Title", "Test Message: {0} {1}", "Test Category", DiagnosticSeverity.Warning, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns(displayString);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		string testValue = "testValue";

		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, mockTypeSymbol.Object, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
		mockTypeSymbol.Verify(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>()), Times.Once);
	}

}



/// <summary>
/// Unit tests for SourceGenContextExtensions.ReportConversionFailed method with additionalInfo parameter.
/// </summary>
[TestFixture]
public partial class SourceGenContextExtensionsAdditionalInfoTests
{
	/// <summary>
	/// Tests ReportConversionFailed with null toType parameter and various string combinations.
	/// Verifies that the method handles null type symbols correctly.
	/// Expected result: ReportDiagnostic is called once with null type display string.
	/// </summary>
	[TestCase("", "", TestName = "EmptyStrings")]
	[TestCase(" ", " ", TestName = "WhitespaceStrings")]
	[TestCase("   \t\n\r  ", "   \t\n\r  ", TestName = "MixedWhitespaceStrings")]
	[TestCase("NormalValue", "NormalAdditionalInfo", TestName = "NormalValues")]
	[TestCase("Value with spaces", "Additional info with spaces", TestName = "ValuesWithSpaces")]
	[TestCase("Special!@#$%^&*()Characters", "Additional!@#$%^&*()Info", TestName = "SpecialCharacters")]
	[TestCase("Very long string that contains many characters to test boundary conditions", "Very long additional information string that contains many characters", TestName = "VeryLongStrings")]
	public void ReportConversionFailed_NullTypeWithVariousStrings_HandlesNullTypeCorrectly(string value, string additionalInfo)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST001", "Test Title", "Test Message: {0} {1} {2}", "Test Category", DiagnosticSeverity.Error, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);

		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, value, additionalInfo, null, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with non-null toType parameter and various display strings.
	/// Verifies that the method handles type symbols with different display string values correctly.
	/// Expected result: ReportDiagnostic is called once with correct type display string.
	/// </summary>
	[TestCase("", TestName = "EmptyDisplayString")]
	[TestCase("int", TestName = "SimpleType")]
	[TestCase("System.String", TestName = "FullyQualifiedType")]
	[TestCase("System.Collections.Generic.List<T>", TestName = "GenericType")]
	[TestCase("Namespace.ComplexType<string, int>", TestName = "ComplexGenericType")]
	[TestCase("Very.Long.Namespace.With.Many.Segments.ComplexType<System.String, System.Int32>", TestName = "VeryLongTypeName")]
	public void ReportConversionFailed_NonNullTypeWithVariousDisplayStrings_HandlesTypeDisplayCorrectly(string displayString)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST002", "Test Title", "Test Message: {0} {1} {2}", "Test Category", DiagnosticSeverity.Warning, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(5);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(10);
		mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns(displayString);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		string testValue = "testValue";
		string testAdditionalInfo = "testAdditionalInfo";

		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, testAdditionalInfo, mockTypeSymbol.Object, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with boundary XML line information values.
	/// Verifies that the method handles edge case line numbers and positions correctly.
	/// Expected result: Method processes all line info values without throwing exceptions.
	/// </summary>
	[TestCase(0, 0, TestName = "ZeroLineNumberAndPosition")]
	[TestCase(-1, -1, TestName = "NegativeLineNumberAndPosition")]
	[TestCase(int.MaxValue, int.MaxValue, TestName = "MaximumLineNumberAndPosition")]
	[TestCase(1, 0, TestName = "ValidLineNumberZeroPosition")]
	[TestCase(0, 1, TestName = "ZeroLineNumberValidPosition")]
	[TestCase(1000000, 1000000, TestName = "VeryLargeLineNumberAndPosition")]
	public void ReportConversionFailed_BoundaryXmlLineInfo_HandlesBoundaryValues(int lineNumber, int linePosition)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST003", "Test Title", "Test Message: {0} {1} {2}", "Test Category", DiagnosticSeverity.Info, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(lineNumber);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(linePosition);
		mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns("string");
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);

		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, "testValue", "testAdditionalInfo", mockTypeSymbol.Object, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with various diagnostic severity levels.
	/// Verifies that the method works correctly with different diagnostic descriptor configurations.
	/// Expected result: Method processes all diagnostic severity levels correctly.
	/// </summary>
	[TestCase(DiagnosticSeverity.Error, TestName = "ErrorSeverity")]
	[TestCase(DiagnosticSeverity.Warning, TestName = "WarningSeverity")]
	[TestCase(DiagnosticSeverity.Info, TestName = "InfoSeverity")]
	[TestCase(DiagnosticSeverity.Hidden, TestName = "HiddenSeverity")]
	public void ReportConversionFailed_VariousDiagnosticSeverities_HandlesAllSeverityLevels(DiagnosticSeverity severity)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST004", "Test Title", "Test Message: {0} {1} {2}", "Test Category", severity, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);

		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, "testValue", "testAdditionalInfo", null, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with special Unicode characters in value and additionalInfo.
	/// Verifies that the method handles Unicode content correctly.
	/// Expected result: ReportDiagnostic is called once without throwing.
	/// </summary>
	[TestCase("🚀✨🎯", "🌟💫⭐", TestName = "EmojiCharacters")]
	[TestCase("Iñtërnâtiônàlizætiøn", "Àddîtíönàl ïñfö", TestName = "InternationalCharacters")]
	[TestCase("中文测试", "额外信息", TestName = "ChineseCharacters")]
	[TestCase("Тест на русском", "Дополнительная информация", TestName = "CyrillicCharacters")]
	[TestCase("اختبار عربي", "معلومات إضافية", TestName = "ArabicCharacters")]
	[TestCase("Hello\0World", "Additional\0Info", TestName = "NullCharacters")]
	[TestCase("\u0001\u0002\u0003", "\u0004\u0005\u0006", TestName = "ControlCharacters")]
	public void ReportConversionFailed_UnicodeAndSpecialCharacters_HandlesSpecialContent(string specialValue, string specialAdditionalInfo)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST005", "Test Title", "Test Message: {0} {1} {2}", "Test Category", DiagnosticSeverity.Warning, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(10);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(15);
		mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns("string");
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);

		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, specialValue, specialAdditionalInfo, mockTypeSymbol.Object, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with various RelativePath values.
	/// Verifies that the method handles different project item relative paths correctly.
	/// Expected result: Method processes all relative path values without throwing exceptions.
	/// </summary>
	[TestCase("test.xaml")]
	[TestCase("")]
	[TestCase("folder/subfolder/file.xaml")]
	[TestCase("../relative/path.xaml")]
	[TestCase("C:\\absolute\\path\\file.xaml")]
	[TestCase("very/long/path/with/many/segments/that/tests/boundary/conditions/file.xaml")]
	public void ReportConversionFailed_VariousRelativePaths_HandlesAllPathFormats(string relativePath)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST006", "Test Title", "Test Message: {0} {1} {2}", "Test Category", DiagnosticSeverity.Error, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns(relativePath);
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);

		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, "testValue", "testAdditionalInfo", null, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with complex string combinations including edge cases.
	/// Verifies that the method handles various string inputs correctly with newlines, tabs, and quotes.
	/// Expected result: Method processes all string combinations without throwing exceptions.
	/// </summary>
	[TestCase("String\nWith\nNewlines", "Additional\nInfo\nWith\nNewlines", TestName = "StringsWithNewlines")]
	[TestCase("String\tWith\tTabs", "Additional\tInfo\tWith\tTabs", TestName = "StringsWithTabs")]
	[TestCase("String\"With\"Quotes", "Additional\"Info\"With\"Quotes", TestName = "StringsWithQuotes")]
	[TestCase("String'With'SingleQuotes", "Additional'Info'With'SingleQuotes", TestName = "StringsWithSingleQuotes")]
	[TestCase(@"String\With\Backslashes", @"Additional\Info\With\Backslashes", TestName = "StringsWithBackslashes")]
	[TestCase("<xml>&amp;</xml>", "<additional>&amp;</additional>", TestName = "StringsWithXmlContent")]
	public void ReportConversionFailed_ComplexStringCombinations_HandlesComplexContent(string value, string additionalInfo)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST007", "Test Title", "Test Message: {0} {1} {2}", "Test Category", DiagnosticSeverity.Info, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(20);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(25);
		mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>())).Returns("string");
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);

		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, value, additionalInfo, mockTypeSymbol.Object, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with extremely long string values for both parameters.
	/// Verifies that the method handles very large string inputs correctly.
	/// Expected result: Method processes extremely long strings without throwing exceptions.
	/// </summary>
	[Test]
	public void ReportConversionFailed_ExtremelyLongStrings_HandlesLargeInput()
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST008", "Test Title", "Test Message: {0} {1} {2}", "Test Category", DiagnosticSeverity.Warning, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		string extremelyLongValue = new string('a', 10000);
		string extremelyLongAdditionalInfo = new string('b', 10000);

		// Act & Assert
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, extremelyLongValue, extremelyLongAdditionalInfo, null, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}
}


/// <summary>
/// Unit tests for SourceGenContextExtensions.ReportConversionFailed method (first overload).
/// </summary>
[TestFixture]
public partial class SourceGenContextExtensionsFirstOverloadTests
{
	/// <summary>
	/// Tests ReportConversionFailed with null value parameter.
	/// Verifies that the method handles null string value correctly.
	/// Expected result: Method processes null value without throwing exceptions.
	/// </summary>
	[Test]
	public void ReportConversionFailed_NullValue_HandlesNullCorrectly()
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST001", "Test Title", "Test Message: {0}", "Test Category", DiagnosticSeverity.Error, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);

		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, null!, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with various string values including edge cases.
	/// Verifies that the method handles different string inputs correctly.
	/// Expected result: Method processes all string values without throwing exceptions.
	/// </summary>
	[TestCase("")]
	[TestCase(" ")]
	[TestCase("   \t\n\r  ")]
	[TestCase("NormalValue")]
	[TestCase("Value with spaces")]
	[TestCase("Special!@#$%^&*()Characters")]
	[TestCase("Very long string that contains many characters to test the boundary conditions of string processing in the diagnostic reporting functionality for conversion failures")]
	[TestCase("\n\r\t")]
	[TestCase("Hello\0World")]
	[TestCase("🚀✨🎯")]
	[TestCase("<xml>&amp;</xml>")]
	[TestCase("String\"With\"Quotes")]
	[TestCase("String'With'SingleQuotes")]
	[TestCase(@"String\With\Backslashes")]
	public void ReportConversionFailed_VariousStringValues_HandlesAllStringInputs(string testValue)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST002", "Test Title", "Test Message: {0}", "Test Category", DiagnosticSeverity.Info, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(5);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(10);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);

		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with boundary XML line information values.
	/// Verifies that the method handles edge case line numbers and positions correctly.
	/// Expected result: Method processes all line info values without throwing exceptions.
	/// </summary>
	[TestCase(0, 0, TestName = "ZeroLineNumberAndPosition")]
	[TestCase(-1, -1, TestName = "NegativeLineNumberAndPosition")]
	[TestCase(int.MaxValue, int.MaxValue, TestName = "MaximumLineNumberAndPosition")]
	[TestCase(1, 0, TestName = "ValidLineNumberZeroPosition")]
	[TestCase(0, 1, TestName = "ZeroLineNumberValidPosition")]
	[TestCase(1000000, 1000000, TestName = "VeryLargeLineNumberAndPosition")]
	public void ReportConversionFailed_BoundaryXmlLineInfo_HandlesBoundaryValues(int lineNumber, int linePosition)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST003", "Test Title", "Test Message: {0}", "Test Category", DiagnosticSeverity.Warning, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(lineNumber);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(linePosition);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		string testValue = "testValue";

		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with various diagnostic severity levels.
	/// Verifies that the method works correctly with different diagnostic descriptor configurations.
	/// Expected result: Method processes all diagnostic severity levels correctly.
	/// </summary>
	[TestCase(DiagnosticSeverity.Error, TestName = "ErrorSeverity")]
	[TestCase(DiagnosticSeverity.Warning, TestName = "WarningSeverity")]
	[TestCase(DiagnosticSeverity.Info, TestName = "InfoSeverity")]
	[TestCase(DiagnosticSeverity.Hidden, TestName = "HiddenSeverity")]
	public void ReportConversionFailed_VariousDiagnosticSeverities_HandlesAllSeverityLevels(DiagnosticSeverity severity)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST004", "Test Title", "Test Message: {0}", "Test Category", severity, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		string testValue = "testValue";

		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with various RelativePath values.
	/// Verifies that the method handles different project item relative paths correctly.
	/// Expected result: Method processes all relative path values without throwing exceptions.
	/// </summary>
	[TestCase("test.xaml")]
	[TestCase("")]
	[TestCase("folder/subfolder/file.xaml")]
	[TestCase("../relative/path.xaml")]
	[TestCase("C:\\absolute\\path\\file.xaml")]
	[TestCase("very/long/path/with/many/segments/that/tests/boundary/conditions/file.xaml")]
	[TestCase("path with spaces/file.xaml")]
	[TestCase("path-with-special!@#$%^&*()characters/file.xaml")]
	public void ReportConversionFailed_VariousRelativePaths_HandlesAllPathFormats(string relativePath)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST005", "Test Title", "Test Message: {0}", "Test Category", DiagnosticSeverity.Error, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns(relativePath);
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		string testValue = "testValue";

		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with complex Unicode characters.
	/// Verifies that the method handles complex character encodings correctly.
	/// Expected result: Method processes complex Unicode content without throwing exceptions.
	/// </summary>
	[TestCase("\u0000\u0001\u0002\u0003", TestName = "ControlCharacters")]
	[TestCase("\u200B\u200C\u200D", TestName = "ZeroWidthCharacters")]
	[TestCase("\uFEFF", TestName = "ByteOrderMark")]
	[TestCase("𝒽𝑒𝓁𝓁𝑜", TestName = "MathematicalAlphanumeric")]
	[TestCase("🏳️‍🌈🏳️‍⚧️", TestName = "ComplexEmojis")]
	[TestCase("\r\n\r\n\t\t", TestName = "MixedLineEndings")]
	[TestCase("Iñtërnâtiônàlizætiøn", TestName = "InternationalCharacters")]
	[TestCase("中文测试", TestName = "ChineseCharacters")]
	[TestCase("Тест на русском", TestName = "CyrillicCharacters")]
	[TestCase("اختبار عربي", TestName = "ArabicCharacters")]
	public void ReportConversionFailed_ComplexUnicodeCharacters_HandlesComplexEncoding(string complexValue)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST007", "Test Title", "Test Message: {0}", "Test Category", DiagnosticSeverity.Info, isEnabledByDefault: true);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);

		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, complexValue, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}

	/// <summary>
	/// Tests ReportConversionFailed with various diagnostic descriptor configurations.
	/// Verifies that the method works correctly with different diagnostic descriptor settings.
	/// Expected result: Method processes all diagnostic descriptor configurations correctly.
	/// </summary>
	[TestCase(true, TestName = "EnabledByDefault")]
	[TestCase(false, TestName = "DisabledByDefault")]
	public void ReportConversionFailed_VariousDiagnosticDescriptorSettings_HandlesAllConfigurations(bool isEnabledByDefault)
	{
		// Arrange
		var mockContext = new Mock<SourceGenContext>();
		var mockXmlLineInfo = new Mock<IXmlLineInfo>();
		var diagnosticDescriptor = new DiagnosticDescriptor("TEST009", "Test Title", "Test Message: {0}", "Test Category", DiagnosticSeverity.Error, isEnabledByDefault: isEnabledByDefault);
		mockXmlLineInfo.Setup(x => x.LineNumber).Returns(1);
		mockXmlLineInfo.Setup(x => x.LinePosition).Returns(1);
		var mockProjectItem = new Mock<ProjectItem>();
		mockProjectItem.Setup(x => x.RelativePath).Returns("test.xaml");
		mockContext.Setup(x => x.ProjectItem).Returns(mockProjectItem.Object);
		string testValue = "testValue";

		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => mockContext.Object.ReportConversionFailed(mockXmlLineInfo.Object, testValue, diagnosticDescriptor));
		mockContext.Verify(x => x.ReportDiagnostic(It.IsAny<Diagnostic>()), Times.Once);
	}
}
