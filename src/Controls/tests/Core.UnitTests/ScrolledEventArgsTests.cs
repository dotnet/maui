#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ScrolledEventArgsTests
    {
        /// <summary>
        /// Tests that the ScrolledEventArgs constructor properly initializes ScrollX and ScrollY properties with normal values.
        /// Input: Normal positive and negative double values.
        /// Expected: Properties should be set to the exact values passed to constructor.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(100.5, 200.3)]
        [InlineData(-50.7, -75.2)]
        [InlineData(1.23456789, 9.87654321)]
        public void Constructor_WithNormalValues_SetsPropertiesCorrectly(double x, double y)
        {
            // Arrange & Act
            var args = new ScrolledEventArgs(x, y);

            // Assert
            Assert.Equal(x, args.ScrollX);
            Assert.Equal(y, args.ScrollY);
        }

        /// <summary>
        /// Tests that the ScrolledEventArgs constructor handles boundary double values correctly.
        /// Input: Double boundary values including MinValue and MaxValue.
        /// Expected: Properties should be set to the exact boundary values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MaxValue)]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue)]
        public void Constructor_WithBoundaryValues_SetsPropertiesCorrectly(double x, double y)
        {
            // Arrange & Act
            var args = new ScrolledEventArgs(x, y);

            // Assert
            Assert.Equal(x, args.ScrollX);
            Assert.Equal(y, args.ScrollY);
        }

        /// <summary>
        /// Tests that the ScrolledEventArgs constructor handles special double values correctly.
        /// Input: Special double values including NaN, PositiveInfinity, and NegativeInfinity.
        /// Expected: Properties should be set to the exact special values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.PositiveInfinity)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.NaN)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        public void Constructor_WithSpecialDoubleValues_SetsPropertiesCorrectly(double x, double y)
        {
            // Arrange & Act
            var args = new ScrolledEventArgs(x, y);

            // Assert
            Assert.Equal(x, args.ScrollX);
            Assert.Equal(y, args.ScrollY);
        }

        /// <summary>
        /// Tests that ScrolledEventArgs correctly inherits from EventArgs.
        /// Input: Any valid double values.
        /// Expected: Instance should be assignable to EventArgs type.
        /// </summary>
        [Fact]
        public void Constructor_CreatesInstanceThatInheritsFromEventArgs()
        {
            // Arrange & Act
            var args = new ScrolledEventArgs(10.0, 20.0);

            // Assert
            Assert.IsAssignableFrom<EventArgs>(args);
        }
    }
}
