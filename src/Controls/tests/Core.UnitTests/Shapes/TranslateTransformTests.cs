#nullable disable

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class TranslateTransformTests
    {
        /// <summary>
        /// Tests that the TranslateTransform constructor correctly sets the X and Y properties
        /// when provided with various double values including normal values, edge cases, and special values.
        /// </summary>
        /// <param name="x">The X coordinate value to test</param>
        /// <param name="y">The Y coordinate value to test</param>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(1.0, 2.0)]
        [InlineData(-1.0, -2.0)]
        [InlineData(100.5, 200.7)]
        [InlineData(-100.5, -200.7)]
        public void Constructor_WithValidDoubleValues_SetsXAndYProperties(double x, double y)
        {
            // Act
            var transform = new TranslateTransform(x, y);

            // Assert
            Assert.Equal(x, transform.X);
            Assert.Equal(y, transform.Y);
        }

        /// <summary>
        /// Tests that the TranslateTransform constructor correctly handles extreme double values
        /// including minimum, maximum, and boundary values.
        /// </summary>
        /// <param name="x">The X coordinate extreme value to test</param>
        /// <param name="y">The Y coordinate extreme value to test</param>
        [Theory]
        [InlineData(double.MinValue, double.MaxValue)]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.Epsilon, -double.Epsilon)]
        [InlineData(-double.Epsilon, double.Epsilon)]
        public void Constructor_WithExtremeDoubleValues_SetsXAndYProperties(double x, double y)
        {
            // Act
            var transform = new TranslateTransform(x, y);

            // Assert
            Assert.Equal(x, transform.X);
            Assert.Equal(y, transform.Y);
        }

        /// <summary>
        /// Tests that the TranslateTransform constructor correctly handles special double values
        /// including NaN and infinity values.
        /// </summary>
        /// <param name="x">The X coordinate special value to test</param>
        /// <param name="y">The Y coordinate special value to test</param>
        [Theory]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.PositiveInfinity)]
        [InlineData(double.NaN, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NaN)]
        public void Constructor_WithSpecialDoubleValues_SetsXAndYProperties(double x, double y)
        {
            // Act
            var transform = new TranslateTransform(x, y);

            // Assert
            Assert.Equal(x, transform.X);
            Assert.Equal(y, transform.Y);
        }

        /// <summary>
        /// Tests that setting the X property with a normal positive value correctly stores the value.
        /// </summary>
        [Fact]
        public void X_SetPositiveValue_ValueIsStored()
        {
            // Arrange
            var transform = new TranslateTransform();
            var expectedValue = 10.5;

            // Act
            transform.X = expectedValue;

            // Assert
            Assert.Equal(expectedValue, transform.X);
        }

        /// <summary>
        /// Tests that setting the X property with a normal negative value correctly stores the value.
        /// </summary>
        [Fact]
        public void X_SetNegativeValue_ValueIsStored()
        {
            // Arrange
            var transform = new TranslateTransform();
            var expectedValue = -15.75;

            // Act
            transform.X = expectedValue;

            // Assert
            Assert.Equal(expectedValue, transform.X);
        }

        /// <summary>
        /// Tests that setting the X property with zero correctly stores the value.
        /// </summary>
        [Fact]
        public void X_SetZero_ValueIsStored()
        {
            // Arrange
            var transform = new TranslateTransform();
            var expectedValue = 0.0;

            // Act
            transform.X = expectedValue;

            // Assert
            Assert.Equal(expectedValue, transform.X);
        }

        /// <summary>
        /// Tests that setting the X property with boundary values correctly stores the values.
        /// </summary>
        /// <param name="value">The boundary value to test.</param>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        public void X_SetBoundaryValues_ValueIsStored(double value)
        {
            // Arrange
            var transform = new TranslateTransform();

            // Act
            transform.X = value;

            // Assert
            Assert.Equal(value, transform.X);
        }

        /// <summary>
        /// Tests that setting the X property with special double values correctly stores the values.
        /// </summary>
        /// <param name="value">The special double value to test.</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void X_SetSpecialDoubleValues_ValueIsStored(double value)
        {
            // Arrange
            var transform = new TranslateTransform();

            // Act
            transform.X = value;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(transform.X));
            }
            else
            {
                Assert.Equal(value, transform.X);
            }
        }

        /// <summary>
        /// Tests that setting the X property with very small values correctly stores the values.
        /// </summary>
        [Fact]
        public void X_SetVerySmallValue_ValueIsStored()
        {
            // Arrange
            var transform = new TranslateTransform();
            var expectedValue = double.Epsilon;

            // Act
            transform.X = expectedValue;

            // Assert
            Assert.Equal(expectedValue, transform.X);
        }

        /// <summary>
        /// Tests that setting the X property multiple times correctly updates the stored value.
        /// </summary>
        [Fact]
        public void X_SetMultipleTimes_LastValueIsStored()
        {
            // Arrange
            var transform = new TranslateTransform();
            var firstValue = 5.0;
            var secondValue = 10.0;
            var finalValue = -3.5;

            // Act
            transform.X = firstValue;
            transform.X = secondValue;
            transform.X = finalValue;

            // Assert
            Assert.Equal(finalValue, transform.X);
        }

        /// <summary>
        /// Tests that the X property has the correct default value when created with parameterless constructor.
        /// </summary>
        [Fact]
        public void X_DefaultValue_IsZero()
        {
            // Arrange & Act
            var transform = new TranslateTransform();

            // Assert
            Assert.Equal(0.0, transform.X);
        }

        /// <summary>
        /// Tests that the X property is correctly set when using the parameterized constructor.
        /// </summary>
        [Fact]
        public void X_ParameterizedConstructor_ValueIsSet()
        {
            // Arrange
            var expectedX = 25.5;
            var expectedY = 30.0;

            // Act
            var transform = new TranslateTransform(expectedX, expectedY);

            // Assert
            Assert.Equal(expectedX, transform.X);
        }

        /// <summary>
        /// Tests that the Y property setter correctly stores normal double values.
        /// Verifies that the value can be set and retrieved correctly.
        /// Expected result: The Y property should return the exact value that was set.
        /// </summary>
        /// <param name="value">The double value to test</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(100.5)]
        [InlineData(-200.75)]
        [InlineData(0.000001)]
        [InlineData(-0.000001)]
        public void Y_SetNormalDoubleValue_ValueIsStoredCorrectly(double value)
        {
            // Arrange
            var transform = new TranslateTransform();

            // Act
            transform.Y = value;

            // Assert
            Assert.Equal(value, transform.Y);
        }

        /// <summary>
        /// Tests that the Y property setter correctly handles extreme double values.
        /// Verifies that edge case values like MinValue, MaxValue, NaN, and infinity are handled properly.
        /// Expected result: The Y property should return the exact extreme value that was set.
        /// </summary>
        /// <param name="value">The extreme double value to test</param>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void Y_SetExtremeDoubleValue_ValueIsStoredCorrectly(double value)
        {
            // Arrange
            var transform = new TranslateTransform();

            // Act
            transform.Y = value;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(transform.Y));
            }
            else
            {
                Assert.Equal(value, transform.Y);
            }
        }

        /// <summary>
        /// Tests that the Y property setter works correctly when called multiple times.
        /// Verifies that subsequent calls to the setter properly update the stored value.
        /// Expected result: Each call to the setter should update the Y property value.
        /// </summary>
        [Fact]
        public void Y_SetMultipleTimes_LastValueIsStored()
        {
            // Arrange
            var transform = new TranslateTransform();
            var firstValue = 10.5;
            var secondValue = -25.75;
            var thirdValue = 0.0;

            // Act & Assert
            transform.Y = firstValue;
            Assert.Equal(firstValue, transform.Y);

            transform.Y = secondValue;
            Assert.Equal(secondValue, transform.Y);

            transform.Y = thirdValue;
            Assert.Equal(thirdValue, transform.Y);
        }

        /// <summary>
        /// Tests that the Y property setter works correctly on an instance created with the parameterized constructor.
        /// Verifies that the setter can override the initial value set by the constructor.
        /// Expected result: The Y property should be updated to the new value, overriding the constructor value.
        /// </summary>
        [Fact]
        public void Y_SetOnInstanceWithParameterizedConstructor_ValueIsUpdated()
        {
            // Arrange
            var initialY = 50.0;
            var newYValue = 100.0;
            var transform = new TranslateTransform(25.0, initialY);

            // Act
            transform.Y = newYValue;

            // Assert
            Assert.Equal(newYValue, transform.Y);
        }

        /// <summary>
        /// Tests that the parameterless constructor successfully creates a TranslateTransform instance
        /// with default property values.
        /// Verifies that X and Y properties are initialized to their default values (0.0).
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_CreatesInstanceWithDefaultValues()
        {
            // Arrange & Act
            var transform = new TranslateTransform();

            // Assert
            Assert.NotNull(transform);
            Assert.IsType<TranslateTransform>(transform);
            Assert.IsAssignableFrom<Transform>(transform);
            Assert.Equal(0.0, transform.X);
            Assert.Equal(0.0, transform.Y);
        }
    }
}