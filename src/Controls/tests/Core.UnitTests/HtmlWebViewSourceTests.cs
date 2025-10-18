#nullable disable

using System;
using System.ComponentModel;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class HtmlWebViewSourceTests
    {
        /// <summary>
        /// Tests that the Html property getter returns the default value when no value has been set.
        /// Input: No value set on Html property.
        /// Expected: Returns null (the default string value).
        /// </summary>
        [Fact]
        public void Html_WhenNotSet_ReturnsDefaultValue()
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource();

            // Act
            var result = htmlWebViewSource.Html;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Html property getter returns the correct value after setting various string values.
        /// Input: Different string values set via the Html property setter.
        /// Expected: Getter returns the exact string value that was set.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("simple html")]
        [InlineData("<html><body>Hello World</body></html>")]
        [InlineData("<div>Test &amp; content</div>")]
        [InlineData("Very long HTML content that spans multiple lines and contains various HTML tags like <p>, <div>, <span> and special characters like &lt;, &gt;, &amp;, &quot;, &#39; and unicode characters like ñáéíóú and emojis 🎉🚀")]
        [InlineData("HTML with control characters: \u0001\u0002\u0003\u0004\u0005")]
        [InlineData("HTML with null character: \0")]
        [InlineData("HTML with tab and newline: \t\n\r")]
        public void Html_WhenValueSet_ReturnsSetValue(string htmlValue)
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource();

            // Act
            htmlWebViewSource.Html = htmlValue;
            var result = htmlWebViewSource.Html;

            // Assert
            Assert.Equal(htmlValue, result);
        }

        /// <summary>
        /// Tests that the Html property getter returns null when explicitly set to null.
        /// Input: Html property set to null.
        /// Expected: Getter returns null.
        /// </summary>
        [Fact]
        public void Html_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource();

            // Act
            htmlWebViewSource.Html = null;
            var result = htmlWebViewSource.Html;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Html property getter returns the most recently set value when changed multiple times.
        /// Input: Html property set to multiple different values sequentially.
        /// Expected: Getter always returns the most recently set value.
        /// </summary>
        [Fact]
        public void Html_WhenChangedMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource();

            // Act & Assert
            htmlWebViewSource.Html = "first value";
            Assert.Equal("first value", htmlWebViewSource.Html);

            htmlWebViewSource.Html = "second value";
            Assert.Equal("second value", htmlWebViewSource.Html);

            htmlWebViewSource.Html = null;
            Assert.Null(htmlWebViewSource.Html);

            htmlWebViewSource.Html = "final value";
            Assert.Equal("final value", htmlWebViewSource.Html);
        }

        /// <summary>
        /// Tests that Load method calls LoadHtml on the renderer with the correct Html and BaseUrl values.
        /// Input: Valid renderer, valid Html and BaseUrl properties.
        /// Expected: LoadHtml is called once with the Html and BaseUrl values.
        /// </summary>
        [Fact]
        public void Load_ValidRendererWithHtmlAndBaseUrl_CallsLoadHtmlWithCorrectParameters()
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource
            {
                Html = "<html><body>Test Content</body></html>",
                BaseUrl = "https://example.com"
            };
            var mockRenderer = Substitute.For<IWebViewDelegate>();

            // Act
            htmlWebViewSource.Load(mockRenderer);

            // Assert
            mockRenderer.Received(1).LoadHtml("<html><body>Test Content</body></html>", "https://example.com");
        }

        /// <summary>
        /// Tests that Load method calls LoadHtml with null values when Html and BaseUrl are null.
        /// Input: Valid renderer, null Html and BaseUrl properties.
        /// Expected: LoadHtml is called once with null values.
        /// </summary>
        [Fact]
        public void Load_ValidRendererWithNullHtmlAndBaseUrl_CallsLoadHtmlWithNullValues()
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource
            {
                Html = null,
                BaseUrl = null
            };
            var mockRenderer = Substitute.For<IWebViewDelegate>();

            // Act
            htmlWebViewSource.Load(mockRenderer);

            // Assert
            mockRenderer.Received(1).LoadHtml(null, null);
        }

        /// <summary>
        /// Tests that Load method calls LoadHtml with empty strings when Html and BaseUrl are empty.
        /// Input: Valid renderer, empty string Html and BaseUrl properties.
        /// Expected: LoadHtml is called once with empty string values.
        /// </summary>
        [Fact]
        public void Load_ValidRendererWithEmptyHtmlAndBaseUrl_CallsLoadHtmlWithEmptyStrings()
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource
            {
                Html = string.Empty,
                BaseUrl = string.Empty
            };
            var mockRenderer = Substitute.For<IWebViewDelegate>();

            // Act
            htmlWebViewSource.Load(mockRenderer);

            // Assert
            mockRenderer.Received(1).LoadHtml(string.Empty, string.Empty);
        }

        /// <summary>
        /// Tests that Load method calls LoadHtml with whitespace strings when Html and BaseUrl contain only whitespace.
        /// Input: Valid renderer, whitespace-only Html and BaseUrl properties.
        /// Expected: LoadHtml is called once with whitespace string values.
        /// </summary>
        [Fact]
        public void Load_ValidRendererWithWhitespaceHtmlAndBaseUrl_CallsLoadHtmlWithWhitespaceStrings()
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource
            {
                Html = "   \t\n  ",
                BaseUrl = "  \t  "
            };
            var mockRenderer = Substitute.For<IWebViewDelegate>();

            // Act
            htmlWebViewSource.Load(mockRenderer);

            // Assert
            mockRenderer.Received(1).LoadHtml("   \t\n  ", "  \t  ");
        }

        /// <summary>
        /// Tests that Load method throws NullReferenceException when renderer is null.
        /// Input: Null renderer parameter.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void Load_NullRenderer_ThrowsNullReferenceException()
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource
            {
                Html = "<html><body>Test</body></html>",
                BaseUrl = "https://example.com"
            };

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => htmlWebViewSource.Load(null));
        }

        /// <summary>
        /// Tests that Load method calls LoadHtml with mixed null and valid values.
        /// Input: Valid renderer, null Html and valid BaseUrl.
        /// Expected: LoadHtml is called once with null Html and valid BaseUrl.
        /// </summary>
        [Fact]
        public void Load_ValidRendererWithNullHtmlAndValidBaseUrl_CallsLoadHtmlWithMixedValues()
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource
            {
                Html = null,
                BaseUrl = "https://example.com"
            };
            var mockRenderer = Substitute.For<IWebViewDelegate>();

            // Act
            htmlWebViewSource.Load(mockRenderer);

            // Assert
            mockRenderer.Received(1).LoadHtml(null, "https://example.com");
        }

        /// <summary>
        /// Tests that Load method calls LoadHtml with mixed valid and null values.
        /// Input: Valid renderer, valid Html and null BaseUrl.
        /// Expected: LoadHtml is called once with valid Html and null BaseUrl.
        /// </summary>
        [Fact]
        public void Load_ValidRendererWithValidHtmlAndNullBaseUrl_CallsLoadHtmlWithMixedValues()
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource
            {
                Html = "<html><body>Content</body></html>",
                BaseUrl = null
            };
            var mockRenderer = Substitute.For<IWebViewDelegate>();

            // Act
            htmlWebViewSource.Load(mockRenderer);

            // Assert
            mockRenderer.Received(1).LoadHtml("<html><body>Content</body></html>", null);
        }

        /// <summary>
        /// Tests that Load method handles very long HTML content correctly.
        /// Input: Valid renderer with very long Html content.
        /// Expected: LoadHtml is called once with the long Html content.
        /// </summary>
        [Fact]
        public void Load_ValidRendererWithVeryLongHtml_CallsLoadHtmlWithLongContent()
        {
            // Arrange
            var longHtml = new string('a', 10000) + "<html><body>" + new string('b', 10000) + "</body></html>";
            var htmlWebViewSource = new HtmlWebViewSource
            {
                Html = longHtml,
                BaseUrl = "https://example.com"
            };
            var mockRenderer = Substitute.For<IWebViewDelegate>();

            // Act
            htmlWebViewSource.Load(mockRenderer);

            // Assert
            mockRenderer.Received(1).LoadHtml(longHtml, "https://example.com");
        }

        /// <summary>
        /// Tests that Load method handles special characters in Html and BaseUrl correctly.
        /// Input: Valid renderer with Html and BaseUrl containing special characters.
        /// Expected: LoadHtml is called once with the special character values.
        /// </summary>
        [Fact]
        public void Load_ValidRendererWithSpecialCharacters_CallsLoadHtmlWithSpecialCharacters()
        {
            // Arrange
            var htmlWithSpecialChars = "<html><body>Test & \"quotes\" 'apostrophes' <tag>content</tag> 中文 🚀</body></html>";
            var baseUrlWithSpecialChars = "https://example.com/path?param=value&other=test#anchor";
            var htmlWebViewSource = new HtmlWebViewSource
            {
                Html = htmlWithSpecialChars,
                BaseUrl = baseUrlWithSpecialChars
            };
            var mockRenderer = Substitute.For<IWebViewDelegate>();

            // Act
            htmlWebViewSource.Load(mockRenderer);

            // Assert
            mockRenderer.Received(1).LoadHtml(htmlWithSpecialChars, baseUrlWithSpecialChars);
        }

        /// <summary>
        /// Tests that BaseUrl property getter returns the value that was set via the setter.
        /// Verifies basic get/set functionality works correctly for normal string values.
        /// </summary>
        [Theory]
        [InlineData("https://example.com")]
        [InlineData("http://localhost:8080")]
        [InlineData("file:///path/to/local/file")]
        [InlineData("https://subdomain.example.com/path?query=value")]
        public void BaseUrl_SetValidUrl_ReturnsSetValue(string expectedUrl)
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource();

            // Act
            htmlWebViewSource.BaseUrl = expectedUrl;
            var actualUrl = htmlWebViewSource.BaseUrl;

            // Assert
            Assert.Equal(expectedUrl, actualUrl);
        }

        /// <summary>
        /// Tests that BaseUrl property can be set to null and returns null when retrieved.
        /// Verifies null handling works correctly since nullable reference types are disabled.
        /// </summary>
        [Fact]
        public void BaseUrl_SetNull_ReturnsNull()
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource();

            // Act
            htmlWebViewSource.BaseUrl = null;
            var actualUrl = htmlWebViewSource.BaseUrl;

            // Assert
            Assert.Null(actualUrl);
        }

        /// <summary>
        /// Tests that BaseUrl property can be set to empty string and returns empty string when retrieved.
        /// Verifies empty string handling works correctly.
        /// </summary>
        [Fact]
        public void BaseUrl_SetEmptyString_ReturnsEmptyString()
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource();

            // Act
            htmlWebViewSource.BaseUrl = string.Empty;
            var actualUrl = htmlWebViewSource.BaseUrl;

            // Assert
            Assert.Equal(string.Empty, actualUrl);
        }

        /// <summary>
        /// Tests that BaseUrl property handles whitespace-only strings correctly.
        /// Verifies various whitespace scenarios are preserved as-is.
        /// </summary>
        [Theory]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("  \t  \n  ")]
        public void BaseUrl_SetWhitespaceString_ReturnsWhitespaceString(string whitespaceUrl)
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource();

            // Act
            htmlWebViewSource.BaseUrl = whitespaceUrl;
            var actualUrl = htmlWebViewSource.BaseUrl;

            // Assert
            Assert.Equal(whitespaceUrl, actualUrl);
        }

        /// <summary>
        /// Tests that BaseUrl property handles strings with special characters correctly.
        /// Verifies that special characters in URLs are preserved without modification.
        /// </summary>
        [Theory]
        [InlineData("https://example.com/path with spaces")]
        [InlineData("https://example.com/path?param=value&other=test")]
        [InlineData("https://example.com/path#fragment")]
        [InlineData("https://user:pass@example.com")]
        [InlineData("ftp://example.com/file.txt")]
        [InlineData("data:text/html,<html><body>Hello</body></html>")]
        public void BaseUrl_SetUrlWithSpecialCharacters_ReturnsUrlWithSpecialCharacters(string specialUrl)
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource();

            // Act
            htmlWebViewSource.BaseUrl = specialUrl;
            var actualUrl = htmlWebViewSource.BaseUrl;

            // Assert
            Assert.Equal(specialUrl, actualUrl);
        }

        /// <summary>
        /// Tests that BaseUrl property handles very long strings correctly.
        /// Verifies that large URL strings are stored and retrieved without truncation.
        /// </summary>
        [Fact]
        public void BaseUrl_SetVeryLongString_ReturnsVeryLongString()
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource();
            var longUrl = "https://example.com/" + new string('a', 10000) + "/path";

            // Act
            htmlWebViewSource.BaseUrl = longUrl;
            var actualUrl = htmlWebViewSource.BaseUrl;

            // Assert
            Assert.Equal(longUrl, actualUrl);
        }

        /// <summary>
        /// Tests that BaseUrl property returns the default value when not explicitly set.
        /// Verifies the initial state behavior of the property.
        /// </summary>
        [Fact]
        public void BaseUrl_NotSet_ReturnsDefaultValue()
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource();

            // Act
            var actualUrl = htmlWebViewSource.BaseUrl;

            // Assert
            Assert.Null(actualUrl); // Based on BindableProperty.Create with default(string)
        }

        /// <summary>
        /// Tests that BaseUrl property can be set multiple times and always returns the most recent value.
        /// Verifies that the property correctly overwrites previous values.
        /// </summary>
        [Fact]
        public void BaseUrl_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var htmlWebViewSource = new HtmlWebViewSource();

            // Act & Assert
            htmlWebViewSource.BaseUrl = "https://first.com";
            Assert.Equal("https://first.com", htmlWebViewSource.BaseUrl);

            htmlWebViewSource.BaseUrl = "https://second.com";
            Assert.Equal("https://second.com", htmlWebViewSource.BaseUrl);

            htmlWebViewSource.BaseUrl = null;
            Assert.Null(htmlWebViewSource.BaseUrl);

            htmlWebViewSource.BaseUrl = "https://third.com";
            Assert.Equal("https://third.com", htmlWebViewSource.BaseUrl);
        }
    }
}