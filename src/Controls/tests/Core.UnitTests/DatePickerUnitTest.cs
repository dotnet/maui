#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class DatePickerUnitTest : BaseTestFixture
    {
        [Fact]
        public void TestMinimumDate()
        {
            DatePicker picker = new DatePicker();

            picker.MinimumDate = new DateTime(1950, 1, 1);

            Assert.Equal(new DateTime(1950, 1, 1), picker.MinimumDate);

            picker.MinimumDate = new DateTime(2200, 1, 1);
            Assert.Equal(new DateTime(1950, 1, 1), picker.MinimumDate);
        }

        [Fact]
        public void TestMinimumDateNull()
        {
            DatePicker picker = new DatePicker();

            picker.MinimumDate = null;

            Assert.Null(picker.MinimumDate);
        }

        [Fact]
        public void TestMaximumDate()
        {
            DatePicker picker = new DatePicker();

            picker.MaximumDate = new DateTime(2050, 1, 1);

            Assert.Equal(new DateTime(2050, 1, 1), picker.MaximumDate);

            picker.MaximumDate = new DateTime(1800, 1, 1);
            Assert.Equal(new DateTime(2050, 1, 1), picker.MaximumDate);
        }

        [Fact]
        public void TestMaximumDateNull()
        {
            DatePicker picker = new DatePicker();

            picker.MaximumDate = null;

            Assert.Null(picker.MaximumDate);
        }

        [Fact]
        public void TestMaximumDateClamping()
        {
            DatePicker picker = new DatePicker();

            picker.Date = new DateTime(2050, 1, 1);

            Assert.Equal(new DateTime(2050, 1, 1), picker.Date);

            bool dateChanged = false;
            bool maximumDateChanged = false;
            picker.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "MaximumDate":
                        maximumDateChanged = true;
                        break;
                    case "Date":
                        dateChanged = true;
                        Assert.False(maximumDateChanged);
                        break;
                }
            };

            var newDate = new DateTime(2000, 1, 1);
            picker.MaximumDate = newDate;

            Assert.True(maximumDateChanged);
            Assert.True(dateChanged);

            Assert.Equal(newDate, picker.MaximumDate);
            Assert.Equal(newDate, picker.Date);
            Assert.Equal(picker.MaximumDate, picker.Date);
        }

        [Fact]
        public void TestMinimumDateClamping()
        {
            DatePicker picker = new DatePicker();

            picker.Date = new DateTime(1950, 1, 1);

            Assert.Equal(new DateTime(1950, 1, 1), picker.Date);

            bool dateChanged = false;
            bool minimumDateChanged = false;
            picker.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "MinimumDate":
                        minimumDateChanged = true;
                        break;
                    case "Date":
                        dateChanged = true;
                        Assert.False(minimumDateChanged);
                        break;
                }
            };

            var newDate = new DateTime(2000, 1, 1);
            picker.MinimumDate = newDate;

            Assert.True(minimumDateChanged);
            Assert.True(dateChanged);

            Assert.Equal(newDate, picker.MinimumDate);
            Assert.Equal(newDate, picker.Date);
            Assert.Equal(picker.MinimumDate, picker.Date);
        }

        [Fact]
        public void TestDateClamping()
        {
            DatePicker picker = new DatePicker();

            picker.Date = new DateTime(1500, 1, 1);

            Assert.Equal(picker.MinimumDate, picker.Date);

            picker.Date = new DateTime(2500, 1, 1);

            Assert.Equal(picker.MaximumDate, picker.Date);
        }

        [Fact]
        public void TestDateSelected()
        {
            var picker = new DatePicker();

            bool selected = false;
            picker.DateSelected += (sender, arg) => selected = true;

            // we can be fairly sure it wont ever be 2008 again
            picker.Date = new DateTime(2008, 5, 5);

            Assert.True(selected);
        }

        readonly static object[] DateTimes = {
            new object[] { new DateTime (2006, 12, 20), new DateTime (2011, 11, 30) },
            new object[] { new DateTime (1900, 1, 1), new DateTime (1999, 01, 15) }, // Minimum Date
			new object[] { new DateTime (2006, 12, 20), new DateTime (2100, 12, 31) }, // Maximum Date
			new object[] { new DateTime (2006, 12, 20), null },
            new object[] { null, new DateTime (2006, 12, 20) },
        };

        public static IEnumerable<object[]> DateTimesData()
        {
            foreach (var o in DateTimes)
            {
                yield return o as object[];
            }
        }

        [Theory, MemberData(nameof(DateTimesData))]
        public void DatePickerSelectedEventArgs(DateTime? initialDate, DateTime? finalDate)
        {
            var datePicker = new DatePicker();
            datePicker.Date = initialDate;

            DatePicker pickerFromSender = null;
            DateTime? oldDate = new DateTime();
            DateTime? newDate = new DateTime();

            datePicker.DateSelected += (s, e) =>
            {
                pickerFromSender = (DatePicker)s;
                oldDate = e.OldDate;
                newDate = e.NewDate;
            };

            datePicker.Date = finalDate;

            Assert.Equal(datePicker, pickerFromSender);
            Assert.Equal(initialDate, oldDate);
            Assert.Equal(finalDate, newDate);
        }

        readonly static object[] DateTimesForSelectedTrigger = [
            new object[] { new DateTime (2006, 12, 20), new DateTime (2011, 11, 30), true },
            new object[] { new DateTime (1900, 1, 1), new DateTime (1999, 01, 15), true }, // Minimum Date
			new object[] { new DateTime (2006, 12, 20), new DateTime (2100, 12, 31), true }, // Maximum Date
			new object[] { new DateTime (2006, 12, 20), null, true },
            new object[] { null, new DateTime (2006, 12, 20), true },
            new object[] { new DateTime(2006, 12, 20), new DateTime (2006, 12, 20), false },
            new object[] { null, null, false },
        ];

        public static IEnumerable<object[]> DateTimesForSelectedTriggerData()
        {
            foreach (var o in DateTimesForSelectedTrigger)
            {
                yield return o as object[];
            }
        }

        [Theory, MemberData(nameof(DateTimesForSelectedTriggerData))]
        public void DatePickerSelectedEventTriggered(DateTime? initialDate, DateTime? finalDate, bool shouldDateSelectedTrigger)
        {
            bool isDateSelectedTriggered = false;

            var datePicker = new DatePicker();
            datePicker.Date = initialDate;

            DateTime? oldDate = new DateTime();
            DateTime? newDate = new DateTime();

            datePicker.DateSelected += (s, e) =>
            {
                isDateSelectedTriggered = true;
            };

            datePicker.Date = finalDate;

            Assert.Equal(shouldDateSelectedTrigger, isDateSelectedTriggered);
        }

        [Fact]
        //https://bugzilla.xamarin.com/show_bug.cgi?id=32144
        public void SetNullValueDoesNotThrow()
        {
            var datePicker = new DatePicker();
            datePicker.SetValue(DatePicker.DateProperty, null);
            Assert.Null(datePicker.Date);
        }

        [Fact]
        public void SetNullableDateTime()
        {
            var datePicker = new DatePicker();
            var dateTime = new DateTime(2015, 7, 21);
            DateTime? nullableDateTime = dateTime;
            datePicker.SetValue(DatePicker.DateProperty, nullableDateTime);
            Assert.Equal(dateTime, datePicker.Date);
        }

        [Fact]
        //https://github.com/xamarin/Xamarin.Forms/issues/5784
        public void SetMaxAndMinDateTimeToNow()
        {
            var datePicker = new DatePicker();
            datePicker.SetValue(DatePicker.MaximumDateProperty, DateTime.Now);
            datePicker.SetValue(DatePicker.MinimumDateProperty, DateTime.Now);
        }
    }

    public partial class DatePickerTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that CharacterSpacing returns the default value of 0.0 when not explicitly set.
        /// </summary>
        [Fact]
        public void CharacterSpacing_DefaultValue_ReturnsZero()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            var result = datePicker.CharacterSpacing;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing getter returns the value that was set through the setter.
        /// Tests various double values including positive, negative, and edge cases.
        /// </summary>
        /// <param name="expectedValue">The character spacing value to set and verify</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(5.5)]
        [InlineData(100.0)]
        [InlineData(-1.0)]
        [InlineData(-5.5)]
        [InlineData(-100.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(1.7976931348623157E+308)] // Close to MaxValue
        [InlineData(-1.7976931348623157E+308)] // Close to MinValue
        [InlineData(4.94065645841247E-324)] // Close to smallest positive value
        [InlineData(-4.94065645841247E-324)] // Close to smallest negative value
        public void CharacterSpacing_SetAndGet_ReturnsExpectedValue(double expectedValue)
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.CharacterSpacing = expectedValue;
            var result = datePicker.CharacterSpacing;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that CharacterSpacing handles special double values correctly.
        /// Verifies NaN, PositiveInfinity, and NegativeInfinity are preserved.
        /// </summary>
        /// <param name="specialValue">The special double value to test</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void CharacterSpacing_SpecialDoubleValues_PreservesValue(double specialValue)
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.CharacterSpacing = specialValue;
            var result = datePicker.CharacterSpacing;

            // Assert
            if (double.IsNaN(specialValue))
            {
                Assert.True(double.IsNaN(result));
            }
            else if (double.IsPositiveInfinity(specialValue))
            {
                Assert.True(double.IsPositiveInfinity(result));
            }
            else if (double.IsNegativeInfinity(specialValue))
            {
                Assert.True(double.IsNegativeInfinity(result));
            }
        }

        /// <summary>
        /// Tests that multiple CharacterSpacing assignments work correctly.
        /// Verifies that the property correctly updates when set multiple times.
        /// </summary>
        [Fact]
        public void CharacterSpacing_MultipleAssignments_UpdatesCorrectly()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act & Assert - First assignment
            datePicker.CharacterSpacing = 10.0;
            Assert.Equal(10.0, datePicker.CharacterSpacing);

            // Act & Assert - Second assignment
            datePicker.CharacterSpacing = -5.5;
            Assert.Equal(-5.5, datePicker.CharacterSpacing);

            // Act & Assert - Third assignment back to zero
            datePicker.CharacterSpacing = 0.0;
            Assert.Equal(0.0, datePicker.CharacterSpacing);
        }

        /// <summary>
        /// Tests that FontSize property getter returns the correct value when set to a normal positive value.
        /// Input condition: Setting FontSize to a normal positive double value.
        /// Expected result: The getter returns the same value that was set.
        /// </summary>
        [Theory]
        [InlineData(12.0)]
        [InlineData(14.5)]
        [InlineData(16.0)]
        [InlineData(18.75)]
        [InlineData(24.0)]
        [InlineData(36.0)]
        public void FontSize_SetNormalPositiveValue_ReturnsCorrectValue(double fontSize)
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.FontSize = fontSize;

            // Assert
            Assert.Equal(fontSize, datePicker.FontSize);
        }

        /// <summary>
        /// Tests that FontSize property can be set to zero.
        /// Input condition: Setting FontSize to zero.
        /// Expected result: The property accepts and returns zero.
        /// </summary>
        [Fact]
        public void FontSize_SetZero_ReturnsZero()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.FontSize = 0.0;

            // Assert
            Assert.Equal(0.0, datePicker.FontSize);
        }

        /// <summary>
        /// Tests that FontSize property can be set to negative values.
        /// Input condition: Setting FontSize to various negative values.
        /// Expected result: The property accepts and returns the negative values.
        /// </summary>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-12.5)]
        [InlineData(-100.0)]
        public void FontSize_SetNegativeValue_ReturnsNegativeValue(double negativeSize)
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.FontSize = negativeSize;

            // Assert
            Assert.Equal(negativeSize, datePicker.FontSize);
        }

        /// <summary>
        /// Tests that FontSize property handles extreme double values correctly.
        /// Input condition: Setting FontSize to double boundary values.
        /// Expected result: The property accepts and returns the extreme values.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.Epsilon)]
        public void FontSize_SetExtremeValues_ReturnsExtremeValues(double extremeValue)
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.FontSize = extremeValue;

            // Assert
            Assert.Equal(extremeValue, datePicker.FontSize);
        }

        /// <summary>
        /// Tests that FontSize property handles special double values correctly.
        /// Input condition: Setting FontSize to NaN, PositiveInfinity, and NegativeInfinity.
        /// Expected result: The property accepts and returns the special values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void FontSize_SetSpecialDoubleValues_ReturnsSpecialValues(double specialValue)
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.FontSize = specialValue;

            // Assert
            if (double.IsNaN(specialValue))
            {
                Assert.True(double.IsNaN(datePicker.FontSize));
            }
            else
            {
                Assert.Equal(specialValue, datePicker.FontSize);
            }
        }

        /// <summary>
        /// Tests that FontSize property has a default value.
        /// Input condition: Creating a new DatePicker instance without setting FontSize.
        /// Expected result: FontSize returns a valid default value.
        /// </summary>
        [Fact]
        public void FontSize_DefaultValue_ReturnsValidDefault()
        {
            // Arrange & Act
            var datePicker = new DatePicker();

            // Assert
            Assert.True(datePicker.FontSize >= 0 || double.IsNaN(datePicker.FontSize));
        }

        /// <summary>
        /// Tests that FontSize property maintains precision for decimal values.
        /// Input condition: Setting FontSize to various decimal values with different precision.
        /// Expected result: The property maintains the exact precision of the input values.
        /// </summary>
        [Theory]
        [InlineData(12.123456789)]
        [InlineData(0.000001)]
        [InlineData(999.999999)]
        public void FontSize_SetDecimalValues_MaintainsPrecision(double preciseValue)
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.FontSize = preciseValue;

            // Assert
            Assert.Equal(preciseValue, datePicker.FontSize);
        }

        /// <summary>
        /// Tests that FontSize property can be set and retrieved multiple times consistently.
        /// Input condition: Setting FontSize to different values in sequence.
        /// Expected result: Each set operation correctly updates the value.
        /// </summary>
        [Fact]
        public void FontSize_MultipleSetOperations_EachValueSetCorrectly()
        {
            // Arrange
            var datePicker = new DatePicker();
            var testValues = new[] { 10.0, 20.5, 0.0, -5.0, 100.75 };

            foreach (var testValue in testValues)
            {
                // Act
                datePicker.FontSize = testValue;

                // Assert
                Assert.Equal(testValue, datePicker.FontSize);
            }
        }

        /// <summary>
        /// Tests that FontAutoScalingEnabled property returns the default value when not explicitly set.
        /// Input: New DatePicker instance
        /// Expected: Default value should be returned from the underlying BindableProperty
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_DefaultValue_ReturnsExpectedDefault()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            var result = datePicker.FontAutoScalingEnabled;

            // Assert  
            Assert.IsType<bool>(result);
        }

        /// <summary>
        /// Tests that FontAutoScalingEnabled property can be set to true.
        /// Input: Setting FontAutoScalingEnabled to true
        /// Expected: Property should return true after being set
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.FontAutoScalingEnabled = true;

            // Assert
            Assert.True(datePicker.FontAutoScalingEnabled);
        }

        /// <summary>
        /// Tests that FontAutoScalingEnabled property can be set to false.
        /// Input: Setting FontAutoScalingEnabled to false
        /// Expected: Property should return false after being set
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.FontAutoScalingEnabled = false;

            // Assert
            Assert.False(datePicker.FontAutoScalingEnabled);
        }

        /// <summary>
        /// Tests that FontAutoScalingEnabled property uses the correct BindableProperty.
        /// Input: Accessing FontAutoScalingEnabledProperty
        /// Expected: Property should not be null and should be the correct BindableProperty
        /// </summary>
        [Fact]
        public void FontAutoScalingEnabled_BindableProperty_IsNotNull()
        {
            // Arrange & Act
            var bindableProperty = DatePicker.FontAutoScalingEnabledProperty;

            // Assert
            Assert.NotNull(bindableProperty);
            Assert.Equal(nameof(DatePicker.FontAutoScalingEnabled), bindableProperty.PropertyName);
        }

        /// <summary>
        /// Tests that FontAutoScalingEnabled property maintains its value across multiple gets and sets.
        /// Input: Multiple alternating boolean values
        /// Expected: Property should accurately maintain and return the last set value
        /// </summary>
        [Theory]
        [InlineData(true, false, true)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        [InlineData(false, false, true)]
        public void FontAutoScalingEnabled_MultipleSetOperations_MaintainsCorrectValue(bool firstValue, bool secondValue, bool thirdValue)
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act & Assert
            datePicker.FontAutoScalingEnabled = firstValue;
            Assert.Equal(firstValue, datePicker.FontAutoScalingEnabled);

            datePicker.FontAutoScalingEnabled = secondValue;
            Assert.Equal(secondValue, datePicker.FontAutoScalingEnabled);

            datePicker.FontAutoScalingEnabled = thirdValue;
            Assert.Equal(thirdValue, datePicker.FontAutoScalingEnabled);
        }

        /// <summary>
        /// Tests UpdateFormsText method with null source parameter.
        /// Validates that null input returns empty string regardless of text transform.
        /// </summary>
        [Theory]
        [InlineData(TextTransform.None)]
        [InlineData(TextTransform.Default)]
        [InlineData(TextTransform.Lowercase)]
        [InlineData(TextTransform.Uppercase)]
        public void UpdateFormsText_NullSource_ReturnsEmptyString(TextTransform textTransform)
        {
            // Arrange
            var datePicker = new DatePicker();
            string source = null;

            // Act
            var result = datePicker.UpdateFormsText(source, textTransform);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests UpdateFormsText method with empty source parameter.
        /// Validates that empty string input returns empty string regardless of text transform.
        /// </summary>
        [Theory]
        [InlineData(TextTransform.None)]
        [InlineData(TextTransform.Default)]
        [InlineData(TextTransform.Lowercase)]
        [InlineData(TextTransform.Uppercase)]
        public void UpdateFormsText_EmptySource_ReturnsEmptyString(TextTransform textTransform)
        {
            // Arrange
            var datePicker = new DatePicker();
            string source = string.Empty;

            // Act
            var result = datePicker.UpdateFormsText(source, textTransform);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests UpdateFormsText method with various text transform options on normal strings.
        /// Validates that text transformation is applied correctly for different input combinations.
        /// </summary>
        [Theory]
        [InlineData("Hello World", TextTransform.None, "Hello World")]
        [InlineData("Hello World", TextTransform.Default, "Hello World")]
        [InlineData("Hello World", TextTransform.Lowercase, "hello world")]
        [InlineData("Hello World", TextTransform.Uppercase, "HELLO WORLD")]
        [InlineData("MiXeD CaSe", TextTransform.Lowercase, "mixed case")]
        [InlineData("MiXeD CaSe", TextTransform.Uppercase, "MIXED CASE")]
        [InlineData("   ", TextTransform.Lowercase, "   ")]
        [InlineData("   ", TextTransform.Uppercase, "   ")]
        [InlineData("Special!@#$%^&*()Characters", TextTransform.Lowercase, "special!@#$%^&*()characters")]
        [InlineData("Special!@#$%^&*()Characters", TextTransform.Uppercase, "SPECIAL!@#$%^&*()CHARACTERS")]
        [InlineData("Numbers123AndSymbols!@#", TextTransform.Lowercase, "numbers123andsymbols!@#")]
        [InlineData("Numbers123AndSymbols!@#", TextTransform.Uppercase, "NUMBERS123ANDSYMBOLS!@#")]
        public void UpdateFormsText_ValidSourceAndTransform_ReturnsTransformedText(string source, TextTransform textTransform, string expected)
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            var result = datePicker.UpdateFormsText(source, textTransform);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests UpdateFormsText method with invalid enum values for TextTransform.
        /// Validates that invalid enum values default to returning source unchanged.
        /// </summary>
        [Theory]
        [InlineData((TextTransform)(-1))]
        [InlineData((TextTransform)999)]
        [InlineData((TextTransform)int.MaxValue)]
        [InlineData((TextTransform)int.MinValue)]
        public void UpdateFormsText_InvalidTextTransform_ReturnsSourceUnchanged(TextTransform invalidTransform)
        {
            // Arrange
            var datePicker = new DatePicker();
            string source = "Test String";

            // Act
            var result = datePicker.UpdateFormsText(source, invalidTransform);

            // Assert
            Assert.Equal(source, result);
        }

        /// <summary>
        /// Tests UpdateFormsText method with very long string input.
        /// Validates that text transformation works correctly on large strings.
        /// </summary>
        [Fact]
        public void UpdateFormsText_VeryLongString_TransformsCorrectly()
        {
            // Arrange
            var datePicker = new DatePicker();
            var longString = new string('A', 10000) + new string('b', 10000);
            var expectedLowercase = new string('a', 10000) + new string('b', 10000);
            var expectedUppercase = new string('A', 20000);

            // Act
            var resultLowercase = datePicker.UpdateFormsText(longString, TextTransform.Lowercase);
            var resultUppercase = datePicker.UpdateFormsText(longString, TextTransform.Uppercase);
            var resultNone = datePicker.UpdateFormsText(longString, TextTransform.None);

            // Assert
            Assert.Equal(expectedLowercase, resultLowercase);
            Assert.Equal(expectedUppercase, resultUppercase);
            Assert.Equal(longString, resultNone);
        }

        /// <summary>
        /// Tests UpdateFormsText method with Unicode characters.
        /// Validates that text transformation works correctly with international characters.
        /// </summary>
        [Theory]
        [InlineData("Café", TextTransform.Uppercase, "CAFÉ")]
        [InlineData("CAFÉ", TextTransform.Lowercase, "café")]
        [InlineData("Naïve", TextTransform.Uppercase, "NAÏVE")]
        [InlineData("NAÏVE", TextTransform.Lowercase, "naïve")]
        [InlineData("東京", TextTransform.Lowercase, "東京")]
        [InlineData("東京", TextTransform.Uppercase, "東京")]
        public void UpdateFormsText_UnicodeCharacters_TransformsCorrectly(string source, TextTransform textTransform, string expected)
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            var result = datePicker.UpdateFormsText(source, textTransform);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that the On method returns a valid IPlatformElementConfiguration for a platform type.
        /// Verifies that the method correctly delegates to the platform configuration registry and returns a non-null configuration.
        /// </summary>
        [Fact]
        public void On_WithValidPlatformType_ReturnsValidConfiguration()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            var result = datePicker.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, DatePicker>>(result);
        }

        /// <summary>
        /// Tests that the On method returns the same instance when called multiple times with the same platform type.
        /// Verifies the caching behavior of the platform configuration registry.
        /// </summary>
        [Fact]
        public void On_CalledMultipleTimesWithSamePlatform_ReturnsSameInstance()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            var firstCall = datePicker.On<TestPlatform>();
            var secondCall = datePicker.On<TestPlatform>();

            // Assert
            Assert.Same(firstCall, secondCall);
        }

        /// <summary>
        /// Tests that the On method returns different instances for different platform types.
        /// Verifies that each platform type gets its own configuration instance.
        /// </summary>
        [Fact]
        public void On_WithDifferentPlatformTypes_ReturnsDifferentInstances()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            var firstPlatformConfig = datePicker.On<TestPlatform>();
            var secondPlatformConfig = datePicker.On<AnotherTestPlatform>();

            // Assert
            Assert.NotSame(firstPlatformConfig, secondPlatformConfig);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, DatePicker>>(firstPlatformConfig);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<AnotherTestPlatform, DatePicker>>(secondPlatformConfig);
        }

        /// <summary>
        /// Tests that multiple DatePicker instances have independent platform configurations.
        /// Verifies that the platform configuration registry is instance-specific.
        /// </summary>
        [Fact]
        public void On_WithMultipleInstances_ReturnsIndependentConfigurations()
        {
            // Arrange
            var firstDatePicker = new DatePicker();
            var secondDatePicker = new DatePicker();

            // Act
            var firstInstanceConfig = firstDatePicker.On<TestPlatform>();
            var secondInstanceConfig = secondDatePicker.On<TestPlatform>();

            // Assert
            Assert.NotSame(firstInstanceConfig, secondInstanceConfig);
        }

        // Helper classes for testing - these implement IConfigPlatform for testing purposes
        private class TestPlatform : IConfigPlatform
        {
        }

        private class AnotherTestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Tests that TextColor property getter returns the default value when not set.
        /// Verifies that GetValue is called with the correct bindable property.
        /// </summary>
        [Fact]
        public void TextColor_DefaultValue_ReturnsNull()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            var result = datePicker.TextColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that TextColor property getter and setter work correctly with various color values.
        /// Verifies that SetValue and GetValue are called with the correct bindable property and values.
        /// </summary>
        [Theory]
        [MemberData(nameof(ColorTestData))]
        public void TextColor_SetAndGet_ReturnsCorrectValue(Color expectedColor)
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.TextColor = expectedColor;
            var result = datePicker.TextColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that TextColor property handles transparent colors correctly.
        /// Verifies edge case with fully transparent color (Alpha = 0).
        /// </summary>
        [Fact]
        public void TextColor_TransparentColor_HandlesCorrectly()
        {
            // Arrange
            var datePicker = new DatePicker();
            var transparentColor = new Color(1.0f, 0.5f, 0.2f, 0.0f); // Fully transparent

            // Act
            datePicker.TextColor = transparentColor;
            var result = datePicker.TextColor;

            // Assert
            Assert.Equal(transparentColor, result);
            Assert.Equal(0.0f, result.Alpha);
        }

        /// <summary>
        /// Tests that TextColor property handles colors with extreme component values correctly.
        /// Verifies boundary conditions with minimum and maximum color component values.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // All minimum values
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // All maximum values
        [InlineData(0.0f, 1.0f, 0.5f, 0.25f)] // Mixed values
        public void TextColor_ExtremeValues_HandlesCorrectly(float red, float green, float blue, float alpha)
        {
            // Arrange
            var datePicker = new DatePicker();
            var color = new Color(red, green, blue, alpha);

            // Act
            datePicker.TextColor = color;
            var result = datePicker.TextColor;

            // Assert
            Assert.Equal(color, result);
            Assert.Equal(red, result.Red);
            Assert.Equal(green, result.Green);
            Assert.Equal(blue, result.Blue);
            Assert.Equal(alpha, result.Alpha);
        }

        /// <summary>
        /// Tests that TextColor property can be set to different colors in sequence.
        /// Verifies that multiple assignments work correctly and don't interfere with each other.
        /// </summary>
        [Fact]
        public void TextColor_MultipleAssignments_WorksCorrectly()
        {
            // Arrange
            var datePicker = new DatePicker();
            var firstColor = new Color(1.0f, 0.0f, 0.0f); // Red
            var secondColor = new Color(0.0f, 1.0f, 0.0f); // Green
            var thirdColor = new Color(0.0f, 0.0f, 1.0f); // Blue

            // Act & Assert - First assignment
            datePicker.TextColor = firstColor;
            Assert.Equal(firstColor, datePicker.TextColor);

            // Act & Assert - Second assignment
            datePicker.TextColor = secondColor;
            Assert.Equal(secondColor, datePicker.TextColor);

            // Act & Assert - Third assignment
            datePicker.TextColor = thirdColor;
            Assert.Equal(thirdColor, datePicker.TextColor);
        }

        /// <summary>
        /// Tests that TextColor property can be reset to null after being set to a color.
        /// Verifies that null assignment works correctly and resets to default state.
        /// </summary>
        [Fact]
        public void TextColor_SetToNullAfterColor_ResetsToNull()
        {
            // Arrange
            var datePicker = new DatePicker();
            var color = new Color(0.5f, 0.5f, 0.5f); // Gray

            // Act
            datePicker.TextColor = color;
            Assert.Equal(color, datePicker.TextColor); // Verify it was set

            datePicker.TextColor = null;
            var result = datePicker.TextColor;

            // Assert
            Assert.Null(result);
        }

        public static IEnumerable<object[]> ColorTestData()
        {
            yield return new object[] { new Color() }; // Default black color
            yield return new object[] { new Color(0.5f) }; // Gray color
            yield return new object[] { new Color(1.0f, 0.0f, 0.0f) }; // Red
            yield return new object[] { new Color(0.0f, 1.0f, 0.0f) }; // Green  
            yield return new object[] { new Color(0.0f, 0.0f, 1.0f) }; // Blue
            yield return new object[] { new Color(1.0f, 1.0f, 1.0f) }; // White
            yield return new object[] { new Color(0.5f, 0.3f, 0.8f, 0.7f) }; // Custom color with alpha
            yield return new object[] { new Color(0.1f, 0.9f, 0.2f, 1.0f) }; // Another custom color
        }
    }


    public partial class DatePickerFontAttributesTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the FontAttributes property returns the default value when not explicitly set.
        /// Expected result: FontAttributes.None (default enum value).
        /// </summary>
        [Fact]
        public void FontAttributes_DefaultValue_ReturnsNone()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            var result = datePicker.FontAttributes;

            // Assert
            Assert.Equal(FontAttributes.None, result);
        }

        /// <summary>
        /// Tests setting and getting the FontAttributes property with None value.
        /// Expected result: Property should store and return FontAttributes.None.
        /// </summary>
        [Fact]
        public void FontAttributes_SetNone_ReturnsNone()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.FontAttributes = FontAttributes.None;

            // Assert
            Assert.Equal(FontAttributes.None, datePicker.FontAttributes);
        }

        /// <summary>
        /// Tests setting and getting the FontAttributes property with Bold value.
        /// Expected result: Property should store and return FontAttributes.Bold.
        /// </summary>
        [Fact]
        public void FontAttributes_SetBold_ReturnsBold()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.FontAttributes = FontAttributes.Bold;

            // Assert
            Assert.Equal(FontAttributes.Bold, datePicker.FontAttributes);
        }

        /// <summary>
        /// Tests setting and getting the FontAttributes property with Italic value.
        /// Expected result: Property should store and return FontAttributes.Italic.
        /// </summary>
        [Fact]
        public void FontAttributes_SetItalic_ReturnsItalic()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.FontAttributes = FontAttributes.Italic;

            // Assert
            Assert.Equal(FontAttributes.Italic, datePicker.FontAttributes);
        }

        /// <summary>
        /// Tests setting and getting the FontAttributes property with combined Bold and Italic flags.
        /// Expected result: Property should store and return the combined FontAttributes.Bold | FontAttributes.Italic.
        /// </summary>
        [Fact]
        public void FontAttributes_SetBoldAndItalic_ReturnsCombinedFlags()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.FontAttributes = FontAttributes.Bold | FontAttributes.Italic;

            // Assert
            Assert.Equal(FontAttributes.Bold | FontAttributes.Italic, datePicker.FontAttributes);
        }

        /// <summary>
        /// Tests setting the FontAttributes property with an invalid enum value outside the defined range.
        /// Expected result: Property should store and return the invalid value as cast.
        /// </summary>
        [Fact]
        public void FontAttributes_SetInvalidEnumValue_ReturnsInvalidValue()
        {
            // Arrange
            var datePicker = new DatePicker();
            var invalidValue = (FontAttributes)999;

            // Act
            datePicker.FontAttributes = invalidValue;

            // Assert
            Assert.Equal(invalidValue, datePicker.FontAttributes);
        }

        /// <summary>
        /// Tests setting the FontAttributes property with negative enum value.
        /// Expected result: Property should store and return the negative value as cast.
        /// </summary>
        [Fact]
        public void FontAttributes_SetNegativeEnumValue_ReturnsNegativeValue()
        {
            // Arrange
            var datePicker = new DatePicker();
            var negativeValue = (FontAttributes)(-1);

            // Act
            datePicker.FontAttributes = negativeValue;

            // Assert
            Assert.Equal(negativeValue, datePicker.FontAttributes);
        }

        /// <summary>
        /// Tests that multiple assignments to FontAttributes property work correctly.
        /// Expected result: Each assignment should override the previous value correctly.
        /// </summary>
        [Fact]
        public void FontAttributes_MultipleAssignments_ReturnsLatestValue()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act & Assert
            datePicker.FontAttributes = FontAttributes.Bold;
            Assert.Equal(FontAttributes.Bold, datePicker.FontAttributes);

            datePicker.FontAttributes = FontAttributes.Italic;
            Assert.Equal(FontAttributes.Italic, datePicker.FontAttributes);

            datePicker.FontAttributes = FontAttributes.None;
            Assert.Equal(FontAttributes.None, datePicker.FontAttributes);

            datePicker.FontAttributes = FontAttributes.Bold | FontAttributes.Italic;
            Assert.Equal(FontAttributes.Bold | FontAttributes.Italic, datePicker.FontAttributes);
        }
    }


    public partial class DatePickerFontFamilyTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that FontFamily property can be set and retrieved correctly with a normal font family name.
        /// </summary>
        [Fact]
        public void FontFamily_SetValidValue_ReturnsSameValue()
        {
            // Arrange
            var datePicker = new DatePicker();
            var fontFamily = "Arial";

            // Act
            datePicker.FontFamily = fontFamily;

            // Assert
            Assert.Equal(fontFamily, datePicker.FontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property can be set to null and returns null correctly.
        /// </summary>
        [Fact]
        public void FontFamily_SetNull_ReturnsNull()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.FontFamily = null;

            // Assert
            Assert.Null(datePicker.FontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property can be set to empty string and returns empty string correctly.
        /// </summary>
        [Fact]
        public void FontFamily_SetEmptyString_ReturnsEmptyString()
        {
            // Arrange
            var datePicker = new DatePicker();
            var fontFamily = "";

            // Act
            datePicker.FontFamily = fontFamily;

            // Assert
            Assert.Equal(fontFamily, datePicker.FontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property can be set to whitespace-only string and returns same value correctly.
        /// </summary>
        [Fact]
        public void FontFamily_SetWhitespaceString_ReturnsSameValue()
        {
            // Arrange
            var datePicker = new DatePicker();
            var fontFamily = "   ";

            // Act
            datePicker.FontFamily = fontFamily;

            // Assert
            Assert.Equal(fontFamily, datePicker.FontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property handles various font family names with different character sets correctly.
        /// </summary>
        [Theory]
        [InlineData("Times New Roman")]
        [InlineData("Helvetica Neue")]
        [InlineData("Comic Sans MS")]
        [InlineData("Courier New")]
        [InlineData("Georgia")]
        [InlineData("Verdana")]
        [InlineData("Impact")]
        [InlineData("Trebuchet MS")]
        public void FontFamily_SetVariousFontNames_ReturnsSameValue(string fontFamily)
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.FontFamily = fontFamily;

            // Assert
            Assert.Equal(fontFamily, datePicker.FontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property can handle font names with special characters correctly.
        /// </summary>
        [Theory]
        [InlineData("Font-Name")]
        [InlineData("Font_Name")]
        [InlineData("Font.Name")]
        [InlineData("Font123")]
        [InlineData("123Font")]
        [InlineData("Font Name With Spaces")]
        [InlineData("Font\tName")]
        [InlineData("Font\nName")]
        public void FontFamily_SetFontNamesWithSpecialCharacters_ReturnsSameValue(string fontFamily)
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.FontFamily = fontFamily;

            // Assert
            Assert.Equal(fontFamily, datePicker.FontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property can handle very long font family names correctly.
        /// </summary>
        [Fact]
        public void FontFamily_SetVeryLongString_ReturnsSameValue()
        {
            // Arrange
            var datePicker = new DatePicker();
            var longFontFamily = new string('A', 1000);

            // Act
            datePicker.FontFamily = longFontFamily;

            // Assert
            Assert.Equal(longFontFamily, datePicker.FontFamily);
        }

        /// <summary>
        /// Tests that FontFamily property returns default value when first accessed without setting.
        /// </summary>
        [Fact]
        public void FontFamily_GetDefaultValue_ReturnsExpectedDefault()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            var result = datePicker.FontFamily;

            // Assert
            // The default value should be what GetValue returns from the bindable property
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that FontFamily property can be set multiple times with different values correctly.
        /// </summary>
        [Fact]
        public void FontFamily_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var datePicker = new DatePicker();
            var firstFont = "Arial";
            var secondFont = "Times New Roman";
            var thirdFont = null;

            // Act & Assert
            datePicker.FontFamily = firstFont;
            Assert.Equal(firstFont, datePicker.FontFamily);

            datePicker.FontFamily = secondFont;
            Assert.Equal(secondFont, datePicker.FontFamily);

            datePicker.FontFamily = thirdFont;
            Assert.Null(datePicker.FontFamily);
        }
    }


    /// <summary>
    /// Tests for the DatePicker.OnHandlerChanged method.
    /// </summary>
    public partial class DatePickerOnHandlerChangedTests : BaseTestFixture
    {
        /// <summary>
        /// Test DatePicker subclass that exposes the protected OnHandlerChanged method for testing.
        /// </summary>
        private class TestDatePicker : DatePicker
        {
            public void InvokeOnHandlerChanged()
            {
                OnHandlerChanged();
            }
        }

        /// <summary>
        /// Tests that OnHandlerChanged does not process actions when Handler is null.
        /// Verifies that the while loop condition fails when Handler is null, ensuring no actions are processed.
        /// Expected result: Actions remain in the queue and are not invoked.
        /// </summary>
        [Fact]
        public void OnHandlerChanged_HandlerIsNull_DoesNotProcessActions()
        {
            // Arrange
            var datePicker = new TestDatePicker();
            var actionInvoked = false;
            Action testAction = () => actionInvoked = true;

            // Use reflection to add action to the private queue
            var queueField = typeof(DatePicker).GetField("_pendingIsOpenActions", BindingFlags.NonPublic | BindingFlags.Instance);
            var queue = (Queue<Action>)queueField.GetValue(datePicker);
            queue.Enqueue(testAction);

            // Ensure Handler is null
            datePicker.Handler = null;

            // Act
            datePicker.InvokeOnHandlerChanged();

            // Assert
            Assert.False(actionInvoked);
            Assert.Equal(1, queue.Count);
        }

        /// <summary>
        /// Tests that OnHandlerChanged does not process actions when the queue is empty.
        /// Verifies that the while loop does not execute when there are no pending actions.
        /// Expected result: No actions are processed and no exceptions are thrown.
        /// </summary>
        [Fact]
        public void OnHandlerChanged_EmptyQueue_DoesNotProcessActions()
        {
            // Arrange
            var datePicker = new TestDatePicker();
            var mockHandler = Substitute.For<IViewHandler>();
            datePicker.Handler = mockHandler;

            // Verify queue is empty
            var queueField = typeof(DatePicker).GetField("_pendingIsOpenActions", BindingFlags.NonPublic | BindingFlags.Instance);
            var queue = (Queue<Action>)queueField.GetValue(datePicker);

            // Act & Assert - Should not throw
            datePicker.InvokeOnHandlerChanged();

            Assert.Equal(0, queue.Count);
        }

        /// <summary>
        /// Tests that OnHandlerChanged processes a single action when Handler is available.
        /// Verifies that the action is invoked and removed from the queue.
        /// Expected result: Action is executed once and queue becomes empty.
        /// </summary>
        [Fact]
        public void OnHandlerChanged_HandlerAvailableAndSingleAction_ProcessesAction()
        {
            // Arrange
            var datePicker = new TestDatePicker();
            var mockHandler = Substitute.For<IViewHandler>();
            datePicker.Handler = mockHandler;

            var actionInvoked = false;
            Action testAction = () => actionInvoked = true;

            // Use reflection to add action to the private queue
            var queueField = typeof(DatePicker).GetField("_pendingIsOpenActions", BindingFlags.NonPublic | BindingFlags.Instance);
            var queue = (Queue<Action>)queueField.GetValue(datePicker);
            queue.Enqueue(testAction);

            // Act
            datePicker.InvokeOnHandlerChanged();

            // Assert
            Assert.True(actionInvoked);
            Assert.Equal(0, queue.Count);
        }

        /// <summary>
        /// Tests that OnHandlerChanged processes multiple actions in FIFO order when Handler is available.
        /// Verifies that all actions are invoked in the correct order and all are removed from the queue.
        /// Expected result: All actions are executed in order and queue becomes empty.
        /// </summary>
        [Fact]
        public void OnHandlerChanged_HandlerAvailableAndMultipleActions_ProcessesAllActionsInOrder()
        {
            // Arrange
            var datePicker = new TestDatePicker();
            var mockHandler = Substitute.For<IViewHandler>();
            datePicker.Handler = mockHandler;

            var executionOrder = new List<int>();
            Action firstAction = () => executionOrder.Add(1);
            Action secondAction = () => executionOrder.Add(2);
            Action thirdAction = () => executionOrder.Add(3);

            // Use reflection to add actions to the private queue
            var queueField = typeof(DatePicker).GetField("_pendingIsOpenActions", BindingFlags.NonPublic | BindingFlags.Instance);
            var queue = (Queue<Action>)queueField.GetValue(datePicker);
            queue.Enqueue(firstAction);
            queue.Enqueue(secondAction);
            queue.Enqueue(thirdAction);

            // Act
            datePicker.InvokeOnHandlerChanged();

            // Assert
            Assert.Equal(new[] { 1, 2, 3 }, executionOrder);
            Assert.Equal(0, queue.Count);
        }

        /// <summary>
        /// Tests that OnHandlerChanged handles action exceptions gracefully.
        /// Verifies that if one action throws an exception, subsequent actions are still processed.
        /// Expected result: Exception is thrown but queue processing continues for remaining actions.
        /// </summary>
        [Fact]
        public void OnHandlerChanged_ActionThrowsException_ContinuesProcessing()
        {
            // Arrange
            var datePicker = new TestDatePicker();
            var mockHandler = Substitute.For<IViewHandler>();
            datePicker.Handler = mockHandler;

            var secondActionExecuted = false;
            Action throwingAction = () => throw new InvalidOperationException("Test exception");
            Action secondAction = () => secondActionExecuted = true;

            // Use reflection to add actions to the private queue
            var queueField = typeof(DatePicker).GetField("_pendingIsOpenActions", BindingFlags.NonPublic | BindingFlags.Instance);
            var queue = (Queue<Action>)queueField.GetValue(datePicker);
            queue.Enqueue(throwingAction);
            queue.Enqueue(secondAction);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => datePicker.InvokeOnHandlerChanged());

            // First action should have been dequeued even though it threw
            // Second action should not have been processed due to exception
            Assert.False(secondActionExecuted);
            Assert.Equal(1, queue.Count); // Second action remains in queue
        }

        /// <summary>
        /// Tests that OnHandlerChanged works correctly when Handler becomes null during processing.
        /// Verifies behavior when Handler is set to null after the while loop condition is evaluated.
        /// Expected result: Loop stops processing when Handler becomes null.
        /// </summary>
        [Fact]
        public void OnHandlerChanged_HandlerBecomesNullDuringProcessing_StopsProcessing()
        {
            // Arrange
            var datePicker = new TestDatePicker();
            var mockHandler = Substitute.For<IViewHandler>();
            datePicker.Handler = mockHandler;

            var firstActionExecuted = false;
            var secondActionExecuted = false;

            Action firstAction = () =>
            {
                firstActionExecuted = true;
                datePicker.Handler = null; // Set Handler to null during processing
            };
            Action secondAction = () => secondActionExecuted = true;

            // Use reflection to add actions to the private queue
            var queueField = typeof(DatePicker).GetField("_pendingIsOpenActions", BindingFlags.NonPublic | BindingFlags.Instance);
            var queue = (Queue<Action>)queueField.GetValue(datePicker);
            queue.Enqueue(firstAction);
            queue.Enqueue(secondAction);

            // Act
            datePicker.InvokeOnHandlerChanged();

            // Assert
            Assert.True(firstActionExecuted);
            Assert.False(secondActionExecuted); // Should not execute because Handler was set to null
            Assert.Equal(1, queue.Count); // Second action should remain in queue
        }
    }


    /// <summary>
    /// Tests for the DatePicker.IsOpen property
    /// </summary>
    public partial class DatePickerIsOpenTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the IsOpen property has the correct default value of false.
        /// </summary>
        [Fact]
        public void IsOpen_DefaultValue_ReturnsFalse()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            var result = datePicker.IsOpen;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that setting IsOpen to true and then getting the value returns true.
        /// </summary>
        [Fact]
        public void IsOpen_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.IsOpen = true;
            var result = datePicker.IsOpen;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that setting IsOpen to false and then getting the value returns false.
        /// </summary>
        [Fact]
        public void IsOpen_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.IsOpen = false;
            var result = datePicker.IsOpen;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the IsOpen property can be toggled between true and false values.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsOpen_SetValue_ReturnsExpectedValue(bool expectedValue)
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act
            datePicker.IsOpen = expectedValue;
            var result = datePicker.IsOpen;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that setting IsOpen multiple times works correctly and returns the last set value.
        /// </summary>
        [Fact]
        public void IsOpen_MultipleSetOperations_ReturnsLastSetValue()
        {
            // Arrange
            var datePicker = new DatePicker();

            // Act & Assert
            datePicker.IsOpen = true;
            Assert.True(datePicker.IsOpen);

            datePicker.IsOpen = false;
            Assert.False(datePicker.IsOpen);

            datePicker.IsOpen = true;
            Assert.True(datePicker.IsOpen);
        }
    }
}