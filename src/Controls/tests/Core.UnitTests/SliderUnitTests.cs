#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;

using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class SliderUnitTests : BaseTestFixture
    {
        [Fact]
        public void TestConstructor()
        {
            var slider = new Slider(20, 200, 50);

            Assert.Equal(20, slider.Minimum);
            Assert.Equal(200, slider.Maximum);
            Assert.Equal(50, slider.Value);
        }

        [Fact]
        public void TestInvalidConstructor()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Slider(10, 5, 10));
        }

        [Fact]
        public void TestConstructorClamping()
        {
            Slider slider = new Slider(50, 100, 0);

            Assert.Equal(50, slider.Value);
        }

        [Fact]
        public void TestMinValueClamp()
        {
            Slider slider = new Slider(0, 100, 0);

            slider.Minimum = 10;

            Assert.Equal(10, slider.Value);
            Assert.Equal(10, slider.Minimum);
        }

        [Fact]
        public void TestMaxValueClamp()
        {
            Slider slider = new Slider(0, 100, 100);

            slider.Maximum = 10;

            Assert.Equal(10, slider.Value);
            Assert.Equal(10, slider.Maximum);
        }

        [Fact]
        public void TestInvalidMaxValue()
        {
            var slider = new Slider();
            slider.Maximum = slider.Minimum - 1;
        }

        [Fact]
        public void TestInvalidMinValue()
        {
            var slider = new Slider();
            slider.Minimum = slider.Maximum + 1;
        }

        [Fact]
        public void TestValueChanged()
        {
            var slider = new Slider();
            var changed = false;

            slider.ValueChanged += (sender, arg) => changed = true;

            slider.Value += 1;

            Assert.True(changed);
        }

        [Theory]
        [InlineData(0.0, 1.0)]
        [InlineData(1.0, 0.5)]
        public void SliderValueChangedEventArgs(double initialValue, double finalValue)
        {
            var slider = new Slider
            {
                Minimum = 0.0,
                Maximum = 1.0,
                Value = initialValue
            };

            Slider sliderFromSender = null;
            double oldValue = 0.0;
            double newValue = 0.0;

            slider.ValueChanged += (s, e) =>
            {
                sliderFromSender = (Slider)s;
                oldValue = e.OldValue;
                newValue = e.NewValue;
            };

            slider.Value = finalValue;

            Assert.Equal(slider, sliderFromSender);
            Assert.Equal(initialValue, oldValue);
            Assert.Equal(finalValue, newValue);
        }

        [Fact]
        public void TestDragStarted()
        {
            var slider = new Slider();
            var started = false;

            slider.DragStarted += (sender, arg) => started = true;
            ((ISliderController)slider).SendDragStarted();

            Assert.True(started);
        }

        [Fact]
        public void TestDragCompleted()
        {
            var slider = new Slider();
            var completed = false;

            slider.DragCompleted += (sender, arg) => completed = true;
            ((ISliderController)slider).SendDragCompleted();

            Assert.True(completed);
        }
    }

    public partial class SliderTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that MinimumTrackColor property getter returns the default null value when not set.
        /// </summary>
        [Fact]
        public void MinimumTrackColor_WhenNotSet_ReturnsNull()
        {
            // Arrange
            var slider = new Slider();

            // Act
            var result = slider.MinimumTrackColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that MinimumTrackColor property getter returns the correct value after setting a color.
        /// </summary>
        [Theory]
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Semi-transparent gray
        public void MinimumTrackColor_WhenSetToValidColor_ReturnsCorrectValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var slider = new Slider();
            var expectedColor = Color.FromRgba(red, green, blue, alpha);

            // Act
            slider.MinimumTrackColor = expectedColor;
            var result = slider.MinimumTrackColor;

            // Assert
            Assert.Equal(expectedColor.Red, result.Red);
            Assert.Equal(expectedColor.Green, result.Green);
            Assert.Equal(expectedColor.Blue, result.Blue);
            Assert.Equal(expectedColor.Alpha, result.Alpha);
        }

        /// <summary>
        /// Tests that MinimumTrackColor property can be set to null and returns null.
        /// </summary>
        [Fact]
        public void MinimumTrackColor_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var slider = new Slider();
            slider.MinimumTrackColor = Colors.Red; // Set to a color first

            // Act
            slider.MinimumTrackColor = null;
            var result = slider.MinimumTrackColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that MinimumTrackColor property getter returns the correct value when using predefined colors.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetPredefinedColors))]
        public void MinimumTrackColor_WhenSetToPredefinedColor_ReturnsCorrectValue(Color expectedColor)
        {
            // Arrange
            var slider = new Slider();

            // Act
            slider.MinimumTrackColor = expectedColor;
            var result = slider.MinimumTrackColor;

            // Assert
            Assert.Equal(expectedColor.Red, result.Red);
            Assert.Equal(expectedColor.Green, result.Green);
            Assert.Equal(expectedColor.Blue, result.Blue);
            Assert.Equal(expectedColor.Alpha, result.Alpha);
        }

        /// <summary>
        /// Tests that MinimumTrackColor property can be set multiple times and returns the latest value.
        /// </summary>
        [Fact]
        public void MinimumTrackColor_WhenSetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var slider = new Slider();

            // Act & Assert
            slider.MinimumTrackColor = Colors.Red;
            Assert.Equal(Colors.Red.Red, slider.MinimumTrackColor.Red);
            Assert.Equal(Colors.Red.Green, slider.MinimumTrackColor.Green);
            Assert.Equal(Colors.Red.Blue, slider.MinimumTrackColor.Blue);
            Assert.Equal(Colors.Red.Alpha, slider.MinimumTrackColor.Alpha);

            slider.MinimumTrackColor = Colors.Blue;
            Assert.Equal(Colors.Blue.Red, slider.MinimumTrackColor.Red);
            Assert.Equal(Colors.Blue.Green, slider.MinimumTrackColor.Green);
            Assert.Equal(Colors.Blue.Blue, slider.MinimumTrackColor.Blue);
            Assert.Equal(Colors.Blue.Alpha, slider.MinimumTrackColor.Alpha);

            slider.MinimumTrackColor = null;
            Assert.Null(slider.MinimumTrackColor);
        }

        public static TheoryData<Color> GetPredefinedColors()
        {
            return new TheoryData<Color>
            {
                Colors.Red,
                Colors.Green,
                Colors.Blue,
                Colors.Black,
                Colors.White,
                Colors.Transparent,
                Colors.Yellow,
                Colors.Cyan,
                Colors.Magenta
            };
        }

        /// <summary>
        /// Tests that DragCompletedCommand getter returns null when no command is set (default value).
        /// </summary>
        [Fact]
        public void DragCompletedCommand_WhenNotSet_ReturnsNull()
        {
            // Arrange
            var slider = new Slider();

            // Act
            var result = slider.DragCompletedCommand;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that DragCompletedCommand getter returns the same command that was set via setter.
        /// </summary>
        [Fact]
        public void DragCompletedCommand_WhenSet_ReturnsSetValue()
        {
            // Arrange
            var slider = new Slider();
            var command = Substitute.For<ICommand>();

            // Act
            slider.DragCompletedCommand = command;
            var result = slider.DragCompletedCommand;

            // Assert
            Assert.Same(command, result);
        }

        /// <summary>
        /// Tests that DragCompletedCommand can be set to null and getter returns null.
        /// </summary>
        [Fact]
        public void DragCompletedCommand_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var slider = new Slider();
            var command = Substitute.For<ICommand>();
            slider.DragCompletedCommand = command; // Set initially to non-null

            // Act
            slider.DragCompletedCommand = null;
            var result = slider.DragCompletedCommand;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that DragCompletedCommand can be set to different commands sequentially.
        /// </summary>
        [Fact]
        public void DragCompletedCommand_WhenSetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var slider = new Slider();
            var firstCommand = Substitute.For<ICommand>();
            var secondCommand = Substitute.For<ICommand>();

            // Act
            slider.DragCompletedCommand = firstCommand;
            var firstResult = slider.DragCompletedCommand;

            slider.DragCompletedCommand = secondCommand;
            var secondResult = slider.DragCompletedCommand;

            // Assert
            Assert.Same(firstCommand, firstResult);
            Assert.Same(secondCommand, secondResult);
            Assert.NotSame(firstCommand, secondCommand);
        }

        /// <summary>
        /// Tests that the On method returns a valid IPlatformElementConfiguration instance for a given platform type.
        /// Input conditions: Valid platform type implementing IConfigPlatform.
        /// Expected result: Non-null IPlatformElementConfiguration instance of correct type.
        /// </summary>
        [Fact]
        public void On_ValidPlatformType_ReturnsNonNullConfiguration()
        {
            // Arrange
            var slider = new Slider();

            // Act
            var result = slider.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, Slider>>(result);
        }

        /// <summary>
        /// Tests that multiple calls to On with the same platform type return the same instance (caching behavior).
        /// Input conditions: Same platform type called multiple times.
        /// Expected result: Same IPlatformElementConfiguration instance returned on subsequent calls.
        /// </summary>
        [Fact]
        public void On_SamePlatformTypeCalledMultipleTimes_ReturnsSameInstance()
        {
            // Arrange
            var slider = new Slider();

            // Act
            var firstCall = slider.On<TestPlatform>();
            var secondCall = slider.On<TestPlatform>();

            // Assert
            Assert.NotNull(firstCall);
            Assert.NotNull(secondCall);
            Assert.Same(firstCall, secondCall);
        }

        /// <summary>
        /// Tests that different platform types return different configuration instances.
        /// Input conditions: Two different platform types.
        /// Expected result: Different IPlatformElementConfiguration instances for different platforms.
        /// </summary>
        [Fact]
        public void On_DifferentPlatformTypes_ReturnsDifferentInstances()
        {
            // Arrange
            var slider = new Slider();

            // Act
            var testPlatformConfig = slider.On<TestPlatform>();
            var anotherPlatformConfig = slider.On<AnotherTestPlatform>();

            // Assert
            Assert.NotNull(testPlatformConfig);
            Assert.NotNull(anotherPlatformConfig);
            Assert.NotSame(testPlatformConfig, anotherPlatformConfig);
        }

        /// <summary>
        /// Tests that On method works correctly after setting slider properties.
        /// Input conditions: Slider with modified properties.
        /// Expected result: Valid configuration instance regardless of slider state.
        /// </summary>
        [Fact]
        public void On_SliderWithModifiedProperties_ReturnsValidConfiguration()
        {
            // Arrange
            var slider = new Slider(0, 100, 50)
            {
                MinimumTrackColor = Colors.Red,
                MaximumTrackColor = Colors.Blue,
                ThumbColor = Colors.Green
            };

            // Act
            var result = slider.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, Slider>>(result);
        }

        private class TestPlatform : IConfigPlatform
        {
        }

        private class AnotherTestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Tests the Slider constructor with valid parameters in the normal case where max > current Minimum (0).
        /// Verifies that Minimum, Maximum, and Value are set correctly.
        /// </summary>
        [Theory]
        [InlineData(1.0, 10.0, 5.0)]
        [InlineData(-5.0, 5.0, 0.0)]
        [InlineData(0.0, 1.0, 0.5)]
        [InlineData(10.0, 100.0, 50.0)]
        public void Slider_ValidParameters_SetsPropertiesCorrectly(double min, double max, double val)
        {
            // Arrange & Act
            var slider = new Slider(min, max, val);

            // Assert
            Assert.Equal(min, slider.Minimum);
            Assert.Equal(max, slider.Maximum);
            Assert.Equal(val, slider.Value);
        }

        /// <summary>
        /// Tests the Slider constructor with max <= current Minimum (0) to cover the uncovered else branch.
        /// This case sets Minimum first, then Maximum.
        /// </summary>
        [Theory]
        [InlineData(-10.0, -5.0, -7.0)]
        [InlineData(-100.0, 0.0, -50.0)]
        [InlineData(-1.0, -0.5, -0.8)]
        public void Slider_MaxLessThanOrEqualToCurrentMinimum_SetsPropertiesCorrectly(double min, double max, double val)
        {
            // Arrange & Act
            var slider = new Slider(min, max, val);

            // Assert
            Assert.Equal(min, slider.Minimum);
            Assert.Equal(max, slider.Maximum);
            Assert.Equal(val, slider.Value);
        }

        /// <summary>
        /// Tests the Slider constructor throws ArgumentOutOfRangeException when min >= max.
        /// Verifies the correct exception type and parameter name.
        /// </summary>
        [Theory]
        [InlineData(10.0, 5.0, 7.0)]
        [InlineData(5.0, 5.0, 5.0)]
        [InlineData(0.0, -1.0, 0.0)]
        [InlineData(100.0, 50.0, 75.0)]
        public void Slider_MinGreaterThanOrEqualToMax_ThrowsArgumentOutOfRangeException(double min, double max, double val)
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Slider(min, max, val));
            Assert.Equal("min", exception.ParamName);
        }

        /// <summary>
        /// Tests the Slider constructor clamps the value when it's outside the [min, max] range.
        /// Verifies that values below min are clamped to min and values above max are clamped to max.
        /// </summary>
        [Theory]
        [InlineData(1.0, 10.0, -5.0, 1.0)] // val < min, should clamp to min
        [InlineData(1.0, 10.0, 15.0, 10.0)] // val > max, should clamp to max
        [InlineData(-10.0, -5.0, -15.0, -10.0)] // val < min (negative range)
        [InlineData(-10.0, -5.0, 0.0, -5.0)] // val > max (negative range)
        [InlineData(0.0, 1.0, 2.0, 1.0)] // val > max (small positive range)
        public void Slider_ValueOutsideRange_ClampsValueCorrectly(double min, double max, double val, double expectedValue)
        {
            // Arrange & Act
            var slider = new Slider(min, max, val);

            // Assert
            Assert.Equal(min, slider.Minimum);
            Assert.Equal(max, slider.Maximum);
            Assert.Equal(expectedValue, slider.Value);
        }

        /// <summary>
        /// Tests the Slider constructor with extreme double values.
        /// Verifies behavior with very large and very small double values.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MaxValue, 0.0)]
        [InlineData(-1000000.0, 1000000.0, 500000.0)]
        [InlineData(double.MinValue, -1.0, double.MinValue)]
        [InlineData(1.0, double.MaxValue, double.MaxValue)]
        public void Slider_ExtremeDoubleValues_HandlesCorrectly(double min, double max, double val)
        {
            // Arrange & Act
            var slider = new Slider(min, max, val);

            // Assert
            Assert.Equal(min, slider.Minimum);
            Assert.Equal(max, slider.Maximum);
            var expectedValue = val < min ? min : (val > max ? max : val);
            Assert.Equal(expectedValue, slider.Value);
        }

        /// <summary>
        /// Tests the Slider constructor with special double values like NaN and Infinity.
        /// Verifies proper handling of these edge cases.
        /// </summary>
        [Theory]
        [InlineData(1.0, 10.0, double.NaN)]
        [InlineData(1.0, 10.0, double.PositiveInfinity)]
        [InlineData(1.0, 10.0, double.NegativeInfinity)]
        public void Slider_SpecialDoubleValues_HandlesCorrectly(double min, double max, double val)
        {
            // Arrange & Act
            var slider = new Slider(min, max, val);

            // Assert
            Assert.Equal(min, slider.Minimum);
            Assert.Equal(max, slider.Maximum);

            // For special values, the clamping behavior depends on the Clamp method implementation
            if (double.IsNaN(val))
            {
                // NaN comparisons are always false, so NaN should be returned as-is by Clamp
                Assert.True(double.IsNaN(slider.Value));
            }
            else if (double.IsPositiveInfinity(val))
            {
                // Positive infinity > max, so should clamp to max
                Assert.Equal(max, slider.Value);
            }
            else if (double.IsNegativeInfinity(val))
            {
                // Negative infinity < min, so should clamp to min
                Assert.Equal(min, slider.Value);
            }
        }

        /// <summary>
        /// Tests the Slider constructor with special double values for min and max parameters.
        /// Verifies that infinity values can be used as bounds.
        /// </summary>
        [Theory]
        [InlineData(double.NegativeInfinity, double.PositiveInfinity, 0.0)]
        [InlineData(double.NegativeInfinity, 100.0, 50.0)]
        [InlineData(-100.0, double.PositiveInfinity, 0.0)]
        public void Slider_InfinityBounds_HandlesCorrectly(double min, double max, double val)
        {
            // Arrange & Act
            var slider = new Slider(min, max, val);

            // Assert
            Assert.Equal(min, slider.Minimum);
            Assert.Equal(max, slider.Maximum);
            Assert.Equal(val, slider.Value);
        }

        /// <summary>
        /// Tests the Slider constructor with NaN values for min parameter.
        /// Should throw ArgumentOutOfRangeException since NaN >= max is always false but NaN < max is also false.
        /// </summary>
        [Fact]
        public void Slider_NaNMinimum_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Slider(double.NaN, 10.0, 5.0));
            Assert.Equal("min", exception.ParamName);
        }

        /// <summary>
        /// Tests the Slider constructor with NaN values for max parameter.
        /// Should throw ArgumentOutOfRangeException since min >= NaN comparison behavior.
        /// </summary>
        [Fact]
        public void Slider_NaNMaximum_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Slider(1.0, double.NaN, 5.0));
            Assert.Equal("min", exception.ParamName);
        }

        /// <summary>
        /// Tests the Slider constructor with zero values.
        /// Verifies proper handling of zero as a boundary value.
        /// </summary>
        [Theory]
        [InlineData(0.0, 1.0, 0.5)]
        [InlineData(-1.0, 0.0, -0.5)]
        [InlineData(0.0, 0.1, 0.05)]
        public void Slider_ZeroBoundaryValues_HandlesCorrectly(double min, double max, double val)
        {
            // Arrange & Act
            var slider = new Slider(min, max, val);

            // Assert
            Assert.Equal(min, slider.Minimum);
            Assert.Equal(max, slider.Maximum);
            Assert.Equal(val, slider.Value);
        }
    }


    public partial class SliderMaximumTrackColorTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that MaximumTrackColor returns the default value (null) when not explicitly set.
        /// Verifies the getter functionality and default value behavior.
        /// Expected result: MaximumTrackColor should return null.
        /// </summary>
        [Fact]
        public void MaximumTrackColor_DefaultValue_ReturnsNull()
        {
            // Arrange
            var slider = new Slider();

            // Act
            var result = slider.MaximumTrackColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that MaximumTrackColor correctly sets and retrieves a non-null Color value.
        /// Verifies both setter and getter functionality with a standard color.
        /// Expected result: MaximumTrackColor should return the same Color value that was set.
        /// </summary>
        [Fact]
        public void MaximumTrackColor_SetNonNullColor_ReturnsSetValue()
        {
            // Arrange
            var slider = new Slider();
            var expectedColor = Colors.Red;

            // Act
            slider.MaximumTrackColor = expectedColor;
            var result = slider.MaximumTrackColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that MaximumTrackColor can be set to null after being set to a color value.
        /// Verifies that null assignment works correctly.
        /// Expected result: MaximumTrackColor should return null after being set to null.
        /// </summary>
        [Fact]
        public void MaximumTrackColor_SetToNull_ReturnsNull()
        {
            // Arrange
            var slider = new Slider();
            slider.MaximumTrackColor = Colors.Blue;

            // Act
            slider.MaximumTrackColor = null;
            var result = slider.MaximumTrackColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests MaximumTrackColor with various standard colors to ensure proper handling of different Color values.
        /// Verifies that the property correctly stores and retrieves different color instances.
        /// Expected result: Each color value should be correctly set and retrieved.
        /// </summary>
        [Theory]
        [InlineData(nameof(Colors.Red))]
        [InlineData(nameof(Colors.Green))]
        [InlineData(nameof(Colors.Blue))]
        [InlineData(nameof(Colors.Transparent))]
        [InlineData(nameof(Colors.Black))]
        [InlineData(nameof(Colors.White))]
        public void MaximumTrackColor_SetStandardColors_ReturnsCorrectValue(string colorName)
        {
            // Arrange
            var slider = new Slider();
            var expectedColor = GetColorByName(colorName);

            // Act
            slider.MaximumTrackColor = expectedColor;
            var result = slider.MaximumTrackColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests MaximumTrackColor with custom Color instances created with specific RGBA values.
        /// Verifies that custom colors with various alpha, red, green, and blue values are handled correctly.
        /// Expected result: Custom color values should be correctly set and retrieved with exact RGBA components.
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Fully transparent black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // Fully opaque white
        [InlineData(0.5f, 0.3f, 0.7f, 0.9f)] // Custom color with mixed values
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Fully opaque red
        [InlineData(0.0f, 1.0f, 0.0f, 0.5f)] // Semi-transparent green
        public void MaximumTrackColor_SetCustomColors_ReturnsCorrectValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var slider = new Slider();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            slider.MaximumTrackColor = expectedColor;
            var result = slider.MaximumTrackColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that MaximumTrackColor property can be overwritten multiple times with different values.
        /// Verifies that subsequent assignments correctly update the stored value.
        /// Expected result: The property should always return the most recently assigned color value.
        /// </summary>
        [Fact]
        public void MaximumTrackColor_MultipleAssignments_ReturnsLatestValue()
        {
            // Arrange
            var slider = new Slider();
            var firstColor = Colors.Red;
            var secondColor = Colors.Blue;
            var thirdColor = Colors.Green;

            // Act & Assert - First assignment
            slider.MaximumTrackColor = firstColor;
            Assert.Equal(firstColor, slider.MaximumTrackColor);

            // Act & Assert - Second assignment
            slider.MaximumTrackColor = secondColor;
            Assert.Equal(secondColor, slider.MaximumTrackColor);

            // Act & Assert - Third assignment
            slider.MaximumTrackColor = thirdColor;
            Assert.Equal(thirdColor, slider.MaximumTrackColor);
        }

        private static Color GetColorByName(string colorName)
        {
            return colorName switch
            {
                nameof(Colors.Red) => Colors.Red,
                nameof(Colors.Green) => Colors.Green,
                nameof(Colors.Blue) => Colors.Blue,
                nameof(Colors.Transparent) => Colors.Transparent,
                nameof(Colors.Black) => Colors.Black,
                nameof(Colors.White) => Colors.White,
                _ => throw new ArgumentException($"Unknown color name: {colorName}")
            };
        }
    }


    public partial class SliderThumbColorTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that ThumbColor getter returns null by default when no value has been set.
        /// </summary>
        [Fact]
        public void ThumbColor_GetDefaultValue_ReturnsNull()
        {
            // Arrange
            var slider = new Slider();

            // Act
            Color result = slider.ThumbColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ThumbColor setter accepts null value and getter returns null.
        /// </summary>
        [Fact]
        public void ThumbColor_SetNull_GetterReturnsNull()
        {
            // Arrange
            var slider = new Slider();

            // Act
            slider.ThumbColor = null;
            Color result = slider.ThumbColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ThumbColor setter and getter work correctly with various predefined colors.
        /// </summary>
        /// <param name="expectedColor">The color to set and verify</param>
        [Theory]
        [InlineData(null)]
        public void ThumbColor_SetPredefinedColors_GetterReturnsExpectedValue(Color expectedColor)
        {
            // Arrange
            var slider = new Slider();

            // Act
            slider.ThumbColor = expectedColor;
            Color result = slider.ThumbColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that ThumbColor setter and getter work correctly with Colors.Red.
        /// </summary>
        [Fact]
        public void ThumbColor_SetRed_GetterReturnsRed()
        {
            // Arrange
            var slider = new Slider();
            var expectedColor = Colors.Red;

            // Act
            slider.ThumbColor = expectedColor;
            Color result = slider.ThumbColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that ThumbColor setter and getter work correctly with Colors.Blue.
        /// </summary>
        [Fact]
        public void ThumbColor_SetBlue_GetterReturnsBlue()
        {
            // Arrange
            var slider = new Slider();
            var expectedColor = Colors.Blue;

            // Act
            slider.ThumbColor = expectedColor;
            Color result = slider.ThumbColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that ThumbColor setter and getter work correctly with Colors.Green.
        /// </summary>
        [Fact]
        public void ThumbColor_SetGreen_GetterReturnsGreen()
        {
            // Arrange
            var slider = new Slider();
            var expectedColor = Colors.Green;

            // Act
            slider.ThumbColor = expectedColor;
            Color result = slider.ThumbColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that ThumbColor setter and getter work correctly with Colors.Transparent.
        /// </summary>
        [Fact]
        public void ThumbColor_SetTransparent_GetterReturnsTransparent()
        {
            // Arrange
            var slider = new Slider();
            var expectedColor = Colors.Transparent;

            // Act
            slider.ThumbColor = expectedColor;
            Color result = slider.ThumbColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that ThumbColor setter and getter work correctly with Colors.White.
        /// </summary>
        [Fact]
        public void ThumbColor_SetWhite_GetterReturnsWhite()
        {
            // Arrange
            var slider = new Slider();
            var expectedColor = Colors.White;

            // Act
            slider.ThumbColor = expectedColor;
            Color result = slider.ThumbColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that ThumbColor setter and getter work correctly with Colors.Black.
        /// </summary>
        [Fact]
        public void ThumbColor_SetBlack_GetterReturnsBlack()
        {
            // Arrange
            var slider = new Slider();
            var expectedColor = Colors.Black;

            // Act
            slider.ThumbColor = expectedColor;
            Color result = slider.ThumbColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that ThumbColor setter and getter work correctly with custom Color values created with RGBA components.
        /// </summary>
        [Fact]
        public void ThumbColor_SetCustomRgbaColor_GetterReturnsExpectedColor()
        {
            // Arrange
            var slider = new Slider();
            var expectedColor = new Color(0.5f, 0.7f, 0.3f, 0.8f);

            // Act
            slider.ThumbColor = expectedColor;
            Color result = slider.ThumbColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that ThumbColor setter and getter work correctly with custom Color values created with RGB components (full alpha).
        /// </summary>
        [Fact]
        public void ThumbColor_SetCustomRgbColor_GetterReturnsExpectedColor()
        {
            // Arrange
            var slider = new Slider();
            var expectedColor = new Color(0.2f, 0.4f, 0.9f);

            // Act
            slider.ThumbColor = expectedColor;
            Color result = slider.ThumbColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that ThumbColor setter and getter work correctly with fully transparent custom color (alpha = 0).
        /// </summary>
        [Fact]
        public void ThumbColor_SetFullyTransparentColor_GetterReturnsExpectedColor()
        {
            // Arrange
            var slider = new Slider();
            var expectedColor = new Color(1.0f, 0.5f, 0.0f, 0.0f);

            // Act
            slider.ThumbColor = expectedColor;
            Color result = slider.ThumbColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that ThumbColor setter and getter work correctly with semi-transparent custom color.
        /// </summary>
        [Fact]
        public void ThumbColor_SetSemiTransparentColor_GetterReturnsExpectedColor()
        {
            // Arrange
            var slider = new Slider();
            var expectedColor = new Color(0.8f, 0.2f, 0.6f, 0.5f);

            // Act
            slider.ThumbColor = expectedColor;
            Color result = slider.ThumbColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that ThumbColor can be set and retrieved multiple times with different values.
        /// </summary>
        [Fact]
        public void ThumbColor_SetMultipleTimes_GetterReturnsLatestValue()
        {
            // Arrange
            var slider = new Slider();
            var firstColor = Colors.Red;
            var secondColor = Colors.Blue;
            var thirdColor = new Color(0.1f, 0.2f, 0.3f, 0.4f);

            // Act & Assert - First color
            slider.ThumbColor = firstColor;
            Assert.Equal(firstColor, slider.ThumbColor);

            // Act & Assert - Second color
            slider.ThumbColor = secondColor;
            Assert.Equal(secondColor, slider.ThumbColor);

            // Act & Assert - Third color
            slider.ThumbColor = thirdColor;
            Assert.Equal(thirdColor, slider.ThumbColor);

            // Act & Assert - Back to null
            slider.ThumbColor = null;
            Assert.Null(slider.ThumbColor);
        }

        /// <summary>
        /// Tests that ThumbColor setter and getter work correctly with boundary color values (min and max RGBA).
        /// </summary>
        [Fact]
        public void ThumbColor_SetBoundaryValues_GetterReturnsExpectedColor()
        {
            // Arrange
            var slider = new Slider();
            var minColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            var maxColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

            // Act & Assert - Min values
            slider.ThumbColor = minColor;
            Assert.Equal(minColor, slider.ThumbColor);

            // Act & Assert - Max values
            slider.ThumbColor = maxColor;
            Assert.Equal(maxColor, slider.ThumbColor);
        }
    }


    public partial class SliderThumbImageSourceTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that ThumbImageSource returns the default value (null) when no value has been set.
        /// This test verifies the getter returns the expected default value from the bindable property system.
        /// Expected result: ThumbImageSource should return null.
        /// </summary>
        [Fact]
        public void ThumbImageSource_DefaultValue_ReturnsNull()
        {
            // Arrange
            var slider = new Slider();

            // Act
            var result = slider.ThumbImageSource;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ThumbImageSource correctly stores and retrieves a non-null ImageSource value.
        /// This test verifies the property getter returns the same value that was set.
        /// Expected result: ThumbImageSource should return the same ImageSource instance that was set.
        /// </summary>
        [Fact]
        public void ThumbImageSource_SetNonNullValue_ReturnsSetValue()
        {
            // Arrange
            var slider = new Slider();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            slider.ThumbImageSource = mockImageSource;
            var result = slider.ThumbImageSource;

            // Assert
            Assert.Same(mockImageSource, result);
        }

        /// <summary>
        /// Tests that ThumbImageSource can be explicitly set to null and returns null when retrieved.
        /// This test verifies null assignment works correctly through the bindable property system.
        /// Expected result: ThumbImageSource should return null after being set to null.
        /// </summary>
        [Fact]
        public void ThumbImageSource_SetNullValue_ReturnsNull()
        {
            // Arrange
            var slider = new Slider();
            var mockImageSource = Substitute.For<ImageSource>();
            slider.ThumbImageSource = mockImageSource; // Set a value first

            // Act
            slider.ThumbImageSource = null;
            var result = slider.ThumbImageSource;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that ThumbImageSource correctly handles multiple assignments of different ImageSource values.
        /// This test verifies the property correctly updates and returns the most recently set value.
        /// Expected result: ThumbImageSource should return the last ImageSource value that was assigned.
        /// </summary>
        [Fact]
        public void ThumbImageSource_MultipleAssignments_ReturnsLatestValue()
        {
            // Arrange
            var slider = new Slider();
            var firstImageSource = Substitute.For<ImageSource>();
            var secondImageSource = Substitute.For<ImageSource>();

            // Act & Assert - First assignment
            slider.ThumbImageSource = firstImageSource;
            Assert.Same(firstImageSource, slider.ThumbImageSource);

            // Act & Assert - Second assignment
            slider.ThumbImageSource = secondImageSource;
            Assert.Same(secondImageSource, slider.ThumbImageSource);
        }

        /// <summary>
        /// Tests that ThumbImageSource getter works correctly after construction with parameters.
        /// This test verifies the property works correctly even when the Slider is constructed with min/max/value parameters.
        /// Expected result: ThumbImageSource should return null by default, and correctly store/retrieve assigned values.
        /// </summary>
        [Fact]
        public void ThumbImageSource_AfterParameterizedConstruction_WorksCorrectly()
        {
            // Arrange
            var slider = new Slider(0, 100, 50);
            var mockImageSource = Substitute.For<ImageSource>();

            // Act & Assert - Default value
            Assert.Null(slider.ThumbImageSource);

            // Act & Assert - Set and get value
            slider.ThumbImageSource = mockImageSource;
            Assert.Same(mockImageSource, slider.ThumbImageSource);
        }
    }


    public partial class SliderDragStartedCommandTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that DragStartedCommand getter returns null when no command is set.
        /// Verifies the default state of the property.
        /// </summary>
        [Fact]
        public void DragStartedCommand_WhenNotSet_ReturnsNull()
        {
            // Arrange
            var slider = new Slider();

            // Act
            var command = slider.DragStartedCommand;

            // Assert
            Assert.Null(command);
        }

        /// <summary>
        /// Tests that DragStartedCommand getter returns the command that was set.
        /// Verifies that the property correctly stores and retrieves ICommand instances.
        /// </summary>
        [Fact]
        public void DragStartedCommand_WhenSet_ReturnsSetCommand()
        {
            // Arrange
            var slider = new Slider();
            var mockCommand = Substitute.For<ICommand>();

            // Act
            slider.DragStartedCommand = mockCommand;
            var retrievedCommand = slider.DragStartedCommand;

            // Assert
            Assert.Same(mockCommand, retrievedCommand);
        }

        /// <summary>
        /// Tests that DragStartedCommand setter accepts null values.
        /// Verifies that the property can be cleared by setting it to null.
        /// </summary>
        [Fact]
        public void DragStartedCommand_SetToNull_AcceptsNull()
        {
            // Arrange
            var slider = new Slider();
            var mockCommand = Substitute.For<ICommand>();
            slider.DragStartedCommand = mockCommand;

            // Act
            slider.DragStartedCommand = null;
            var retrievedCommand = slider.DragStartedCommand;

            // Assert
            Assert.Null(retrievedCommand);
        }

        /// <summary>
        /// Tests that DragStartedCommand can be set multiple times with different commands.
        /// Verifies that subsequent assignments overwrite the previous value.
        /// </summary>
        [Fact]
        public void DragStartedCommand_SetMultipleTimes_ReturnsLatestCommand()
        {
            // Arrange
            var slider = new Slider();
            var firstCommand = Substitute.For<ICommand>();
            var secondCommand = Substitute.For<ICommand>();

            // Act
            slider.DragStartedCommand = firstCommand;
            slider.DragStartedCommand = secondCommand;
            var retrievedCommand = slider.DragStartedCommand;

            // Assert
            Assert.Same(secondCommand, retrievedCommand);
            Assert.NotSame(firstCommand, retrievedCommand);
        }
    }
}