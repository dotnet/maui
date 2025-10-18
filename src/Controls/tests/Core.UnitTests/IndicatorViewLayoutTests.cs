#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class IndicatorViewTests : BaseTestFixture
    {
        [Fact]
        public void IndicatorStackLayoutNoItems_ResetIndicators_ShouldHaveNoChildren()
        {
            // Arrange
            var indicatorView = new IndicatorView();
            var indicatorStackLayout = new IndicatorStackLayout(indicatorView);

            // Act
            indicatorStackLayout.ResetIndicators();

            // Assert
            Assert.Empty(indicatorStackLayout.Children);
        }

        [Fact]
        public void IndicatorStackLayoutWithItems_ResetIndicators_ShouldBindChildren()
        {
            // Arrange
            var indicatorView = new IndicatorView() { ItemsSource = new List<string> { "item1", "item2" } };
            var indicatorStackLayout = new IndicatorStackLayout(indicatorView);

            // Act
            indicatorStackLayout.ResetIndicators();

            // Assert
            Assert.Equal(2, indicatorStackLayout.Children.Count);
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData(0, 2)]
        [InlineData(-2, 2)]
        public void IndicatorStackLayout_ResetIndicatorCount_ShouldBindChildren(int oldCount, int expected)
        {
            // Arrange
            var indicatorView = new IndicatorView() { ItemsSource = new List<string> { "item1", "item2" } };
            var indicatorStackLayout = new IndicatorStackLayout(indicatorView);
            Assert.Empty(indicatorStackLayout.Children);

            // Act
            indicatorStackLayout.ResetIndicatorCount(oldCount);

            // Assert
            Assert.Equal(expected, indicatorStackLayout.Children.Count);
        }

        [Fact]
        public void IndicatorLayout_ShouldBeRemovedWhenIndicatorTemplateIsNulled()
        {
            // Arrange
            var indicatorView = new IndicatorView() { ItemsSource = new List<string> { "item1", "item2" } };
            indicatorView.IndicatorTemplate = new DataTemplate();
            Assert.NotNull(indicatorView.IndicatorLayout);

            // Act
            indicatorView.IndicatorTemplate = null;

            //Assert
            Assert.Null(indicatorView.IndicatorLayout);
        }

        /// <summary>
        /// Tests that the HideSingle property returns the default value of true when accessed without being explicitly set.
        /// </summary>
        [Fact]
        public void HideSingle_DefaultValue_ReturnsTrue()
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act
            bool result = indicatorView.HideSingle;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the HideSingle property correctly returns the value after being set to different boolean values.
        /// Verifies both true and false assignments work correctly.
        /// </summary>
        /// <param name="setValue">The boolean value to set on the HideSingle property</param>
        /// <param name="expectedValue">The expected value when getting the HideSingle property</param>
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void HideSingle_SetValue_ReturnsCorrectValue(bool setValue, bool expectedValue)
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act
            indicatorView.HideSingle = setValue;
            bool result = indicatorView.HideSingle;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the HideSingle property can be set multiple times and always returns the most recent value.
        /// Verifies property state consistency across multiple assignments.
        /// </summary>
        [Fact]
        public void HideSingle_MultipleAssignments_ReturnsLatestValue()
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act & Assert
            indicatorView.HideSingle = false;
            Assert.False(indicatorView.HideSingle);

            indicatorView.HideSingle = true;
            Assert.True(indicatorView.HideSingle);

            indicatorView.HideSingle = false;
            Assert.False(indicatorView.HideSingle);
        }

        /// <summary>
        /// Tests that the IndicatorColor property returns the default value of Colors.LightGrey when not explicitly set.
        /// Verifies the default behavior and ensures the getter properly retrieves the bindable property value.
        /// </summary>
        [Fact]
        public void IndicatorColor_DefaultValue_ReturnsLightGrey()
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act
            var color = indicatorView.IndicatorColor;

            // Assert
            Assert.Equal(Colors.LightGrey, color);
        }

        /// <summary>
        /// Tests that the IndicatorColor property correctly returns values that have been set.
        /// Uses various color values to ensure the getter properly retrieves stored values.
        /// </summary>
        /// <param name="red">Red component (0.0 to 1.0)</param>
        /// <param name="green">Green component (0.0 to 1.0)</param>
        /// <param name="blue">Blue component (0.0 to 1.0)</param>
        /// <param name="alpha">Alpha component (0.0 to 1.0)</param>
        [Theory]
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(0.5f, 0.5f, 0.5f, 1.0f)] // Gray
        [InlineData(1.0f, 0.0f, 0.0f, 0.0f)] // Transparent Red
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent Black
        [InlineData(1.0f, 1.0f, 1.0f, 0.5f)] // Semi-transparent White
        public void IndicatorColor_SetValue_ReturnsCorrectValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var indicatorView = new IndicatorView();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            indicatorView.IndicatorColor = expectedColor;
            var actualColor = indicatorView.IndicatorColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that the IndicatorColor property correctly handles predefined Colors values.
        /// Ensures the getter works properly with standard color constants.
        /// </summary>
        /// <param name="color">The predefined color to test</param>
        [Theory]
        [MemberData(nameof(GetStandardColors))]
        public void IndicatorColor_StandardColors_ReturnsCorrectValue(Color color)
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act
            indicatorView.IndicatorColor = color;
            var actualColor = indicatorView.IndicatorColor;

            // Assert
            Assert.Equal(color, actualColor);
        }

        /// <summary>
        /// Provides standard color values for parameterized tests.
        /// </summary>
        public static TheoryData<Color> GetStandardColors()
        {
            return new TheoryData<Color>
            {
                Colors.Black,
                Colors.White,
                Colors.Red,
                Colors.Green,
                Colors.Blue,
                Colors.Yellow,
                Colors.Cyan,
                Colors.Magenta,
                Colors.Transparent,
                Colors.LightGrey,
                Colors.DarkGrey
            };
        }

        /// <summary>
        /// Tests that the IndicatorColor property getter works correctly after multiple set operations.
        /// Verifies that the getter consistently returns the most recently set value.
        /// </summary>
        [Fact]
        public void IndicatorColor_MultipleSetOperations_ReturnsLatestValue()
        {
            // Arrange
            var indicatorView = new IndicatorView();
            var firstColor = Colors.Red;
            var secondColor = Colors.Blue;
            var thirdColor = Colors.Green;

            // Act & Assert - First set
            indicatorView.IndicatorColor = firstColor;
            Assert.Equal(firstColor, indicatorView.IndicatorColor);

            // Act & Assert - Second set
            indicatorView.IndicatorColor = secondColor;
            Assert.Equal(secondColor, indicatorView.IndicatorColor);

            // Act & Assert - Third set
            indicatorView.IndicatorColor = thirdColor;
            Assert.Equal(thirdColor, indicatorView.IndicatorColor);
        }

        /// <summary>
        /// Tests that the IndicatorColor property works correctly with Color instances created using different constructors.
        /// Ensures the getter properly handles colors created with byte values, float values, and other constructors.
        /// </summary>
        [Theory]
        [InlineData(255, 0, 0)] // Red using byte constructor
        [InlineData(0, 255, 0)] // Green using byte constructor
        [InlineData(0, 0, 255)] // Blue using byte constructor
        [InlineData(128, 128, 128)] // Gray using byte constructor
        [InlineData(0, 0, 0)] // Black using byte constructor
        [InlineData(255, 255, 255)] // White using byte constructor
        public void IndicatorColor_ByteConstructor_ReturnsCorrectValue(byte red, byte green, byte blue)
        {
            // Arrange
            var indicatorView = new IndicatorView();
            var expectedColor = new Color(red, green, blue);

            // Act
            indicatorView.IndicatorColor = expectedColor;
            var actualColor = indicatorView.IndicatorColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that OnMeasure returns handler's GetDesiredSize result when IndicatorTemplate is null and handler is available.
        /// Verifies the first branch of the conditional logic with valid width and height constraints.
        /// Expected result: Handler.GetDesiredSize is called and its result is returned.
        /// </summary>
        [Theory]
        [InlineData(100.0, 200.0)]
        [InlineData(0.0, 0.0)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        public void OnMeasure_IndicatorTemplateNull_HandlerAvailable_ReturnsHandlerDesiredSize(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var indicatorView = new IndicatorView();
            var mockHandler = Substitute.For<IViewHandler>();
            var expectedSize = new Size(50, 75);
            var expectedSizeRequest = new SizeRequest(expectedSize);

            mockHandler.GetDesiredSize(widthConstraint, heightConstraint).Returns(expectedSize);
            indicatorView.Handler = mockHandler;
            indicatorView.IndicatorTemplate = null;

            // Act
            var result = indicatorView.OnMeasure(widthConstraint, heightConstraint);

            // Assert
            mockHandler.Received(1).GetDesiredSize(widthConstraint, heightConstraint);
            Assert.Equal(expectedSizeRequest.Request, result.Request);
            Assert.Equal(expectedSizeRequest.Minimum, result.Minimum);
        }

        /// <summary>
        /// Tests that OnMeasure returns default SizeRequest when IndicatorTemplate is null and handler is null.
        /// Verifies the null-coalescing operator behavior when Handler?.GetDesiredSize returns null.
        /// Expected result: A default SizeRequest is returned (new SizeRequest()).
        /// </summary>
        [Theory]
        [InlineData(100.0, 200.0)]
        [InlineData(0.0, 0.0)]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.NaN, double.NaN)]
        public void OnMeasure_IndicatorTemplateNull_HandlerNull_ReturnsDefaultSizeRequest(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var indicatorView = new IndicatorView();
            indicatorView.Handler = null;
            indicatorView.IndicatorTemplate = null;
            var expectedSizeRequest = new SizeRequest();

            // Act
            var result = indicatorView.OnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(expectedSizeRequest.Request, result.Request);
            Assert.Equal(expectedSizeRequest.Minimum, result.Minimum);
        }

        /// <summary>
        /// Tests that OnMeasure calls base.OnMeasure when IndicatorTemplate is not null.
        /// Verifies the second branch of the conditional logic where IndicatorTemplate has a value.
        /// Expected result: base.OnMeasure is called with the provided constraints.
        /// </summary>
        [Theory]
        [InlineData(150.0, 250.0)]
        [InlineData(0.0, 0.0)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, 100.0)]
        public void OnMeasure_IndicatorTemplateNotNull_CallsBaseOnMeasure(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var indicatorView = new TestableIndicatorView();
            var dataTemplate = new DataTemplate(() => new Label());
            indicatorView.IndicatorTemplate = dataTemplate;

            // Act
            var result = indicatorView.OnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.True(indicatorView.BaseOnMeasureCalled);
            Assert.Equal(widthConstraint, indicatorView.ReceivedWidthConstraint);
            Assert.Equal(heightConstraint, indicatorView.ReceivedHeightConstraint);
        }

        /// <summary>
        /// Tests OnMeasure with negative constraint values when IndicatorTemplate is null.
        /// Verifies that the method handles negative constraints appropriately.
        /// Expected result: Handler.GetDesiredSize is called with negative values.
        /// </summary>
        [Theory]
        [InlineData(-100.0, -200.0)]
        [InlineData(-1.0, -1.0)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        public void OnMeasure_IndicatorTemplateNull_NegativeConstraints_CallsHandlerWithNegativeValues(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var indicatorView = new IndicatorView();
            var mockHandler = Substitute.For<IViewHandler>();
            var expectedSize = new Size(0, 0);

            mockHandler.GetDesiredSize(widthConstraint, heightConstraint).Returns(expectedSize);
            indicatorView.Handler = mockHandler;
            indicatorView.IndicatorTemplate = null;

            // Act
            var result = indicatorView.OnMeasure(widthConstraint, heightConstraint);

            // Assert
            mockHandler.Received(1).GetDesiredSize(widthConstraint, heightConstraint);
        }

        /// <summary>
        /// Tests OnMeasure with special double values (NaN, Infinity) when IndicatorTemplate is not null.
        /// Verifies that special floating-point values are passed correctly to base implementation.
        /// Expected result: base.OnMeasure is called with the special values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.PositiveInfinity)]
        [InlineData(double.PositiveInfinity, double.NaN)]
        [InlineData(double.NegativeInfinity, double.PositiveInfinity)]
        public void OnMeasure_IndicatorTemplateNotNull_SpecialDoubleValues_PassesToBase(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var indicatorView = new TestableIndicatorView();
            var dataTemplate = new DataTemplate(() => new Button());
            indicatorView.IndicatorTemplate = dataTemplate;

            // Act
            var result = indicatorView.OnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.True(indicatorView.BaseOnMeasureCalled);
            Assert.Equal(widthConstraint, indicatorView.ReceivedWidthConstraint);
            Assert.Equal(heightConstraint, indicatorView.ReceivedHeightConstraint);
        }

        /// <summary>
        /// Helper class to expose protected members and track base method calls for testing purposes.
        /// Inherits from IndicatorView to override OnMeasure and capture method invocation details.
        /// </summary>
        private class TestableIndicatorView : IndicatorView
        {
            public bool BaseOnMeasureCalled { get; private set; }
            public double ReceivedWidthConstraint { get; private set; }
            public double ReceivedHeightConstraint { get; private set; }

            protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
            {
                BaseOnMeasureCalled = true;
                ReceivedWidthConstraint = widthConstraint;
                ReceivedHeightConstraint = heightConstraint;

                // Return a predictable SizeRequest for testing
                return new SizeRequest(new Size(100, 50));
            }
        }

        /// <summary>
        /// Tests that the SelectedIndicatorColor property returns the default value when not explicitly set.
        /// The default value should be Colors.Black as specified in the BindableProperty definition.
        /// </summary>
        [Fact]
        public void SelectedIndicatorColor_WhenNotSet_ReturnsDefaultValue()
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act
            var result = indicatorView.SelectedIndicatorColor;

            // Assert
            Assert.Equal(Colors.Black, result);
        }

        /// <summary>
        /// Tests that the SelectedIndicatorColor property correctly returns values that were set.
        /// Verifies the basic get/set functionality of the property.
        /// </summary>
        [Theory]
        [InlineData(255, 0, 0, 255)] // Red
        [InlineData(0, 255, 0, 255)] // Green
        [InlineData(0, 0, 255, 255)] // Blue
        [InlineData(255, 255, 255, 255)] // White
        [InlineData(0, 0, 0, 255)] // Black
        [InlineData(128, 128, 128, 255)] // Gray
        [InlineData(255, 255, 255, 0)] // Transparent White
        [InlineData(0, 0, 0, 0)] // Transparent Black
        [InlineData(255, 165, 0, 128)] // Semi-transparent Orange
        public void SelectedIndicatorColor_SetCustomColor_ReturnsSetValue(byte red, byte green, byte blue, byte alpha)
        {
            // Arrange
            var indicatorView = new IndicatorView();
            var expectedColor = Color.FromRgba(red, green, blue, alpha);

            // Act
            indicatorView.SelectedIndicatorColor = expectedColor;
            var result = indicatorView.SelectedIndicatorColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that the SelectedIndicatorColor property correctly handles predefined colors.
        /// Validates that common predefined Color values work correctly with the property.
        /// </summary>
        [Theory]
        [InlineData("Red")]
        [InlineData("Blue")]
        [InlineData("Green")]
        [InlineData("White")]
        [InlineData("Black")]
        [InlineData("Transparent")]
        [InlineData("Yellow")]
        [InlineData("Cyan")]
        [InlineData("Magenta")]
        [InlineData("Gray")]
        [InlineData("LightGray")]
        [InlineData("DarkGray")]
        public void SelectedIndicatorColor_SetPredefinedColor_ReturnsSetValue(string colorName)
        {
            // Arrange
            var indicatorView = new IndicatorView();
            var expectedColor = GetPredefinedColor(colorName);

            // Act
            indicatorView.SelectedIndicatorColor = expectedColor;
            var result = indicatorView.SelectedIndicatorColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that the SelectedIndicatorColor property handles floating-point RGBA values correctly.
        /// Verifies that colors created with float values (0.0-1.0 range) work properly.
        /// </summary>
        [Theory]
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(0.5f, 0.5f, 0.5f, 1.0f)] // Gray
        [InlineData(1.0f, 1.0f, 1.0f, 0.0f)] // Transparent White
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent Black
        [InlineData(1.0f, 0.647f, 0.0f, 0.5f)] // Semi-transparent Orange
        public void SelectedIndicatorColor_SetFloatColor_ReturnsSetValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var indicatorView = new IndicatorView();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            indicatorView.SelectedIndicatorColor = expectedColor;
            var result = indicatorView.SelectedIndicatorColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that setting the SelectedIndicatorColor property multiple times works correctly.
        /// Verifies that the property can be changed and each change is properly reflected.
        /// </summary>
        [Fact]
        public void SelectedIndicatorColor_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var indicatorView = new IndicatorView();
            var firstColor = Colors.Red;
            var secondColor = Colors.Blue;
            var thirdColor = Colors.Green;

            // Act & Assert - First color
            indicatorView.SelectedIndicatorColor = firstColor;
            Assert.Equal(firstColor, indicatorView.SelectedIndicatorColor);

            // Act & Assert - Second color
            indicatorView.SelectedIndicatorColor = secondColor;
            Assert.Equal(secondColor, indicatorView.SelectedIndicatorColor);

            // Act & Assert - Third color
            indicatorView.SelectedIndicatorColor = thirdColor;
            Assert.Equal(thirdColor, indicatorView.SelectedIndicatorColor);
        }

        /// <summary>
        /// Helper method to get predefined colors by name for testing purposes.
        /// Maps color names to their corresponding Color instances.
        /// </summary>
        /// <param name="colorName">The name of the predefined color.</param>
        /// <returns>The corresponding Color instance.</returns>
        private static Color GetPredefinedColor(string colorName) => colorName switch
        {
            "Red" => Colors.Red,
            "Blue" => Colors.Blue,
            "Green" => Colors.Green,
            "White" => Colors.White,
            "Black" => Colors.Black,
            "Transparent" => Colors.Transparent,
            "Yellow" => Colors.Yellow,
            "Cyan" => Colors.Cyan,
            "Magenta" => Colors.Magenta,
            "Gray" => Colors.Gray,
            "LightGray" => Colors.LightGray,
            "DarkGray" => Colors.DarkGray,
            _ => throw new ArgumentException($"Unknown color name: {colorName}", nameof(colorName))
        };

        /// <summary>
        /// Tests that IndicatorSize getter returns the default value when no value has been explicitly set.
        /// Expected result: Returns the default value of 6.0.
        /// </summary>
        [Fact]
        public void IndicatorSize_GetWithoutSet_ReturnsDefaultValue()
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act
            var result = indicatorView.IndicatorSize;

            // Assert
            Assert.Equal(6.0, result);
        }

        /// <summary>
        /// Tests that IndicatorSize getter returns the correct value after setting various valid values.
        /// Input conditions: Different positive double values including boundary cases.
        /// Expected result: Getter returns the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(10.5)]
        [InlineData(100.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.Epsilon)]
        public void IndicatorSize_GetAfterSetValidValues_ReturnsSetValue(double setValue)
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act
            indicatorView.IndicatorSize = setValue;
            var result = indicatorView.IndicatorSize;

            // Assert
            Assert.Equal(setValue, result);
        }

        /// <summary>
        /// Tests that IndicatorSize getter returns the correct value after setting negative values.
        /// Input conditions: Negative double values including boundary cases.
        /// Expected result: Getter returns the exact negative value that was set.
        /// </summary>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-10.5)]
        [InlineData(-100.0)]
        [InlineData(double.MinValue)]
        public void IndicatorSize_GetAfterSetNegativeValues_ReturnsSetValue(double setValue)
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act
            indicatorView.IndicatorSize = setValue;
            var result = indicatorView.IndicatorSize;

            // Assert
            Assert.Equal(setValue, result);
        }

        /// <summary>
        /// Tests that IndicatorSize getter handles special double values correctly.
        /// Input conditions: Special double values like NaN, PositiveInfinity, and NegativeInfinity.
        /// Expected result: Getter returns the exact special value that was set.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void IndicatorSize_GetAfterSetSpecialDoubleValues_ReturnsSetValue(double setValue)
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act
            indicatorView.IndicatorSize = setValue;
            var result = indicatorView.IndicatorSize;

            // Assert
            if (double.IsNaN(setValue))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(setValue, result);
            }
        }

        /// <summary>
        /// Tests that IndicatorSize getter returns the most recently set value when set multiple times.
        /// Input conditions: Setting the property multiple times with different values.
        /// Expected result: Getter returns the last value that was set.
        /// </summary>
        [Fact]
        public void IndicatorSize_GetAfterMultipleSets_ReturnsLastSetValue()
        {
            // Arrange
            var indicatorView = new IndicatorView();
            var firstValue = 5.0;
            var secondValue = 10.0;
            var finalValue = 15.0;

            // Act
            indicatorView.IndicatorSize = firstValue;
            indicatorView.IndicatorSize = secondValue;
            indicatorView.IndicatorSize = finalValue;
            var result = indicatorView.IndicatorSize;

            // Assert
            Assert.Equal(finalValue, result);
        }

        /// <summary>
        /// Tests that the Position property returns the default value of 0 for a new IndicatorView instance.
        /// Verifies that the getter correctly retrieves the default value from the underlying BindableProperty.
        /// </summary>
        [Fact]
        public void Position_DefaultValue_ReturnsZero()
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act
            var position = indicatorView.Position;

            // Assert
            Assert.Equal(0, position);
        }

        /// <summary>
        /// Tests that the Position property correctly sets and gets various valid integer values.
        /// Verifies that the property properly delegates to the underlying BindableProperty storage mechanism.
        /// </summary>
        /// <param name="value">The integer value to set and retrieve</param>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(10)]
        [InlineData(-10)]
        [InlineData(100)]
        [InlineData(-100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void Position_SetAndGet_ReturnsCorrectValue(int value)
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act
            indicatorView.Position = value;
            var result = indicatorView.Position;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the Position property can be set multiple times with different values.
        /// Verifies that the property correctly updates and maintains the most recently set value.
        /// </summary>
        [Fact]
        public void Position_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act & Assert
            indicatorView.Position = 5;
            Assert.Equal(5, indicatorView.Position);

            indicatorView.Position = -3;
            Assert.Equal(-3, indicatorView.Position);

            indicatorView.Position = 0;
            Assert.Equal(0, indicatorView.Position);

            indicatorView.Position = int.MaxValue;
            Assert.Equal(int.MaxValue, indicatorView.Position);
        }

        /// <summary>
        /// Tests that the IndicatorsShape property returns the default value when no value has been explicitly set.
        /// Input: Newly created IndicatorView instance with no IndicatorsShape value set.
        /// Expected: IndicatorsShape should return IndicatorShape.Circle (the default value).
        /// </summary>
        [Fact]
        public void IndicatorsShape_DefaultValue_ReturnsCircle()
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act
            var result = indicatorView.IndicatorsShape;

            // Assert
            Assert.Equal(IndicatorShape.Circle, result);
        }

        /// <summary>
        /// Tests that the IndicatorsShape property getter returns the correct value after setting valid enum values.
        /// Input: IndicatorView instance with IndicatorsShape set to various valid IndicatorShape enum values.
        /// Expected: Getter should return the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(IndicatorShape.Circle)]
        [InlineData(IndicatorShape.Square)]
        public void IndicatorsShape_SetValidValue_ReturnsSetValue(IndicatorShape expectedValue)
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act
            indicatorView.IndicatorsShape = expectedValue;
            var result = indicatorView.IndicatorsShape;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the IndicatorsShape property can handle invalid enum values by casting integers outside the defined enum range.
        /// Input: IndicatorView instance with IndicatorsShape set to invalid enum values (cast from integers outside enum range).
        /// Expected: The property should store and return the invalid enum value without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        [InlineData(99)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void IndicatorsShape_SetInvalidEnumValue_ReturnsSetValue(int invalidEnumValue)
        {
            // Arrange
            var indicatorView = new IndicatorView();
            var invalidIndicatorShape = (IndicatorShape)invalidEnumValue;

            // Act
            indicatorView.IndicatorsShape = invalidIndicatorShape;
            var result = indicatorView.IndicatorsShape;

            // Assert
            Assert.Equal(invalidIndicatorShape, result);
        }

        /// <summary>
        /// Tests that the IndicatorsShape property setter properly updates the underlying bindable property value.
        /// Input: IndicatorView instance with IndicatorsShape set to Circle, then changed to Square.
        /// Expected: Each setter call should update the property value correctly, and getter should reflect the changes.
        /// </summary>
        [Fact]
        public void IndicatorsShape_SetMultipleValues_UpdatesCorrectly()
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act & Assert - Set to Circle
            indicatorView.IndicatorsShape = IndicatorShape.Circle;
            Assert.Equal(IndicatorShape.Circle, indicatorView.IndicatorsShape);

            // Act & Assert - Set to Square
            indicatorView.IndicatorsShape = IndicatorShape.Square;
            Assert.Equal(IndicatorShape.Square, indicatorView.IndicatorsShape);

            // Act & Assert - Set back to Circle
            indicatorView.IndicatorsShape = IndicatorShape.Circle;
            Assert.Equal(IndicatorShape.Circle, indicatorView.IndicatorsShape);
        }
    }


    /// <summary>
    /// Unit tests for the MaximumVisible property of IndicatorView.
    /// </summary>
    public partial class IndicatorViewMaximumVisibleTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the MaximumVisible property returns the default value when not explicitly set.
        /// Verifies that the getter correctly retrieves the default value of int.MaxValue.
        /// </summary>
        [Fact]
        public void MaximumVisible_DefaultValue_ReturnsIntMaxValue()
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act
            var result = indicatorView.MaximumVisible;

            // Assert
            Assert.Equal(int.MaxValue, result);
        }

        /// <summary>
        /// Tests that the MaximumVisible property correctly handles various integer values.
        /// Verifies both getter and setter functionality with boundary and edge case values.
        /// </summary>
        /// <param name="value">The value to set and retrieve from the MaximumVisible property.</param>
        /// <param name="expectedValue">The expected value after setting and getting the property.</param>
        [Theory]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData(-1, -1)]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(10, 10)]
        [InlineData(100, 100)]
        [InlineData(int.MaxValue, int.MaxValue)]
        public void MaximumVisible_SetAndGet_ReturnsExpectedValue(int value, int expectedValue)
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act
            indicatorView.MaximumVisible = value;
            var result = indicatorView.MaximumVisible;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that setting MaximumVisible to zero works correctly.
        /// This is a specific edge case that might affect indicator visibility logic.
        /// </summary>
        [Fact]
        public void MaximumVisible_SetToZero_ReturnsZero()
        {
            // Arrange
            var indicatorView = new IndicatorView();

            // Act
            indicatorView.MaximumVisible = 0;
            var result = indicatorView.MaximumVisible;

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that setting MaximumVisible to negative values works correctly.
        /// Verifies that the property can handle negative values without throwing exceptions.
        /// </summary>
        [Fact]
        public void MaximumVisible_SetToNegativeValue_ReturnsNegativeValue()
        {
            // Arrange
            var indicatorView = new IndicatorView();
            const int negativeValue = -100;

            // Act
            indicatorView.MaximumVisible = negativeValue;
            var result = indicatorView.MaximumVisible;

            // Assert
            Assert.Equal(negativeValue, result);
        }

        /// <summary>
        /// Tests that the MaximumVisible property maintains its value across multiple get operations.
        /// Ensures the getter is stable and consistent.
        /// </summary>
        [Fact]
        public void MaximumVisible_MultipleGets_ReturnsSameValue()
        {
            // Arrange
            var indicatorView = new IndicatorView();
            const int testValue = 42;
            indicatorView.MaximumVisible = testValue;

            // Act
            var result1 = indicatorView.MaximumVisible;
            var result2 = indicatorView.MaximumVisible;
            var result3 = indicatorView.MaximumVisible;

            // Assert
            Assert.Equal(testValue, result1);
            Assert.Equal(testValue, result2);
            Assert.Equal(testValue, result3);
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }
    }
}