#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class WebViewHelperTests
    {
        [Fact]
        public void EscapeJsString_NullInput_ReturnsNull()
        {
            const string input = null;
            var result = WebViewHelper.EscapeJsString(input);
            Assert.Null(result);
        }

        [Fact]
        public void EscapeJsString_NoSingleQuote_ReturnsSameString()
        {
            const string input = """console.log("Hello, world!");""";
            var result = WebViewHelper.EscapeJsString(input);
            Assert.Equal(input, result);
        }

        [Fact]
        public void EscapeJsString_UnescapedQuote_EscapesCorrectly()
        {
            // Each unescaped single quote should be preceded by one backslash.
            const string input = """console.log('Hello, world!');""";
            // Expected: each occurrence of "'" becomes "\'"
            const string expected = """console.log(\'Hello, world!\');""";
            var result = WebViewHelper.EscapeJsString(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void EscapeJsString_AlreadyEscapedQuote_EscapesFurther()
        {
            const string input = """var str = 'Don\'t do that';""";
            const string expected = """var str = \'Don\\\'t do that\';""";
            var result = WebViewHelper.EscapeJsString(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void EscapeJsString_MultipleLinesAndMixedQuotes()
        {
            const string input = """
			function test() {
				console.log('Test "string" with a single quote');
				var example = 'It\\'s tricky!';
			}
			""";
            const string expected = """
			function test() {
				console.log(\'Test "string" with a single quote\');
				var example = \'It\\\\\'s tricky!\';
			}
			""";
            var result = WebViewHelper.EscapeJsString(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void EscapeJsString_MultipleBackslashesBeforeQuote()
        {
            const string input = @"var tricky = 'Backslash: \\\' tricky!';";
            const string expected = @"var tricky = \'Backslash: \\\\\\\' tricky!\';";
            var result = WebViewHelper.EscapeJsString(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void EscapeJsString_QuoteAtBeginning()
        {
            const string input = @"'Start with quote";
            const string expected = @"\'Start with quote";
            var result = WebViewHelper.EscapeJsString(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void EscapeJsString_QuoteAtEnd()
        {
            const string input = @"Ends with a quote'";
            const string expected = @"Ends with a quote\'";
            var result = WebViewHelper.EscapeJsString(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void EscapeJsString_OnlyQuote()
        {
            const string input = @"'";
            const string expected = @"\'";
            var result = WebViewHelper.EscapeJsString(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void EscapeJsString_RepeatedEscapedQuotes()
        {
            const string input = @"'Quote' and again \'Quote\'";
            const string expected = @"\'Quote\' and again \\\'Quote\\\'";
            var result = WebViewHelper.EscapeJsString(input);
            Assert.Equal(expected, result);
        }
    }

    /// <summary>
    /// Unit tests for the AndroidSpecific WebView EnableZoomControls functionality.
    /// </summary>
    public class AndroidSpecificWebViewEnableZoomControlsTests
    {
        /// <summary>
        /// Tests that EnableZoomControls calls SetValue with true when value parameter is true.
        /// </summary>
        [Fact]
        public void EnableZoomControls_WithTrueValue_CallsSetValueWithTrue()
        {
            // Arrange
            var mockElement = Substitute.For<WebView>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, WebView>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            mockConfig.EnableZoomControls(true);

            // Assert
            mockElement.Received(1).SetValue(AndroidSpecific.WebView.EnableZoomControlsProperty, true);
        }

        /// <summary>
        /// Tests that EnableZoomControls calls SetValue with false when value parameter is false.
        /// </summary>
        [Fact]
        public void EnableZoomControls_WithFalseValue_CallsSetValueWithFalse()
        {
            // Arrange
            var mockElement = Substitute.For<WebView>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, WebView>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            mockConfig.EnableZoomControls(false);

            // Assert
            mockElement.Received(1).SetValue(AndroidSpecific.WebView.EnableZoomControlsProperty, false);
        }

        /// <summary>
        /// Tests that EnableZoomControls throws ArgumentNullException when config is null and called as static method.
        /// </summary>
        [Fact]
        public void EnableZoomControls_WithNullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Android, WebView> nullConfig = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => AndroidSpecific.WebView.EnableZoomControls(nullConfig, true));
        }

        /// <summary>
        /// Tests that EnableZoomControls works correctly when Element property returns null.
        /// This tests the behavior when the configuration is valid but Element is null.
        /// </summary>
        [Fact]
        public void EnableZoomControls_WithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, WebView>>();
            mockConfig.Element.Returns((WebView)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => mockConfig.EnableZoomControls(true));
        }
    }


    public class AndroidSpecificWebViewTests
    {
        /// <summary>
        /// Tests that JavaScriptEnabled extension method correctly calls SetJavaScriptEnabled with the provided boolean value.
        /// Verifies that the method passes both true and false values correctly to the underlying SetValue method.
        /// </summary>
        /// <param name="value">The boolean value indicating whether JavaScript should be enabled.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void JavaScriptEnabled_ValidConfigAndValue_CallsSetJavaScriptEnabledWithCorrectParameters(bool value)
        {
            // Arrange
            var mockWebView = Substitute.For<WebView>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, WebView>>();
            mockConfig.Element.Returns(mockWebView);

            // Act
            mockConfig.JavaScriptEnabled(value);

            // Assert
            mockWebView.Received(1).SetValue(AndroidSpecific.WebView.JavaScriptEnabledProperty, value);
        }

        /// <summary>
        /// Tests that JavaScriptEnabled extension method throws ArgumentNullException when called with null config.
        /// Verifies proper null parameter validation for the config parameter.
        /// </summary>
        [Fact]
        public void JavaScriptEnabled_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Android, WebView> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.JavaScriptEnabled(true));
        }

        /// <summary>
        /// Tests that JavaScriptEnabled extension method throws NullReferenceException when config.Element is null.
        /// Verifies behavior when the Element property returns null, which should fail when trying to call SetValue.
        /// </summary>
        [Fact]
        public void JavaScriptEnabled_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, WebView>>();
            mockConfig.Element.Returns((WebView)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => mockConfig.JavaScriptEnabled(true));
        }
    }


    /// <summary>
    /// Unit tests for the DisplayZoomControls extension method in AndroidSpecific.WebView.
    /// </summary>
    public class WebViewDisplayZoomControlsTests
    {
        /// <summary>
        /// Tests that DisplayZoomControls correctly calls SetValue on the element when value is true.
        /// </summary>
        [Fact]
        public void DisplayZoomControls_ValidConfigWithTrue_CallsSetValueWithTrue()
        {
            // Arrange
            var webView = Substitute.For<WebView>();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, WebView>>();
            config.Element.Returns(webView);
            bool value = true;

            // Act
            config.DisplayZoomControls(value);

            // Assert
            webView.Received(1).SetValue(AndroidSpecific.WebView.DisplayZoomControlsProperty, value);
        }

        /// <summary>
        /// Tests that DisplayZoomControls correctly calls SetValue on the element when value is false.
        /// </summary>
        [Fact]
        public void DisplayZoomControls_ValidConfigWithFalse_CallsSetValueWithFalse()
        {
            // Arrange
            var webView = Substitute.For<WebView>();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, WebView>>();
            config.Element.Returns(webView);
            bool value = false;

            // Act
            config.DisplayZoomControls(value);

            // Assert
            webView.Received(1).SetValue(AndroidSpecific.WebView.DisplayZoomControlsProperty, value);
        }

        /// <summary>
        /// Tests that DisplayZoomControls throws NullReferenceException when config parameter is null.
        /// </summary>
        [Fact]
        public void DisplayZoomControls_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Android, WebView> config = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => config.DisplayZoomControls(value));
        }

        /// <summary>
        /// Tests that DisplayZoomControls works correctly with parameterized boolean values.
        /// </summary>
        /// <param name="value">The boolean value to test with DisplayZoomControls method.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DisplayZoomControls_ParameterizedBooleanValues_CallsSetValueCorrectly(bool value)
        {
            // Arrange
            var webView = Substitute.For<WebView>();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, WebView>>();
            config.Element.Returns(webView);

            // Act
            config.DisplayZoomControls(value);

            // Assert
            webView.Received(1).SetValue(AndroidSpecific.WebView.DisplayZoomControlsProperty, value);
        }
    }
}
