#nullable disable

using System;
using System.Collections.Specialized;
using System.ComponentModel;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class TabbedPageTests
    {
        /// <summary>
        /// Tests that BarBackground getter returns the value from the underlying BindableProperty.
        /// Input: TabbedPage instance with BarBackground set to null.
        /// Expected: Property returns null.
        /// </summary>
        [Fact]
        public void BarBackground_GetWhenNull_ReturnsNull()
        {
            // Arrange
            var tabbedPage = new TabbedPage();

            // Act
            var result = tabbedPage.BarBackground;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that BarBackground setter and getter work correctly with a brush value.
        /// Input: TabbedPage instance and a mocked Brush.
        /// Expected: Getter returns the same brush that was set.
        /// </summary>
        [Fact]
        public void BarBackground_SetAndGet_ReturnsSameValue()
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var brush = Substitute.For<Brush>();

            // Act
            tabbedPage.BarBackground = brush;
            var result = tabbedPage.BarBackground;

            // Assert
            Assert.Same(brush, result);
        }

        /// <summary>
        /// Tests that BarBackground setter accepts null value and getter returns null.
        /// Input: TabbedPage instance with BarBackground set to null explicitly.
        /// Expected: Property returns null after setting to null.
        /// </summary>
        [Fact]
        public void BarBackground_SetNull_GetReturnsNull()
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var brush = Substitute.For<Brush>();
            tabbedPage.BarBackground = brush; // Set to non-null first

            // Act
            tabbedPage.BarBackground = null;
            var result = tabbedPage.BarBackground;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that BarBackground property uses the correct BindableProperty.
        /// Input: TabbedPage instance and a mocked Brush.
        /// Expected: Setting BarBackground should update the value retrieved via GetValue with BarBackgroundProperty.
        /// </summary>
        [Fact]
        public void BarBackground_UsesCorrectBindableProperty()
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var brush = Substitute.For<Brush>();

            // Act
            tabbedPage.BarBackground = brush;
            var valueViaProperty = tabbedPage.BarBackground;
            var valueViaBindableProperty = (Brush)tabbedPage.GetValue(BarElement.BarBackgroundProperty);

            // Assert
            Assert.Same(brush, valueViaProperty);
            Assert.Same(brush, valueViaBindableProperty);
            Assert.Same(valueViaProperty, valueViaBindableProperty);
        }

        /// <summary>
        /// Tests that multiple assignments to BarBackground work correctly.
        /// Input: TabbedPage instance and multiple different Brush instances.
        /// Expected: Getter always returns the most recently set value.
        /// </summary>
        [Fact]
        public void BarBackground_MultipleAssignments_ReturnsLatestValue()
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var brush1 = Substitute.For<Brush>();
            var brush2 = Substitute.For<Brush>();
            var brush3 = Substitute.For<Brush>();

            // Act & Assert
            tabbedPage.BarBackground = brush1;
            Assert.Same(brush1, tabbedPage.BarBackground);

            tabbedPage.BarBackground = brush2;
            Assert.Same(brush2, tabbedPage.BarBackground);

            tabbedPage.BarBackground = brush3;
            Assert.Same(brush3, tabbedPage.BarBackground);

            tabbedPage.BarBackground = null;
            Assert.Null(tabbedPage.BarBackground);
        }

        /// <summary>
        /// Tests that BarTextColor property returns the default Color value when not explicitly set.
        /// Verifies the getter retrieves the correct default value from the underlying BindableProperty.
        /// Expected result: Returns default(Color).
        /// </summary>
        [Fact]
        public void BarTextColor_DefaultValue_ReturnsDefaultColor()
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var expectedColor = default(Color);

            // Act
            var actualColor = tabbedPage.BarTextColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that BarTextColor property correctly sets and retrieves various Color values.
        /// Verifies the getter returns the exact Color value that was set through the setter.
        /// Expected result: Retrieved color matches the set color for all test cases.
        /// </summary>
        [Theory]
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Black
        [InlineData(0.5f, 0.3f, 0.8f, 0.6f)] // Custom color
        public void BarTextColor_SetValidColor_ReturnsSetColor(float red, float green, float blue, float alpha)
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            tabbedPage.BarTextColor = expectedColor;
            var actualColor = tabbedPage.BarTextColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that BarTextColor property correctly handles predefined Color values.
        /// Verifies the getter returns standard predefined colors correctly.
        /// Expected result: Retrieved color matches the predefined color value.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetPredefinedColors))]
        public void BarTextColor_SetPredefinedColor_ReturnsSetColor(Color expectedColor)
        {
            // Arrange
            var tabbedPage = new TabbedPage();

            // Act
            tabbedPage.BarTextColor = expectedColor;
            var actualColor = tabbedPage.BarTextColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        public static TheoryData<Color> GetPredefinedColors()
        {
            return new TheoryData<Color>
            {
                Colors.Red,
                Colors.Green,
                Colors.Blue,
                Colors.Yellow,
                Colors.Cyan,
                Colors.Magenta,
                Colors.White,
                Colors.Black,
                Colors.Transparent,
                Colors.Gray
            };
        }

        /// <summary>
        /// Tests that BarTextColor property handles extreme Color component values correctly.
        /// Verifies the getter works with boundary values for color components.
        /// Expected result: Retrieved color matches the color with extreme component values.
        /// </summary>
        [Theory]
        [InlineData(float.MinValue, 0.0f, 0.0f, 1.0f)]
        [InlineData(float.MaxValue, 0.0f, 0.0f, 1.0f)]
        [InlineData(0.0f, float.MinValue, 0.0f, 1.0f)]
        [InlineData(0.0f, float.MaxValue, 0.0f, 1.0f)]
        [InlineData(0.0f, 0.0f, float.MinValue, 1.0f)]
        [InlineData(0.0f, 0.0f, float.MaxValue, 1.0f)]
        [InlineData(0.0f, 0.0f, 0.0f, float.MinValue)]
        [InlineData(0.0f, 0.0f, 0.0f, float.MaxValue)]
        public void BarTextColor_SetExtremeColorValues_ReturnsSetColor(float red, float green, float blue, float alpha)
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            tabbedPage.BarTextColor = expectedColor;
            var actualColor = tabbedPage.BarTextColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that UnselectedTabColor getter returns the default value when not explicitly set.
        /// Verifies that the property correctly retrieves the default Color value through GetValue().
        /// Expected result: Returns default(Color) which is a black color (0,0,0,1).
        /// </summary>
        [Fact]
        public void UnselectedTabColor_DefaultValue_ReturnsDefaultColor()
        {
            // Arrange
            var tabbedPage = new TabbedPage();

            // Act
            var result = tabbedPage.UnselectedTabColor;

            // Assert
            Assert.Equal(default(Color), result);
        }

        /// <summary>
        /// Tests that UnselectedTabColor getter returns the correct value after setting it.
        /// Verifies that the property correctly retrieves values through GetValue() after SetValue() calls.
        /// Expected result: Returns the exact Color value that was set.
        /// </summary>
        [Theory]
        [InlineData(0f, 0f, 0f, 1f)] // Black
        [InlineData(1f, 1f, 1f, 1f)] // White
        [InlineData(1f, 0f, 0f, 1f)] // Red
        [InlineData(0f, 1f, 0f, 1f)] // Green
        [InlineData(0f, 0f, 1f, 1f)] // Blue
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Semi-transparent gray
        [InlineData(0f, 0f, 0f, 0f)] // Transparent
        public void UnselectedTabColor_SetAndGet_ReturnsCorrectValue(float red, float green, float blue, float alpha)
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            tabbedPage.UnselectedTabColor = expectedColor;
            var result = tabbedPage.UnselectedTabColor;

            // Assert
            Assert.Equal(expectedColor.Red, result.Red);
            Assert.Equal(expectedColor.Green, result.Green);
            Assert.Equal(expectedColor.Blue, result.Blue);
            Assert.Equal(expectedColor.Alpha, result.Alpha);
        }

        /// <summary>
        /// Tests that UnselectedTabColor getter works correctly with boundary color component values.
        /// Verifies behavior with minimum and maximum float values for color components.
        /// Expected result: Returns colors with clamped component values between 0.0 and 1.0.
        /// </summary>
        [Theory]
        [InlineData(float.MinValue, float.MinValue, float.MinValue, float.MinValue)] // Extreme negative values
        [InlineData(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue)] // Extreme positive values
        [InlineData(-1f, -1f, -1f, -1f)] // Negative values
        [InlineData(2f, 2f, 2f, 2f)] // Values above 1.0
        public void UnselectedTabColor_BoundaryValues_ReturnsClamppedColor(float red, float green, float blue, float alpha)
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var inputColor = new Color(red, green, blue, alpha);

            // Act
            tabbedPage.UnselectedTabColor = inputColor;
            var result = tabbedPage.UnselectedTabColor;

            // Assert
            Assert.True(result.Red >= 0f && result.Red <= 1f, $"Red component {result.Red} should be between 0 and 1");
            Assert.True(result.Green >= 0f && result.Green <= 1f, $"Green component {result.Green} should be between 0 and 1");
            Assert.True(result.Blue >= 0f && result.Blue <= 1f, $"Blue component {result.Blue} should be between 0 and 1");
            Assert.True(result.Alpha >= 0f && result.Alpha <= 1f, $"Alpha component {result.Alpha} should be between 0 and 1");
        }

        /// <summary>
        /// Tests that UnselectedTabColor getter handles multiple successive get operations correctly.
        /// Verifies that multiple calls to the getter return consistent results without side effects.
        /// Expected result: All getter calls return the same Color value.
        /// </summary>
        [Fact]
        public void UnselectedTabColor_MultipleGets_ReturnsConsistentValue()
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var testColor = new Color(0.75f, 0.25f, 0.5f, 0.8f);
            tabbedPage.UnselectedTabColor = testColor;

            // Act
            var result1 = tabbedPage.UnselectedTabColor;
            var result2 = tabbedPage.UnselectedTabColor;
            var result3 = tabbedPage.UnselectedTabColor;

            // Assert
            Assert.Equal(result1.Red, result2.Red);
            Assert.Equal(result1.Green, result2.Green);
            Assert.Equal(result1.Blue, result2.Blue);
            Assert.Equal(result1.Alpha, result2.Alpha);
            Assert.Equal(result2.Red, result3.Red);
            Assert.Equal(result2.Green, result3.Green);
            Assert.Equal(result2.Blue, result3.Blue);
            Assert.Equal(result2.Alpha, result3.Alpha);
        }

        /// <summary>
        /// Tests that UnselectedTabColor getter returns correct values after overwriting previous values.
        /// Verifies that the property correctly handles value updates through the BindableProperty mechanism.
        /// Expected result: Returns the most recently set Color value.
        /// </summary>
        [Fact]
        public void UnselectedTabColor_OverwriteValue_ReturnsNewValue()
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var initialColor = new Color(1f, 0f, 0f, 1f); // Red
            var updatedColor = new Color(0f, 1f, 0f, 1f); // Green

            // Act
            tabbedPage.UnselectedTabColor = initialColor;
            var initialResult = tabbedPage.UnselectedTabColor;

            tabbedPage.UnselectedTabColor = updatedColor;
            var finalResult = tabbedPage.UnselectedTabColor;

            // Assert
            Assert.Equal(initialColor.Red, initialResult.Red);
            Assert.Equal(initialColor.Green, initialResult.Green);
            Assert.Equal(initialColor.Blue, initialResult.Blue);
            Assert.Equal(initialColor.Alpha, initialResult.Alpha);

            Assert.Equal(updatedColor.Red, finalResult.Red);
            Assert.Equal(updatedColor.Green, finalResult.Green);
            Assert.Equal(updatedColor.Blue, finalResult.Blue);
            Assert.Equal(updatedColor.Alpha, finalResult.Alpha);
        }

        /// <summary>
        /// Tests that the SelectedTabColor property getter returns the correct Color value
        /// by verifying it calls GetValue with SelectedTabColorProperty and casts the result properly.
        /// Expected result: The getter should return the Color value that was previously set.
        /// </summary>
        [Fact]
        public void SelectedTabColor_GetterReturnsCorrectValue_WhenValueIsSet()
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var expectedColor = Colors.Red;
            tabbedPage.SelectedTabColor = expectedColor;

            // Act
            var actualColor = tabbedPage.SelectedTabColor;

            // Assert
            Assert.Equal(expectedColor, actualColor);
        }

        /// <summary>
        /// Tests that the SelectedTabColor property setter correctly stores the provided Color value
        /// by verifying the value can be retrieved through the getter.
        /// Expected result: The setter should store the Color value and make it retrievable via getter.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0, 0.0, 0.0)] // Transparent
        [InlineData(1.0, 0.0, 0.0, 1.0)] // Red
        [InlineData(0.0, 1.0, 0.0, 1.0)] // Green
        [InlineData(0.0, 0.0, 1.0, 1.0)] // Blue
        [InlineData(1.0, 1.0, 1.0, 1.0)] // White
        [InlineData(0.5, 0.25, 0.75, 0.8)] // Custom color with alpha
        public void SelectedTabColor_SetterStoresValue_ForVariousColorValues(float red, float green, float blue, float alpha)
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var colorToSet = new Color(red, green, blue, alpha);

            // Act
            tabbedPage.SelectedTabColor = colorToSet;

            // Assert
            Assert.Equal(colorToSet, tabbedPage.SelectedTabColor);
        }

        /// <summary>
        /// Tests that the SelectedTabColor property returns the default Color value when not explicitly set.
        /// Expected result: The getter should return the default Color value initially.
        /// </summary>
        [Fact]
        public void SelectedTabColor_GetterReturnsDefaultValue_WhenNotSet()
        {
            // Arrange
            var tabbedPage = new TabbedPage();

            // Act
            var actualColor = tabbedPage.SelectedTabColor;

            // Assert
            Assert.Equal(default(Color), actualColor);
        }

        /// <summary>
        /// Tests that the SelectedTabColor property can be set and retrieved multiple times with different values.
        /// Expected result: Each set operation should properly update the stored value.
        /// </summary>
        [Fact]
        public void SelectedTabColor_MultipleSetAndGetOperations_WorkCorrectly()
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var firstColor = Colors.Blue;
            var secondColor = Colors.Yellow;

            // Act & Assert - First set/get
            tabbedPage.SelectedTabColor = firstColor;
            Assert.Equal(firstColor, tabbedPage.SelectedTabColor);

            // Act & Assert - Second set/get
            tabbedPage.SelectedTabColor = secondColor;
            Assert.Equal(secondColor, tabbedPage.SelectedTabColor);
        }

        /// <summary>
        /// Tests that On method returns a valid IPlatformElementConfiguration instance
        /// when called with a type that implements IConfigPlatform.
        /// Expected result: Non-null IPlatformElementConfiguration instance.
        /// </summary>
        [Fact]
        public void On_WithValidConfigPlatformType_ReturnsValidConfiguration()
        {
            // Arrange
            var tabbedPage = new TabbedPage();

            // Act
            var result = tabbedPage.On<TestConfigPlatform>();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestConfigPlatform, TabbedPage>>(result);
        }

        /// <summary>
        /// Tests that On method returns the same instance when called multiple times
        /// with the same generic type parameter, verifying caching behavior.
        /// Expected result: Same instance returned on subsequent calls.
        /// </summary>
        [Fact]
        public void On_CalledMultipleTimesWithSameType_ReturnsSameInstance()
        {
            // Arrange
            var tabbedPage = new TabbedPage();

            // Act
            var firstCall = tabbedPage.On<TestConfigPlatform>();
            var secondCall = tabbedPage.On<TestConfigPlatform>();

            // Assert
            Assert.Same(firstCall, secondCall);
        }

        /// <summary>
        /// Tests that On method returns different instances when called with
        /// different generic type parameters.
        /// Expected result: Different instances for different platform types.
        /// </summary>
        [Fact]
        public void On_CalledWithDifferentTypes_ReturnsDifferentInstances()
        {
            // Arrange
            var tabbedPage = new TabbedPage();

            // Act
            var firstResult = tabbedPage.On<TestConfigPlatform>();
            var secondResult = tabbedPage.On<AnotherTestConfigPlatform>();

            // Assert
            Assert.NotSame(firstResult, secondResult);
        }

        /// <summary>
        /// Helper class implementing IConfigPlatform for testing purposes.
        /// Used to satisfy the generic constraint where T : IConfigPlatform.
        /// </summary>
        private class TestConfigPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Additional helper class implementing IConfigPlatform for testing purposes.
        /// Used to test different generic type parameters.
        /// </summary>
        private class AnotherTestConfigPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Tests that LayoutChildren can be called with normal positive values without throwing exceptions.
        /// The method is obsolete and intentionally empty, so we verify it executes successfully.
        /// </summary>
        [Fact]
        public void LayoutChildren_WithNormalValues_ExecutesWithoutException()
        {
            // Arrange
            var tabbedPageTestHelper = new TabbedPageTestHelper();
            double x = 10.0;
            double y = 20.0;
            double width = 300.0;
            double height = 400.0;

            // Act & Assert
            var exception = Record.Exception(() => tabbedPageTestHelper.TestLayoutChildren(x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that LayoutChildren can be called with zero values without throwing exceptions.
        /// Verifies boundary condition where position and size are zero.
        /// </summary>
        [Fact]
        public void LayoutChildren_WithZeroValues_ExecutesWithoutException()
        {
            // Arrange
            var tabbedPageTestHelper = new TabbedPageTestHelper();
            double x = 0.0;
            double y = 0.0;
            double width = 0.0;
            double height = 0.0;

            // Act & Assert
            var exception = Record.Exception(() => tabbedPageTestHelper.TestLayoutChildren(x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that LayoutChildren can be called with negative values without throwing exceptions.
        /// Verifies that the method handles negative coordinates and dimensions gracefully.
        /// </summary>
        [Fact]
        public void LayoutChildren_WithNegativeValues_ExecutesWithoutException()
        {
            // Arrange
            var tabbedPageTestHelper = new TabbedPageTestHelper();
            double x = -50.0;
            double y = -100.0;
            double width = -200.0;
            double height = -300.0;

            // Act & Assert
            var exception = Record.Exception(() => tabbedPageTestHelper.TestLayoutChildren(x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that LayoutChildren can be called with extreme double values without throwing exceptions.
        /// Verifies boundary conditions with minimum and maximum double values.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MinValue, double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue)]
        [InlineData(double.NaN, double.NaN, double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity)]
        public void LayoutChildren_WithExtremeValues_ExecutesWithoutException(double x, double y, double width, double height)
        {
            // Arrange
            var tabbedPageTestHelper = new TabbedPageTestHelper();

            // Act & Assert
            var exception = Record.Exception(() => tabbedPageTestHelper.TestLayoutChildren(x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that LayoutChildren can be called with mixed extreme values without throwing exceptions.
        /// Verifies various combinations of special double values work correctly.
        /// </summary>
        [Theory]
        [InlineData(0.0, double.MaxValue, double.MinValue, 100.0)]
        [InlineData(double.NaN, 50.0, double.PositiveInfinity, -200.0)]
        [InlineData(double.NegativeInfinity, double.NaN, 0.0, double.MaxValue)]
        public void LayoutChildren_WithMixedExtremeValues_ExecutesWithoutException(double x, double y, double width, double height)
        {
            // Arrange
            var tabbedPageTestHelper = new TabbedPageTestHelper();

            // Act & Assert
            var exception = Record.Exception(() => tabbedPageTestHelper.TestLayoutChildren(x, y, width, height));
            Assert.Null(exception);
        }

        /// <summary>
        /// Helper class that inherits from TabbedPage to expose the protected LayoutChildren method for testing.
        /// This allows us to test the protected method without violating encapsulation principles.
        /// </summary>
        private class TabbedPageTestHelper : TabbedPage
        {
            /// <summary>
            /// Exposes the protected LayoutChildren method for testing purposes.
            /// </summary>
            /// <param name="x">The x-coordinate of the layout area.</param>
            /// <param name="y">The y-coordinate of the layout area.</param>
            /// <param name="width">The width of the layout area.</param>
            /// <param name="height">The height of the layout area.</param>
            public void TestLayoutChildren(double x, double y, double width, double height)
            {
                LayoutChildren(x, y, width, height);
            }
        }

        /// <summary>
        /// Tests that the BarBackgroundColor property getter returns the default value when not explicitly set.
        /// This test verifies that GetValue is called with the correct bindable property and returns the expected default Color.
        /// </summary>
        [Fact]
        public void BarBackgroundColor_Get_ReturnsDefaultValue()
        {
            // Arrange
            var tabbedPage = new TabbedPage();

            // Act
            var result = tabbedPage.BarBackgroundColor;

            // Assert
            Assert.Equal(default(Color), result);
        }

        /// <summary>
        /// Tests that the BarBackgroundColor property can be set and retrieved correctly with various predefined colors.
        /// This test verifies the getter retrieves values that were set using the setter through the bindable property system.
        /// </summary>
        /// <param name="red">Red component of the test color</param>
        /// <param name="green">Green component of the test color</param>
        /// <param name="blue">Blue component of the test color</param>
        /// <param name="alpha">Alpha component of the test color</param>
        [Theory]
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Black
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Semi-transparent gray
        [InlineData(1.0f, 0.0f, 1.0f, 0.0f)] // Transparent magenta
        public void BarBackgroundColor_SetAndGet_RetievesCorrectColor(float red, float green, float blue, float alpha)
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            tabbedPage.BarBackgroundColor = expectedColor;
            var actualColor = tabbedPage.BarBackgroundColor;

            // Assert
            Assert.Equal(expectedColor.Red, actualColor.Red, precision: 6);
            Assert.Equal(expectedColor.Green, actualColor.Green, precision: 6);
            Assert.Equal(expectedColor.Blue, actualColor.Blue, precision: 6);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha, precision: 6);
        }

        /// <summary>
        /// Tests that the BarBackgroundColor property can be set to transparent color and retrieved correctly.
        /// This test verifies that transparent colors (alpha = 0) are handled properly by the getter.
        /// </summary>
        [Fact]
        public void BarBackgroundColor_SetTransparentColor_ReturnsTransparentColor()
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var transparentColor = new Color(0.8f, 0.2f, 0.4f, 0.0f);

            // Act
            tabbedPage.BarBackgroundColor = transparentColor;
            var result = tabbedPage.BarBackgroundColor;

            // Assert
            Assert.Equal(0.0f, result.Alpha, precision: 6);
            Assert.Equal(0.8f, result.Red, precision: 6);
            Assert.Equal(0.2f, result.Green, precision: 6);
            Assert.Equal(0.4f, result.Blue, precision: 6);
        }

        /// <summary>
        /// Tests that the BarBackgroundColor property can be set multiple times and always returns the most recent value.
        /// This test verifies that the getter consistently retrieves the correct value after multiple setter operations.
        /// </summary>
        [Fact]
        public void BarBackgroundColor_MultipleSetOperations_ReturnsLatestValue()
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var firstColor = new Color(1.0f, 0.0f, 0.0f, 1.0f); // Red
            var secondColor = new Color(0.0f, 1.0f, 0.0f, 1.0f); // Green
            var thirdColor = new Color(0.0f, 0.0f, 1.0f, 0.5f); // Semi-transparent blue

            // Act & Assert - First color
            tabbedPage.BarBackgroundColor = firstColor;
            var firstResult = tabbedPage.BarBackgroundColor;
            Assert.Equal(firstColor.Red, firstResult.Red, precision: 6);
            Assert.Equal(firstColor.Green, firstResult.Green, precision: 6);
            Assert.Equal(firstColor.Blue, firstResult.Blue, precision: 6);
            Assert.Equal(firstColor.Alpha, firstResult.Alpha, precision: 6);

            // Act & Assert - Second color
            tabbedPage.BarBackgroundColor = secondColor;
            var secondResult = tabbedPage.BarBackgroundColor;
            Assert.Equal(secondColor.Red, secondResult.Red, precision: 6);
            Assert.Equal(secondColor.Green, secondResult.Green, precision: 6);
            Assert.Equal(secondColor.Blue, secondResult.Blue, precision: 6);
            Assert.Equal(secondColor.Alpha, secondResult.Alpha, precision: 6);

            // Act & Assert - Third color
            tabbedPage.BarBackgroundColor = thirdColor;
            var thirdResult = tabbedPage.BarBackgroundColor;
            Assert.Equal(thirdColor.Red, thirdResult.Red, precision: 6);
            Assert.Equal(thirdColor.Green, thirdResult.Green, precision: 6);
            Assert.Equal(thirdColor.Blue, thirdResult.Blue, precision: 6);
            Assert.Equal(thirdColor.Alpha, thirdResult.Alpha, precision: 6);
        }

        /// <summary>
        /// Tests that the BarBackgroundColor property getter works correctly with extreme color component values.
        /// This test verifies that the getter handles edge cases for color components (0.0 and 1.0).
        /// </summary>
        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // All minimum values
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // All maximum values
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Mixed extreme values
        [InlineData(1.0f, 0.0f, 1.0f, 0.0f)] // Mixed extreme values
        public void BarBackgroundColor_ExtremeColorValues_ReturnsCorrectValues(float red, float green, float blue, float alpha)
        {
            // Arrange
            var tabbedPage = new TabbedPage();
            var extremeColor = new Color(red, green, blue, alpha);

            // Act
            tabbedPage.BarBackgroundColor = extremeColor;
            var result = tabbedPage.BarBackgroundColor;

            // Assert
            Assert.Equal(red, result.Red, precision: 6);
            Assert.Equal(green, result.Green, precision: 6);
            Assert.Equal(blue, result.Blue, precision: 6);
            Assert.Equal(alpha, result.Alpha, precision: 6);
        }

        /// <summary>
        /// Tests that the BarBackgroundColor property getter works correctly with colors created using different constructors.
        /// This test verifies that the getter retrieves colors regardless of how they were constructed.
        /// </summary>
        [Fact]
        public void BarBackgroundColor_ColorsFromDifferentConstructors_RetrievesCorrectly()
        {
            // Arrange
            var tabbedPage = new TabbedPage();

            // Test default constructor (black)
            var defaultColor = new Color();
            tabbedPage.BarBackgroundColor = defaultColor;
            var defaultResult = tabbedPage.BarBackgroundColor;
            Assert.Equal(0.0f, defaultResult.Red, precision: 6);
            Assert.Equal(0.0f, defaultResult.Green, precision: 6);
            Assert.Equal(0.0f, defaultResult.Blue, precision: 6);

            // Test gray constructor
            var grayColor = new Color(0.7f);
            tabbedPage.BarBackgroundColor = grayColor;
            var grayResult = tabbedPage.BarBackgroundColor;
            Assert.Equal(0.7f, grayResult.Red, precision: 6);
            Assert.Equal(0.7f, grayResult.Green, precision: 6);
            Assert.Equal(0.7f, grayResult.Blue, precision: 6);

            // Test RGB constructor
            var rgbColor = new Color(0.2f, 0.4f, 0.6f);
            tabbedPage.BarBackgroundColor = rgbColor;
            var rgbResult = tabbedPage.BarBackgroundColor;
            Assert.Equal(0.2f, rgbResult.Red, precision: 6);
            Assert.Equal(0.4f, rgbResult.Green, precision: 6);
            Assert.Equal(0.6f, rgbResult.Blue, precision: 6);
            Assert.Equal(1.0f, rgbResult.Alpha, precision: 6);
        }
    }
}