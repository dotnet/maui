#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class WebNavigatingEventArgsTests
    {
        /// <summary>
        /// Tests that the constructor successfully creates an instance with valid enum value Back.
        /// Input conditions: WebNavigationEvent.Back, mocked WebViewSource, valid URL string.
        /// Expected result: Constructor completes successfully and Cancel property is initialized to false.
        /// </summary>
        [Fact]
        public void Constructor_WithBackNavigationEventAndValidParameters_CreatesInstance()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.Back;
            var source = Substitute.For<WebViewSource>();
            var url = "https://example.com";

            // Act
            var result = new WebNavigatingEventArgs(navigationEvent, source, url);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Cancel);
        }

        /// <summary>
        /// Tests that the constructor successfully creates an instance with valid enum value Forward.
        /// Input conditions: WebNavigationEvent.Forward, mocked WebViewSource, valid URL string.
        /// Expected result: Constructor completes successfully and Cancel property is initialized to false.
        /// </summary>
        [Fact]
        public void Constructor_WithForwardNavigationEventAndValidParameters_CreatesInstance()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.Forward;
            var source = Substitute.For<WebViewSource>();
            var url = "https://example.com";

            // Act
            var result = new WebNavigatingEventArgs(navigationEvent, source, url);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Cancel);
        }

        /// <summary>
        /// Tests that the constructor successfully creates an instance with valid enum value NewPage.
        /// Input conditions: WebNavigationEvent.NewPage, mocked WebViewSource, valid URL string.
        /// Expected result: Constructor completes successfully and Cancel property is initialized to false.
        /// </summary>
        [Fact]
        public void Constructor_WithNewPageNavigationEventAndValidParameters_CreatesInstance()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.NewPage;
            var source = Substitute.For<WebViewSource>();
            var url = "https://example.com";

            // Act
            var result = new WebNavigatingEventArgs(navigationEvent, source, url);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Cancel);
        }

        /// <summary>
        /// Tests that the constructor successfully creates an instance with valid enum value Refresh.
        /// Input conditions: WebNavigationEvent.Refresh, mocked WebViewSource, valid URL string.
        /// Expected result: Constructor completes successfully and Cancel property is initialized to false.
        /// </summary>
        [Fact]
        public void Constructor_WithRefreshNavigationEventAndValidParameters_CreatesInstance()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.Refresh;
            var source = Substitute.For<WebViewSource>();
            var url = "https://example.com";

            // Act
            var result = new WebNavigatingEventArgs(navigationEvent, source, url);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Cancel);
        }

        /// <summary>
        /// Tests that the constructor successfully creates an instance with invalid enum value cast from integer.
        /// Input conditions: Invalid WebNavigationEvent value (999), mocked WebViewSource, valid URL string.
        /// Expected result: Constructor completes successfully despite invalid enum value.
        /// </summary>
        [Fact]
        public void Constructor_WithInvalidNavigationEventValue_CreatesInstance()
        {
            // Arrange
            var navigationEvent = (WebNavigationEvent)999;
            var source = Substitute.For<WebViewSource>();
            var url = "https://example.com";

            // Act
            var result = new WebNavigatingEventArgs(navigationEvent, source, url);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Cancel);
        }

        /// <summary>
        /// Tests that the constructor successfully creates an instance with null WebViewSource.
        /// Input conditions: Valid WebNavigationEvent, null WebViewSource, valid URL string.
        /// Expected result: Constructor completes successfully with null source.
        /// </summary>
        [Fact]
        public void Constructor_WithNullSource_CreatesInstance()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.NewPage;
            WebViewSource source = null;
            var url = "https://example.com";

            // Act
            var result = new WebNavigatingEventArgs(navigationEvent, source, url);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Cancel);
        }

        /// <summary>
        /// Tests that the constructor successfully creates an instance with null URL.
        /// Input conditions: Valid WebNavigationEvent, mocked WebViewSource, null URL string.
        /// Expected result: Constructor completes successfully with null URL.
        /// </summary>
        [Fact]
        public void Constructor_WithNullUrl_CreatesInstance()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.NewPage;
            var source = Substitute.For<WebViewSource>();
            string url = null;

            // Act
            var result = new WebNavigatingEventArgs(navigationEvent, source, url);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Cancel);
        }

        /// <summary>
        /// Tests that the constructor successfully creates an instance with empty URL string.
        /// Input conditions: Valid WebNavigationEvent, mocked WebViewSource, empty URL string.
        /// Expected result: Constructor completes successfully with empty URL.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyUrl_CreatesInstance()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.NewPage;
            var source = Substitute.For<WebViewSource>();
            var url = string.Empty;

            // Act
            var result = new WebNavigatingEventArgs(navigationEvent, source, url);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Cancel);
        }

        /// <summary>
        /// Tests that the constructor successfully creates an instance with whitespace-only URL.
        /// Input conditions: Valid WebNavigationEvent, mocked WebViewSource, whitespace-only URL string.
        /// Expected result: Constructor completes successfully with whitespace URL.
        /// </summary>
        [Fact]
        public void Constructor_WithWhitespaceUrl_CreatesInstance()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.NewPage;
            var source = Substitute.For<WebViewSource>();
            var url = "   \t\n\r   ";

            // Act
            var result = new WebNavigatingEventArgs(navigationEvent, source, url);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Cancel);
        }

        /// <summary>
        /// Tests that the constructor successfully creates an instance with very long URL string.
        /// Input conditions: Valid WebNavigationEvent, mocked WebViewSource, very long URL string.
        /// Expected result: Constructor completes successfully with long URL.
        /// </summary>
        [Fact]
        public void Constructor_WithVeryLongUrl_CreatesInstance()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.NewPage;
            var source = Substitute.For<WebViewSource>();
            var url = "https://example.com/" + new string('a', 10000);

            // Act
            var result = new WebNavigatingEventArgs(navigationEvent, source, url);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Cancel);
        }

        /// <summary>
        /// Tests that the constructor successfully creates an instance with URL containing special characters.
        /// Input conditions: Valid WebNavigationEvent, mocked WebViewSource, URL with special characters.
        /// Expected result: Constructor completes successfully with special character URL.
        /// </summary>
        [Fact]
        public void Constructor_WithSpecialCharactersInUrl_CreatesInstance()
        {
            // Arrange
            var navigationEvent = WebNavigationEvent.NewPage;
            var source = Substitute.For<WebViewSource>();
            var url = "https://example.com/path?param=value&special=!@#$%^&*(){}[]|\\:;\"'<>?,./~`";

            // Act
            var result = new WebNavigatingEventArgs(navigationEvent, source, url);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Cancel);
        }

        /// <summary>
        /// Tests that the constructor successfully creates an instance with all null parameters.
        /// Input conditions: Invalid WebNavigationEvent (0), null WebViewSource, null URL.
        /// Expected result: Constructor completes successfully with all problematic parameters.
        /// </summary>
        [Fact]
        public void Constructor_WithAllNullOrInvalidParameters_CreatesInstance()
        {
            // Arrange
            var navigationEvent = (WebNavigationEvent)0;
            WebViewSource source = null;
            string url = null;

            // Act
            var result = new WebNavigatingEventArgs(navigationEvent, source, url);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Cancel);
        }
    }
}
