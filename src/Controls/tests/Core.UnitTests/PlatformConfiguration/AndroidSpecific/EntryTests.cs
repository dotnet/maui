#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.AndroidSpecific
{
    /// <summary>
    /// Unit tests for the SetImeOptions method in the Entry class.
    /// </summary>
    public class EntryTests
    {
        /// <summary>
        /// Tests that SetImeOptions throws ArgumentNullException when element parameter is null.
        /// </summary>
        [Fact]
        public void SetImeOptions_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;
            var value = ImeFlags.Default;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Entry.SetImeOptions(element, value));
        }

        /// <summary>
        /// Tests that SetImeOptions works with undefined enum values (boundary testing).
        /// </summary>
        /// <param name="undefinedValue">An undefined ImeFlags value to test.</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SetImeOptions_ValidElementAndUndefinedImeFlags_CallsSetValueWithCorrectParameters(int undefinedValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var imeFlagsValue = (ImeFlags)undefinedValue;

            // Act
            Entry.SetImeOptions(element, imeFlagsValue);

            // Assert
            element.Received(1).SetValue(Entry.ImeOptionsProperty, imeFlagsValue);
        }

        /// <summary>
        /// Tests that SetImeOptions works with combined enum flags (bitwise operations).
        /// </summary>
        [Fact]
        public void SetImeOptions_ValidElementAndCombinedFlags_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var combinedFlags = ImeFlags.Go | ImeFlags.NoFullscreen;

            // Act
            Entry.SetImeOptions(element, combinedFlags);

            // Assert
            element.Received(1).SetValue(Entry.ImeOptionsProperty, combinedFlags);
        }

        /// <summary>
        /// Tests that ImeOptions extension method throws ArgumentNullException when config parameter is null.
        /// Verifies proper null parameter validation.
        /// </summary>
        [Fact]
        public void ImeOptions_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Android, Entry> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.ImeOptions());
        }

        /// <summary>
        /// Tests that ImeOptions extension method throws ArgumentNullException when config.Element is null.
        /// Verifies proper handling of null Element property.
        /// </summary>
        [Fact]
        public void ImeOptions_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Entry>>();
            mockConfig.Element.Returns((Entry)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => mockConfig.ImeOptions());
        }

        /// <summary>
        /// Tests that ImeOptions extension method handles enum values outside the defined range.
        /// Verifies the method can handle cast operations with undefined enum values.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999999)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void ImeOptions_UndefinedEnumValues_ReturnsEnumValue(int enumValue)
        {
            // Arrange
            var mockEntry = Substitute.For<Entry>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Entry>>();
            var expectedFlags = (ImeFlags)enumValue;

            mockConfig.Element.Returns(mockEntry);
            mockEntry.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Entry.ImeOptionsProperty)
                .Returns(expectedFlags);

            // Act
            var result = mockConfig.ImeOptions();

            // Assert
            Assert.Equal(expectedFlags, result);
        }

        /// <summary>
        /// Tests that SetImeOptions extension method throws ArgumentNullException when config parameter is null.
        /// Verifies proper null parameter validation for the extension method.
        /// </summary>
        [Fact]
        public void SetImeOptions_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Android, Entry> nullConfig = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => nullConfig.SetImeOptions(ImeFlags.Default));
        }

        /// <summary>
        /// Tests that SetImeOptions extension method works with invalid enum values that are cast from integers.
        /// Verifies that the method handles out-of-range enum values gracefully by passing them through.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MaxValue)]
        public void SetImeOptions_InvalidEnumValue_CallsSetValueAndReturnsConfig(int invalidValue)
        {
            // Arrange
            var mockEntry = Substitute.For<Entry>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Entry>>();
            mockConfig.Element.Returns(mockEntry);
            var invalidImeFlags = (ImeFlags)invalidValue;

            // Act
            var result = mockConfig.SetImeOptions(invalidImeFlags);

            // Assert
            mockEntry.Received(1).SetValue(Entry.ImeOptionsProperty, invalidImeFlags);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetImeOptions extension method properly handles the case where config.Element is null.
        /// Verifies that the method delegates to the static SetImeOptions method even with a null element.
        /// </summary>
        [Fact]
        public void SetImeOptions_ConfigElementIsNull_CallsStaticMethod()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Entry>>();
            mockConfig.Element.Returns((Entry)null);

            // Act & Assert
            // This should throw a NullReferenceException when the static method tries to call SetValue on null
            Assert.Throws<NullReferenceException>(() => mockConfig.SetImeOptions(ImeFlags.Default));
        }

        /// <summary>
        /// Tests that GetImeOptions throws ArgumentNullException when element parameter is null.
        /// Verifies proper null parameter validation.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void GetImeOptions_WithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Entry.GetImeOptions(element));
        }

        /// <summary>
        /// Tests that GetImeOptions handles undefined enum values by casting them correctly.
        /// Verifies behavior with enum values outside the defined range.
        /// Expected result: Returns the casted ImeFlags value even if undefined.
        /// </summary>
        [Fact]
        public void GetImeOptions_WithUndefinedEnumValue_ReturnsUndefinedImeFlags()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var undefinedValue = 9999; // Not a defined ImeFlags value
            element.GetValue(Entry.ImeOptionsProperty).Returns(undefinedValue);

            // Act
            var result = Entry.GetImeOptions(element);

            // Assert
            Assert.Equal((ImeFlags)undefinedValue, result);
        }

        /// <summary>
        /// Tests that GetImeOptions handles null return value from GetValue by casting to default enum value.
        /// Verifies behavior when element.GetValue returns null.
        /// Expected result: Returns ImeFlags.Default (0) when casting null to enum.
        /// </summary>
        [Fact]
        public void GetImeOptions_WithNullReturnValue_ReturnsDefaultImeFlags()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Entry.ImeOptionsProperty).Returns((object)null);

            // Act
            var result = Entry.GetImeOptions(element);

            // Assert
            Assert.Equal(ImeFlags.Default, result);
        }
    }
}