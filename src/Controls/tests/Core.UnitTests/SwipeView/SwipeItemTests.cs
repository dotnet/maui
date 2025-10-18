#nullable disable

using System;
using System.Windows.Input;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the SwipeItem class.
    /// </summary>
    public partial class SwipeItemTests
    {
        /// <summary>
        /// Tests that the IsVisible property returns the default value of true when no explicit value has been set.
        /// This test verifies the default behavior defined by the IsVisibleProperty BindableProperty.
        /// </summary>
        [Fact]
        public void IsVisible_DefaultValue_ReturnsTrue()
        {
            // Arrange
            var swipeItem = new SwipeItem();

            // Act
            bool result = swipeItem.IsVisible;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the IsVisible property can be set to various boolean values and returns the correct value when retrieved.
        /// This test verifies both the setter and getter functionality of the IsVisible property.
        /// </summary>
        /// <param name="value">The boolean value to test setting and getting.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsVisible_SetAndGet_ReturnsCorrectValue(bool value)
        {
            // Arrange
            var swipeItem = new SwipeItem();

            // Act
            swipeItem.IsVisible = value;
            bool result = swipeItem.IsVisible;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the IsVisible property can be set multiple times with different values.
        /// This test verifies that the property correctly updates when changed multiple times.
        /// </summary>
        [Fact]
        public void IsVisible_MultipleSetOperations_UpdatesCorrectly()
        {
            // Arrange
            var swipeItem = new SwipeItem();

            // Act & Assert - Set to false
            swipeItem.IsVisible = false;
            Assert.False(swipeItem.IsVisible);

            // Act & Assert - Set back to true
            swipeItem.IsVisible = true;
            Assert.True(swipeItem.IsVisible);

            // Act & Assert - Set to false again
            swipeItem.IsVisible = false;
            Assert.False(swipeItem.IsVisible);
        }

        /// <summary>
        /// Tests that the BackgroundColor property returns the default value when not explicitly set.
        /// This test verifies the default behavior of the BackgroundColor bindable property.
        /// </summary>
        [Fact]
        public void BackgroundColor_DefaultValue_ReturnsDefaultColor()
        {
            // Arrange
            var swipeItem = new SwipeItem();

            // Act
            var backgroundColor = swipeItem.BackgroundColor;

            // Assert
            Assert.Null(backgroundColor);
        }

        /// <summary>
        /// Tests that the BackgroundColor property correctly stores and retrieves various Color values.
        /// This test verifies that the property setter and getter work correctly with different color inputs.
        /// </summary>
        [Theory]
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green  
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(0.0f, 0.0f, 0.0f, 1.0f)] // Black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Gray with transparency
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent
        [InlineData(1.0f, 0.5f, 0.25f, 0.75f)] // Custom color with partial alpha
        public void BackgroundColor_SetValidColor_ReturnsSetColor(float red, float green, float blue, float alpha)
        {
            // Arrange
            var swipeItem = new SwipeItem();
            var expectedColor = new Color(red, green, blue, alpha);

            // Act
            swipeItem.BackgroundColor = expectedColor;
            var actualColor = swipeItem.BackgroundColor;

            // Assert
            Assert.Equal(expectedColor.Red, actualColor.Red, 5);
            Assert.Equal(expectedColor.Green, actualColor.Green, 5);
            Assert.Equal(expectedColor.Blue, actualColor.Blue, 5);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha, 5);
        }

        /// <summary>
        /// Tests that the BackgroundColor property works correctly with predefined named colors.
        /// This test verifies that standard color constants can be assigned and retrieved properly.
        /// </summary>
        [Theory]
        [InlineData("Red")]
        [InlineData("Blue")]
        [InlineData("Green")]
        [InlineData("Yellow")]
        [InlineData("Black")]
        [InlineData("White")]
        [InlineData("Transparent")]
        public void BackgroundColor_SetNamedColors_ReturnsCorrectColor(string colorName)
        {
            // Arrange
            var swipeItem = new SwipeItem();
            var expectedColor = GetNamedColor(colorName);

            // Act
            swipeItem.BackgroundColor = expectedColor;
            var actualColor = swipeItem.BackgroundColor;

            // Assert
            Assert.Equal(expectedColor.Red, actualColor.Red, 5);
            Assert.Equal(expectedColor.Green, actualColor.Green, 5);
            Assert.Equal(expectedColor.Blue, actualColor.Blue, 5);
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha, 5);
        }

        /// <summary>
        /// Tests that the BackgroundColor property correctly handles colors with boundary alpha values.
        /// This test verifies edge cases for transparency handling.
        /// </summary>
        [Theory]
        [InlineData(0.0f)] // Fully transparent
        [InlineData(1.0f)] // Fully opaque
        [InlineData(0.5f)] // Semi-transparent
        public void BackgroundColor_SetColorsWithBoundaryAlpha_ReturnsCorrectColor(float alpha)
        {
            // Arrange
            var swipeItem = new SwipeItem();
            var expectedColor = new Color(0.5f, 0.5f, 0.5f, alpha);

            // Act
            swipeItem.BackgroundColor = expectedColor;
            var actualColor = swipeItem.BackgroundColor;

            // Assert
            Assert.Equal(expectedColor.Alpha, actualColor.Alpha, 5);
        }

        /// <summary>
        /// Tests that the BackgroundColor property handles colors created using different factory methods.
        /// This test verifies compatibility with various Color creation approaches.
        /// </summary>
        [Fact]
        public void BackgroundColor_SetColorsFromFactoryMethods_ReturnsCorrectColor()
        {
            // Arrange
            var swipeItem = new SwipeItem();

            // Test FromRgb factory method
            var colorFromRgb = Color.FromRgb(255, 128, 64);
            swipeItem.BackgroundColor = colorFromRgb;
            var retrievedFromRgb = swipeItem.BackgroundColor;

            Assert.Equal(colorFromRgb.Red, retrievedFromRgb.Red, 5);
            Assert.Equal(colorFromRgb.Green, retrievedFromRgb.Green, 5);
            Assert.Equal(colorFromRgb.Blue, retrievedFromRgb.Blue, 5);
            Assert.Equal(colorFromRgb.Alpha, retrievedFromRgb.Alpha, 5);

            // Test FromRgba factory method
            var colorFromRgba = Color.FromRgba(200, 100, 50, 128);
            swipeItem.BackgroundColor = colorFromRgba;
            var retrievedFromRgba = swipeItem.BackgroundColor;

            Assert.Equal(colorFromRgba.Red, retrievedFromRgba.Red, 5);
            Assert.Equal(colorFromRgba.Green, retrievedFromRgba.Green, 5);
            Assert.Equal(colorFromRgba.Blue, retrievedFromRgba.Blue, 5);
            Assert.Equal(colorFromRgba.Alpha, retrievedFromRgba.Alpha, 5);
        }

        /// <summary>
        /// Tests that setting BackgroundColor multiple times correctly updates the value.
        /// This test verifies that the property can be changed and the changes are persisted.
        /// </summary>
        [Fact]
        public void BackgroundColor_SetMultipleTimes_ReturnsLatestColor()
        {
            // Arrange
            var swipeItem = new SwipeItem();
            var firstColor = Colors.Red;
            var secondColor = Colors.Blue;

            // Act & Assert
            swipeItem.BackgroundColor = firstColor;
            Assert.Equal(firstColor.Red, swipeItem.BackgroundColor.Red, 5);

            swipeItem.BackgroundColor = secondColor;
            Assert.Equal(secondColor.Blue, swipeItem.BackgroundColor.Blue, 5);
        }

        /// <summary>
        /// Helper method to get named colors for testing.
        /// Returns the appropriate Color constant based on the color name.
        /// </summary>
        /// <param name="colorName">The name of the color to retrieve.</param>
        /// <returns>The corresponding Color value.</returns>
        private Color GetNamedColor(string colorName)
        {
            return colorName switch
            {
                "Red" => Colors.Red,
                "Blue" => Colors.Blue,
                "Green" => Colors.Green,
                "Yellow" => Colors.Yellow,
                "Black" => Colors.Black,
                "White" => Colors.White,
                "Transparent" => Colors.Transparent,
                _ => Colors.Black
            };
        }
    }
}