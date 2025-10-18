#nullable disable

using System;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class DisplayDensityChangedEventArgsTests
    {
        /// <summary>
        /// Tests the DisplayDensityChangedEventArgs constructor with various valid display density values.
        /// Verifies that the DisplayDensity property is correctly set to the provided value.
        /// </summary>
        /// <param name="displayDensity">The display density value to test.</param>
        [Theory]
        [InlineData(1.0f)]
        [InlineData(2.0f)]
        [InlineData(3.5f)]
        [InlineData(0.5f)]
        [InlineData(4.0f)]
        public void Constructor_WithValidDisplayDensity_SetsDisplayDensityProperty(float displayDensity)
        {
            // Act
            var eventArgs = new DisplayDensityChangedEventArgs(displayDensity);

            // Assert
            Assert.Equal(displayDensity, eventArgs.DisplayDensity);
        }

        /// <summary>
        /// Tests the DisplayDensityChangedEventArgs constructor with zero display density.
        /// Verifies that zero is accepted and stored correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithZeroDisplayDensity_SetsDisplayDensityToZero()
        {
            // Arrange
            float displayDensity = 0.0f;

            // Act
            var eventArgs = new DisplayDensityChangedEventArgs(displayDensity);

            // Assert
            Assert.Equal(0.0f, eventArgs.DisplayDensity);
        }

        /// <summary>
        /// Tests the DisplayDensityChangedEventArgs constructor with negative display density.
        /// Verifies that negative values are accepted without validation.
        /// </summary>
        [Fact]
        public void Constructor_WithNegativeDisplayDensity_SetsDisplayDensityToNegativeValue()
        {
            // Arrange
            float displayDensity = -1.5f;

            // Act
            var eventArgs = new DisplayDensityChangedEventArgs(displayDensity);

            // Assert
            Assert.Equal(-1.5f, eventArgs.DisplayDensity);
        }

        /// <summary>
        /// Tests the DisplayDensityChangedEventArgs constructor with extreme float values.
        /// Verifies that boundary values like float.MinValue and float.MaxValue are handled correctly.
        /// </summary>
        /// <param name="displayDensity">The extreme float value to test.</param>
        [Theory]
        [InlineData(float.MinValue)]
        [InlineData(float.MaxValue)]
        public void Constructor_WithExtremeFloatValues_SetsDisplayDensityCorrectly(float displayDensity)
        {
            // Act
            var eventArgs = new DisplayDensityChangedEventArgs(displayDensity);

            // Assert
            Assert.Equal(displayDensity, eventArgs.DisplayDensity);
        }

        /// <summary>
        /// Tests the DisplayDensityChangedEventArgs constructor with special float values.
        /// Verifies that NaN, PositiveInfinity, and NegativeInfinity are handled correctly.
        /// </summary>
        /// <param name="displayDensity">The special float value to test.</param>
        [Theory]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        [InlineData(float.NegativeInfinity)]
        public void Constructor_WithSpecialFloatValues_SetsDisplayDensityCorrectly(float displayDensity)
        {
            // Act
            var eventArgs = new DisplayDensityChangedEventArgs(displayDensity);

            // Assert
            if (float.IsNaN(displayDensity))
            {
                Assert.True(float.IsNaN(eventArgs.DisplayDensity));
            }
            else
            {
                Assert.Equal(displayDensity, eventArgs.DisplayDensity);
            }
        }

        /// <summary>
        /// Tests that DisplayDensityChangedEventArgs inherits from EventArgs correctly.
        /// Verifies the inheritance relationship is properly established.
        /// </summary>
        [Fact]
        public void Constructor_CreatesInstanceThatInheritsFromEventArgs()
        {
            // Act
            var eventArgs = new DisplayDensityChangedEventArgs(1.0f);

            // Assert
            Assert.IsAssignableFrom<EventArgs>(eventArgs);
        }
    }
}
