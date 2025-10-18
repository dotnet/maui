#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class WebNavigationEventArgsTests
    {
        /// <summary>
        /// Test helper class to expose the protected constructor for testing.
        /// </summary>
        private class TestableWebNavigationEventArgs : WebNavigationEventArgs
        {
            public TestableWebNavigationEventArgs(WebNavigationEvent navigationEvent, WebViewSource source, string url)
                : base(navigationEvent, source, url)
            {
            }
        }

        /// <summary>
        /// Tests that the constructor properly initializes all properties with valid enum values.
        /// Uses each defined WebNavigationEvent enum value with a mocked source and valid URL.
        /// </summary>
        [Theory]
        [InlineData(WebNavigationEvent.Back, "https://example.com")]
        [InlineData(WebNavigationEvent.Forward, "https://test.com")]
        [InlineData(WebNavigationEvent.NewPage, "https://newpage.com")]
        [InlineData(WebNavigationEvent.Refresh, "https://refresh.com")]
        public void Constructor_WithValidEnumAndUrl_SetsPropertiesCorrectly(WebNavigationEvent navigationEvent, string url)
        {
            // Arrange
            var mockSource = Substitute.For<WebViewSource>();

            // Act
            var eventArgs = new TestableWebNavigationEventArgs(navigationEvent, mockSource, url);

            // Assert
            Assert.Equal(navigationEvent, eventArgs.NavigationEvent);
            Assert.Equal(mockSource, eventArgs.Source);
            Assert.Equal(url, eventArgs.Url);
        }

        /// <summary>
        /// Tests that the constructor handles null WebViewSource parameter correctly.
        /// Verifies that null source is properly assigned to the Source property.
        /// </summary>
        [Fact]
        public void Constructor_WithNullSource_SetsSourceToNull()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.NewPage;
            var url = "https://example.com";

            // Act
            var eventArgs = new TestableWebNavigationEventArgs(navigationEvent, null, url);

            // Assert
            Assert.Equal(navigationEvent, eventArgs.NavigationEvent);
            Assert.Null(eventArgs.Source);
            Assert.Equal(url, eventArgs.Url);
        }

        /// <summary>
        /// Tests that the constructor handles various string edge cases for the URL parameter.
        /// Verifies null, empty, whitespace-only, and very long URL strings are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n\r")]
        public void Constructor_WithStringEdgeCases_SetsUrlCorrectly(string url)
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.Back;
            var mockSource = Substitute.For<WebViewSource>();

            // Act
            var eventArgs = new TestableWebNavigationEventArgs(navigationEvent, mockSource, url);

            // Assert
            Assert.Equal(navigationEvent, eventArgs.NavigationEvent);
            Assert.Equal(mockSource, eventArgs.Source);
            Assert.Equal(url, eventArgs.Url);
        }

        /// <summary>
        /// Tests that the constructor handles a very long URL string correctly.
        /// Ensures memory efficiency and proper string handling for edge cases.
        /// </summary>
        [Fact]
        public void Constructor_WithVeryLongUrl_SetsUrlCorrectly()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.Forward;
            var mockSource = Substitute.For<WebViewSource>();
            var longUrl = new string('a', 10000); // Very long URL

            // Act
            var eventArgs = new TestableWebNavigationEventArgs(navigationEvent, mockSource, longUrl);

            // Assert
            Assert.Equal(navigationEvent, eventArgs.NavigationEvent);
            Assert.Equal(mockSource, eventArgs.Source);
            Assert.Equal(longUrl, eventArgs.Url);
        }

        /// <summary>
        /// Tests that the constructor handles special characters and control characters in URL.
        /// Verifies proper handling of URLs with various special and control characters.
        /// </summary>
        [Theory]
        [InlineData("https://example.com/path?query=value&other=test")]
        [InlineData("file:///C:/path/to/file.html")]
        [InlineData("data:text/html,<html><body>Hello</body></html>")]
        [InlineData("javascript:alert('test')")]
        [InlineData("https://тест.рф")]
        [InlineData("https://example.com/\u0001\u0002\u0003")]
        public void Constructor_WithSpecialCharacterUrls_SetsUrlCorrectly(string url)
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.Refresh;
            var mockSource = Substitute.For<WebViewSource>();

            // Act
            var eventArgs = new TestableWebNavigationEventArgs(navigationEvent, mockSource, url);

            // Assert
            Assert.Equal(navigationEvent, eventArgs.NavigationEvent);
            Assert.Equal(mockSource, eventArgs.Source);
            Assert.Equal(url, eventArgs.Url);
        }

        /// <summary>
        /// Tests that the constructor handles invalid enum values correctly.
        /// Verifies behavior when WebNavigationEvent is cast from values outside the defined range.
        /// </summary>
        [Theory]
        [InlineData((WebNavigationEvent)0)]
        [InlineData((WebNavigationEvent)5)]
        [InlineData((WebNavigationEvent)int.MinValue)]
        [InlineData((WebNavigationEvent)int.MaxValue)]
        public void Constructor_WithInvalidEnumValues_SetsNavigationEventCorrectly(WebNavigationEvent invalidNavigationEvent)
        {
            // Arrange
            var mockSource = Substitute.For<WebViewSource>();
            var url = "https://example.com";

            // Act
            var eventArgs = new TestableWebNavigationEventArgs(invalidNavigationEvent, mockSource, url);

            // Assert
            Assert.Equal(invalidNavigationEvent, eventArgs.NavigationEvent);
            Assert.Equal(mockSource, eventArgs.Source);
            Assert.Equal(url, eventArgs.Url);
        }

        /// <summary>
        /// Tests that the constructor properly handles all parameters being edge cases simultaneously.
        /// Verifies correct assignment when all parameters are at their boundary values.
        /// </summary>
        [Fact]
        public void Constructor_WithAllEdgeCaseParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            var invalidNavigationEvent = (WebNavigationEvent)(-1);
            WebViewSource nullSource = null;
            string nullUrl = null;

            // Act
            var eventArgs = new TestableWebNavigationEventArgs(invalidNavigationEvent, nullSource, nullUrl);

            // Assert
            Assert.Equal(invalidNavigationEvent, eventArgs.NavigationEvent);
            Assert.Null(eventArgs.Source);
            Assert.Null(eventArgs.Url);
        }
    }
}
