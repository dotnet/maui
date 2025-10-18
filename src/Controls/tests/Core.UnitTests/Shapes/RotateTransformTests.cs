#nullable disable

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class RotateTransformTests
    {
        /// <summary>
        /// Tests that the RotateTransform constructor with angle parameter properly assigns the angle value to the Angle property.
        /// Tests various angle values including normal rotation angles, negative angles, fractional values, and boundary cases.
        /// Verifies that the Angle property is correctly set to the provided parameter value.
        /// </summary>
        /// <param name="angle">The angle value to test</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(45.0)]
        [InlineData(90.0)]
        [InlineData(180.0)]
        [InlineData(270.0)]
        [InlineData(360.0)]
        [InlineData(-45.0)]
        [InlineData(-90.0)]
        [InlineData(-180.0)]
        [InlineData(-360.0)]
        [InlineData(45.5)]
        [InlineData(90.25)]
        [InlineData(720.0)]
        [InlineData(1080.0)]
        [InlineData(0.001)]
        [InlineData(-0.001)]
        public void Constructor_WithAngle_SetsAngleProperty(double angle)
        {
            // Arrange & Act
            var rotateTransform = new RotateTransform(angle);

            // Assert
            Assert.Equal(angle, rotateTransform.Angle);
        }

        /// <summary>
        /// Tests that the RotateTransform constructor with angle parameter handles special double values correctly.
        /// Tests boundary values including MinValue, MaxValue, NaN, PositiveInfinity, and NegativeInfinity.
        /// Verifies that the Angle property is correctly set to the provided special value.
        /// </summary>
        /// <param name="angle">The special double value to test</param>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void Constructor_WithSpecialDoubleValues_SetsAngleProperty(double angle)
        {
            // Arrange & Act
            var rotateTransform = new RotateTransform(angle);

            // Assert
            if (double.IsNaN(angle))
            {
                Assert.True(double.IsNaN(rotateTransform.Angle));
            }
            else
            {
                Assert.Equal(angle, rotateTransform.Angle);
            }
        }

        /// <summary>
        /// Tests that the RotateTransform constructor with angle parameter properly initializes other properties to their default values.
        /// Verifies that CenterX and CenterY properties are set to their default values (0.0) when using the single-parameter constructor.
        /// Ensures the constructor only affects the Angle property and leaves other properties at their defaults.
        /// </summary>
        [Fact]
        public void Constructor_WithAngle_InitializesOtherPropertiesToDefaults()
        {
            // Arrange
            double testAngle = 45.0;

            // Act
            var rotateTransform = new RotateTransform(testAngle);

            // Assert
            Assert.Equal(testAngle, rotateTransform.Angle);
            Assert.Equal(0.0, rotateTransform.CenterX);
            Assert.Equal(0.0, rotateTransform.CenterY);
        }

        /// <summary>
        /// Tests that the RotateTransform constructor with angle, centerX, and centerY parameters
        /// correctly assigns the provided values to their respective properties.
        /// Tests normal double values to ensure basic functionality works as expected.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0, 0.0)]
        [InlineData(45.0, 10.0, 20.0)]
        [InlineData(-90.0, -5.0, -15.0)]
        [InlineData(180.0, 100.5, 200.75)]
        [InlineData(360.0, 0.1, 0.2)]
        public void Constructor_WithAngleCenterXCenterY_SetsPropertiesCorrectly(double angle, double centerX, double centerY)
        {
            // Arrange & Act
            var rotateTransform = new RotateTransform(angle, centerX, centerY);

            // Assert
            Assert.Equal(angle, rotateTransform.Angle);
            Assert.Equal(centerX, rotateTransform.CenterX);
            Assert.Equal(centerY, rotateTransform.CenterY);
        }

        /// <summary>
        /// Tests that the RotateTransform constructor handles extreme double values correctly.
        /// Tests boundary values including MinValue and MaxValue to ensure no overflow or precision issues.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, 0.0, double.MaxValue)]
        [InlineData(0.0, double.MinValue, double.MaxValue)]
        public void Constructor_WithExtremeValues_SetsPropertiesCorrectly(double angle, double centerX, double centerY)
        {
            // Arrange & Act
            var rotateTransform = new RotateTransform(angle, centerX, centerY);

            // Assert
            Assert.Equal(angle, rotateTransform.Angle);
            Assert.Equal(centerX, rotateTransform.CenterX);
            Assert.Equal(centerY, rotateTransform.CenterY);
        }

        /// <summary>
        /// Tests that the RotateTransform constructor handles special double values correctly.
        /// Tests NaN, PositiveInfinity, and NegativeInfinity values to ensure they are stored properly.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, 0.0, double.PositiveInfinity)]
        [InlineData(double.PositiveInfinity, double.NaN, double.NegativeInfinity)]
        public void Constructor_WithSpecialDoubleValues_SetsPropertiesCorrectly(double angle, double centerX, double centerY)
        {
            // Arrange & Act
            var rotateTransform = new RotateTransform(angle, centerX, centerY);

            // Assert
            Assert.Equal(angle, rotateTransform.Angle);
            Assert.Equal(centerX, rotateTransform.CenterX);
            Assert.Equal(centerY, rotateTransform.CenterY);
        }

        /// <summary>
        /// Tests that the RotateTransform constructor handles very small values correctly.
        /// Tests epsilon and very small positive/negative values to ensure precision is maintained.
        /// </summary>
        [Theory]
        [InlineData(double.Epsilon, double.Epsilon, double.Epsilon)]
        [InlineData(-double.Epsilon, -double.Epsilon, -double.Epsilon)]
        [InlineData(1e-300, 1e-300, 1e-300)]
        [InlineData(-1e-300, -1e-300, -1e-300)]
        public void Constructor_WithVerySmallValues_SetsPropertiesCorrectly(double angle, double centerX, double centerY)
        {
            // Arrange & Act
            var rotateTransform = new RotateTransform(angle, centerX, centerY);

            // Assert
            Assert.Equal(angle, rotateTransform.Angle);
            Assert.Equal(centerX, rotateTransform.CenterX);
            Assert.Equal(centerY, rotateTransform.CenterY);
        }

        /// <summary>
        /// Tests that the RotateTransform constructor handles large values correctly.
        /// Tests very large positive and negative values to ensure they don't cause issues.
        /// </summary>
        [Theory]
        [InlineData(1e300, 1e300, 1e300)]
        [InlineData(-1e300, -1e300, -1e300)]
        [InlineData(1e100, -1e100, 1e200)]
        public void Constructor_WithVeryLargeValues_SetsPropertiesCorrectly(double angle, double centerX, double centerY)
        {
            // Arrange & Act
            var rotateTransform = new RotateTransform(angle, centerX, centerY);

            // Assert
            Assert.Equal(angle, rotateTransform.Angle);
            Assert.Equal(centerX, rotateTransform.CenterX);
            Assert.Equal(centerY, rotateTransform.CenterY);
        }

        /// <summary>
        /// Tests that the Angle property setter stores the value correctly and the getter retrieves it.
        /// Tests various double values including edge cases like infinity, NaN, and boundary values.
        /// </summary>
        /// <param name="angleValue">The angle value to test</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(90.0)]
        [InlineData(-90.0)]
        [InlineData(180.0)]
        [InlineData(-180.0)]
        [InlineData(360.0)]
        [InlineData(-360.0)]
        [InlineData(720.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(1.23456789)]
        [InlineData(-1.23456789)]
        [InlineData(0.000001)]
        [InlineData(-0.000001)]
        public void Angle_SetValue_StoresAndRetrievesCorrectly(double angleValue)
        {
            // Arrange
            var rotateTransform = new RotateTransform();

            // Act
            rotateTransform.Angle = angleValue;
            var retrievedValue = rotateTransform.Angle;

            // Assert
            Assert.Equal(angleValue, retrievedValue);
        }

        /// <summary>
        /// Tests that the Angle property setter handles NaN values correctly.
        /// NaN values should be stored and retrieved as NaN.
        /// </summary>
        [Fact]
        public void Angle_SetToNaN_RetrievesNaN()
        {
            // Arrange
            var rotateTransform = new RotateTransform();

            // Act
            rotateTransform.Angle = double.NaN;
            var retrievedValue = rotateTransform.Angle;

            // Assert
            Assert.True(double.IsNaN(retrievedValue));
        }

        /// <summary>
        /// Tests that the Angle property has the correct default value when not explicitly set.
        /// The default value should be 0.0 as defined in the BindableProperty.
        /// </summary>
        [Fact]
        public void Angle_DefaultValue_IsZero()
        {
            // Arrange & Act
            var rotateTransform = new RotateTransform();
            var defaultValue = rotateTransform.Angle;

            // Assert
            Assert.Equal(0.0, defaultValue);
        }

        /// <summary>
        /// Tests that multiple assignments to the Angle property work correctly.
        /// Each assignment should override the previous value.
        /// </summary>
        [Fact]
        public void Angle_MultipleAssignments_LastValueWins()
        {
            // Arrange
            var rotateTransform = new RotateTransform();

            // Act & Assert
            rotateTransform.Angle = 45.0;
            Assert.Equal(45.0, rotateTransform.Angle);

            rotateTransform.Angle = 90.0;
            Assert.Equal(90.0, rotateTransform.Angle);

            rotateTransform.Angle = -30.0;
            Assert.Equal(-30.0, rotateTransform.Angle);

            rotateTransform.Angle = 0.0;
            Assert.Equal(0.0, rotateTransform.Angle);
        }

        /// <summary>
        /// Tests that the CenterX property can be set and retrieved with normal positive values.
        /// Validates that the setter properly calls SetValue and the getter returns the correct value.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(100.5)]
        [InlineData(1000.0)]
        public void CenterX_SetPositiveValue_ReturnsCorrectValue(double expectedValue)
        {
            // Arrange
            var rotateTransform = new RotateTransform();

            // Act
            rotateTransform.CenterX = expectedValue;
            var actualValue = rotateTransform.CenterX;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the CenterX property can be set and retrieved with negative values.
        /// Validates that negative coordinates are properly handled by the bindable property system.
        /// </summary>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-100.5)]
        [InlineData(-1000.0)]
        public void CenterX_SetNegativeValue_ReturnsCorrectValue(double expectedValue)
        {
            // Arrange
            var rotateTransform = new RotateTransform();

            // Act
            rotateTransform.CenterX = expectedValue;
            var actualValue = rotateTransform.CenterX;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the CenterX property can handle extreme double boundary values.
        /// Validates that the bindable property system properly stores and retrieves boundary values.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        public void CenterX_SetBoundaryValues_ReturnsCorrectValue(double expectedValue)
        {
            // Arrange
            var rotateTransform = new RotateTransform();

            // Act
            rotateTransform.CenterX = expectedValue;
            var actualValue = rotateTransform.CenterX;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        /// <summary>
        /// Tests that the CenterX property can handle special double values including NaN and infinities.
        /// Validates that special floating-point values are properly handled by the setter and getter.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void CenterX_SetSpecialValues_ReturnsCorrectValue(double expectedValue)
        {
            // Arrange
            var rotateTransform = new RotateTransform();

            // Act
            rotateTransform.CenterX = expectedValue;
            var actualValue = rotateTransform.CenterX;

            // Assert
            if (double.IsNaN(expectedValue))
            {
                Assert.True(double.IsNaN(actualValue));
            }
            else
            {
                Assert.Equal(expectedValue, actualValue);
            }
        }

        /// <summary>
        /// Tests that the CenterX property has the correct default value when not explicitly set.
        /// Validates that the bindable property returns the expected default value of 0.0.
        /// </summary>
        [Fact]
        public void CenterX_DefaultValue_ReturnsZero()
        {
            // Arrange & Act
            var rotateTransform = new RotateTransform();
            var actualValue = rotateTransform.CenterX;

            // Assert
            Assert.Equal(0.0, actualValue);
        }

        /// <summary>
        /// Tests that the CenterX property can be set multiple times with different values.
        /// Validates that subsequent sets properly overwrite previous values in the bindable property system.
        /// </summary>
        [Fact]
        public void CenterX_SetMultipleTimes_ReturnsLastSetValue()
        {
            // Arrange
            var rotateTransform = new RotateTransform();
            var firstValue = 10.0;
            var secondValue = 20.0;
            var thirdValue = -5.0;

            // Act
            rotateTransform.CenterX = firstValue;
            rotateTransform.CenterX = secondValue;
            rotateTransform.CenterX = thirdValue;
            var actualValue = rotateTransform.CenterX;

            // Assert
            Assert.Equal(thirdValue, actualValue);
        }

        /// <summary>
        /// Tests that setting CenterX to zero explicitly works correctly.
        /// Validates that zero is properly distinguished from the default value in the setter.
        /// </summary>
        [Fact]
        public void CenterX_SetExplicitZero_ReturnsZero()
        {
            // Arrange
            var rotateTransform = new RotateTransform();

            // Act
            rotateTransform.CenterX = 0.0;
            var actualValue = rotateTransform.CenterX;

            // Assert
            Assert.Equal(0.0, actualValue);
        }

        /// <summary>
        /// Tests that the CenterY property correctly sets and gets various double values.
        /// Validates the setter and getter behavior with normal values, boundary values, and special double values.
        /// </summary>
        /// <param name="value">The double value to set and verify</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(100.5)]
        [InlineData(-100.5)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(1.7976931348623157E+308)] // Near double.MaxValue
        [InlineData(-1.7976931348623157E+308)] // Near double.MinValue
        [InlineData(4.9E-324)] // Near double.Epsilon
        [InlineData(-4.9E-324)] // Near negative double.Epsilon
        public void CenterY_SetAndGet_ReturnsCorrectValue(double value)
        {
            // Arrange
            var rotateTransform = new RotateTransform();

            // Act
            rotateTransform.CenterY = value;
            var result = rotateTransform.CenterY;

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
        /// Tests that the CenterY property has the correct default value when not explicitly set.
        /// Verifies that the default value is 0.0 as defined in the BindableProperty creation.
        /// </summary>
        [Fact]
        public void CenterY_DefaultValue_IsZero()
        {
            // Arrange & Act
            var rotateTransform = new RotateTransform();
            var result = rotateTransform.CenterY;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that setting CenterY multiple times correctly updates the value.
        /// Verifies that the property can be changed from one value to another.
        /// </summary>
        [Fact]
        public void CenterY_SetMultipleTimes_UpdatesCorrectly()
        {
            // Arrange
            var rotateTransform = new RotateTransform();
            const double firstValue = 10.5;
            const double secondValue = -20.75;

            // Act & Assert - First value
            rotateTransform.CenterY = firstValue;
            Assert.Equal(firstValue, rotateTransform.CenterY);

            // Act & Assert - Second value
            rotateTransform.CenterY = secondValue;
            Assert.Equal(secondValue, rotateTransform.CenterY);
        }

        /// <summary>
        /// Tests CenterY property with very small decimal values to ensure precision is maintained.
        /// Verifies that tiny positive and negative values are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(0.000001)]
        [InlineData(-0.000001)]
        [InlineData(1E-10)]
        [InlineData(-1E-10)]
        public void CenterY_SmallDecimalValues_MaintainsPrecision(double value)
        {
            // Arrange
            var rotateTransform = new RotateTransform();

            // Act
            rotateTransform.CenterY = value;
            var result = rotateTransform.CenterY;

            // Assert
            Assert.Equal(value, result, precision: 15);
        }

        /// <summary>
        /// Tests that the default constructor creates a RotateTransform instance with default property values.
        /// Verifies that Angle, CenterX, and CenterY properties are initialized to their default values (0.0).
        /// </summary>
        [Fact]
        public void Constructor_Default_InitializesWithDefaultValues()
        {
            // Arrange & Act
            var rotateTransform = new RotateTransform();

            // Assert
            Assert.NotNull(rotateTransform);
            Assert.Equal(0.0, rotateTransform.Angle);
            Assert.Equal(0.0, rotateTransform.CenterX);
            Assert.Equal(0.0, rotateTransform.CenterY);
        }
    }
}