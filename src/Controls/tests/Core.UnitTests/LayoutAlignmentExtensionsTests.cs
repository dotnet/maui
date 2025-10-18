using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class LayoutAlignmentExtensionsTests
    {
        /// <summary>
        /// Tests that ToDouble returns the correct double value for valid LayoutAlignment values.
        /// Verifies that Start returns 0, Center returns 0.5, and End returns 1.
        /// </summary>
        /// <param name="alignment">The LayoutAlignment value to test</param>
        /// <param name="expected">The expected double return value</param>
        [Theory]
        [InlineData(LayoutAlignment.Start, 0.0)]
        [InlineData(LayoutAlignment.Center, 0.5)]
        [InlineData(LayoutAlignment.End, 1.0)]
        public void ToDouble_ValidLayoutAlignment_ReturnsExpectedValue(LayoutAlignment alignment, double expected)
        {
            // Act
            double result = alignment.ToDouble();

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that ToDouble throws ArgumentOutOfRangeException for the Fill enum value.
        /// The Fill value is a valid enum member but is not handled by the ToDouble method.
        /// </summary>
        [Fact]
        public void ToDouble_FillAlignment_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var alignment = LayoutAlignment.Fill;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => alignment.ToDouble());
            Assert.Equal("align", exception.ParamName);
        }

        /// <summary>
        /// Tests that ToDouble throws ArgumentOutOfRangeException for invalid LayoutAlignment values.
        /// Tests values that are outside the defined enum range to ensure proper error handling.
        /// </summary>
        /// <param name="invalidValue">An invalid integer value cast to LayoutAlignment</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(10)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void ToDouble_InvalidLayoutAlignment_ThrowsArgumentOutOfRangeException(int invalidValue)
        {
            // Arrange
            var alignment = (LayoutAlignment)invalidValue;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => alignment.ToDouble());
            Assert.Equal("align", exception.ParamName);
        }
    }
}
