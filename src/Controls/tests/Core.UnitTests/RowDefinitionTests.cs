#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the RowDefinition class.
    /// </summary>
    public partial class RowDefinitionTests
    {
        /// <summary>
        /// Tests that the RowDefinition constructor with GridLength parameter correctly sets the Height property to GridLength.Auto.
        /// </summary>
        [Fact]
        public void Constructor_WithAutoGridLength_SetsHeightToAuto()
        {
            // Arrange
            var height = GridLength.Auto;

            // Act
            var rowDefinition = new RowDefinition(height);

            // Assert
            Assert.Equal(GridLength.Auto, rowDefinition.Height);
            Assert.True(rowDefinition.Height.IsAuto);
        }

        /// <summary>
        /// Tests that the RowDefinition constructor with GridLength parameter correctly sets the Height property to GridLength.Star.
        /// </summary>
        [Fact]
        public void Constructor_WithStarGridLength_SetsHeightToStar()
        {
            // Arrange
            var height = GridLength.Star;

            // Act
            var rowDefinition = new RowDefinition(height);

            // Assert
            Assert.Equal(GridLength.Star, rowDefinition.Height);
            Assert.True(rowDefinition.Height.IsStar);
        }

        /// <summary>
        /// Tests that the RowDefinition constructor with GridLength parameter correctly sets the Height property with various absolute values.
        /// </summary>
        /// <param name="value">The absolute value to test.</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(42.5)]
        [InlineData(100.0)]
        [InlineData(1000.5)]
        public void Constructor_WithAbsoluteGridLength_SetsHeightToAbsoluteValue(double value)
        {
            // Arrange
            var height = new GridLength(value);

            // Act
            var rowDefinition = new RowDefinition(height);

            // Assert
            Assert.Equal(height, rowDefinition.Height);
            Assert.True(rowDefinition.Height.IsAbsolute);
            Assert.Equal(value, rowDefinition.Height.Value);
        }

        /// <summary>
        /// Tests that the RowDefinition constructor with GridLength parameter correctly handles boundary values.
        /// </summary>
        /// <param name="value">The boundary value to test.</param>
        /// <param name="unitType">The GridUnitType to test.</param>
        [Theory]
        [InlineData(double.MaxValue, GridUnitType.Absolute)]
        [InlineData(double.Epsilon, GridUnitType.Absolute)]
        [InlineData(double.MaxValue, GridUnitType.Star)]
        [InlineData(double.Epsilon, GridUnitType.Star)]
        public void Constructor_WithBoundaryGridLengthValues_SetsHeightCorrectly(double value, GridUnitType unitType)
        {
            // Arrange
            var height = new GridLength(value, unitType);

            // Act
            var rowDefinition = new RowDefinition(height);

            // Assert
            Assert.Equal(height, rowDefinition.Height);
            Assert.Equal(value, rowDefinition.Height.Value);
            Assert.Equal(unitType, rowDefinition.Height.GridUnitType);
        }

        /// <summary>
        /// Tests that the RowDefinition constructor with GridLength parameter correctly handles custom Star GridLength with specific values.
        /// </summary>
        /// <param name="starValue">The star weight value to test.</param>
        [Theory]
        [InlineData(2.0)]
        [InlineData(0.5)]
        [InlineData(10.0)]
        public void Constructor_WithCustomStarGridLength_SetsHeightToStarValue(double starValue)
        {
            // Arrange
            var height = new GridLength(starValue, GridUnitType.Star);

            // Act
            var rowDefinition = new RowDefinition(height);

            // Assert
            Assert.Equal(height, rowDefinition.Height);
            Assert.True(rowDefinition.Height.IsStar);
            Assert.Equal(starValue, rowDefinition.Height.Value);
        }

        /// <summary>
        /// Tests that the RowDefinition constructor with GridLength parameter correctly handles implicit conversion from double.
        /// </summary>
        [Fact]
        public void Constructor_WithImplicitDoubleConversion_SetsHeightToAbsoluteValue()
        {
            // Arrange
            double value = 50.0;
            GridLength height = value; // Implicit conversion

            // Act
            var rowDefinition = new RowDefinition(height);

            // Assert
            Assert.Equal(new GridLength(value), rowDefinition.Height);
            Assert.True(rowDefinition.Height.IsAbsolute);
            Assert.Equal(value, rowDefinition.Height.Value);
        }
    }
}
