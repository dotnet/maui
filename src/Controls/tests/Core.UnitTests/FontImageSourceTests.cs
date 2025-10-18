#nullable disable

using System;
using System.ComponentModel;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the FontImageSource class Color property.
    /// </summary>
    public partial class FontImageSourceTests
    {
        /// <summary>
        /// Tests that the Color property getter returns the default Color value when no value has been explicitly set.
        /// Input conditions: Newly created FontImageSource instance.
        /// Expected result: Color property returns the default Color value (black with alpha 1).
        /// </summary>
        [Fact]
        public void Color_WhenNotSet_ReturnsDefaultValue()
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            var result = fontImageSource.Color;

            // Assert
            Assert.Equal(default(Color), result);
        }

        /// <summary>
        /// Tests that the Color property getter returns the value that was previously set via the setter.
        /// Input conditions: FontImageSource with Color set to a specific Color value.
        /// Expected result: Color property getter returns the same Color value that was set.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Semi-transparent gray
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent black
        public void Color_WhenSetToSpecificValue_ReturnsSetValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var fontImageSource = new FontImageSource();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            fontImageSource.Color = expectedColor;
            var result = fontImageSource.Color;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that setting the Color property multiple times correctly updates the stored value.
        /// Input conditions: FontImageSource with Color set to different values sequentially.
        /// Expected result: Color property getter always returns the most recently set value.
        /// </summary>
        [Fact]
        public void Color_WhenSetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var fontImageSource = new FontImageSource();
            var firstColor = new Color(1.0f, 0.0f, 0.0f, 1.0f); // Red
            var secondColor = new Color(0.0f, 1.0f, 0.0f, 1.0f); // Green
            var thirdColor = new Color(0.0f, 0.0f, 1.0f, 1.0f); // Blue

            // Act & Assert - First setting
            fontImageSource.Color = firstColor;
            Assert.Equal(firstColor, fontImageSource.Color);

            // Act & Assert - Second setting
            fontImageSource.Color = secondColor;
            Assert.Equal(secondColor, fontImageSource.Color);

            // Act & Assert - Third setting
            fontImageSource.Color = thirdColor;
            Assert.Equal(thirdColor, fontImageSource.Color);
        }

        /// <summary>
        /// Tests that the Color property handles boundary values correctly.
        /// Input conditions: FontImageSource with Color set to boundary Color values (0.0 and 1.0 components).
        /// Expected result: Color property correctly stores and returns boundary values.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // All minimum values
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // All maximum values
        [InlineData(1.0f, 0.0f, 0.0f, 0.0f)] // Mixed boundary values
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Mixed boundary values
        public void Color_WithBoundaryValues_HandlesCorrectly(float red, float green, float blue, float alpha)
        {
            // Arrange
            var fontImageSource = new FontImageSource();
            var boundaryColor = new Color(red, green, blue, alpha);

            // Act
            fontImageSource.Color = boundaryColor;
            var result = fontImageSource.Color;

            // Assert
            Assert.Equal(boundaryColor, result);
        }

        /// <summary>
        /// Tests that FontFamily property returns null by default when no value has been set.
        /// Validates the default behavior of the property getter.
        /// </summary>
        [Fact]
        public void FontFamily_DefaultValue_ReturnsNull()
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            var result = fontImageSource.FontFamily;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that FontFamily property getter returns the exact value that was set via the setter.
        /// Validates the round-trip behavior of the property with various string values.
        /// </summary>
        /// <param name="fontFamily">The font family string to test with</param>
        /// <param name="description">Description of the test case</param>
        [Theory]
        [InlineData(null, "null value")]
        [InlineData("", "empty string")]
        [InlineData("   ", "whitespace only")]
        [InlineData("\t\n", "tab and newline")]
        [InlineData("Arial", "normal font name")]
        [InlineData("Times New Roman", "font name with spaces")]
        [InlineData("Segoe UI Emoji", "font name with multiple spaces")]
        [InlineData("MyCustomFont123", "font name with numbers")]
        [InlineData("Font-Family_Name", "font name with special characters")]
        [InlineData("\"Quoted Font\"", "font name with quotes")]
        [InlineData("Font\\Family", "font name with backslash")]
        [InlineData("Символы", "font name with Unicode characters")]
        public void FontFamily_SetAndGet_ReturnsSetValue(string fontFamily, string description)
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            fontImageSource.FontFamily = fontFamily;
            var result = fontImageSource.FontFamily;

            // Assert
            Assert.Equal(fontFamily, result);
        }

        /// <summary>
        /// Tests that FontFamily property getter works correctly with very long strings.
        /// Validates that the property can handle edge cases with string length.
        /// </summary>
        [Fact]
        public void FontFamily_VeryLongString_ReturnsSetValue()
        {
            // Arrange
            var fontImageSource = new FontImageSource();
            var longFontFamily = new string('A', 1000);

            // Act
            fontImageSource.FontFamily = longFontFamily;
            var result = fontImageSource.FontFamily;

            // Assert
            Assert.Equal(longFontFamily, result);
        }

        /// <summary>
        /// Tests that FontFamily property can be set multiple times and getter returns the latest value.
        /// Validates that the property correctly handles multiple assignments.
        /// </summary>
        [Fact]
        public void FontFamily_MultipleAssignments_ReturnsLatestValue()
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act & Assert - First assignment
            fontImageSource.FontFamily = "Arial";
            Assert.Equal("Arial", fontImageSource.FontFamily);

            // Act & Assert - Second assignment
            fontImageSource.FontFamily = "Times New Roman";
            Assert.Equal("Times New Roman", fontImageSource.FontFamily);

            // Act & Assert - Assignment to null
            fontImageSource.FontFamily = null;
            Assert.Null(fontImageSource.FontFamily);

            // Act & Assert - Assignment to empty string
            fontImageSource.FontFamily = "";
            Assert.Equal("", fontImageSource.FontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property getter returns correct value immediately after setting.
        /// Validates the immediate consistency of the property getter with the setter.
        /// </summary>
        [Theory]
        [InlineData("Helvetica")]
        [InlineData("Courier New")]
        [InlineData("Comic Sans MS")]
        public void FontFamily_ImmediateGetAfterSet_ReturnsCorrectValue(string expectedFontFamily)
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            fontImageSource.FontFamily = expectedFontFamily;

            // Assert - Immediate get should return the set value
            Assert.Equal(expectedFontFamily, fontImageSource.FontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property handles edge case string values correctly.
        /// Validates the property behavior with strings containing special formatting characters.
        /// </summary>
        [Theory]
        [InlineData(" Arial ", "string with leading and trailing spaces")]
        [InlineData("\r\nFont\r\n", "string with carriage returns and newlines")]
        [InlineData("Font\0Name", "string with null character")]
        [InlineData("Font\u0001Name", "string with control character")]
        public void FontFamily_SpecialCharacters_ReturnsSetValue(string fontFamily, string description)
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            fontImageSource.FontFamily = fontFamily;
            var result = fontImageSource.FontFamily;

            // Assert
            Assert.Equal(fontFamily, result);
        }

        /// <summary>
        /// Tests that the Glyph property getter returns the correct value after setting it.
        /// This test covers the getter implementation that calls GetValue(GlyphProperty).
        /// </summary>
        [Fact]
        public void Glyph_GetAfterSet_ReturnsSetValue()
        {
            // Arrange
            var fontImageSource = new FontImageSource();
            var expectedGlyph = "TestGlyph";

            // Act
            fontImageSource.Glyph = expectedGlyph;
            var actualGlyph = fontImageSource.Glyph;

            // Assert
            Assert.Equal(expectedGlyph, actualGlyph);
        }

        /// <summary>
        /// Tests that the Glyph property getter returns the default value when not explicitly set.
        /// This test verifies the default behavior of the getter implementation.
        /// </summary>
        [Fact]
        public void Glyph_GetWithoutSet_ReturnsDefaultValue()
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            var actualGlyph = fontImageSource.Glyph;

            // Assert
            Assert.Null(actualGlyph);
        }

        /// <summary>
        /// Tests the Glyph property setter and getter with various string edge cases.
        /// This parameterized test covers multiple edge cases including null, empty, whitespace, and special strings.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   \t\n\r   ")]
        [InlineData("A")]
        [InlineData("🙂")]
        [InlineData("★")]
        [InlineData("\u0041\u0042\u0043")]
        [InlineData("Very long glyph string that exceeds typical glyph length expectations")]
        public void Glyph_SetAndGet_WithVariousStringValues_ReturnsCorrectValue(string glyphValue)
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            fontImageSource.Glyph = glyphValue;
            var actualGlyph = fontImageSource.Glyph;

            // Assert
            Assert.Equal(glyphValue, actualGlyph);
        }

        /// <summary>
        /// Tests the Glyph property with Unicode characters and emojis commonly used in font glyphs.
        /// This test verifies proper handling of Unicode characters that are typical in icon fonts.
        /// </summary>
        [Theory]
        [InlineData("\uF100")] // Private Use Area character
        [InlineData("\u2665")] // Black Heart Suit
        [InlineData("\u26A0")] // Warning Sign
        [InlineData("\uD83D\uDE00")] // Grinning Face emoji
        [InlineData("\u00A9")] // Copyright symbol
        [InlineData("\u2122")] // Trade Mark symbol
        public void Glyph_SetAndGet_WithUnicodeCharacters_ReturnsCorrectValue(string unicodeGlyph)
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            fontImageSource.Glyph = unicodeGlyph;
            var actualGlyph = fontImageSource.Glyph;

            // Assert
            Assert.Equal(unicodeGlyph, actualGlyph);
        }

        /// <summary>
        /// Tests the Glyph property with control characters and special whitespace characters.
        /// This test verifies handling of various control and whitespace characters.
        /// </summary>
        [Theory]
        [InlineData("\t")] // Tab
        [InlineData("\n")] // Newline
        [InlineData("\r")] // Carriage return
        [InlineData("\u0000")] // Null character
        [InlineData("\u000B")] // Vertical tab
        [InlineData("\u000C")] // Form feed
        [InlineData("\u00A0")] // Non-breaking space
        public void Glyph_SetAndGet_WithControlCharacters_ReturnsCorrectValue(string controlChar)
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            fontImageSource.Glyph = controlChar;
            var actualGlyph = fontImageSource.Glyph;

            // Assert
            Assert.Equal(controlChar, actualGlyph);
        }

        /// <summary>
        /// Tests that setting the Glyph property multiple times correctly updates the value.
        /// This test ensures the setter properly overwrites previous values.
        /// </summary>
        [Fact]
        public void Glyph_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var fontImageSource = new FontImageSource();
            var firstGlyph = "FirstGlyph";
            var secondGlyph = "SecondGlyph";

            // Act
            fontImageSource.Glyph = firstGlyph;
            fontImageSource.Glyph = secondGlyph;
            var actualGlyph = fontImageSource.Glyph;

            // Assert
            Assert.Equal(secondGlyph, actualGlyph);
        }

        /// <summary>
        /// Tests that setting the Glyph property to null after setting a value correctly returns null.
        /// This test verifies that null values properly override previously set non-null values.
        /// </summary>
        [Fact]
        public void Glyph_SetToNullAfterValue_ReturnsNull()
        {
            // Arrange
            var fontImageSource = new FontImageSource();
            var initialGlyph = "InitialGlyph";

            // Act
            fontImageSource.Glyph = initialGlyph;
            fontImageSource.Glyph = null;
            var actualGlyph = fontImageSource.Glyph;

            // Assert
            Assert.Null(actualGlyph);
        }

        /// <summary>
        /// Tests that the Size property returns the default value (30.0) when no value has been set.
        /// This test verifies the default behavior and exercises the getter implementation.
        /// Expected result: Size should return 30.0.
        /// </summary>
        [Fact]
        public void Size_WhenNotSet_ReturnsDefaultValue()
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            var size = fontImageSource.Size;

            // Assert
            Assert.Equal(30.0, size);
        }

        /// <summary>
        /// Tests that the Size property returns the correct value after being set.
        /// This test verifies the getter returns the value that was previously set via the setter.
        /// Expected result: Size should return the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(12.5)]
        [InlineData(100.0)]
        [InlineData(999.99)]
        public void Size_AfterSetting_ReturnsSetValue(double expectedSize)
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            fontImageSource.Size = expectedSize;
            var actualSize = fontImageSource.Size;

            // Assert
            Assert.Equal(expectedSize, actualSize);
        }

        /// <summary>
        /// Tests that the Size property handles negative values correctly.
        /// This test verifies behavior with negative double values.
        /// Expected result: Size should return the negative value that was set.
        /// </summary>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-100.5)]
        [InlineData(double.MinValue)]
        public void Size_WithNegativeValues_ReturnsSetValue(double negativeSize)
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            fontImageSource.Size = negativeSize;
            var actualSize = fontImageSource.Size;

            // Assert
            Assert.Equal(negativeSize, actualSize);
        }

        /// <summary>
        /// Tests that the Size property handles extreme double values correctly.
        /// This test verifies behavior with boundary values for double type.
        /// Expected result: Size should return the extreme value that was set.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void Size_WithExtremeValues_ReturnsSetValue(double extremeSize)
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            fontImageSource.Size = extremeSize;
            var actualSize = fontImageSource.Size;

            // Assert
            Assert.Equal(extremeSize, actualSize);
        }

        /// <summary>
        /// Tests that the Size property handles NaN (Not a Number) values correctly.
        /// This test verifies behavior with NaN double values.
        /// Expected result: Size should return NaN when NaN was set.
        /// </summary>
        [Fact]
        public void Size_WithNaN_ReturnsNaN()
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            fontImageSource.Size = double.NaN;
            var actualSize = fontImageSource.Size;

            // Assert
            Assert.True(double.IsNaN(actualSize));
        }

        /// <summary>
        /// Tests that the Size property has the correct TypeConverter attribute applied.
        /// This test verifies that the FontSizeConverter is properly attributed to the Size property.
        /// Expected result: The TypeConverter attribute should specify FontSizeConverter.
        /// </summary>
        [Fact]
        public void Size_HasCorrectTypeConverterAttribute()
        {
            // Arrange
            var propertyInfo = typeof(FontImageSource).GetProperty(nameof(FontImageSource.Size));

            // Act
            var typeConverterAttribute = propertyInfo.GetCustomAttributes(typeof(TypeConverterAttribute), false);

            // Assert
            Assert.Single(typeConverterAttribute);
            var attribute = (TypeConverterAttribute)typeConverterAttribute[0];
            Assert.Equal(typeof(FontSizeConverter).AssemblyQualifiedName, attribute.ConverterTypeName);
        }

        /// <summary>
        /// Tests that multiple get operations on Size property return consistent values.
        /// This test verifies the getter implementation stability and consistency.
        /// Expected result: Multiple calls to Size getter should return the same value.
        /// </summary>
        [Fact]
        public void Size_MultipleGetOperations_ReturnsConsistentValues()
        {
            // Arrange
            var fontImageSource = new FontImageSource();
            var testValue = 42.75;
            fontImageSource.Size = testValue;

            // Act
            var size1 = fontImageSource.Size;
            var size2 = fontImageSource.Size;
            var size3 = fontImageSource.Size;

            // Assert
            Assert.Equal(testValue, size1);
            Assert.Equal(testValue, size2);
            Assert.Equal(testValue, size3);
            Assert.Equal(size1, size2);
            Assert.Equal(size2, size3);
        }

        /// <summary>
        /// Tests that FontAutoScalingEnabled returns the default value of false when not explicitly set.
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_DefaultValue_ReturnsFalse()
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            bool result = fontImageSource.FontAutoScalingEnabled;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that FontAutoScalingEnabled can be set to true and returns the correct value.
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            fontImageSource.FontAutoScalingEnabled = true;

            // Assert
            Assert.True(fontImageSource.FontAutoScalingEnabled);
        }

        /// <summary>
        /// Tests that FontAutoScalingEnabled can be set to false and returns the correct value.
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            fontImageSource.FontAutoScalingEnabled = false;

            // Assert
            Assert.False(fontImageSource.FontAutoScalingEnabled);
        }

        /// <summary>
        /// Tests that FontAutoScalingEnabled can be toggled between true and false values.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FontAutoScalingEnabled_SetValue_ReturnsExpectedValue(bool expectedValue)
        {
            // Arrange
            var fontImageSource = new FontImageSource();

            // Act
            fontImageSource.FontAutoScalingEnabled = expectedValue;

            // Assert
            Assert.Equal(expectedValue, fontImageSource.FontAutoScalingEnabled);
        }

        /// <summary>
        /// Tests that the IsEmpty property correctly evaluates whether the Glyph property is null or empty.
        /// Verifies the behavior for null, empty string, whitespace, and valid glyph values.
        /// </summary>
        /// <param name="glyph">The glyph value to test</param>
        /// <param name="expectedIsEmpty">The expected IsEmpty result</param>
        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData(" ", false)]
        [InlineData("🔥", false)]
        [InlineData("A", false)]
        [InlineData("   ", false)]
        [InlineData("\t", false)]
        [InlineData("\n", false)]
        public void IsEmpty_WithVariousGlyphValues_ReturnsExpectedResult(string glyph, bool expectedIsEmpty)
        {
            // Arrange
            var fontImageSource = new FontImageSource();
            fontImageSource.Glyph = glyph;

            // Act
            var result = fontImageSource.IsEmpty;

            // Assert
            Assert.Equal(expectedIsEmpty, result);
        }
    }
}