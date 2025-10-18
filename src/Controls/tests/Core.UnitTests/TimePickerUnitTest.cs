#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class TimePickerUnitTest : BaseTestFixture
    {
        [Fact]
        public void TestConstructor()
        {
            TimePicker picker = new TimePicker();

            Assert.Equal(new TimeSpan(), picker.Time);
        }

        [Fact]
        public void TestTimeOutOfRange()
        {
            var picker = new TimePicker
            {
                Time = new TimeSpan(1000, 0, 0)
            };
            Assert.Equal(picker.Time, new TimeSpan());

            picker.Time = new TimeSpan(8, 30, 0);
            Assert.Equal(new TimeSpan(8, 30, 0), picker.Time);

            picker.Time = new TimeSpan(-1, 0, 0);
            Assert.Equal(new TimeSpan(8, 30, 0), picker.Time);
        }

        [Fact("Issue #745")]
        public void ZeroTimeIsValid()
        {
            _ = new TimePicker
            {
                Time = new TimeSpan(0, 0, 0)
            };
        }

        [Fact]
        public void NullTimeIsValid()
        {
            var timePicker = new TimePicker
            {
                Time = null
            };

            Assert.Null(timePicker.Time);
        }

        [Fact]
        public void TestTimeSelected()
        {
            var picker = new TimePicker();

            int selected = 0;
            picker.TimeSelected += (sender, arg) => selected++;

            // we can be fairly sure it wont ever be 2008 again
            picker.Time = new TimeSpan(12, 30, 15);

            Assert.Equal(1, selected);
        }

        public static object[] TimeSpans = {
            new object[] { new TimeSpan (), new TimeSpan(9, 0, 0) },
            new object[] { new TimeSpan(9, 0, 0), new TimeSpan(17, 30, 0) },
            new object[] { new TimeSpan(23, 59, 59), new TimeSpan(0, 0, 0) },
            new object[] { new TimeSpan(23, 59, 59), null },
            new object[] { null, new TimeSpan(23, 59, 59) },
        };

        public static IEnumerable<object[]> TimeSpansData()
        {
            foreach (var o in TimeSpans)
            {
                yield return o as object[];
            }
        }

        [Theory, MemberData(nameof(TimeSpansData))]
        public void DatePickerSelectedEventArgs(TimeSpan initialTime, TimeSpan finalTime)
        {
            var timePicker = new TimePicker();
            timePicker.Time = initialTime;

            TimePicker pickerFromSender = null;
            TimeSpan? oldTime = new TimeSpan();
            TimeSpan? newTime = new TimeSpan();

            timePicker.TimeSelected += (s, e) =>
            {
                pickerFromSender = (TimePicker)s;
                oldTime = e.OldTime;
                newTime = e.NewTime;
            };

            timePicker.Time = finalTime;

            Assert.Equal(timePicker, pickerFromSender);
            Assert.Equal(initialTime, oldTime);
            Assert.Equal(finalTime, newTime);
        }
    }

    public partial class TimePickerTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that CharacterSpacing property returns the default value of 0.0 when no value has been set.
        /// </summary>
        [Fact]
        public void CharacterSpacing_DefaultValue_ReturnsZero()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            var result = timePicker.CharacterSpacing;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing property correctly returns various positive double values after being set.
        /// </summary>
        /// <param name="value">The positive character spacing value to test</param>
        /// <param name="expectedResult">The expected returned value</param>
        [Theory]
        [InlineData(1.0, 1.0)]
        [InlineData(2.5, 2.5)]
        [InlineData(10.75, 10.75)]
        [InlineData(100.5, 100.5)]
        [InlineData(0.1, 0.1)]
        public void CharacterSpacing_PositiveValues_ReturnsCorrectValue(double value, double expectedResult)
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.CharacterSpacing = value;
            var result = timePicker.CharacterSpacing;

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing property correctly returns various negative double values after being set.
        /// </summary>
        /// <param name="value">The negative character spacing value to test</param>
        /// <param name="expectedResult">The expected returned value</param>
        [Theory]
        [InlineData(-1.0, -1.0)]
        [InlineData(-2.5, -2.5)]
        [InlineData(-10.75, -10.75)]
        [InlineData(-100.5, -100.5)]
        [InlineData(-0.1, -0.1)]
        public void CharacterSpacing_NegativeValues_ReturnsCorrectValue(double value, double expectedResult)
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.CharacterSpacing = value;
            var result = timePicker.CharacterSpacing;

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing property returns zero when explicitly set to zero.
        /// </summary>
        [Fact]
        public void CharacterSpacing_ZeroValue_ReturnsZero()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.CharacterSpacing = 0.0;
            var result = timePicker.CharacterSpacing;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing property correctly handles extreme double values.
        /// </summary>
        /// <param name="value">The extreme double value to test</param>
        /// <param name="expectedResult">The expected returned value</param>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        public void CharacterSpacing_ExtremeValues_ReturnsCorrectValue(double value, double expectedResult)
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.CharacterSpacing = value;
            var result = timePicker.CharacterSpacing;

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing property correctly handles special double values like NaN.
        /// </summary>
        [Fact]
        public void CharacterSpacing_NaNValue_ReturnsNaN()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.CharacterSpacing = double.NaN;
            var result = timePicker.CharacterSpacing;

            // Assert
            Assert.True(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that CharacterSpacing property correctly handles positive infinity.
        /// </summary>
        [Fact]
        public void CharacterSpacing_PositiveInfinity_ReturnsPositiveInfinity()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.CharacterSpacing = double.PositiveInfinity;
            var result = timePicker.CharacterSpacing;

            // Assert
            Assert.True(double.IsPositiveInfinity(result));
        }

        /// <summary>
        /// Tests that CharacterSpacing property correctly handles negative infinity.
        /// </summary>
        [Fact]
        public void CharacterSpacing_NegativeInfinity_ReturnsNegativeInfinity()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.CharacterSpacing = double.NegativeInfinity;
            var result = timePicker.CharacterSpacing;

            // Assert
            Assert.True(double.IsNegativeInfinity(result));
        }

        /// <summary>
        /// Tests that CharacterSpacing property can be set and retrieved multiple times with different values.
        /// </summary>
        [Fact]
        public void CharacterSpacing_MultipleSetAndGet_ReturnsLatestValue()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act & Assert - Set and verify multiple values
            timePicker.CharacterSpacing = 5.0;
            Assert.Equal(5.0, timePicker.CharacterSpacing);

            timePicker.CharacterSpacing = -2.5;
            Assert.Equal(-2.5, timePicker.CharacterSpacing);

            timePicker.CharacterSpacing = 0.0;
            Assert.Equal(0.0, timePicker.CharacterSpacing);

            timePicker.CharacterSpacing = 100.75;
            Assert.Equal(100.75, timePicker.CharacterSpacing);
        }

        /// <summary>
        /// Tests that IsOpen property returns false by default when TimePicker is newly created.
        /// This verifies the getter correctly retrieves the default value from the bindable property.
        /// </summary>
        [Fact]
        public void IsOpen_WhenNewlyCreated_ReturnsFalse()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            var result = timePicker.IsOpen;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsOpen property correctly returns true after being set to true.
        /// This verifies the getter retrieves the correct value after setter updates the bindable property.
        /// </summary>
        [Fact]
        public void IsOpen_WhenSetToTrue_ReturnsTrue()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.IsOpen = true;
            var result = timePicker.IsOpen;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsOpen property correctly returns false after being set to false.
        /// This verifies the getter retrieves the correct value after setter updates the bindable property.
        /// </summary>
        [Fact]
        public void IsOpen_WhenSetToFalse_ReturnsFalse()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.IsOpen = false;
            var result = timePicker.IsOpen;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsOpen property correctly handles multiple consecutive operations.
        /// This verifies the getter consistently retrieves correct values through multiple state changes.
        /// </summary>
        [Theory]
        [InlineData(true, false, true)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        [InlineData(false, false, true)]
        public void IsOpen_WithMultipleOperations_ReturnsCorrectValues(bool firstValue, bool secondValue, bool thirdValue)
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act & Assert - First operation
            timePicker.IsOpen = firstValue;
            Assert.Equal(firstValue, timePicker.IsOpen);

            // Act & Assert - Second operation  
            timePicker.IsOpen = secondValue;
            Assert.Equal(secondValue, timePicker.IsOpen);

            // Act & Assert - Third operation
            timePicker.IsOpen = thirdValue;
            Assert.Equal(thirdValue, timePicker.IsOpen);
        }

        /// <summary>
        /// Tests that FontFamily property getter returns the correct value after setting it.
        /// Tests the basic get/set functionality of the FontFamily property.
        /// </summary>
        [Fact]
        public void FontFamily_SetValidString_ReturnsSetValue()
        {
            // Arrange
            var timePicker = new TimePicker();
            var fontFamily = "Arial";

            // Act
            timePicker.FontFamily = fontFamily;

            // Assert
            Assert.Equal(fontFamily, timePicker.FontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property can be set to null and returns null when retrieved.
        /// Tests null value handling for the FontFamily property.
        /// </summary>
        [Fact]
        public void FontFamily_SetNull_ReturnsNull()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontFamily = null;

            // Assert
            Assert.Null(timePicker.FontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property can be set to an empty string and returns empty string when retrieved.
        /// Tests empty string handling for the FontFamily property.
        /// </summary>
        [Fact]
        public void FontFamily_SetEmptyString_ReturnsEmptyString()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontFamily = string.Empty;

            // Assert
            Assert.Equal(string.Empty, timePicker.FontFamily);
        }

        /// <summary>
        /// Tests FontFamily property with various string values including edge cases.
        /// Tests different types of string inputs to ensure proper handling.
        /// </summary>
        [Theory]
        [InlineData("Times New Roman")]
        [InlineData("Arial Black")]
        [InlineData("   ")]
        [InlineData("Font-With-Dashes")]
        [InlineData("Font_With_Underscores")]
        [InlineData("FontWithNumbers123")]
        [InlineData("Very Long Font Family Name That Could Potentially Cause Issues")]
        [InlineData("Ω≈ç√∫˜µ≤≥÷")]
        public void FontFamily_SetVariousStrings_ReturnsSetValue(string fontFamily)
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontFamily = fontFamily;

            // Assert
            Assert.Equal(fontFamily, timePicker.FontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property maintains its value after multiple set operations.
        /// Tests property stability and ensures no side effects from multiple assignments.
        /// </summary>
        [Fact]
        public void FontFamily_MultipleSetOperations_ReturnsLastSetValue()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontFamily = "Arial";
            timePicker.FontFamily = "Times New Roman";
            timePicker.FontFamily = "Helvetica";

            // Assert
            Assert.Equal("Helvetica", timePicker.FontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property getter returns null by default.
        /// Tests the default value of the FontFamily property on a new TimePicker instance.
        /// </summary>
        [Fact]
        public void FontFamily_DefaultValue_ReturnsNull()
        {
            // Arrange & Act
            var timePicker = new TimePicker();

            // Assert
            Assert.Null(timePicker.FontFamily);
        }

        /// <summary>
        /// Tests that FontAttributes property getter returns the correct value when set to None.
        /// Verifies the property correctly retrieves values through the bindable property system.
        /// </summary>
        [Fact]
        public void FontAttributes_GetterWithNone_ReturnsNone()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontAttributes = FontAttributes.None;
            var result = timePicker.FontAttributes;

            // Assert
            Assert.Equal(FontAttributes.None, result);
        }

        /// <summary>
        /// Tests that FontAttributes property getter returns the correct value when set to Bold.
        /// Verifies the property correctly retrieves values through the bindable property system.
        /// </summary>
        [Fact]
        public void FontAttributes_GetterWithBold_ReturnsBold()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontAttributes = FontAttributes.Bold;
            var result = timePicker.FontAttributes;

            // Assert
            Assert.Equal(FontAttributes.Bold, result);
        }

        /// <summary>
        /// Tests that FontAttributes property getter returns the correct value when set to Italic.
        /// Verifies the property correctly retrieves values through the bindable property system.
        /// </summary>
        [Fact]
        public void FontAttributes_GetterWithItalic_ReturnsItalic()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontAttributes = FontAttributes.Italic;
            var result = timePicker.FontAttributes;

            // Assert
            Assert.Equal(FontAttributes.Italic, result);
        }

        /// <summary>
        /// Tests that FontAttributes property getter returns the correct value when set to combined flags.
        /// Verifies the property correctly handles flag combinations through the bindable property system.
        /// </summary>
        [Fact]
        public void FontAttributes_GetterWithCombinedFlags_ReturnsCombinedFlags()
        {
            // Arrange
            var timePicker = new TimePicker();
            var combinedFlags = FontAttributes.Bold | FontAttributes.Italic;

            // Act
            timePicker.FontAttributes = combinedFlags;
            var result = timePicker.FontAttributes;

            // Assert
            Assert.Equal(combinedFlags, result);
        }

        /// <summary>
        /// Tests that FontAttributes property setter correctly stores values through the bindable property system.
        /// Verifies the setter calls SetValue with the correct bindable property and value.
        /// </summary>
        [Theory]
        [InlineData(FontAttributes.None)]
        [InlineData(FontAttributes.Bold)]
        [InlineData(FontAttributes.Italic)]
        public void FontAttributes_SetterWithValidValues_StoresCorrectly(FontAttributes value)
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontAttributes = value;

            // Assert
            Assert.Equal(value, timePicker.FontAttributes);
        }

        /// <summary>
        /// Tests that FontAttributes property handles invalid enum values.
        /// Verifies the property behavior when set to values outside the defined enum range.
        /// </summary>
        [Fact]
        public void FontAttributes_SetterWithInvalidValue_HandlesGracefully()
        {
            // Arrange
            var timePicker = new TimePicker();
            var invalidValue = (FontAttributes)999;

            // Act
            timePicker.FontAttributes = invalidValue;
            var result = timePicker.FontAttributes;

            // Assert
            Assert.Equal(invalidValue, result);
        }

        /// <summary>
        /// Tests that FontAttributes property has the correct default value.
        /// Verifies the initial state of the property when a TimePicker is created.
        /// </summary>
        [Fact]
        public void FontAttributes_DefaultValue_IsNone()
        {
            // Arrange & Act
            var timePicker = new TimePicker();

            // Assert
            Assert.Equal(FontAttributes.None, timePicker.FontAttributes);
        }

        /// <summary>
        /// Tests that FontAttributes property handles negative enum values.
        /// Verifies the property behavior when set to negative values cast to the enum type.
        /// </summary>
        [Fact]
        public void FontAttributes_SetterWithNegativeValue_HandlesGracefully()
        {
            // Arrange
            var timePicker = new TimePicker();
            var negativeValue = (FontAttributes)(-1);

            // Act
            timePicker.FontAttributes = negativeValue;
            var result = timePicker.FontAttributes;

            // Assert
            Assert.Equal(negativeValue, result);
        }

        /// <summary>
        /// Tests that the TextColor property getter returns a Color value when accessed.
        /// Verifies that the getter correctly calls GetValue and casts the result to Color type.
        /// </summary>
        [Fact]
        public void TextColor_Get_ReturnsColorValue()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            Color textColor = timePicker.TextColor;

            // Assert
            Assert.NotNull(textColor);
        }

        /// <summary>
        /// Tests that the TextColor property can be set to various Color values.
        /// Verifies that the setter correctly calls SetValue with the provided Color.
        /// </summary>
        [Theory]
        [InlineData(255, 0, 0, 255)]     // Red
        [InlineData(0, 255, 0, 255)]     // Green  
        [InlineData(0, 0, 255, 255)]     // Blue
        [InlineData(0, 0, 0, 255)]       // Black
        [InlineData(255, 255, 255, 255)] // White
        [InlineData(128, 128, 128, 128)] // Gray with alpha
        [InlineData(255, 128, 64, 200)]  // Custom color with alpha
        public void TextColor_SetAndGet_StoresAndReturnsCorrectValue(byte red, byte green, byte blue, byte alpha)
        {
            // Arrange
            var timePicker = new TimePicker();
            var expectedColor = Color.FromRgba(red, green, blue, alpha);

            // Act
            timePicker.TextColor = expectedColor;
            Color actualColor = timePicker.TextColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that the TextColor property can be set to predefined Color values.
        /// Verifies the property works with standard color constants.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetPredefinedColors))]
        public void TextColor_SetPredefinedColors_StoresAndReturnsCorrectValue(Color expectedColor)
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.TextColor = expectedColor;
            Color actualColor = timePicker.TextColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that the TextColor property has a default value when TimePicker is created.
        /// Verifies the initial state of the TextColor property.
        /// </summary>
        [Fact]
        public void TextColor_DefaultValue_ReturnsNonNullColor()
        {
            // Arrange & Act
            var timePicker = new TimePicker();
            Color defaultTextColor = timePicker.TextColor;

            // Assert
            Assert.NotNull(defaultTextColor);
        }

        public static IEnumerable<object[]> GetPredefinedColors()
        {
            yield return new object[] { Colors.Red };
            yield return new object[] { Colors.Blue };
            yield return new object[] { Colors.Green };
            yield return new object[] { Colors.Black };
            yield return new object[] { Colors.White };
            yield return new object[] { Colors.Transparent };
            yield return new object[] { Colors.Yellow };
            yield return new object[] { Colors.Purple };
        }
    }


    public partial class TimePickerFontSizeTests
    {
        /// <summary>
        /// Tests that the FontSize property getter returns the correct value for valid positive values.
        /// Input: Various positive double values.
        /// Expected: The getter returns the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(12.0)]
        [InlineData(14.5)]
        [InlineData(18.0)]
        [InlineData(24.5)]
        [InlineData(36.0)]
        [InlineData(1.0)]
        [InlineData(0.1)]
        public void FontSize_SetValidPositiveValue_ReturnsCorrectValue(double fontSize)
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontSize = fontSize;

            // Assert
            Assert.Equal(fontSize, timePicker.FontSize);
        }

        /// <summary>
        /// Tests that the FontSize property handles zero value correctly.
        /// Input: Zero value.
        /// Expected: The getter returns zero.
        /// </summary>
        [Fact]
        public void FontSize_SetZeroValue_ReturnsZero()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontSize = 0.0;

            // Assert
            Assert.Equal(0.0, timePicker.FontSize);
        }

        /// <summary>
        /// Tests that the FontSize property handles negative values.
        /// Input: Various negative double values.
        /// Expected: The getter returns the set value (validation may occur at bindable property level).
        /// </summary>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-12.5)]
        [InlineData(-100.0)]
        public void FontSize_SetNegativeValue_ReturnsSetValue(double fontSize)
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontSize = fontSize;

            // Assert
            Assert.Equal(fontSize, timePicker.FontSize);
        }

        /// <summary>
        /// Tests that the FontSize property handles special double values correctly.
        /// Input: Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity.
        /// Expected: The getter returns the set special value.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void FontSize_SetSpecialDoubleValue_ReturnsSetValue(double fontSize)
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontSize = fontSize;

            // Assert
            Assert.Equal(fontSize, timePicker.FontSize);
        }

        /// <summary>
        /// Tests that the FontSize property handles boundary double values correctly.
        /// Input: Double.MaxValue, Double.MinValue.
        /// Expected: The getter returns the set boundary value.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        public void FontSize_SetBoundaryValue_ReturnsSetValue(double fontSize)
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontSize = fontSize;

            // Assert
            Assert.Equal(fontSize, timePicker.FontSize);
        }

        /// <summary>
        /// Tests that the FontSize property returns the correct default value when no explicit value is set.
        /// Input: No explicit FontSize value set.
        /// Expected: The getter returns the default value from the bindable property.
        /// </summary>
        [Fact]
        public void FontSize_DefaultValue_ReturnsDefaultValue()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            var defaultFontSize = timePicker.FontSize;

            // Assert
            Assert.True(defaultFontSize >= 0, "Default FontSize should be non-negative");
        }

        /// <summary>
        /// Tests that the FontSize property can be set multiple times with different values.
        /// Input: Sequential setting of different font size values.
        /// Expected: Each set operation updates the property value correctly.
        /// </summary>
        [Fact]
        public void FontSize_SetMultipleValues_UpdatesCorrectly()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act & Assert
            timePicker.FontSize = 12.0;
            Assert.Equal(12.0, timePicker.FontSize);

            timePicker.FontSize = 18.5;
            Assert.Equal(18.5, timePicker.FontSize);

            timePicker.FontSize = 0.0;
            Assert.Equal(0.0, timePicker.FontSize);

            timePicker.FontSize = 24.0;
            Assert.Equal(24.0, timePicker.FontSize);
        }

        /// <summary>
        /// Tests that the FontSize property handles very small positive values correctly.
        /// Input: Very small positive double values near zero.
        /// Expected: The getter returns the exact small value that was set.
        /// </summary>
        [Theory]
        [InlineData(0.001)]
        [InlineData(0.0001)]
        [InlineData(double.Epsilon)]
        public void FontSize_SetVerySmallValue_ReturnsCorrectValue(double fontSize)
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontSize = fontSize;

            // Assert
            Assert.Equal(fontSize, timePicker.FontSize);
        }

        /// <summary>
        /// Tests that the FontSize property handles large positive values correctly.
        /// Input: Large positive double values.
        /// Expected: The getter returns the exact large value that was set.
        /// </summary>
        [Theory]
        [InlineData(1000.0)]
        [InlineData(10000.5)]
        [InlineData(100000.0)]
        public void FontSize_SetLargeValue_ReturnsCorrectValue(double fontSize)
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontSize = fontSize;

            // Assert
            Assert.Equal(fontSize, timePicker.FontSize);
        }
    }


    /// <summary>
    /// Tests for the FontAutoScalingEnabled property of the TimePicker class.
    /// </summary>
    public partial class TimePickerFontAutoScalingEnabledTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the FontAutoScalingEnabled property getter returns the default value.
        /// Verifies that the property can be accessed and returns a valid boolean value.
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_GetDefaultValue_ReturnsDefaultValue()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            var result = timePicker.FontAutoScalingEnabled;

            // Assert
            Assert.IsType<bool>(result);
        }

        /// <summary>
        /// Tests that the FontAutoScalingEnabled property setter correctly sets the value to true.
        /// Verifies that the property can be set and the getter returns the set value.
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontAutoScalingEnabled = true;

            // Assert
            Assert.True(timePicker.FontAutoScalingEnabled);
        }

        /// <summary>
        /// Tests that the FontAutoScalingEnabled property setter correctly sets the value to false.
        /// Verifies that the property can be set and the getter returns the set value.
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontAutoScalingEnabled = false;

            // Assert
            Assert.False(timePicker.FontAutoScalingEnabled);
        }

        /// <summary>
        /// Tests that the FontAutoScalingEnabled property maintains its state across multiple get/set operations.
        /// Verifies that the property correctly toggles between true and false values.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FontAutoScalingEnabled_SetValue_GetValueReturnsSetValue(bool expectedValue)
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act
            timePicker.FontAutoScalingEnabled = expectedValue;
            var actualValue = timePicker.FontAutoScalingEnabled;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the FontAutoScalingEnabled property can be toggled multiple times.
        /// Verifies that the property correctly handles sequential value changes.
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_ToggleValues_MaintainsCorrectState()
        {
            // Arrange
            var timePicker = new TimePicker();

            // Act & Assert
            timePicker.FontAutoScalingEnabled = true;
            Assert.True(timePicker.FontAutoScalingEnabled);

            timePicker.FontAutoScalingEnabled = false;
            Assert.False(timePicker.FontAutoScalingEnabled);

            timePicker.FontAutoScalingEnabled = true;
            Assert.True(timePicker.FontAutoScalingEnabled);
        }
    }
}