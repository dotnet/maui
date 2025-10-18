#nullable disable

using System;
using System.ComponentModel;
using System.Reflection;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class SpanTests : BaseTestFixture
    {
        [Fact]
        public void StyleApplied()
        {
            var pinkStyle = new Style(typeof(Span))
            {
                Setters = {
                    new Setter { Property = Span.TextColorProperty, Value = Colors.Pink },
                },
                Class = "pink",
                ApplyToDerivedTypes = true,
            };

            var span = new Span
            {
                Style = pinkStyle
            };

            var formattedText = new FormattedString();
            formattedText.Spans.Add(span);

            var label = new Label()
            {
                FormattedText = formattedText
            };

            new ContentView
            {
                Resources = new ResourceDictionary { pinkStyle },
                Content = label
            };

            Assert.Equal(Colors.Pink, span.TextColor);
        }

        [Fact]
        public void BindingApplied()
        {
            var vm = new ViewModel()
            {
                Text = "CheckBindingWorked"
            };

            var formattedText = new FormattedString();

            var label = new Label()
            {
                FormattedText = formattedText
            };

            var span = new Span();
            span.SetBinding(Span.TextProperty, "Text");

            formattedText.Spans.Add(span);

            label.BindingContext = vm;

            Assert.Equal(vm.Text, span.Text);
        }

        class ViewModel
        {
            public string Text { get; set; }
        }

        /// <summary>
        /// Tests that the BackgroundColor property getter returns the default value when not explicitly set.
        /// Verifies the default transparent color behavior.
        /// </summary>
        [Fact]
        public void BackgroundColor_DefaultValue_ReturnsTransparent()
        {
            // Arrange
            var span = new Span();

            // Act
            var backgroundColor = span.BackgroundColor;

            // Assert
            Assert.Equal(Colors.Transparent, backgroundColor);
        }

        /// <summary>
        /// Tests that the BackgroundColor property setter and getter work correctly with various color values.
        /// Verifies that different color values can be set and retrieved properly.
        /// </summary>
        [Theory]
        [InlineData(255, 0, 0, 255)] // Red
        [InlineData(0, 255, 0, 255)] // Green  
        [InlineData(0, 0, 255, 255)] // Blue
        [InlineData(255, 255, 255, 255)] // White
        [InlineData(0, 0, 0, 255)] // Black
        [InlineData(128, 128, 128, 128)] // Semi-transparent gray
        [InlineData(0, 0, 0, 0)] // Transparent
        [InlineData(255, 255, 255, 0)] // Fully transparent white
        public void BackgroundColor_SetAndGet_ReturnsCorrectValue(byte red, byte green, byte blue, byte alpha)
        {
            // Arrange
            var span = new Span();
            var expectedColor = Color.FromRgba(red, green, blue, alpha);

            // Act
            span.BackgroundColor = expectedColor;
            var actualColor = span.BackgroundColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that the BackgroundColor property works with predefined color constants.
        /// Verifies that common color constants can be set and retrieved correctly.
        /// </summary>
        [Theory]
        [InlineData("Red")]
        [InlineData("Green")]
        [InlineData("Blue")]
        [InlineData("White")]
        [InlineData("Black")]
        [InlineData("Transparent")]
        [InlineData("Pink")]
        [InlineData("Yellow")]
        public void BackgroundColor_SetPredefinedColors_ReturnsCorrectValue(string colorName)
        {
            // Arrange
            var span = new Span();
            var expectedColor = GetColorByName(colorName);

            // Act
            span.BackgroundColor = expectedColor;
            var actualColor = span.BackgroundColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that the BackgroundColor property works with floating-point RGBA values at boundary conditions.
        /// Verifies edge cases with minimum, maximum, and intermediate floating-point values.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // All minimum values
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // All maximum values
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // All middle values
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green with full alpha
        [InlineData(1.0f, 0.0f, 0.0f, 0.0f)] // Red with no alpha
        public void BackgroundColor_SetFloatRgbaValues_ReturnsCorrectValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var span = new Span();
            var expectedColor = Color.FromRgba(red, green, blue, alpha);

            // Act
            span.BackgroundColor = expectedColor;
            var actualColor = span.BackgroundColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that the BackgroundColor property change triggers proper binding updates.
        /// Verifies that property change notifications work correctly.
        /// </summary>
        [Fact]
        public void BackgroundColor_PropertyChanged_TriggersNotification()
        {
            // Arrange
            var span = new Span();
            bool propertyChanged = false;
            string changedPropertyName = null;

            span.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(Span.BackgroundColor))
                {
                    propertyChanged = true;
                    changedPropertyName = e.PropertyName;
                }
            };

            // Act
            span.BackgroundColor = Colors.Red;

            // Assert
            Assert.True(propertyChanged);
            Assert.Equal(nameof(Span.BackgroundColor), changedPropertyName);
        }

        /// <summary>
        /// Tests that multiple BackgroundColor changes work correctly.
        /// Verifies that the property can be changed multiple times and retains the latest value.
        /// </summary>
        [Fact]
        public void BackgroundColor_MultipleChanges_RetainsLatestValue()
        {
            // Arrange
            var span = new Span();

            // Act & Assert
            span.BackgroundColor = Colors.Red;
            Assert.Equal(Colors.Red, span.BackgroundColor);

            span.BackgroundColor = Colors.Green;
            Assert.Equal(Colors.Green, span.BackgroundColor);

            span.BackgroundColor = Colors.Blue;
            Assert.Equal(Colors.Blue, span.BackgroundColor);

            span.BackgroundColor = Colors.Transparent;
            Assert.Equal(Colors.Transparent, span.BackgroundColor);
        }

        private static Color GetColorByName(string colorName)
        {
            return colorName switch
            {
                "Red" => Colors.Red,
                "Green" => Colors.Green,
                "Blue" => Colors.Blue,
                "White" => Colors.White,
                "Black" => Colors.Black,
                "Transparent" => Colors.Transparent,
                "Pink" => Colors.Pink,
                "Yellow" => Colors.Yellow,
                _ => Colors.Transparent
            };
        }

        /// <summary>
        /// Tests that CharacterSpacing property returns the default value of 0.0 when accessed on a new Span instance.
        /// Validates the getter implementation and default BindableProperty behavior.
        /// </summary>
        [Fact]
        public void CharacterSpacing_DefaultValue_ReturnsZero()
        {
            // Arrange
            var span = new Span();

            // Act
            var result = span.CharacterSpacing;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing property correctly sets and retrieves various double values.
        /// Validates both getter and setter implementations with normal, boundary, and special double values.
        /// </summary>
        /// <param name="value">The character spacing value to test</param>
        /// <param name="expectedValue">The expected value to be returned by the getter</param>
        [Theory]
        [InlineData(1.5, 1.5)]
        [InlineData(0.0, 0.0)]
        [InlineData(-2.5, -2.5)]
        [InlineData(100.0, 100.0)]
        [InlineData(0.1, 0.1)]
        [InlineData(-0.1, -0.1)]
        public void CharacterSpacing_SetValidValues_ReturnsExpectedValue(double value, double expectedValue)
        {
            // Arrange
            var span = new Span();

            // Act
            span.CharacterSpacing = value;
            var result = span.CharacterSpacing;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing property handles extreme double values correctly.
        /// Validates behavior with boundary values like MaxValue and MinValue.
        /// </summary>
        /// <param name="extremeValue">The extreme double value to test</param>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        public void CharacterSpacing_SetExtremeValues_HandlesCorrectly(double extremeValue)
        {
            // Arrange
            var span = new Span();

            // Act
            span.CharacterSpacing = extremeValue;
            var result = span.CharacterSpacing;

            // Assert
            Assert.Equal(extremeValue, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing property handles special double values like infinity and NaN.
        /// Validates the property behavior with non-finite double values.
        /// </summary>
        [Fact]
        public void CharacterSpacing_SetPositiveInfinity_ReturnsPositiveInfinity()
        {
            // Arrange
            var span = new Span();

            // Act
            span.CharacterSpacing = double.PositiveInfinity;
            var result = span.CharacterSpacing;

            // Assert
            Assert.Equal(double.PositiveInfinity, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing property handles negative infinity correctly.
        /// Validates the property behavior with negative infinite values.
        /// </summary>
        [Fact]
        public void CharacterSpacing_SetNegativeInfinity_ReturnsNegativeInfinity()
        {
            // Arrange
            var span = new Span();

            // Act
            span.CharacterSpacing = double.NegativeInfinity;
            var result = span.CharacterSpacing;

            // Assert
            Assert.Equal(double.NegativeInfinity, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing property handles NaN (Not-a-Number) values correctly.
        /// Validates the property behavior with invalid numeric values.
        /// </summary>
        [Fact]
        public void CharacterSpacing_SetNaN_ReturnsNaN()
        {
            // Arrange
            var span = new Span();

            // Act
            span.CharacterSpacing = double.NaN;
            var result = span.CharacterSpacing;

            // Assert
            Assert.True(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that CharacterSpacing property can be set multiple times with different values.
        /// Validates that the setter correctly overwrites previous values.
        /// </summary>
        [Fact]
        public void CharacterSpacing_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var span = new Span();

            // Act
            span.CharacterSpacing = 1.0;
            span.CharacterSpacing = 2.5;
            span.CharacterSpacing = -1.5;
            var result = span.CharacterSpacing;

            // Assert
            Assert.Equal(-1.5, result);
        }

        /// <summary>
        /// Tests that the TextTransform getter returns None when set to None.
        /// Verifies the casting behavior from GetValue to TextTransform enum.
        /// </summary>
        [Fact]
        public void TextTransform_GetNone_ReturnsNone()
        {
            // Arrange
            var span = new Span();

            // Act
            span.TextTransform = TextTransform.None;
            var result = span.TextTransform;

            // Assert
            Assert.Equal(TextTransform.None, result);
        }

        /// <summary>
        /// Tests that the TextTransform getter returns Default when set to Default.
        /// Verifies the casting behavior from GetValue to TextTransform enum.
        /// </summary>
        [Fact]
        public void TextTransform_GetDefault_ReturnsDefault()
        {
            // Arrange
            var span = new Span();

            // Act
            span.TextTransform = TextTransform.Default;
            var result = span.TextTransform;

            // Assert
            Assert.Equal(TextTransform.Default, result);
        }

        /// <summary>
        /// Tests that the TextTransform getter returns Lowercase when set to Lowercase.
        /// Verifies the casting behavior from GetValue to TextTransform enum.
        /// </summary>
        [Fact]
        public void TextTransform_GetLowercase_ReturnsLowercase()
        {
            // Arrange
            var span = new Span();

            // Act
            span.TextTransform = TextTransform.Lowercase;
            var result = span.TextTransform;

            // Assert
            Assert.Equal(TextTransform.Lowercase, result);
        }

        /// <summary>
        /// Tests that the TextTransform getter returns Uppercase when set to Uppercase.
        /// Verifies the casting behavior from GetValue to TextTransform enum.
        /// </summary>
        [Fact]
        public void TextTransform_GetUppercase_ReturnsUppercase()
        {
            // Arrange
            var span = new Span();

            // Act
            span.TextTransform = TextTransform.Uppercase;
            var result = span.TextTransform;

            // Assert
            Assert.Equal(TextTransform.Uppercase, result);
        }

        /// <summary>
        /// Tests the TextTransform getter with all valid enum values using parameterized testing.
        /// Verifies the round-trip behavior and casting from GetValue for each enum value.
        /// </summary>
        /// <param name="textTransform">The TextTransform enum value to test</param>
        [Theory]
        [InlineData(TextTransform.None)]
        [InlineData(TextTransform.Default)]
        [InlineData(TextTransform.Lowercase)]
        [InlineData(TextTransform.Uppercase)]
        public void TextTransform_GetValidValues_ReturnsExpectedValue(TextTransform textTransform)
        {
            // Arrange
            var span = new Span();

            // Act
            span.TextTransform = textTransform;
            var result = span.TextTransform;

            // Assert
            Assert.Equal(textTransform, result);
        }

        /// <summary>
        /// Tests that the TextTransform getter handles invalid enum values by casting them.
        /// Verifies the casting behavior from GetValue when an out-of-range enum value is used.
        /// </summary>
        [Fact]
        public void TextTransform_GetInvalidEnumValue_ReturnsCastedValue()
        {
            // Arrange
            var span = new Span();
            var invalidEnumValue = (TextTransform)999;

            // Act
            span.TextTransform = invalidEnumValue;
            var result = span.TextTransform;

            // Assert
            Assert.Equal(invalidEnumValue, result);
        }

        /// <summary>
        /// Tests that the TextTransform getter works with negative enum values.
        /// Verifies the casting behavior from GetValue with negative integer values.
        /// </summary>
        [Fact]
        public void TextTransform_GetNegativeEnumValue_ReturnsCastedValue()
        {
            // Arrange
            var span = new Span();
            var negativeEnumValue = (TextTransform)(-1);

            // Act
            span.TextTransform = negativeEnumValue;
            var result = span.TextTransform;

            // Assert
            Assert.Equal(negativeEnumValue, result);
        }

        /// <summary>
        /// Tests that the TextTransform getter works with boundary enum values.
        /// Verifies the casting behavior at integer boundaries for the enum.
        /// </summary>
        /// <param name="enumValue">The boundary enum value to test</param>
        [Theory]
        [InlineData((TextTransform)int.MinValue)]
        [InlineData((TextTransform)int.MaxValue)]
        [InlineData((TextTransform)0)]
        [InlineData((TextTransform)100)]
        [InlineData((TextTransform)(-100))]
        public void TextTransform_GetBoundaryValues_ReturnsCastedValue(TextTransform enumValue)
        {
            // Arrange
            var span = new Span();

            // Act
            span.TextTransform = enumValue;
            var result = span.TextTransform;

            // Assert
            Assert.Equal(enumValue, result);
        }

        /// <summary>
        /// Tests that UpdateFormsText correctly delegates to TextTransformUtilities.GetTransformedText 
        /// and returns the expected transformed text for valid TextTransform enum values with various string inputs.
        /// </summary>
        /// <param name="source">The input string to transform</param>
        /// <param name="textTransform">The TextTransform enum value to apply</param>
        /// <param name="expected">The expected transformed output</param>
        [Theory]
        [InlineData("Hello World", TextTransform.None, "Hello World")]
        [InlineData("Hello World", TextTransform.Default, "Hello World")]
        [InlineData("Hello World", TextTransform.Lowercase, "hello world")]
        [InlineData("Hello World", TextTransform.Uppercase, "HELLO WORLD")]
        [InlineData("", TextTransform.None, "")]
        [InlineData("", TextTransform.Lowercase, "")]
        [InlineData("", TextTransform.Uppercase, "")]
        [InlineData("   ", TextTransform.Lowercase, "   ")]
        [InlineData("123!@#", TextTransform.Uppercase, "123!@#")]
        [InlineData("MiXeD CaSe", TextTransform.Lowercase, "mixed case")]
        [InlineData("lower case", TextTransform.Uppercase, "LOWER CASE")]
        [InlineData("Ñoño", TextTransform.Lowercase, "ñoño")]
        [InlineData("ñoño", TextTransform.Uppercase, "ÑOÑO")]
        public void UpdateFormsText_ValidInputs_ReturnsExpectedTransformation(string source, TextTransform textTransform, string expected)
        {
            // Arrange
            var span = new Span();

            // Act
            var result = span.UpdateFormsText(source, textTransform);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that UpdateFormsText handles null source input correctly by returning an empty string.
        /// </summary>
        /// <param name="textTransform">The TextTransform enum value to apply</param>
        [Theory]
        [InlineData(TextTransform.None)]
        [InlineData(TextTransform.Default)]
        [InlineData(TextTransform.Lowercase)]
        [InlineData(TextTransform.Uppercase)]
        public void UpdateFormsText_NullSource_ReturnsEmptyString(TextTransform textTransform)
        {
            // Arrange
            var span = new Span();

            // Act
            var result = span.UpdateFormsText(null, textTransform);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that UpdateFormsText handles whitespace-only strings correctly for various TextTransform values.
        /// </summary>
        /// <param name="source">The whitespace-only input string</param>
        /// <param name="textTransform">The TextTransform enum value to apply</param>
        [Theory]
        [InlineData("\t", TextTransform.None)]
        [InlineData("\n", TextTransform.Lowercase)]
        [InlineData("\r\n", TextTransform.Uppercase)]
        [InlineData("  \t  ", TextTransform.Default)]
        public void UpdateFormsText_WhitespaceSource_PreservesWhitespace(string source, TextTransform textTransform)
        {
            // Arrange
            var span = new Span();

            // Act
            var result = span.UpdateFormsText(source, textTransform);

            // Assert
            Assert.Equal(source, result);
        }

        /// <summary>
        /// Tests that UpdateFormsText handles special characters and symbols correctly.
        /// </summary>
        /// <param name="source">The input string containing special characters</param>
        /// <param name="textTransform">The TextTransform enum value to apply</param>
        /// <param name="expected">The expected output</param>
        [Theory]
        [InlineData("!@#$%^&*()", TextTransform.Lowercase, "!@#$%^&*()")]
        [InlineData("abc123XYZ", TextTransform.Uppercase, "ABC123XYZ")]
        [InlineData("emoji🔥test", TextTransform.Lowercase, "emoji🔥test")]
        [InlineData("EMOJI🔥TEST", TextTransform.Lowercase, "emoji🔥test")]
        public void UpdateFormsText_SpecialCharacters_HandlesCorrectly(string source, TextTransform textTransform, string expected)
        {
            // Arrange
            var span = new Span();

            // Act
            var result = span.UpdateFormsText(source, textTransform);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that UpdateFormsText handles invalid TextTransform enum values by treating them as default (no transformation).
        /// </summary>
        /// <param name="invalidEnumValue">An invalid TextTransform enum value cast from integer</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(99)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void UpdateFormsText_InvalidTextTransform_TreatsAsDefault(int invalidEnumValue)
        {
            // Arrange
            var span = new Span();
            var source = "Test String";
            var invalidTransform = (TextTransform)invalidEnumValue;

            // Act
            var result = span.UpdateFormsText(source, invalidTransform);

            // Assert
            Assert.Equal(source, result);
        }

        /// <summary>
        /// Tests that UpdateFormsText correctly handles very long strings without issues.
        /// </summary>
        [Fact]
        public void UpdateFormsText_VeryLongString_ProcessesCorrectly()
        {
            // Arrange
            var span = new Span();
            var longString = new string('A', 10000) + new string('b', 10000);
            var expectedLower = new string('a', 10000) + new string('b', 10000);
            var expectedUpper = new string('A', 20000);

            // Act
            var resultLower = span.UpdateFormsText(longString, TextTransform.Lowercase);
            var resultUpper = span.UpdateFormsText(longString, TextTransform.Uppercase);
            var resultNone = span.UpdateFormsText(longString, TextTransform.None);

            // Assert
            Assert.Equal(expectedLower, resultLower);
            Assert.Equal(expectedUpper, resultUpper);
            Assert.Equal(longString, resultNone);
        }

        /// <summary>
        /// Tests that UpdateFormsText handles numeric boundaries correctly with text transformations.
        /// </summary>
        /// <param name="textTransform">The TextTransform enum value at boundary</param>
        [Theory]
        [InlineData((TextTransform)0)] // TextTransform.None
        [InlineData((TextTransform)1)] // TextTransform.Default
        [InlineData((TextTransform)2)] // TextTransform.Lowercase  
        [InlineData((TextTransform)3)] // TextTransform.Uppercase
        public void UpdateFormsText_EnumBoundaryValues_HandlesCorrectly(TextTransform textTransform)
        {
            // Arrange
            var span = new Span();
            var source = "Boundary Test";

            // Act
            var result = span.UpdateFormsText(source, textTransform);

            // Assert
            Assert.NotNull(result);
            switch (textTransform)
            {
                case TextTransform.None:
                case TextTransform.Default:
                    Assert.Equal(source, result);
                    break;
                case TextTransform.Lowercase:
                    Assert.Equal("boundary test", result);
                    break;
                case TextTransform.Uppercase:
                    Assert.Equal("BOUNDARY TEST", result);
                    break;
            }
        }

        /// <summary>
        /// Tests that the FontFamily property getter returns the default value when not explicitly set.
        /// Verifies that the property correctly delegates to GetValue with FontElement.FontFamilyProperty.
        /// Expected result: Returns null as the default font family value.
        /// </summary>
        [Fact]
        public void FontFamily_Get_ReturnsDefaultValue()
        {
            // Arrange
            var span = new Span();

            // Act
            var result = span.FontFamily;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the FontFamily property can be set and retrieved with various string values.
        /// Verifies that the property correctly delegates to SetValue and GetValue with FontElement.FontFamilyProperty.
        /// Expected result: The getter returns the same value that was set.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Arial")]
        [InlineData("Times New Roman")]
        [InlineData("Helvetica")]
        [InlineData("Comic Sans MS")]
        [InlineData("Georgia")]
        [InlineData("Verdana")]
        public void FontFamily_SetAndGet_ReturnsSetValue(string fontFamily)
        {
            // Arrange
            var span = new Span();

            // Act
            span.FontFamily = fontFamily;
            var result = span.FontFamily;

            // Assert
            Assert.Equal(fontFamily, result);
        }

        /// <summary>
        /// Tests that the FontFamily property handles whitespace-only strings correctly.
        /// Verifies that whitespace values are preserved as-is.
        /// Expected result: The getter returns the exact whitespace string that was set.
        /// </summary>
        [Theory]
        [InlineData(" ")]
        [InlineData("  ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("   \t  \n  ")]
        public void FontFamily_SetWhitespace_ReturnsWhitespaceValue(string whitespaceValue)
        {
            // Arrange
            var span = new Span();

            // Act
            span.FontFamily = whitespaceValue;
            var result = span.FontFamily;

            // Assert
            Assert.Equal(whitespaceValue, result);
        }

        /// <summary>
        /// Tests that the FontFamily property handles special characters and long strings correctly.
        /// Verifies that the property can store and retrieve various edge case string values.
        /// Expected result: The getter returns the exact string that was set, regardless of special characters or length.
        /// </summary>
        [Theory]
        [InlineData("Font-Name-With-Hyphens")]
        [InlineData("Font_Name_With_Underscores")]
        [InlineData("FontNameWithNumbers123")]
        [InlineData("@#$%^&*()")]
        [InlineData("Font Name With Spaces")]
        [InlineData("Very Long Font Family Name That Exceeds Normal Length Expectations")]
        public void FontFamily_SetSpecialValues_ReturnsSpecialValues(string specialValue)
        {
            // Arrange
            var span = new Span();

            // Act
            span.FontFamily = specialValue;
            var result = span.FontFamily;

            // Assert
            Assert.Equal(specialValue, result);
        }

        /// <summary>
        /// Tests that multiple consecutive sets and gets of FontFamily work correctly.
        /// Verifies that the property maintains its state correctly across multiple operations.
        /// Expected result: Each get operation returns the value from the most recent set operation.
        /// </summary>
        [Fact]
        public void FontFamily_MultipleSetAndGet_MaintainsCorrectState()
        {
            // Arrange
            var span = new Span();
            var firstValue = "Arial";
            var secondValue = "Helvetica";
            var thirdValue = null;

            // Act & Assert
            span.FontFamily = firstValue;
            Assert.Equal(firstValue, span.FontFamily);

            span.FontFamily = secondValue;
            Assert.Equal(secondValue, span.FontFamily);

            span.FontFamily = thirdValue;
            Assert.Equal(thirdValue, span.FontFamily);
        }

        /// <summary>
        /// Tests that the FontAttributes property returns the default value (None) when no value is explicitly set.
        /// Verifies the getter retrieves the correct default FontAttributes value from the underlying BindableProperty.
        /// </summary>
        [Fact]
        public void FontAttributes_DefaultValue_ReturnsNone()
        {
            // Arrange
            var span = new Span();

            // Act
            var result = span.FontAttributes;

            // Assert
            Assert.Equal(FontAttributes.None, result);
        }

        /// <summary>
        /// Tests that the FontAttributes property correctly sets and gets various FontAttributes enum values.
        /// Verifies both setter and getter work correctly with valid enum values including None, Bold, Italic, and combined flags.
        /// </summary>
        /// <param name="fontAttributes">The FontAttributes value to test</param>
        [Theory]
        [InlineData(FontAttributes.None)]
        [InlineData(FontAttributes.Bold)]
        [InlineData(FontAttributes.Italic)]
        [InlineData(FontAttributes.Bold | FontAttributes.Italic)]
        public void FontAttributes_SetValidValues_ReturnsCorrectValue(FontAttributes fontAttributes)
        {
            // Arrange
            var span = new Span();

            // Act
            span.FontAttributes = fontAttributes;
            var result = span.FontAttributes;

            // Assert
            Assert.Equal(fontAttributes, result);
        }

        /// <summary>
        /// Tests that the FontAttributes property can handle invalid enum values by casting integers outside the defined enum range.
        /// Verifies the getter returns the cast value even when it represents an undefined enum value.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void FontAttributes_SetInvalidEnumValues_ReturnsSetValue(int invalidEnumValue)
        {
            // Arrange
            var span = new Span();
            var invalidFontAttributes = (FontAttributes)invalidEnumValue;

            // Act
            span.FontAttributes = invalidFontAttributes;
            var result = span.FontAttributes;

            // Assert
            Assert.Equal(invalidFontAttributes, result);
        }

        /// <summary>
        /// Tests that setting FontAttributes to the same value multiple times maintains consistency.
        /// Verifies the property getter consistently returns the last set value.
        /// </summary>
        [Fact]
        public void FontAttributes_SetSameValueMultipleTimes_ReturnsConsistentValue()
        {
            // Arrange
            var span = new Span();
            var expectedValue = FontAttributes.Bold | FontAttributes.Italic;

            // Act
            span.FontAttributes = expectedValue;
            span.FontAttributes = expectedValue;
            span.FontAttributes = expectedValue;
            var result = span.FontAttributes;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that FontAttributes property changes are reflected immediately when switching between different values.
        /// Verifies the getter returns the most recently set value correctly.
        /// </summary>
        [Fact]
        public void FontAttributes_ChangeValueSequentially_ReturnsLatestValue()
        {
            // Arrange
            var span = new Span();

            // Act & Assert - Test sequence of value changes
            span.FontAttributes = FontAttributes.Bold;
            Assert.Equal(FontAttributes.Bold, span.FontAttributes);

            span.FontAttributes = FontAttributes.Italic;
            Assert.Equal(FontAttributes.Italic, span.FontAttributes);

            span.FontAttributes = FontAttributes.Bold | FontAttributes.Italic;
            Assert.Equal(FontAttributes.Bold | FontAttributes.Italic, span.FontAttributes);

            span.FontAttributes = FontAttributes.None;
            Assert.Equal(FontAttributes.None, span.FontAttributes);
        }

        /// <summary>
        /// Tests that the FontSize property returns the default value when no value is explicitly set.
        /// Verifies the getter retrieves the correct default value from the bindable property.
        /// </summary>
        [Fact]
        public void FontSize_DefaultValue_ReturnsZero()
        {
            // Arrange
            var span = new Span();

            // Act
            var fontSize = span.FontSize;

            // Assert
            Assert.Equal(0.0, fontSize);
        }

        /// <summary>
        /// Tests that the FontSize property correctly sets and gets various valid double values.
        /// Verifies the getter returns the exact value that was set by the setter.
        /// </summary>
        /// <param name="value">The font size value to test</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(12.0)]
        [InlineData(16.0)]
        [InlineData(100.0)]
        [InlineData(999.99)]
        public void FontSize_SetValidValue_ReturnsSetValue(double value)
        {
            // Arrange
            var span = new Span();

            // Act
            span.FontSize = value;
            var retrievedValue = span.FontSize;

            // Assert
            Assert.Equal(value, retrievedValue);
        }

        /// <summary>
        /// Tests that the FontSize property correctly handles negative values.
        /// Verifies the getter returns negative values that were set by the setter.
        /// </summary>
        /// <param name="value">The negative font size value to test</param>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-10.0)]
        [InlineData(-100.0)]
        public void FontSize_SetNegativeValue_ReturnsSetValue(double value)
        {
            // Arrange
            var span = new Span();

            // Act
            span.FontSize = value;
            var retrievedValue = span.FontSize;

            // Assert
            Assert.Equal(value, retrievedValue);
        }

        /// <summary>
        /// Tests that the FontSize property correctly handles extreme double values.
        /// Verifies the getter returns extreme boundary values that were set by the setter.
        /// </summary>
        /// <param name="value">The extreme double value to test</param>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        public void FontSize_SetExtremeValue_ReturnsSetValue(double value)
        {
            // Arrange
            var span = new Span();

            // Act
            span.FontSize = value;
            var retrievedValue = span.FontSize;

            // Assert
            Assert.Equal(value, retrievedValue);
        }

        /// <summary>
        /// Tests that the FontSize property correctly handles special double values like NaN and infinity.
        /// Verifies the getter returns special values that were set by the setter.
        /// </summary>
        /// <param name="value">The special double value to test</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void FontSize_SetSpecialDoubleValue_ReturnsSetValue(double value)
        {
            // Arrange
            var span = new Span();

            // Act
            span.FontSize = value;
            var retrievedValue = span.FontSize;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(retrievedValue));
            }
            else
            {
                Assert.Equal(value, retrievedValue);
            }
        }

        /// <summary>
        /// Tests that multiple FontSize assignments work correctly.
        /// Verifies the getter always returns the most recently set value.
        /// </summary>
        [Fact]
        public void FontSize_MultipleAssignments_ReturnsLatestValue()
        {
            // Arrange
            var span = new Span();

            // Act & Assert
            span.FontSize = 10.0;
            Assert.Equal(10.0, span.FontSize);

            span.FontSize = 20.0;
            Assert.Equal(20.0, span.FontSize);

            span.FontSize = 0.0;
            Assert.Equal(0.0, span.FontSize);

            span.FontSize = -5.0;
            Assert.Equal(-5.0, span.FontSize);
        }

        /// <summary>
        /// Tests that the FontAutoScalingEnabled property returns the default value when not explicitly set.
        /// Verifies that the getter correctly retrieves the value from the underlying bindable property.
        /// Expected result: Should return the default value (typically true for font auto-scaling).
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_DefaultValue_ReturnsExpectedDefault()
        {
            // Arrange
            var span = new Span();

            // Act
            var result = span.FontAutoScalingEnabled;

            // Assert
            Assert.True(result); // Default is typically true for font auto-scaling
        }

        /// <summary>
        /// Tests that the FontAutoScalingEnabled property can be set to true and retrieved correctly.
        /// Verifies both the setter and getter functionality for the true value.
        /// Expected result: Property should return true after being set to true.
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var span = new Span();

            // Act
            span.FontAutoScalingEnabled = true;
            var result = span.FontAutoScalingEnabled;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the FontAutoScalingEnabled property can be set to false and retrieved correctly.
        /// Verifies both the setter and getter functionality for the false value.
        /// Expected result: Property should return false after being set to false.
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var span = new Span();

            // Act
            span.FontAutoScalingEnabled = false;
            var result = span.FontAutoScalingEnabled;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the FontAutoScalingEnabled property can be toggled between true and false values.
        /// Verifies that the property correctly maintains its state after multiple value changes.
        /// Expected result: Property should correctly reflect each assigned value.
        /// </summary>
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void FontAutoScalingEnabled_SetMultipleValues_ReturnsCorrectValue(bool firstValue, bool secondValue)
        {
            // Arrange
            var span = new Span();

            // Act
            span.FontAutoScalingEnabled = firstValue;
            var firstResult = span.FontAutoScalingEnabled;

            span.FontAutoScalingEnabled = secondValue;
            var secondResult = span.FontAutoScalingEnabled;

            // Assert
            Assert.Equal(firstValue, firstResult);
            Assert.Equal(secondValue, secondResult);
        }

        /// <summary>
        /// Tests that the FontAutoScalingEnabled property uses the correct bindable property.
        /// Verifies that the property is backed by FontAutoScalingEnabledProperty.
        /// Expected result: Direct property access and GetValue should return the same value.
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_UsesCorrectBindableProperty_ConsistentWithGetValue()
        {
            // Arrange
            var span = new Span();
            span.FontAutoScalingEnabled = false;

            // Act
            var propertyValue = span.FontAutoScalingEnabled;
            var bindableValue = (bool)span.GetValue(Span.FontAutoScalingEnabledProperty);

            // Assert
            Assert.Equal(propertyValue, bindableValue);
        }

        /// <summary>
        /// Tests that the TextDecorations property getter returns the correct value when set to None.
        /// Verifies that the property correctly retrieves the value from the underlying BindableProperty.
        /// </summary>
        [Fact]
        public void TextDecorations_WhenSetToNone_ReturnsNone()
        {
            // Arrange
            var span = new Span();

            // Act
            span.TextDecorations = TextDecorations.None;
            var result = span.TextDecorations;

            // Assert
            Assert.Equal(TextDecorations.None, result);
        }

        /// <summary>
        /// Tests that the TextDecorations property getter returns the correct value when set to Underline.
        /// Verifies that the property correctly retrieves the value from the underlying BindableProperty.
        /// </summary>
        [Fact]
        public void TextDecorations_WhenSetToUnderline_ReturnsUnderline()
        {
            // Arrange
            var span = new Span();

            // Act
            span.TextDecorations = TextDecorations.Underline;
            var result = span.TextDecorations;

            // Assert
            Assert.Equal(TextDecorations.Underline, result);
        }

        /// <summary>
        /// Tests that the TextDecorations property getter returns the correct value when set to Strikethrough.
        /// Verifies that the property correctly retrieves the value from the underlying BindableProperty.
        /// </summary>
        [Fact]
        public void TextDecorations_WhenSetToStrikethrough_ReturnsStrikethrough()
        {
            // Arrange
            var span = new Span();

            // Act
            span.TextDecorations = TextDecorations.Strikethrough;
            var result = span.TextDecorations;

            // Assert
            Assert.Equal(TextDecorations.Strikethrough, result);
        }

        /// <summary>
        /// Tests that the TextDecorations property getter returns the correct value when set to combined flags.
        /// Verifies that the property correctly handles flag combinations (Underline | Strikethrough).
        /// </summary>
        [Fact]
        public void TextDecorations_WhenSetToCombinedFlags_ReturnsCombinedFlags()
        {
            // Arrange
            var span = new Span();
            var combinedDecorations = TextDecorations.Underline | TextDecorations.Strikethrough;

            // Act
            span.TextDecorations = combinedDecorations;
            var result = span.TextDecorations;

            // Assert
            Assert.Equal(combinedDecorations, result);
        }

        /// <summary>
        /// Tests that the TextDecorations property getter returns the default value when not explicitly set.
        /// Verifies that the default value is None as expected.
        /// </summary>
        [Fact]
        public void TextDecorations_WhenNotSet_ReturnsDefaultValue()
        {
            // Arrange
            var span = new Span();

            // Act
            var result = span.TextDecorations;

            // Assert
            Assert.Equal(TextDecorations.None, result);
        }

        /// <summary>
        /// Tests that the TextDecorations property getter handles various valid enum values correctly.
        /// Uses parameterized test data to verify all valid TextDecorations enum values.
        /// </summary>
        [Theory]
        [InlineData(TextDecorations.None)]
        [InlineData(TextDecorations.Underline)]
        [InlineData(TextDecorations.Strikethrough)]
        [InlineData(TextDecorations.Underline | TextDecorations.Strikethrough)]
        public void TextDecorations_WithVariousValidValues_ReturnsExpectedValue(TextDecorations expectedValue)
        {
            // Arrange
            var span = new Span();

            // Act
            span.TextDecorations = expectedValue;
            var result = span.TextDecorations;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the TextDecorations property getter correctly handles casting from the underlying BindableProperty value.
        /// Verifies that invalid enum values (outside defined range) are handled appropriately.
        /// </summary>
        [Fact]
        public void TextDecorations_WithInvalidEnumValue_ReturnsSetValue()
        {
            // Arrange
            var span = new Span();
            var invalidEnumValue = (TextDecorations)999;

            // Act
            span.TextDecorations = invalidEnumValue;
            var result = span.TextDecorations;

            // Assert
            Assert.Equal(invalidEnumValue, result);
        }

        /// <summary>
        /// Verifies that the LineHeight property returns the default value when not explicitly set.
        /// Tests the getter with the default bindable property value of -1.0.
        /// Expected result: LineHeight should return -1.0 (the default value).
        /// </summary>
        [Fact]
        public void LineHeight_DefaultValue_ReturnsDefaultValue()
        {
            // Arrange
            var span = new Span();

            // Act
            var result = span.LineHeight;

            // Assert
            Assert.Equal(-1.0, result);
        }

        /// <summary>
        /// Verifies that the LineHeight property correctly returns various double values after being set.
        /// Tests the getter with multiple valid double values including boundary conditions.
        /// Expected result: LineHeight should return the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(1.5)]
        [InlineData(2.0)]
        [InlineData(10.5)]
        [InlineData(100.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        public void LineHeight_SetValidValues_ReturnsSetValue(double value)
        {
            // Arrange
            var span = new Span();

            // Act
            span.LineHeight = value;
            var result = span.LineHeight;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Verifies that the LineHeight property correctly handles negative values.
        /// Tests the getter with negative double values.
        /// Expected result: LineHeight should return the exact negative value that was set.
        /// </summary>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-0.5)]
        [InlineData(-10.0)]
        [InlineData(-100.5)]
        public void LineHeight_SetNegativeValues_ReturnsSetValue(double negativeValue)
        {
            // Arrange
            var span = new Span();

            // Act
            span.LineHeight = negativeValue;
            var result = span.LineHeight;

            // Assert
            Assert.Equal(negativeValue, result);
        }

        /// <summary>
        /// Verifies that the LineHeight property correctly handles special double values.
        /// Tests the getter with NaN, PositiveInfinity, and NegativeInfinity values.
        /// Expected result: LineHeight should return the exact special value that was set.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void LineHeight_SetSpecialDoubleValues_ReturnsSetValue(double specialValue)
        {
            // Arrange
            var span = new Span();

            // Act
            span.LineHeight = specialValue;
            var result = span.LineHeight;

            // Assert
            Assert.Equal(specialValue, result);
        }

        /// <summary>
        /// Tests that ValidateGesture allows TapGestureRecognizer without throwing an exception.
        /// Verifies that TapGestureRecognizer is a supported gesture type for Span elements.
        /// </summary>
        [Fact]
        public void ValidateGesture_TapGestureRecognizer_DoesNotThrow()
        {
            // Arrange
            var span = new Span();
            var tapGesture = new TapGestureRecognizer();

            // Act & Assert
            var exception = Record.Exception(() => span.ValidateGesture(tapGesture));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that ValidateGesture allows null gesture without throwing an exception.
        /// Verifies that null is a valid input for gesture validation.
        /// </summary>
        [Fact]
        public void ValidateGesture_NullGesture_DoesNotThrow()
        {
            // Arrange
            var span = new Span();

            // Act & Assert
            var exception = Record.Exception(() => span.ValidateGesture(null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that ValidateGesture throws InvalidOperationException for unsupported gesture types.
        /// Verifies that only TapGestureRecognizer and null are supported, all other gestures should throw.
        /// </summary>
        [Fact]
        public void ValidateGesture_UnsupportedGestureType_ThrowsInvalidOperationException()
        {
            // Arrange
            var span = new Span();
            var unsupportedGesture = Substitute.For<IGestureRecognizer>();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => span.ValidateGesture(unsupportedGesture));
            Assert.Contains("is not supported on a Span", exception.Message);
        }

        /// <summary>
        /// Tests that ValidateGesture exception message contains the correct gesture type name.
        /// Verifies that the exception message properly identifies the unsupported gesture type.
        /// </summary>
        [Fact]
        public void ValidateGesture_UnsupportedGestureType_ExceptionMessageContainsGestureTypeName()
        {
            // Arrange
            var span = new Span();
            var unsupportedGesture = Substitute.For<IGestureRecognizer>();
            var expectedTypeName = unsupportedGesture.GetType().Name;

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => span.ValidateGesture(unsupportedGesture));

            // Assert
            Assert.Contains(expectedTypeName, exception.Message);
            Assert.Contains("is not supported on a Span", exception.Message);
        }

        /// <summary>
        /// Tests that the Style property returns the default value when no style is set.
        /// Verifies the initial state of a new Span instance.
        /// Expected result: Style should be null (default value).
        /// </summary>
        [Fact]
        public void Style_DefaultValue_ReturnsNull()
        {
            // Arrange & Act
            var span = new Span();

            // Assert
            Assert.Null(span.Style);
        }

        /// <summary>
        /// Tests that the Style property can be set to null and retrieved correctly.
        /// Verifies null assignment behavior.
        /// Expected result: Style should be null after setting to null.
        /// </summary>
        [Fact]
        public void Style_SetToNull_ReturnsNull()
        {
            // Arrange
            var span = new Span();

            // Act
            span.Style = null;

            // Assert
            Assert.Null(span.Style);
        }

        /// <summary>
        /// Tests that the Style property can be set to a valid Style object and retrieved correctly.
        /// Verifies basic setter and getter functionality with a valid style.
        /// Expected result: The retrieved style should be the same instance that was set.
        /// </summary>
        [Fact]
        public void Style_SetValidStyle_ReturnsSetStyle()
        {
            // Arrange
            var span = new Span();
            var style = new Style(typeof(Span))
            {
                Setters = { new Setter { Property = Span.TextColorProperty, Value = Colors.Red } }
            };

            // Act
            span.Style = style;

            // Assert
            Assert.Same(style, span.Style);
        }

        /// <summary>
        /// Tests that the Style property can be changed from one style to another.
        /// Verifies that the property correctly handles style replacement.
        /// Expected result: The retrieved style should be the last style that was set.
        /// </summary>
        [Fact]
        public void Style_ChangeFromOneStyleToAnother_ReturnsLatestStyle()
        {
            // Arrange
            var span = new Span();
            var firstStyle = new Style(typeof(Span))
            {
                Setters = { new Setter { Property = Span.TextColorProperty, Value = Colors.Blue } }
            };
            var secondStyle = new Style(typeof(Span))
            {
                Setters = { new Setter { Property = Span.FontSizeProperty, Value = 20.0 } }
            };

            // Act
            span.Style = firstStyle;
            span.Style = secondStyle;

            // Assert
            Assert.Same(secondStyle, span.Style);
        }

        /// <summary>
        /// Tests that the Style property can be set from a valid style back to null.
        /// Verifies the ability to clear a previously set style.
        /// Expected result: Style should be null after being cleared.
        /// </summary>
        [Fact]
        public void Style_SetValidStyleThenNull_ReturnsNull()
        {
            // Arrange
            var span = new Span();
            var style = new Style(typeof(Span))
            {
                Setters = { new Setter { Property = Span.TextColorProperty, Value = Colors.Green } }
            };

            // Act
            span.Style = style;
            span.Style = null;

            // Assert
            Assert.Null(span.Style);
        }

        /// <summary>
        /// Tests that the Style property handles multiple consecutive assignments correctly.
        /// Verifies that the property maintains referential integrity across multiple operations.
        /// Expected result: Each assignment should be correctly stored and retrieved.
        /// </summary>
        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        public void Style_MultipleConsecutiveAssignments_MaintainsCorrectReference(int assignmentCount)
        {
            // Arrange
            var span = new Span();
            Style lastStyle = null;

            // Act & Assert
            for (int i = 0; i < assignmentCount; i++)
            {
                var style = new Style(typeof(Span))
                {
                    Setters = { new Setter { Property = Span.FontSizeProperty, Value = 10.0 + i } }
                };

                span.Style = style;
                lastStyle = style;

                Assert.Same(style, span.Style);
            }

            // Final verification
            Assert.Same(lastStyle, span.Style);
        }
    }


    /// <summary>
    /// Unit tests for the TextColor property of the Span class.
    /// </summary>
    public partial class SpanTextColorTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the TextColor property getter returns the correct color value when set to a predefined color.
        /// This test verifies that the getter calls GetValue with the correct BindableProperty and returns the expected color.
        /// </summary>
        [Theory]
        [InlineData(typeof(Color), "Red")]
        [InlineData(typeof(Color), "Blue")]
        [InlineData(typeof(Color), "Green")]
        [InlineData(typeof(Color), "Black")]
        [InlineData(typeof(Color), "White")]
        [InlineData(typeof(Color), "Transparent")]
        public void TextColor_GetterWithPredefinedColors_ReturnsExpectedColor(Type colorType, string colorName)
        {
            // Arrange
            var span = new Span();
            var expectedColor = (Color)colorType.GetProperty(colorName + "s").GetValue(null).GetType().GetProperty(colorName).GetValue(null);
            span.TextColor = expectedColor;

            // Act
            var actualColor = span.TextColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that the TextColor property getter returns the correct color when set to custom RGB values.
        /// This test exercises the getter with various RGB color combinations to ensure proper value retrieval.
        /// </summary>
        [Theory]
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Pure red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Pure green  
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Pure blue
        [InlineData(0.5f, 0.5f, 0.5f, 1.0f)] // Gray
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent black
        [InlineData(1.0f, 1.0f, 1.0f, 0.5f)] // Semi-transparent white
        public void TextColor_GetterWithCustomColors_ReturnsExpectedColor(float red, float green, float blue, float alpha)
        {
            // Arrange
            var span = new Span();
            var expectedColor = new Color(red, green, blue, alpha);
            span.TextColor = expectedColor;

            // Act
            var actualColor = span.TextColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that the TextColor property setter correctly stores the color value.
        /// This test verifies that the setter calls SetValue with the correct BindableProperty and value.
        /// </summary>
        [Fact]
        public void TextColor_Setter_StoresColorCorrectly()
        {
            // Arrange
            var span = new Span();
            var expectedColor = Colors.Purple;

            // Act
            span.TextColor = expectedColor;

            // Assert
            Assert.Equal(expectedColor, span.TextColor);
        }

        /// <summary>
        /// Tests that the TextColor property returns the default color when not explicitly set.
        /// This test exercises the getter when no color has been assigned to verify default behavior.
        /// </summary>
        [Fact]
        public void TextColor_GetterWhenNotSet_ReturnsDefaultColor()
        {
            // Arrange
            var span = new Span();

            // Act
            var actualColor = span.TextColor;

            // Assert
            // The default value for TextColorProperty is null, which gets converted to default Color
            Assert.Equal(default(Color), actualColor);
        }

        /// <summary>
        /// Tests that the TextColor property handles transparent colors correctly.
        /// This test verifies edge case behavior with fully transparent colors.
        /// </summary>
        [Fact]
        public void TextColor_TransparentColor_HandlesCorrectly()
        {
            // Arrange
            var span = new Span();
            var transparentColor = Colors.Transparent;

            // Act
            span.TextColor = transparentColor;
            var actualColor = span.TextColor;

            // Assert
            Assert.Equal(transparentColor, actualColor);
            Assert.Equal(0.0f, actualColor.Alpha);
        }

        /// <summary>
        /// Tests that the TextColor property handles colors with extreme RGB values correctly.
        /// This test verifies boundary value handling for color components.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f)] // Minimum values
        [InlineData(1.0f, 1.0f, 1.0f)] // Maximum values
        [InlineData(0.0f, 1.0f, 0.0f)] // Mixed extremes
        [InlineData(1.0f, 0.0f, 1.0f)] // Mixed extremes
        public void TextColor_ExtremeRGBValues_HandlesCorrectly(float red, float green, float blue)
        {
            // Arrange
            var span = new Span();
            var color = new Color(red, green, blue);

            // Act
            span.TextColor = color;
            var actualColor = span.TextColor;

            // Assert
            Assert.Equal(color, actualColor);
            Assert.Equal(red, actualColor.Red);
            Assert.Equal(green, actualColor.Green);
            Assert.Equal(blue, actualColor.Blue);
        }

        /// <summary>
        /// Tests that multiple TextColor assignments work correctly.
        /// This test verifies that the getter consistently returns the most recently set value.
        /// </summary>
        [Fact]
        public void TextColor_MultipleAssignments_ReturnsMostRecentValue()
        {
            // Arrange
            var span = new Span();
            var firstColor = Colors.Red;
            var secondColor = Colors.Blue;
            var thirdColor = Colors.Green;

            // Act & Assert
            span.TextColor = firstColor;
            Assert.Equal(firstColor, span.TextColor);

            span.TextColor = secondColor;
            Assert.Equal(secondColor, span.TextColor);

            span.TextColor = thirdColor;
            Assert.Equal(thirdColor, span.TextColor);
        }

        /// <summary>
        /// Tests that the TextColor property maintains color precision.
        /// This test verifies that color values are not lost or altered during get/set operations.
        /// </summary>
        [Fact]
        public void TextColor_ColorPrecision_MaintainsPrecision()
        {
            // Arrange
            var span = new Span();
            var preciseColor = new Color(0.123f, 0.456f, 0.789f, 0.321f);

            // Act
            span.TextColor = preciseColor;
            var retrievedColor = span.TextColor;

            // Assert
            Assert.Equal(preciseColor.Red, retrievedColor.Red, 5);
            Assert.Equal(preciseColor.Green, retrievedColor.Green, 5);
            Assert.Equal(preciseColor.Blue, retrievedColor.Blue, 5);
            Assert.Equal(preciseColor.Alpha, retrievedColor.Alpha, 5);
        }
    }
}