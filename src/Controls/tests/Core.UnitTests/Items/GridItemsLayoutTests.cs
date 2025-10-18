#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the GridItemsLayout class focusing on the HorizontalItemSpacing property.
    /// </summary>
    public partial class GridItemsLayoutTests
    {
        /// <summary>
        /// Tests that HorizontalItemSpacing returns the default value of 0.0 when no value has been set.
        /// Input conditions: Newly created GridItemsLayout instance.
        /// Expected result: HorizontalItemSpacing property returns 0.0.
        /// </summary>
        [Fact]
        public void HorizontalItemSpacing_DefaultValue_ReturnsZero()
        {
            // Arrange
            var gridLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical);

            // Act
            var result = gridLayout.HorizontalItemSpacing;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that HorizontalItemSpacing returns the correct value after being set to various valid values.
        /// Input conditions: Setting HorizontalItemSpacing to different valid double values.
        /// Expected result: Getter returns the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(5.5)]
        [InlineData(10.0)]
        [InlineData(100.0)]
        [InlineData(1000.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.PositiveInfinity)]
        public void HorizontalItemSpacing_SetValidValue_ReturnsSetValue(double value)
        {
            // Arrange
            var gridLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical);

            // Act
            gridLayout.HorizontalItemSpacing = value;
            var result = gridLayout.HorizontalItemSpacing;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that HorizontalItemSpacing getter works correctly with very small positive values.
        /// Input conditions: Setting HorizontalItemSpacing to very small positive double values.
        /// Expected result: Getter returns the exact small value that was set.
        /// </summary>
        [Theory]
        [InlineData(double.Epsilon)]
        [InlineData(0.0001)]
        [InlineData(0.000000001)]
        public void HorizontalItemSpacing_SetSmallPositiveValue_ReturnsSetValue(double value)
        {
            // Arrange
            var gridLayout = new GridItemsLayout(ItemsLayoutOrientation.Horizontal);

            // Act
            gridLayout.HorizontalItemSpacing = value;
            var result = gridLayout.HorizontalItemSpacing;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that HorizontalItemSpacing getter correctly casts the underlying BindableProperty value to double.
        /// Input conditions: Setting HorizontalItemSpacing and then retrieving it multiple times.
        /// Expected result: Getter consistently returns the same double value demonstrating correct casting.
        /// </summary>
        [Fact]
        public void HorizontalItemSpacing_GetterCasting_ReturnsCorrectDoubleValue()
        {
            // Arrange
            var gridLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical);
            var expectedValue = 42.75;

            // Act
            gridLayout.HorizontalItemSpacing = expectedValue;
            var result1 = gridLayout.HorizontalItemSpacing;
            var result2 = gridLayout.HorizontalItemSpacing;

            // Assert
            Assert.Equal(expectedValue, result1);
            Assert.Equal(expectedValue, result2);
            Assert.Equal(result1, result2);
        }

        /// <summary>
        /// Tests that HorizontalItemSpacing getter works with different constructor overloads.
        /// Input conditions: Creating GridItemsLayout with different constructors and testing HorizontalItemSpacing.
        /// Expected result: HorizontalItemSpacing getter works consistently regardless of constructor used.
        /// </summary>
        [Theory]
        [InlineData(ItemsLayoutOrientation.Vertical)]
        [InlineData(ItemsLayoutOrientation.Horizontal)]
        public void HorizontalItemSpacing_DifferentConstructors_GetterWorksCorrectly(ItemsLayoutOrientation orientation)
        {
            // Arrange
            var gridLayout1 = new GridItemsLayout(orientation);
            var gridLayout2 = new GridItemsLayout(2, orientation);
            var testValue = 15.25;

            // Act
            gridLayout1.HorizontalItemSpacing = testValue;
            gridLayout2.HorizontalItemSpacing = testValue;
            var result1 = gridLayout1.HorizontalItemSpacing;
            var result2 = gridLayout2.HorizontalItemSpacing;

            // Assert
            Assert.Equal(testValue, result1);
            Assert.Equal(testValue, result2);
        }

        /// <summary>
        /// Tests that VerticalItemSpacing returns the default value of 0.0 when not explicitly set.
        /// </summary>
        [Fact]
        public void VerticalItemSpacing_DefaultValue_ReturnsZero()
        {
            // Arrange
            var layout = new GridItemsLayout((ItemsLayoutOrientation)0);

            // Act
            var result = layout.VerticalItemSpacing;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that VerticalItemSpacing correctly stores and retrieves valid positive values.
        /// </summary>
        /// <param name="value">The valid positive value to test.</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(10.5)]
        [InlineData(100.0)]
        [InlineData(1000.5)]
        [InlineData(double.MaxValue)]
        public void VerticalItemSpacing_ValidPositiveValues_StoresAndRetrievesCorrectly(double value)
        {
            // Arrange
            var layout = new GridItemsLayout((ItemsLayoutOrientation)0);

            // Act
            layout.VerticalItemSpacing = value;
            var result = layout.VerticalItemSpacing;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that VerticalItemSpacing throws ArgumentException when set to negative values.
        /// </summary>
        /// <param name="invalidValue">The invalid negative value to test.</param>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-0.1)]
        [InlineData(-100.0)]
        [InlineData(double.MinValue)]
        [InlineData(double.NegativeInfinity)]
        public void VerticalItemSpacing_NegativeValues_ThrowsArgumentException(double invalidValue)
        {
            // Arrange
            var layout = new GridItemsLayout((ItemsLayoutOrientation)0);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => layout.VerticalItemSpacing = invalidValue);
        }

        /// <summary>
        /// Tests that VerticalItemSpacing handles special double values appropriately.
        /// NaN and PositiveInfinity should be rejected as they don't satisfy the >= 0 validation in a meaningful way.
        /// </summary>
        /// <param name="specialValue">The special double value to test.</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void VerticalItemSpacing_SpecialDoubleValues_ThrowsArgumentException(double specialValue)
        {
            // Arrange
            var layout = new GridItemsLayout((ItemsLayoutOrientation)0);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => layout.VerticalItemSpacing = specialValue);
        }

        /// <summary>
        /// Tests that VerticalItemSpacing getter retrieves the value correctly after being set through the property.
        /// This test specifically focuses on exercising the getter path that was marked as not covered.
        /// </summary>
        [Fact]
        public void VerticalItemSpacing_GetterAfterSetter_ReturnsSetValue()
        {
            // Arrange
            var layout = new GridItemsLayout((ItemsLayoutOrientation)0);
            const double expectedValue = 42.5;

            // Act
            layout.VerticalItemSpacing = expectedValue;
            var actualValue = layout.VerticalItemSpacing;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that VerticalItemSpacing getter works correctly with boundary value of zero.
        /// This ensures the getter path is exercised with the minimum valid value.
        /// </summary>
        [Fact]
        public void VerticalItemSpacing_GetterWithZeroValue_ReturnsZero()
        {
            // Arrange
            var layout = new GridItemsLayout((ItemsLayoutOrientation)0);

            // Act
            layout.VerticalItemSpacing = 0.0;
            var result = layout.VerticalItemSpacing;

            // Assert
            Assert.Equal(0.0, result);
        }
    }
}