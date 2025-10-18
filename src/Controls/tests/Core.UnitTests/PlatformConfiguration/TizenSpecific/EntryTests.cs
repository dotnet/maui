#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.TizenSpecific
{
    /// <summary>
    /// Unit tests for the Entry class in TizenSpecific platform configuration.
    /// </summary>
    public class EntryTests
    {
        /// <summary>
        /// Tests that SetFontWeight calls SetValue with the correct parameters when given valid inputs.
        /// Input conditions: Valid BindableObject and valid weight string.
        /// Expected result: SetValue is called once with FontWeightProperty and the weight parameter.
        /// </summary>
        [Fact]
        public void SetFontWeight_ValidElementAndWeight_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var weight = "Bold";

            // Act
            Entry.SetFontWeight(element, weight);

            // Assert
            element.Received(1).SetValue(Entry.FontWeightProperty, weight);
        }

        /// <summary>
        /// Tests that SetFontWeight handles null weight parameter correctly.
        /// Input conditions: Valid BindableObject and null weight.
        /// Expected result: SetValue is called once with FontWeightProperty and null.
        /// </summary>
        [Fact]
        public void SetFontWeight_ValidElementAndNullWeight_CallsSetValueWithNull()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            string weight = null;

            // Act
            Entry.SetFontWeight(element, weight);

            // Assert
            element.Received(1).SetValue(Entry.FontWeightProperty, null);
        }

        /// <summary>
        /// Tests that SetFontWeight handles empty weight parameter correctly.
        /// Input conditions: Valid BindableObject and empty weight string.
        /// Expected result: SetValue is called once with FontWeightProperty and empty string.
        /// </summary>
        [Fact]
        public void SetFontWeight_ValidElementAndEmptyWeight_CallsSetValueWithEmptyString()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var weight = "";

            // Act
            Entry.SetFontWeight(element, weight);

            // Assert
            element.Received(1).SetValue(Entry.FontWeightProperty, "");
        }

        /// <summary>
        /// Tests that SetFontWeight handles whitespace-only weight parameter correctly.
        /// Input conditions: Valid BindableObject and whitespace-only weight string.
        /// Expected result: SetValue is called once with FontWeightProperty and the whitespace string.
        /// </summary>
        [Fact]
        public void SetFontWeight_ValidElementAndWhitespaceWeight_CallsSetValueWithWhitespace()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var weight = "   ";

            // Act
            Entry.SetFontWeight(element, weight);

            // Assert
            element.Received(1).SetValue(Entry.FontWeightProperty, "   ");
        }

        /// <summary>
        /// Tests that SetFontWeight throws ArgumentNullException when element parameter is null.
        /// Input conditions: Null BindableObject element.
        /// Expected result: ArgumentNullException is thrown when SetValue is called on null.
        /// </summary>
        [Fact]
        public void SetFontWeight_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;
            var weight = "Bold";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Entry.SetFontWeight(element, weight));
        }

        /// <summary>
        /// Tests that SetFontWeight handles very long weight strings correctly.
        /// Input conditions: Valid BindableObject and very long weight string.
        /// Expected result: SetValue is called once with FontWeightProperty and the long string.
        /// </summary>
        [Fact]
        public void SetFontWeight_ValidElementAndLongWeight_CallsSetValueWithLongString()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var weight = new string('a', 1000);

            // Act
            Entry.SetFontWeight(element, weight);

            // Assert
            element.Received(1).SetValue(Entry.FontWeightProperty, weight);
        }

        /// <summary>
        /// Tests that SetFontWeight handles weight strings with special characters correctly.
        /// Input conditions: Valid BindableObject and weight string containing special characters.
        /// Expected result: SetValue is called once with FontWeightProperty and the special character string.
        /// </summary>
        [Theory]
        [InlineData("Bold\n")]
        [InlineData("Bold\t")]
        [InlineData("Bold\r\n")]
        [InlineData("Bold\0")]
        [InlineData("Bold@#$%")]
        [InlineData("Bold🎯")]
        public void SetFontWeight_ValidElementAndSpecialCharacterWeight_CallsSetValueCorrectly(string weight)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            Entry.SetFontWeight(element, weight);

            // Assert
            element.Received(1).SetValue(Entry.FontWeightProperty, weight);
        }

        /// <summary>
        /// Tests that GetFontWeight extension method returns the correct font weight value when called with a valid configuration.
        /// Input: Valid IPlatformElementConfiguration with mocked Element that returns "Bold" font weight.
        /// Expected: Method returns "Bold".
        /// </summary>
        [Fact]
        public void GetFontWeight_ValidConfig_ReturnsExpectedFontWeight()
        {
            // Arrange
            var mockEntry = Substitute.For<Entry>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Entry>>();
            mockConfig.Element.Returns(mockEntry);

            // Mock the static method behavior by setting up the BindableProperty
            mockEntry.GetValue(Entry.FontWeightProperty).Returns("Bold");

            // Act
            var result = Entry.GetFontWeight(mockConfig);

            // Assert
            Assert.Equal("Bold", result);
        }

        /// <summary>
        /// Tests that GetFontWeight extension method returns the default font weight when called with a configuration containing an entry with default font weight.
        /// Input: Valid IPlatformElementConfiguration with mocked Element that returns default font weight "None".
        /// Expected: Method returns "None".
        /// </summary>
        [Fact]
        public void GetFontWeight_ConfigWithDefaultFontWeight_ReturnsNone()
        {
            // Arrange
            var mockEntry = Substitute.For<Entry>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Entry>>();
            mockConfig.Element.Returns(mockEntry);

            // Mock the static method behavior by setting up the BindableProperty with default value
            mockEntry.GetValue(Entry.FontWeightProperty).Returns(FontWeight.None);

            // Act
            var result = Entry.GetFontWeight(mockConfig);

            // Assert
            Assert.Equal(FontWeight.None, result);
        }

        /// <summary>
        /// Tests that GetFontWeight extension method throws ArgumentNullException when called with null configuration.
        /// Input: Null IPlatformElementConfiguration parameter.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void GetFontWeight_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Tizen, Entry> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Entry.GetFontWeight(config));
        }

        /// <summary>
        /// Tests that GetFontWeight extension method returns empty string when the underlying GetValue returns empty string.
        /// Input: Valid IPlatformElementConfiguration with mocked Element that returns empty string font weight.
        /// Expected: Method returns empty string.
        /// </summary>
        [Fact]
        public void GetFontWeight_EmptyStringFontWeight_ReturnsEmptyString()
        {
            // Arrange
            var mockEntry = Substitute.For<Entry>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Entry>>();
            mockConfig.Element.Returns(mockEntry);

            // Mock the static method behavior by setting up the BindableProperty with empty string
            mockEntry.GetValue(Entry.FontWeightProperty).Returns(string.Empty);

            // Act
            var result = Entry.GetFontWeight(mockConfig);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that GetFontWeight extension method returns null when the underlying GetValue returns null.
        /// Input: Valid IPlatformElementConfiguration with mocked Element that returns null font weight.
        /// Expected: Method returns null.
        /// </summary>
        [Fact]
        public void GetFontWeight_NullFontWeight_ReturnsNull()
        {
            // Arrange
            var mockEntry = Substitute.For<Entry>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Entry>>();
            mockConfig.Element.Returns(mockEntry);

            // Mock the static method behavior by setting up the BindableProperty with null
            mockEntry.GetValue(Entry.FontWeightProperty).Returns(null);

            // Act
            var result = Entry.GetFontWeight(mockConfig);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that SetFontWeight extension method calls the static SetFontWeight method and returns the same config object.
        /// Input: Valid config and weight string.
        /// Expected: Method executes successfully and returns the same config object.
        /// </summary>
        [Fact]
        public void SetFontWeight_ValidConfigAndWeight_ReturnsConfigAndCallsStaticMethod()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Entry>>();
            var mockEntry = Substitute.For<Entry>();
            mockConfig.Element.Returns(mockEntry);
            string weight = "Bold";

            // Act
            var result = mockConfig.SetFontWeight(weight);

            // Assert
            Assert.Same(mockConfig, result);
            mockEntry.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Entry.FontWeightProperty, weight);
        }

        /// <summary>
        /// Tests that SetFontWeight extension method throws ArgumentNullException when config is null.
        /// Input: Null config parameter.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void SetFontWeight_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Tizen, Entry> config = null;
            string weight = "Normal";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.SetFontWeight(weight));
        }

        /// <summary>
        /// Tests that SetFontWeight extension method handles null weight parameter correctly.
        /// Input: Valid config with null weight string.
        /// Expected: Method executes successfully with null weight value.
        /// </summary>
        [Fact]
        public void SetFontWeight_NullWeight_ExecutesSuccessfully()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Entry>>();
            var mockEntry = Substitute.For<Entry>();
            mockConfig.Element.Returns(mockEntry);
            string weight = null;

            // Act
            var result = mockConfig.SetFontWeight(weight);

            // Assert
            Assert.Same(mockConfig, result);
            mockEntry.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Entry.FontWeightProperty, weight);
        }

        /// <summary>
        /// Tests that SetFontWeight extension method handles empty weight string correctly.
        /// Input: Valid config with empty weight string.
        /// Expected: Method executes successfully with empty weight value.
        /// </summary>
        [Fact]
        public void SetFontWeight_EmptyWeight_ExecutesSuccessfully()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Entry>>();
            var mockEntry = Substitute.For<Entry>();
            mockConfig.Element.Returns(mockEntry);
            string weight = string.Empty;

            // Act
            var result = mockConfig.SetFontWeight(weight);

            // Assert
            Assert.Same(mockConfig, result);
            mockEntry.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Entry.FontWeightProperty, weight);
        }

        /// <summary>
        /// Tests that SetFontWeight extension method handles whitespace-only weight string correctly.
        /// Input: Valid config with whitespace-only weight string.
        /// Expected: Method executes successfully with whitespace weight value.
        /// </summary>
        [Fact]
        public void SetFontWeight_WhitespaceWeight_ExecutesSuccessfully()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Entry>>();
            var mockEntry = Substitute.For<Entry>();
            mockConfig.Element.Returns(mockEntry);
            string weight = "   ";

            // Act
            var result = mockConfig.SetFontWeight(weight);

            // Assert
            Assert.Same(mockConfig, result);
            mockEntry.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Entry.FontWeightProperty, weight);
        }

        /// <summary>
        /// Tests that SetFontWeight extension method handles very long weight string correctly.
        /// Input: Valid config with very long weight string.
        /// Expected: Method executes successfully with long weight value.
        /// </summary>
        [Fact]
        public void SetFontWeight_VeryLongWeight_ExecutesSuccessfully()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Entry>>();
            var mockEntry = Substitute.For<Entry>();
            mockConfig.Element.Returns(mockEntry);
            string weight = new string('A', 10000);

            // Act
            var result = mockConfig.SetFontWeight(weight);

            // Assert
            Assert.Same(mockConfig, result);
            mockEntry.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Entry.FontWeightProperty, weight);
        }

        /// <summary>
        /// Tests that SetFontWeight extension method handles weight string with special characters correctly.
        /// Input: Valid config with weight string containing special characters.
        /// Expected: Method executes successfully with special character weight value.
        /// </summary>
        [Theory]
        [InlineData("Bold!@#")]
        [InlineData("Normal\n\r\t")]
        [InlineData("Light\u0000\u001F")]
        [InlineData("Medium\ud800\udc00")] // Unicode surrogate pair
        public void SetFontWeight_WeightWithSpecialCharacters_ExecutesSuccessfully(string weight)
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Entry>>();
            var mockEntry = Substitute.For<Entry>();
            mockConfig.Element.Returns(mockEntry);

            // Act
            var result = mockConfig.SetFontWeight(weight);

            // Assert
            Assert.Same(mockConfig, result);
            mockEntry.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Entry.FontWeightProperty, weight);
        }

        /// <summary>
        /// Tests that SetFontWeight extension method works correctly with various valid weight values.
        /// Input: Valid config with different weight string values.
        /// Expected: Method executes successfully and sets the correct weight value.
        /// </summary>
        [Theory]
        [InlineData("Normal")]
        [InlineData("Bold")]
        [InlineData("Light")]
        [InlineData("Medium")]
        [InlineData("Thin")]
        [InlineData("Black")]
        [InlineData("100")]
        [InlineData("900")]
        public void SetFontWeight_ValidWeightValues_ExecutesSuccessfully(string weight)
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Entry>>();
            var mockEntry = Substitute.For<Entry>();
            mockConfig.Element.Returns(mockEntry);

            // Act
            var result = mockConfig.SetFontWeight(weight);

            // Assert
            Assert.Same(mockConfig, result);
            mockEntry.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Entry.FontWeightProperty, weight);
        }

        /// <summary>
        /// Tests that GetFontWeight throws ArgumentNullException when element parameter is null.
        /// Input: null element parameter.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void GetFontWeight_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Entry.GetFontWeight(element));
        }

        /// <summary>
        /// Tests that GetFontWeight returns the font weight value when element contains a valid string value.
        /// Input: Valid BindableObject with string font weight value.
        /// Expected: Returns the string value from the font weight property.
        /// </summary>
        [Theory]
        [InlineData("Bold")]
        [InlineData("Normal")]
        [InlineData("Light")]
        [InlineData("")]
        [InlineData("   ")]
        public void GetFontWeight_ValidElementWithStringValue_ReturnsStringValue(string expectedValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Entry.FontWeightProperty).Returns(expectedValue);

            // Act
            var result = Entry.GetFontWeight(element);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetFontWeight returns null when element's font weight property value is null.
        /// Input: Valid BindableObject with null font weight property value.
        /// Expected: Returns null.
        /// </summary>
        [Fact]
        public void GetFontWeight_ElementWithNullValue_ReturnsNull()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Entry.FontWeightProperty).Returns((object)null);

            // Act
            var result = Entry.GetFontWeight(element);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetFontWeight throws InvalidCastException when element's font weight property contains non-string value.
        /// Input: Valid BindableObject with non-string font weight property value.
        /// Expected: InvalidCastException is thrown during cast to string.
        /// </summary>
        [Theory]
        [InlineData(123)]
        [InlineData(true)]
        [InlineData(45.67)]
        public void GetFontWeight_ElementWithNonStringValue_ThrowsInvalidCastException(object nonStringValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Entry.FontWeightProperty).Returns(nonStringValue);

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => Entry.GetFontWeight(element));
        }
    }
}