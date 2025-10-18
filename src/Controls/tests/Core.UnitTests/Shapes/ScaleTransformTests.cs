#nullable disable

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ScaleTransformTests
    {
        /// <summary>
        /// Tests that the parameterless constructor creates a ScaleTransform instance with default property values.
        /// Verifies that ScaleX and ScaleY default to 1.0, and CenterX and CenterY default to 0.0.
        /// </summary>
        [Fact]
        public void Constructor_Default_SetsDefaultPropertyValues()
        {
            // Arrange & Act
            var scaleTransform = new ScaleTransform();

            // Assert
            Assert.NotNull(scaleTransform);
            Assert.Equal(1.0, scaleTransform.ScaleX);
            Assert.Equal(1.0, scaleTransform.ScaleY);
            Assert.Equal(0.0, scaleTransform.CenterX);
            Assert.Equal(0.0, scaleTransform.CenterY);
        }

        /// <summary>
        /// Tests the ScaleTransform constructor with four parameters using normal positive values.
        /// Verifies that all parameters are correctly assigned to their respective properties.
        /// </summary>
        [Fact]
        public void Constructor_WithFourParameters_NormalPositiveValues_SetsAllPropertiesCorrectly()
        {
            // Arrange
            double scaleX = 2.5;
            double scaleY = 1.8;
            double centerX = 10.0;
            double centerY = 15.0;

            // Act
            var transform = new ScaleTransform(scaleX, scaleY, centerX, centerY);

            // Assert
            Assert.Equal(scaleX, transform.ScaleX);
            Assert.Equal(scaleY, transform.ScaleY);
            Assert.Equal(centerX, transform.CenterX);
            Assert.Equal(centerY, transform.CenterY);
        }

        /// <summary>
        /// Tests the ScaleTransform constructor with four parameters using various edge case values.
        /// Verifies that boundary values and special double values are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0, 0.0, 0.0)]
        [InlineData(-1.0, -2.0, -3.0, -4.0)]
        [InlineData(1.0, -1.0, 0.0, 100.0)]
        [InlineData(double.MaxValue, double.MinValue, 0.0, 0.0)]
        [InlineData(double.MinValue, double.MaxValue, double.MaxValue, double.MinValue)]
        public void Constructor_WithFourParameters_VariousValues_SetsAllPropertiesCorrectly(double scaleX, double scaleY, double centerX, double centerY)
        {
            // Act
            var transform = new ScaleTransform(scaleX, scaleY, centerX, centerY);

            // Assert
            Assert.Equal(scaleX, transform.ScaleX);
            Assert.Equal(scaleY, transform.ScaleY);
            Assert.Equal(centerX, transform.CenterX);
            Assert.Equal(centerY, transform.CenterY);
        }

        /// <summary>
        /// Tests the ScaleTransform constructor with four parameters using special double values.
        /// Verifies that NaN and infinity values are handled correctly without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 1.0, 2.0, 3.0)]
        [InlineData(1.0, double.NaN, 2.0, 3.0)]
        [InlineData(1.0, 2.0, double.NaN, 3.0)]
        [InlineData(1.0, 2.0, 3.0, double.NaN)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity, 0.0, 0.0)]
        [InlineData(double.NegativeInfinity, double.PositiveInfinity, double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN, double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity)]
        public void Constructor_WithFourParameters_SpecialDoubleValues_SetsAllPropertiesCorrectly(double scaleX, double scaleY, double centerX, double centerY)
        {
            // Act
            var transform = new ScaleTransform(scaleX, scaleY, centerX, centerY);

            // Assert
            Assert.Equal(scaleX, transform.ScaleX);
            Assert.Equal(scaleY, transform.ScaleY);
            Assert.Equal(centerX, transform.CenterX);
            Assert.Equal(centerY, transform.CenterY);
        }

        /// <summary>
        /// Tests that ScaleX property has the correct default value when using parameterless constructor.
        /// </summary>
        [Fact]
        public void ScaleX_DefaultConstructor_ReturnsDefaultValue()
        {
            // Arrange & Act
            var transform = new ScaleTransform();

            // Assert
            Assert.Equal(1.0, transform.ScaleX);
        }

        /// <summary>
        /// Tests that ScaleX property can be set and retrieved with normal double values.
        /// </summary>
        /// <param name="value">The ScaleX value to test.</param>
        /// <param name="expected">The expected retrieved value.</param>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(1.0, 1.0)]
        [InlineData(-1.0, -1.0)]
        [InlineData(2.5, 2.5)]
        [InlineData(-3.7, -3.7)]
        [InlineData(100.0, 100.0)]
        [InlineData(-100.0, -100.0)]
        public void ScaleX_SetValidValues_ReturnsSetValue(double value, double expected)
        {
            // Arrange
            var transform = new ScaleTransform();

            // Act
            transform.ScaleX = value;

            // Assert
            Assert.Equal(expected, transform.ScaleX);
        }

        /// <summary>
        /// Tests that ScaleX property can handle extreme boundary double values.
        /// </summary>
        /// <param name="value">The extreme ScaleX value to test.</param>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void ScaleX_SetBoundaryValues_ReturnsSetValue(double value)
        {
            // Arrange
            var transform = new ScaleTransform();

            // Act
            transform.ScaleX = value;

            // Assert
            Assert.Equal(value, transform.ScaleX);
        }

        /// <summary>
        /// Tests that ScaleX property can handle special double values like NaN and infinities.
        /// </summary>
        /// <param name="value">The special ScaleX value to test.</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ScaleX_SetSpecialValues_ReturnsSetValue(double value)
        {
            // Arrange
            var transform = new ScaleTransform();

            // Act
            transform.ScaleX = value;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(transform.ScaleX));
            }
            else
            {
                Assert.Equal(value, transform.ScaleX);
            }
        }

        /// <summary>
        /// Tests that ScaleX is correctly set when using the two-parameter constructor.
        /// </summary>
        /// <param name="scaleX">The ScaleX value for constructor.</param>
        /// <param name="scaleY">The ScaleY value for constructor.</param>
        [Theory]
        [InlineData(0.0, 1.0)]
        [InlineData(2.0, 3.0)]
        [InlineData(-1.5, 2.5)]
        [InlineData(double.MaxValue, 1.0)]
        [InlineData(double.NaN, 1.0)]
        public void ScaleX_TwoParameterConstructor_SetsCorrectValue(double scaleX, double scaleY)
        {
            // Arrange & Act
            var transform = new ScaleTransform(scaleX, scaleY);

            // Assert
            if (double.IsNaN(scaleX))
            {
                Assert.True(double.IsNaN(transform.ScaleX));
            }
            else
            {
                Assert.Equal(scaleX, transform.ScaleX);
            }
        }

        /// <summary>
        /// Tests that ScaleX is correctly set when using the four-parameter constructor.
        /// </summary>
        /// <param name="scaleX">The ScaleX value for constructor.</param>
        /// <param name="scaleY">The ScaleY value for constructor.</param>
        /// <param name="centerX">The CenterX value for constructor.</param>
        /// <param name="centerY">The CenterY value for constructor.</param>
        [Theory]
        [InlineData(0.5, 1.5, 10.0, 20.0)]
        [InlineData(-2.0, 3.0, -5.0, 15.0)]
        [InlineData(double.MinValue, 1.0, 0.0, 0.0)]
        [InlineData(double.PositiveInfinity, 1.0, 0.0, 0.0)]
        public void ScaleX_FourParameterConstructor_SetsCorrectValue(double scaleX, double scaleY, double centerX, double centerY)
        {
            // Arrange & Act
            var transform = new ScaleTransform(scaleX, scaleY, centerX, centerY);

            // Assert
            Assert.Equal(scaleX, transform.ScaleX);
        }

        /// <summary>
        /// Tests that ScaleX property can be set multiple times and always returns the last set value.
        /// </summary>
        [Fact]
        public void ScaleX_SetMultipleTimes_ReturnsLastSetValue()
        {
            // Arrange
            var transform = new ScaleTransform();

            // Act & Assert
            transform.ScaleX = 1.0;
            Assert.Equal(1.0, transform.ScaleX);

            transform.ScaleX = 2.5;
            Assert.Equal(2.5, transform.ScaleX);

            transform.ScaleX = -3.7;
            Assert.Equal(-3.7, transform.ScaleX);

            transform.ScaleX = 0.0;
            Assert.Equal(0.0, transform.ScaleX);
        }

        /// <summary>
        /// Tests that the ScaleY property can be set and retrieved with normal double values.
        /// </summary>
        /// <param name="value">The value to set for ScaleY.</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(2.5)]
        [InlineData(-1.0)]
        [InlineData(100.0)]
        [InlineData(0.001)]
        public void ScaleY_SetAndGet_ReturnsExpectedValue(double value)
        {
            // Arrange
            var transform = new ScaleTransform();

            // Act
            transform.ScaleY = value;
            var result = transform.ScaleY;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the ScaleY property handles extreme double values correctly.
        /// </summary>
        /// <param name="value">The extreme double value to test.</param>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ScaleY_SetExtremeValues_ReturnsExpectedValue(double value)
        {
            // Arrange
            var transform = new ScaleTransform();

            // Act
            transform.ScaleY = value;
            var result = transform.ScaleY;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(value, result);
            }
        }

        /// <summary>
        /// Tests that the ScaleY property has the correct default value of 1.0.
        /// </summary>
        [Fact]
        public void ScaleY_DefaultValue_ReturnsOne()
        {
            // Arrange
            var transform = new ScaleTransform();

            // Act
            var result = transform.ScaleY;

            // Assert
            Assert.Equal(1.0, result);
        }

        /// <summary>
        /// Tests that the ScaleY property can be set multiple times and maintains the last set value.
        /// </summary>
        [Fact]
        public void ScaleY_SetMultipleTimes_ReturnsLastSetValue()
        {
            // Arrange
            var transform = new ScaleTransform();

            // Act
            transform.ScaleY = 2.0;
            transform.ScaleY = 3.5;
            transform.ScaleY = -1.5;
            var result = transform.ScaleY;

            // Assert
            Assert.Equal(-1.5, result);
        }

        /// <summary>
        /// Tests that the CenterX property can be set and retrieved with a normal positive value.
        /// Validates the basic setter and getter functionality of the CenterX property.
        /// </summary>
        [Fact]
        public void CenterX_SetPositiveValue_ReturnsSetValue()
        {
            // Arrange
            var transform = new ScaleTransform();
            double expectedValue = 100.5;

            // Act
            transform.CenterX = expectedValue;
            double actualValue = transform.CenterX;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the CenterX property can be set and retrieved with zero value.
        /// Validates edge case handling for zero coordinate.
        /// </summary>
        [Fact]
        public void CenterX_SetZeroValue_ReturnsZero()
        {
            // Arrange
            var transform = new ScaleTransform();
            double expectedValue = 0.0;

            // Act
            transform.CenterX = expectedValue;
            double actualValue = transform.CenterX;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the CenterX property can be set and retrieved with a negative value.
        /// Validates handling of negative coordinates for center point.
        /// </summary>
        [Fact]
        public void CenterX_SetNegativeValue_ReturnsNegativeValue()
        {
            // Arrange
            var transform = new ScaleTransform();
            double expectedValue = -50.75;

            // Act
            transform.CenterX = expectedValue;
            double actualValue = transform.CenterX;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests CenterX property with various boundary and special double values.
        /// Validates edge case handling for extreme double values including infinity and NaN.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void CenterX_SetBoundaryValues_ReturnsSetValue(double value)
        {
            // Arrange
            var transform = new ScaleTransform();

            // Act
            transform.CenterX = value;
            double actualValue = transform.CenterX;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(actualValue));
            }
            else
            {
                Assert.Equal(value, actualValue);
            }
        }

        /// <summary>
        /// Tests that CenterX property has the correct default value when not explicitly set.
        /// Validates the initial state of the CenterX property matches the bindable property default.
        /// </summary>
        [Fact]
        public void CenterX_DefaultValue_ReturnsZero()
        {
            // Arrange & Act
            var transform = new ScaleTransform();
            double actualValue = transform.CenterX;

            // Assert
            Assert.Equal(0.0, actualValue);
        }

        /// <summary>
        /// Tests that CenterX property can be set multiple times with different values.
        /// Validates that subsequent assignments properly update the property value.
        /// </summary>
        [Fact]
        public void CenterX_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var transform = new ScaleTransform();

            // Act & Assert
            transform.CenterX = 10.0;
            Assert.Equal(10.0, transform.CenterX);

            transform.CenterX = -25.5;
            Assert.Equal(-25.5, transform.CenterX);

            transform.CenterX = 0.0;
            Assert.Equal(0.0, transform.CenterX);
        }

        /// <summary>
        /// Tests that the CenterY property can be set and retrieved correctly for various double values.
        /// Validates the setter and getter functionality with normal, edge case, and special double values.
        /// </summary>
        /// <param name="value">The double value to set and verify</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(100.5)]
        [InlineData(-100.5)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void CenterY_SetAndGet_ReturnsExpectedValue(double value)
        {
            // Arrange
            var scaleTransform = new ScaleTransform();

            // Act
            scaleTransform.CenterY = value;
            var result = scaleTransform.CenterY;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(value, result);
            }
        }

        /// <summary>
        /// Tests that the CenterY property has the correct default value of 0.0.
        /// Verifies that a new ScaleTransform instance initializes CenterY to its expected default.
        /// </summary>
        [Fact]
        public void CenterY_DefaultValue_ReturnsZero()
        {
            // Arrange
            var scaleTransform = new ScaleTransform();

            // Act
            var result = scaleTransform.CenterY;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that the CenterY property maintains its value after multiple set operations.
        /// Verifies that the property correctly handles value updates and maintains state.
        /// </summary>
        [Fact]
        public void CenterY_MultipleSetOperations_MaintainsCorrectValue()
        {
            // Arrange
            var scaleTransform = new ScaleTransform();

            // Act & Assert
            scaleTransform.CenterY = 10.0;
            Assert.Equal(10.0, scaleTransform.CenterY);

            scaleTransform.CenterY = -5.5;
            Assert.Equal(-5.5, scaleTransform.CenterY);

            scaleTransform.CenterY = 0.0;
            Assert.Equal(0.0, scaleTransform.CenterY);
        }

        /// <summary>
        /// Tests the ScaleTransform constructor with two parameters to verify that the scaleX and scaleY values
        /// are correctly assigned to the ScaleX and ScaleY properties for normal input values.
        /// </summary>
        /// <param name="scaleX">The X-axis scaling factor to test</param>
        /// <param name="scaleY">The Y-axis scaling factor to test</param>
        [Theory]
        [InlineData(1.0, 1.0)]
        [InlineData(2.0, 3.0)]
        [InlineData(-1.0, -2.0)]
        [InlineData(0.0, 0.0)]
        [InlineData(0.5, 1.5)]
        [InlineData(-0.5, -1.5)]
        [InlineData(100.0, 200.0)]
        [InlineData(-100.0, -200.0)]
        public void Constructor_WithScaleXAndScaleY_SetsPropertiesCorrectly(double scaleX, double scaleY)
        {
            // Arrange & Act
            var scaleTransform = new ScaleTransform(scaleX, scaleY);

            // Assert
            Assert.Equal(scaleX, scaleTransform.ScaleX);
            Assert.Equal(scaleY, scaleTransform.ScaleY);
        }

        /// <summary>
        /// Tests the ScaleTransform constructor with boundary double values to verify that extreme values
        /// are correctly handled and assigned to the ScaleX and ScaleY properties.
        /// </summary>
        [Fact]
        public void Constructor_WithBoundaryValues_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var scaleTransform = new ScaleTransform(double.MinValue, double.MaxValue);

            // Assert
            Assert.Equal(double.MinValue, scaleTransform.ScaleX);
            Assert.Equal(double.MaxValue, scaleTransform.ScaleY);
        }

        /// <summary>
        /// Tests the ScaleTransform constructor with NaN values to verify that NaN values
        /// are correctly assigned to the ScaleX and ScaleY properties.
        /// </summary>
        [Fact]
        public void Constructor_WithNaNValues_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var scaleTransform = new ScaleTransform(double.NaN, double.NaN);

            // Assert
            Assert.True(double.IsNaN(scaleTransform.ScaleX));
            Assert.True(double.IsNaN(scaleTransform.ScaleY));
        }

        /// <summary>
        /// Tests the ScaleTransform constructor with positive infinity values to verify that infinity values
        /// are correctly assigned to the ScaleX and ScaleY properties.
        /// </summary>
        [Fact]
        public void Constructor_WithPositiveInfinityValues_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var scaleTransform = new ScaleTransform(double.PositiveInfinity, double.PositiveInfinity);

            // Assert
            Assert.True(double.IsPositiveInfinity(scaleTransform.ScaleX));
            Assert.True(double.IsPositiveInfinity(scaleTransform.ScaleY));
        }

        /// <summary>
        /// Tests the ScaleTransform constructor with negative infinity values to verify that negative infinity values
        /// are correctly assigned to the ScaleX and ScaleY properties.
        /// </summary>
        [Fact]
        public void Constructor_WithNegativeInfinityValues_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var scaleTransform = new ScaleTransform(double.NegativeInfinity, double.NegativeInfinity);

            // Assert
            Assert.True(double.IsNegativeInfinity(scaleTransform.ScaleX));
            Assert.True(double.IsNegativeInfinity(scaleTransform.ScaleY));
        }

        /// <summary>
        /// Tests the ScaleTransform constructor with mixed special values to verify that different special double values
        /// can be correctly assigned to different properties simultaneously.
        /// </summary>
        [Fact]
        public void Constructor_WithMixedSpecialValues_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var scaleTransform = new ScaleTransform(double.NaN, double.PositiveInfinity);

            // Assert
            Assert.True(double.IsNaN(scaleTransform.ScaleX));
            Assert.True(double.IsPositiveInfinity(scaleTransform.ScaleY));
        }
    }
}