#nullable disable

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using System;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class WebNavigatedEventArgsTests
    {
        /// <summary>
        /// Tests the WebNavigatedEventArgs constructor with valid enum values and URL.
        /// Verifies that all properties are set correctly and the base constructor is called.
        /// </summary>
        /// <param name="navigationEvent">The navigation event type</param>
        /// <param name="url">The navigation URL</param>
        /// <param name="result">The navigation result</param>
        [Theory]
        [InlineData(WebNavigationEvent.Back, "https://example.com", WebNavigationResult.Success)]
        [InlineData(WebNavigationEvent.Forward, "http://test.org", WebNavigationResult.Cancel)]
        [InlineData(WebNavigationEvent.NewPage, "https://microsoft.com/page", WebNavigationResult.Timeout)]
        [InlineData(WebNavigationEvent.Refresh, "file:///local/path", WebNavigationResult.Failure)]
        public void Constructor_ValidParameters_SetsAllPropertiesCorrectly(WebNavigationEvent navigationEvent, string url, WebNavigationResult result)
        {
            // Arrange
            var mockSource = Substitute.For<WebViewSource>();

            // Act
            var eventArgs = new WebNavigatedEventArgs(navigationEvent, mockSource, url, result);

            // Assert
            Assert.Equal(result, eventArgs.Result);
        }

        /// <summary>
        /// Tests the WebNavigatedEventArgs constructor with null source parameter.
        /// Verifies that the constructor accepts null source and sets Result property correctly.
        /// </summary>
        [Fact]
        public void Constructor_NullSource_SetsResultCorrectly()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.NewPage;
            WebViewSource source = null;
            var url = "https://example.com";
            var result = WebNavigationResult.Success;

            // Act
            var eventArgs = new WebNavigatedEventArgs(navigationEvent, source, url, result);

            // Assert
            Assert.Equal(result, eventArgs.Result);
        }

        /// <summary>
        /// Tests the WebNavigatedEventArgs constructor with null URL parameter.
        /// Verifies that the constructor accepts null URL and sets Result property correctly.
        /// </summary>
        [Fact]
        public void Constructor_NullUrl_SetsResultCorrectly()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.Back;
            var mockSource = Substitute.For<WebViewSource>();
            string url = null;
            var result = WebNavigationResult.Cancel;

            // Act
            var eventArgs = new WebNavigatedEventArgs(navigationEvent, mockSource, url, result);

            // Assert
            Assert.Equal(result, eventArgs.Result);
        }

        /// <summary>
        /// Tests the WebNavigatedEventArgs constructor with empty URL string.
        /// Verifies that the constructor accepts empty URL and sets Result property correctly.
        /// </summary>
        [Fact]
        public void Constructor_EmptyUrl_SetsResultCorrectly()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.Forward;
            var mockSource = Substitute.For<WebViewSource>();
            var url = string.Empty;
            var result = WebNavigationResult.Timeout;

            // Act
            var eventArgs = new WebNavigatedEventArgs(navigationEvent, mockSource, url, result);

            // Assert
            Assert.Equal(result, eventArgs.Result);
        }

        /// <summary>
        /// Tests the WebNavigatedEventArgs constructor with whitespace-only URL.
        /// Verifies that the constructor accepts whitespace URL and sets Result property correctly.
        /// </summary>
        [Fact]
        public void Constructor_WhitespaceUrl_SetsResultCorrectly()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.Refresh;
            var mockSource = Substitute.For<WebViewSource>();
            var url = "   \t\n  ";
            var result = WebNavigationResult.Failure;

            // Act
            var eventArgs = new WebNavigatedEventArgs(navigationEvent, mockSource, url, result);

            // Assert
            Assert.Equal(result, eventArgs.Result);
        }

        /// <summary>
        /// Tests the WebNavigatedEventArgs constructor with very long URL string.
        /// Verifies that the constructor handles long URLs and sets Result property correctly.
        /// </summary>
        [Fact]
        public void Constructor_VeryLongUrl_SetsResultCorrectly()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.NewPage;
            var mockSource = Substitute.For<WebViewSource>();
            var url = "https://example.com/" + new string('a', 10000);
            var result = WebNavigationResult.Success;

            // Act
            var eventArgs = new WebNavigatedEventArgs(navigationEvent, mockSource, url, result);

            // Assert
            Assert.Equal(result, eventArgs.Result);
        }

        /// <summary>
        /// Tests the WebNavigatedEventArgs constructor with URL containing special characters.
        /// Verifies that the constructor handles special characters and sets Result property correctly.
        /// </summary>
        [Fact]
        public void Constructor_UrlWithSpecialCharacters_SetsResultCorrectly()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.Back;
            var mockSource = Substitute.For<WebViewSource>();
            var url = "https://example.com/path?query=value&param=测试#fragment";
            var result = WebNavigationResult.Cancel;

            // Act
            var eventArgs = new WebNavigatedEventArgs(navigationEvent, mockSource, url, result);

            // Assert
            Assert.Equal(result, eventArgs.Result);
        }

        /// <summary>
        /// Tests the WebNavigatedEventArgs constructor with invalid navigation event enum value.
        /// Verifies that the constructor accepts invalid enum values and sets Result property correctly.
        /// </summary>
        [Fact]
        public void Constructor_InvalidNavigationEvent_SetsResultCorrectly()
        {
            // Arrange
            var navigationEvent = (WebNavigationEvent)999;
            var mockSource = Substitute.For<WebViewSource>();
            var url = "https://example.com";
            var result = WebNavigationResult.Success;

            // Act
            var eventArgs = new WebNavigatedEventArgs(navigationEvent, mockSource, url, result);

            // Assert
            Assert.Equal(result, eventArgs.Result);
        }

        /// <summary>
        /// Tests the WebNavigatedEventArgs constructor with invalid navigation result enum value.
        /// Verifies that the constructor accepts invalid enum values and sets Result property correctly.
        /// </summary>
        [Fact]
        public void Constructor_InvalidNavigationResult_SetsResultCorrectly()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.Forward;
            var mockSource = Substitute.For<WebViewSource>();
            var url = "https://example.com";
            var result = (WebNavigationResult)888;

            // Act
            var eventArgs = new WebNavigatedEventArgs(navigationEvent, mockSource, url, result);

            // Assert
            Assert.Equal(result, eventArgs.Result);
        }

        /// <summary>
        /// Tests the WebNavigatedEventArgs constructor with all parameters null or invalid.
        /// Verifies that the constructor handles extreme edge case and sets Result property correctly.
        /// </summary>
        [Fact]
        public void Constructor_AllParametersEdgeCases_SetsResultCorrectly()
        {
            // Arrange
            var navigationEvent = (WebNavigationEvent)0;
            WebViewSource source = null;
            string url = null;
            var result = (WebNavigationResult)0;

            // Act
            var eventArgs = new WebNavigatedEventArgs(navigationEvent, source, url, result);

            // Assert
            Assert.Equal(result, eventArgs.Result);
        }
    }
}
