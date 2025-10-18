#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the LinearItemsLayout class.
    /// </summary>
    public class LinearItemsLayoutTests
    {
        /// <summary>
        /// Tests that ItemSpacing property getter returns the default value when not explicitly set.
        /// Input: Default constructed LinearItemsLayout
        /// Expected: ItemSpacing returns default double value (0.0)
        /// </summary>
        [Fact]
        public void ItemSpacing_WhenNotSet_ReturnsDefaultValue()
        {
            // Arrange
            var layout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);

            // Act
            var result = layout.ItemSpacing;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that ItemSpacing property can be set and retrieved with valid positive values.
        /// Input: Valid positive double values
        /// Expected: Property getter returns the exact value that was set
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(10.5)]
        [InlineData(100.0)]
        [InlineData(1000.123)]
        [InlineData(double.MaxValue)]
        public void ItemSpacing_WithValidPositiveValues_SetsAndGetsCorrectly(double value)
        {
            // Arrange
            var layout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);

            // Act
            layout.ItemSpacing = value;
            var result = layout.ItemSpacing;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that ItemSpacing property setter handles very small positive values correctly.
        /// Input: Very small positive double values near zero
        /// Expected: Property getter returns the exact value that was set
        /// </summary>
        [Theory]
        [InlineData(double.Epsilon)]
        [InlineData(1e-10)]
        [InlineData(1e-100)]
        public void ItemSpacing_WithVerySmallPositiveValues_SetsAndGetsCorrectly(double value)
        {
            // Arrange
            var layout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);

            // Act
            layout.ItemSpacing = value;
            var result = layout.ItemSpacing;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that ItemSpacing property setter handles boundary and edge case values.
        /// Input: Edge case double values including infinities and NaN
        /// Expected: Property getter returns the exact value that was set (if validation allows)
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NaN)]
        public void ItemSpacing_WithEdgeCaseValues_SetsAndGetsCorrectly(double value)
        {
            // Arrange
            var layout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);

            // Act
            layout.ItemSpacing = value;
            var result = layout.ItemSpacing;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that ItemSpacing property handles negative values according to validation rules.
        /// Input: Negative double values that should be rejected by validation
        /// Expected: Values that don't pass validation should not be set
        /// </summary>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-10.5)]
        [InlineData(-100.0)]
        [InlineData(double.MinValue)]
        [InlineData(double.NegativeInfinity)]
        public void ItemSpacing_WithNegativeValues_DoesNotChangeValue(double negativeValue)
        {
            // Arrange
            var layout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
            var initialValue = layout.ItemSpacing; // Should be 0.0 (default)

            // Act
            layout.ItemSpacing = negativeValue;
            var result = layout.ItemSpacing;

            // Assert
            // Since validation should reject negative values, the property should retain its initial value
            Assert.Equal(initialValue, result);
        }

        /// <summary>
        /// Tests that ItemSpacing property can be set multiple times with different valid values.
        /// Input: Sequence of different valid positive values
        /// Expected: Each subsequent set operation updates the property correctly
        /// </summary>
        [Fact]
        public void ItemSpacing_SetMultipleTimes_UpdatesCorrectly()
        {
            // Arrange
            var layout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);

            // Act & Assert
            layout.ItemSpacing = 5.0;
            Assert.Equal(5.0, layout.ItemSpacing);

            layout.ItemSpacing = 0.0;
            Assert.Equal(0.0, layout.ItemSpacing);

            layout.ItemSpacing = 999.99;
            Assert.Equal(999.99, layout.ItemSpacing);
        }

        /// <summary>
        /// Tests that ItemSpacing property works correctly with both horizontal and vertical orientations.
        /// Input: LinearItemsLayout with different orientations
        /// Expected: ItemSpacing behavior is consistent regardless of orientation
        /// </summary>
        [Theory]
        [InlineData(ItemsLayoutOrientation.Horizontal)]
        [InlineData(ItemsLayoutOrientation.Vertical)]
        public void ItemSpacing_WithDifferentOrientations_WorksCorrectly(ItemsLayoutOrientation orientation)
        {
            // Arrange
            var layout = new LinearItemsLayout(orientation);
            const double testValue = 42.5;

            // Act
            layout.ItemSpacing = testValue;
            var result = layout.ItemSpacing;

            // Assert
            Assert.Equal(testValue, result);
        }
    }
}
