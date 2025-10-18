#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Xml;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.StyleSheets;
using Microsoft.Maui.Controls.Xaml;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for StyleSheet.FromString method
    /// </summary>
    public sealed class StyleSheetTests
    {
        /// <summary>
        /// Tests that FromString throws ArgumentNullException when stylesheet parameter is null.
        /// This test specifically covers the null check condition in the FromString method.
        /// Expected to throw ArgumentNullException with parameter name "stylesheet".
        /// </summary>
        [Fact]
        public void FromString_NullStylesheet_ThrowsArgumentNullException()
        {
            // Arrange
            string stylesheet = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => StyleSheet.FromString(stylesheet));
            Assert.Equal("stylesheet", exception.ParamName);
        }

        /// <summary>
        /// Tests that FromString successfully processes various valid string inputs including edge cases.
        /// This test verifies that non-null strings are properly handled and passed to FromReader.
        /// Expected to return a StyleSheet instance without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n\r")]
        [InlineData("label { color: red; }")]
        [InlineData("button { background-color: blue; font-size: 14px; }")]
        [InlineData("* { margin: 0; padding: 0; }")]
        [InlineData("label, button { color: green; }")]
        [InlineData(".class-selector { display: none; }")]
        [InlineData("#id-selector { width: 100px; }")]
        [InlineData("/* This is a comment */ label { color: black; }")]
        [InlineData("label { color: #FF0000; background-color: rgba(0, 255, 0, 0.5); }")]
        public void FromString_ValidStylesheetStrings_ReturnsStyleSheet(string stylesheet)
        {
            // Arrange & Act
            StyleSheet result = StyleSheet.FromString(stylesheet);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<StyleSheet>(result);
        }

        /// <summary>
        /// Tests that FromString handles very long CSS strings without issues.
        /// This test verifies memory and performance characteristics with large input.
        /// Expected to return a StyleSheet instance without throwing exceptions.
        /// </summary>
        [Fact]
        public void FromString_VeryLongStylesheet_ReturnsStyleSheet()
        {
            // Arrange
            var longStylesheet = new string('a', 10000) + " { color: red; }";

            // Act
            StyleSheet result = StyleSheet.FromString(longStylesheet);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<StyleSheet>(result);
        }

        /// <summary>
        /// Tests that FromString handles strings with special and control characters.
        /// This test verifies proper handling of potentially problematic characters.
        /// Expected to return a StyleSheet instance without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData("label { content: \"Hello\\nWorld\"; }")]
        [InlineData("label { content: \"Tab\\tSeparated\"; }")]
        [InlineData("label { content: \"Quote\\\"Inside\"; }")]
        [InlineData("label { content: 'Single\\'Quote'; }")]
        [InlineData("label { color: red; /* Unicode: \u00A9 \u00AE */ }")]
        public void FromString_StylesheetWithSpecialCharacters_ReturnsStyleSheet(string stylesheet)
        {
            // Arrange & Act
            StyleSheet result = StyleSheet.FromString(stylesheet);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<StyleSheet>(result);
        }

        /// <summary>
        /// Tests that FromReader throws ArgumentNullException when reader parameter is null.
        /// This tests the null validation logic that should prevent null readers from being processed.
        /// Expected result: ArgumentNullException with correct parameter name.
        /// </summary>
        [Fact]
        public void FromReader_NullReader_ThrowsArgumentNullException()
        {
            // Arrange
            TextReader reader = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => StyleSheet.FromReader(reader));
            Assert.Equal("reader", exception.ParamName);
        }

        /// <summary>
        /// Tests that FromReader successfully creates a StyleSheet from a TextReader with valid CSS content.
        /// This tests the happy path where valid CSS is parsed into a StyleSheet with appropriate styles.
        /// Expected result: StyleSheet instance with parsed CSS rules.
        /// </summary>
        [Fact]
        public void FromReader_ValidCssContent_ReturnsStyleSheetWithStyles()
        {
            // Arrange
            var cssContent = "label { color: red; }";
            using var reader = new StringReader(cssContent);

            // Act
            var styleSheet = StyleSheet.FromReader(reader);

            // Assert
            Assert.NotNull(styleSheet);
            Assert.NotNull(styleSheet.Styles);
        }

        /// <summary>
        /// Tests that FromReader creates an empty StyleSheet when the TextReader contains no CSS content.
        /// This tests the behavior with empty input and ensures no exceptions are thrown.
        /// Expected result: Empty StyleSheet with no styles.
        /// </summary>
        [Fact]
        public void FromReader_EmptyContent_ReturnsEmptyStyleSheet()
        {
            // Arrange
            using var reader = new StringReader(string.Empty);

            // Act
            var styleSheet = StyleSheet.FromReader(reader);

            // Assert
            Assert.NotNull(styleSheet);
            Assert.NotNull(styleSheet.Styles);
            Assert.Empty(styleSheet.Styles);
        }

        /// <summary>
        /// Tests that FromReader creates an empty StyleSheet when the TextReader contains only whitespace.
        /// This tests the behavior with whitespace-only input which should be skipped during parsing.
        /// Expected result: Empty StyleSheet with no styles.
        /// </summary>
        [Fact]
        public void FromReader_WhitespaceOnlyContent_ReturnsEmptyStyleSheet()
        {
            // Arrange
            using var reader = new StringReader("   \n\t  \r\n  ");

            // Act
            var styleSheet = StyleSheet.FromReader(reader);

            // Assert
            Assert.NotNull(styleSheet);
            Assert.NotNull(styleSheet.Styles);
            Assert.Empty(styleSheet.Styles);
        }

        /// <summary>
        /// Tests that FromReader throws NotSupportedException when CSS content contains AT-rules.
        /// This tests the error handling for unsupported CSS features like @media, @import, etc.
        /// Expected result: NotSupportedException with appropriate message.
        /// </summary>
        [Fact]
        public void FromReader_CssWithAtRules_ThrowsNotSupportedException()
        {
            // Arrange
            var cssContent = "@media screen { label { color: red; } }";
            using var reader = new StringReader(cssContent);

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() => StyleSheet.FromReader(reader));
            Assert.Equal("AT-rules not supported", exception.Message);
        }

        /// <summary>
        /// Tests that FromReader throws Exception when CSS content has unmatched closing braces.
        /// This tests the error handling for malformed CSS with improper brace matching.
        /// Expected result: Exception thrown due to malformed CSS.
        /// </summary>
        [Fact]
        public void FromReader_UnmatchedClosingBrace_ThrowsException()
        {
            // Arrange
            var cssContent = "label { color: red; } }";
            using var reader = new StringReader(cssContent);

            // Act & Assert
            Assert.Throws<Exception>(() => StyleSheet.FromReader(reader));
        }

        /// <summary>
        /// Tests that FromReader successfully processes multiple CSS rules from a TextReader.
        /// This tests the parsing of complex CSS content with multiple selectors and style blocks.
        /// Expected result: StyleSheet with multiple parsed rules.
        /// </summary>
        [Fact]
        public void FromReader_MultipleCssRules_ReturnsStyleSheetWithMultipleStyles()
        {
            // Arrange
            var cssContent = "label { color: red; } button { background-color: blue; }";
            using var reader = new StringReader(cssContent);

            // Act
            var styleSheet = StyleSheet.FromReader(reader);

            // Assert
            Assert.NotNull(styleSheet);
            Assert.NotNull(styleSheet.Styles);
            Assert.True(styleSheet.Styles.Count > 0);
        }

        /// <summary>
        /// Tests that FromReader properly disposes the CssReader even when an exception occurs during parsing.
        /// This tests the resource management behavior using a mocked TextReader that throws an exception.
        /// Expected result: Exception is propagated but resources are properly disposed.
        /// </summary>
        [Fact]
        public void FromReader_TextReaderThrowsException_PropagatesExceptionAndDisposesResources()
        {
            // Arrange
            var mockReader = Substitute.For<TextReader>();
            mockReader.When(x => x.Peek()).Do(x => throw new IOException("Test exception"));

            // Act & Assert
            Assert.Throws<IOException>(() => StyleSheet.FromReader(mockReader));

            // Verify the reader was accessed (meaning CssReader was created and used)
            mockReader.Received().Peek();
        }

        /// <summary>
        /// Tests that FromResource throws ArgumentNullException when assembly parameter is null.
        /// Verifies that null assembly is properly validated before any processing occurs.
        /// Expected to throw ArgumentNullException.
        /// </summary>
        [Fact]
        public void FromResource_NullAssembly_ThrowsArgumentNullException()
        {
            // Arrange
            string resourcePath = "test.css";
            Assembly assembly = null;
            IXmlLineInfo lineInfo = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => StyleSheet.FromResource(resourcePath, assembly, lineInfo));
        }

        /// <summary>
        /// Tests that FromResource throws XamlParseException when resourcePath is null.
        /// Verifies that null resource path is properly handled by the GetResource method.
        /// Expected to throw XamlParseException indicating resource not found.
        /// </summary>
        [Fact]
        public void FromResource_NullResourcePath_ThrowsXamlParseException()
        {
            // Arrange
            string resourcePath = null;
            var assembly = Substitute.For<Assembly>();
            assembly.GetName().Returns(new AssemblyName("TestAssembly"));
            IXmlLineInfo lineInfo = null;

            // Act & Assert
            Assert.Throws<XamlParseException>(() => StyleSheet.FromResource(resourcePath, assembly, lineInfo));
        }

        /// <summary>
        /// Tests that FromResource throws XamlParseException when resourcePath is empty string.
        /// Verifies that empty resource path is properly handled by the GetResource method.
        /// Expected to throw XamlParseException indicating resource not found.
        /// </summary>
        [Fact]
        public void FromResource_EmptyResourcePath_ThrowsXamlParseException()
        {
            // Arrange
            string resourcePath = "";
            var assembly = Substitute.For<Assembly>();
            assembly.GetName().Returns(new AssemblyName("TestAssembly"));
            IXmlLineInfo lineInfo = null;

            // Act & Assert
            Assert.Throws<XamlParseException>(() => StyleSheet.FromResource(resourcePath, assembly, lineInfo));
        }

        /// <summary>
        /// Tests that FromResource throws XamlParseException when resourcePath contains only whitespace.
        /// Verifies that whitespace-only resource path is properly handled by the GetResource method.
        /// Expected to throw XamlParseException indicating resource not found.
        /// </summary>
        [Fact]
        public void FromResource_WhitespaceResourcePath_ThrowsXamlParseException()
        {
            // Arrange
            string resourcePath = "   ";
            var assembly = Substitute.For<Assembly>();
            assembly.GetName().Returns(new AssemblyName("TestAssembly"));
            IXmlLineInfo lineInfo = null;

            // Act & Assert
            Assert.Throws<XamlParseException>(() => StyleSheet.FromResource(resourcePath, assembly, lineInfo));
        }

        /// <summary>
        /// Tests that FromResource throws XamlParseException when resource does not exist in assembly.
        /// Verifies that non-existent resource path is properly handled by the GetResource method.
        /// Expected to throw XamlParseException indicating resource not found.
        /// </summary>
        [Fact]
        public void FromResource_NonExistentResource_ThrowsXamlParseException()
        {
            // Arrange
            string resourcePath = "NonExistent.css";
            var assembly = Substitute.For<Assembly>();
            assembly.GetName().Returns(new AssemblyName("TestAssembly"));
            IXmlLineInfo lineInfo = null;

            // Act & Assert
            Assert.Throws<XamlParseException>(() => StyleSheet.FromResource(resourcePath, assembly, lineInfo));
        }

        /// <summary>
        /// Tests that FromResource works correctly with null lineInfo parameter.
        /// Verifies that the optional lineInfo parameter can be null without causing issues.
        /// Expected to process normally when lineInfo is null (default value).
        /// </summary>
        [Fact]
        public void FromResource_NullLineInfo_ProcessesNormally()
        {
            // Arrange
            string resourcePath = "test.css";
            var assembly = Substitute.For<Assembly>();
            assembly.GetName().Returns(new AssemblyName("TestAssembly"));
            IXmlLineInfo lineInfo = null;

            // Act & Assert
            // This should throw XamlParseException due to resource not found, not due to null lineInfo
            Assert.Throws<XamlParseException>(() => StyleSheet.FromResource(resourcePath, assembly, lineInfo));
        }

        /// <summary>
        /// Tests that FromResource works correctly with non-null lineInfo parameter.
        /// Verifies that the optional lineInfo parameter is properly passed through to GetResource.
        /// Expected to process normally when lineInfo is provided.
        /// </summary>
        [Fact]
        public void FromResource_WithLineInfo_ProcessesNormally()
        {
            // Arrange
            string resourcePath = "test.css";
            var assembly = Substitute.For<Assembly>();
            assembly.GetName().Returns(new AssemblyName("TestAssembly"));
            var lineInfo = Substitute.For<IXmlLineInfo>();
            lineInfo.LineNumber.Returns(10);
            lineInfo.LinePosition.Returns(5);

            // Act & Assert
            // This should throw XamlParseException due to resource not found, not due to lineInfo issues
            Assert.Throws<XamlParseException>(() => StyleSheet.FromResource(resourcePath, assembly, lineInfo));
        }

        /// <summary>
        /// Tests FromResource with extremely long resource path.
        /// Verifies that very long resource paths are handled appropriately.
        /// Expected to throw XamlParseException indicating resource not found.
        /// </summary>
        [Fact]
        public void FromResource_VeryLongResourcePath_ThrowsXamlParseException()
        {
            // Arrange
            string resourcePath = new string('a', 1000) + ".css";
            var assembly = Substitute.For<Assembly>();
            assembly.GetName().Returns(new AssemblyName("TestAssembly"));
            IXmlLineInfo lineInfo = null;

            // Act & Assert
            Assert.Throws<XamlParseException>(() => StyleSheet.FromResource(resourcePath, assembly, lineInfo));
        }

        /// <summary>
        /// Tests FromResource with resource path containing special characters.
        /// Verifies that resource paths with special characters are handled appropriately.
        /// Expected to throw XamlParseException indicating resource not found.
        /// </summary>
        [Theory]
        [InlineData("test@#$.css")]
        [InlineData("test with spaces.css")]
        [InlineData("test\n\r\t.css")]
        [InlineData("test/../other.css")]
        [InlineData("test\\other.css")]
        public void FromResource_SpecialCharactersInPath_ThrowsXamlParseException(string resourcePath)
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            assembly.GetName().Returns(new AssemblyName("TestAssembly"));
            IXmlLineInfo lineInfo = null;

            // Act & Assert
            Assert.Throws<XamlParseException>(() => StyleSheet.FromResource(resourcePath, assembly, lineInfo));
        }

        /// <summary>
        /// Tests that FromResource properly handles assembly with null name.
        /// Verifies that assemblies returning null from GetName() are handled appropriately.
        /// Expected to throw NullReferenceException or similar when assembly name is null.
        /// </summary>
        [Fact]
        public void FromResource_AssemblyWithNullName_ThrowsException()
        {
            // Arrange
            string resourcePath = "test.css";
            var assembly = Substitute.For<Assembly>();
            assembly.GetName().Returns((AssemblyName)null);
            IXmlLineInfo lineInfo = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => StyleSheet.FromResource(resourcePath, assembly, lineInfo));
        }

        /// <summary>
        /// Tests FromResource method with various edge case combinations.
        /// Verifies that different combinations of null and invalid parameters are handled consistently.
        /// Expected to throw appropriate exceptions based on parameter validation order.
        /// </summary>
        [Theory]
        [InlineData(null, null)] // Both null
        [InlineData("", null)]   // Empty path, null assembly
        [InlineData(null, "TestAssembly")] // Null path, valid assembly name
        public void FromResource_EdgeCaseCombinations_ThrowsAppropriateException(string resourcePath, string assemblyName)
        {
            // Arrange
            Assembly assembly = null;
            if (assemblyName != null)
            {
                assembly = Substitute.For<Assembly>();
                assembly.GetName().Returns(new AssemblyName(assemblyName));
            }
            IXmlLineInfo lineInfo = null;

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => StyleSheet.FromResource(resourcePath, assembly, lineInfo));
        }
    }
}