#nullable disable

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class SkewTransformTests
    {
        /// <summary>
        /// Tests the SkewTransform constructor with two double parameters to ensure both AngleX and AngleY properties are set correctly.
        /// Validates normal angle values including zero, positive, and negative values.
        /// </summary>
        /// <param name="angleX">The X-axis skew angle to test</param>
        /// <param name="angleY">The Y-axis skew angle to test</param>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(45.0, 30.0)]
        [InlineData(-45.0, -30.0)]
        [InlineData(90.0, 180.0)]
        [InlineData(360.0, -360.0)]
        [InlineData(123.456, -789.123)]
        public void Constructor_WithAngleXAndAngleY_SetsPropertiesCorrectly(double angleX, double angleY)
        {
            // Arrange & Act
            var skewTransform = new SkewTransform(angleX, angleY);

            // Assert
            Assert.Equal(angleX, skewTransform.AngleX);
            Assert.Equal(angleY, skewTransform.AngleY);
        }

        /// <summary>
        /// Tests the SkewTransform constructor with extreme double values to ensure proper handling of boundary cases.
        /// Validates that extreme values like MinValue and MaxValue are properly assigned to properties.
        /// </summary>
        /// <param name="angleX">The extreme X-axis skew angle to test</param>
        /// <param name="angleY">The extreme Y-axis skew angle to test</param>
        [Theory]
        [InlineData(double.MinValue, double.MaxValue)]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.Epsilon, -double.Epsilon)]
        [InlineData(1e-10, 1e10)]
        [InlineData(-1e10, 1e-10)]
        public void Constructor_WithExtremeValues_SetsPropertiesCorrectly(double angleX, double angleY)
        {
            // Arrange & Act
            var skewTransform = new SkewTransform(angleX, angleY);

            // Assert
            Assert.Equal(angleX, skewTransform.AngleX);
            Assert.Equal(angleY, skewTransform.AngleY);
        }

        /// <summary>
        /// Tests the SkewTransform constructor with special double values (NaN, Infinity) to ensure proper handling.
        /// Validates that special IEEE 754 double values are correctly assigned to properties.
        /// </summary>
        /// <param name="angleX">The special X-axis skew angle to test</param>
        /// <param name="angleY">The special Y-axis skew angle to test</param>
        [Theory]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.PositiveInfinity)]
        [InlineData(double.NaN, 45.0)]
        [InlineData(90.0, double.NaN)]
        [InlineData(double.PositiveInfinity, 0.0)]
        [InlineData(0.0, double.NegativeInfinity)]
        public void Constructor_WithSpecialValues_SetsPropertiesCorrectly(double angleX, double angleY)
        {
            // Arrange & Act
            var skewTransform = new SkewTransform(angleX, angleY);

            // Assert
            if (double.IsNaN(angleX))
                Assert.True(double.IsNaN(skewTransform.AngleX));
            else
                Assert.Equal(angleX, skewTransform.AngleX);

            if (double.IsNaN(angleY))
                Assert.True(double.IsNaN(skewTransform.AngleY));
            else
                Assert.Equal(angleY, skewTransform.AngleY);
        }

        /// <summary>
        /// Tests that the SkewTransform constructor creates a valid instance that inherits from Transform.
        /// Validates the inheritance hierarchy and object creation.
        /// </summary>
        [Fact]
        public void Constructor_WithAngleXAndAngleY_CreatesValidInstance()
        {
            // Arrange & Act
            var skewTransform = new SkewTransform(45.0, 30.0);

            // Assert
            Assert.NotNull(skewTransform);
            Assert.IsType<SkewTransform>(skewTransform);
            Assert.IsAssignableFrom<Transform>(skewTransform);
        }

        /// <summary>
        /// Tests the 4-parameter constructor with normal positive values.
        /// Verifies that all properties are set correctly to the provided values.
        /// </summary>
        [Fact]
        public void Constructor_WithPositiveValues_SetsPropertiesCorrectly()
        {
            // Arrange
            double angleX = 30.5;
            double angleY = 45.2;
            double centerX = 100.0;
            double centerY = 200.0;

            // Act
            var skewTransform = new SkewTransform(angleX, angleY, centerX, centerY);

            // Assert
            Assert.Equal(angleX, skewTransform.AngleX);
            Assert.Equal(angleY, skewTransform.AngleY);
            Assert.Equal(centerX, skewTransform.CenterX);
            Assert.Equal(centerY, skewTransform.CenterY);
        }

        /// <summary>
        /// Tests the 4-parameter constructor with negative values.
        /// Verifies that negative values are accepted and stored correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithNegativeValues_SetsPropertiesCorrectly()
        {
            // Arrange
            double angleX = -15.7;
            double angleY = -89.3;
            double centerX = -50.0;
            double centerY = -75.5;

            // Act
            var skewTransform = new SkewTransform(angleX, angleY, centerX, centerY);

            // Assert
            Assert.Equal(angleX, skewTransform.AngleX);
            Assert.Equal(angleY, skewTransform.AngleY);
            Assert.Equal(centerX, skewTransform.CenterX);
            Assert.Equal(centerY, skewTransform.CenterY);
        }

        /// <summary>
        /// Tests the 4-parameter constructor with zero values.
        /// Verifies that zero values are handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithZeroValues_SetsPropertiesCorrectly()
        {
            // Arrange
            double angleX = 0.0;
            double angleY = 0.0;
            double centerX = 0.0;
            double centerY = 0.0;

            // Act
            var skewTransform = new SkewTransform(angleX, angleY, centerX, centerY);

            // Assert
            Assert.Equal(0.0, skewTransform.AngleX);
            Assert.Equal(0.0, skewTransform.AngleY);
            Assert.Equal(0.0, skewTransform.CenterX);
            Assert.Equal(0.0, skewTransform.CenterY);
        }

        /// <summary>
        /// Tests the 4-parameter constructor with extreme boundary values.
        /// Verifies that minimum and maximum double values are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MaxValue, double.MinValue, double.MaxValue)]
        [InlineData(double.MaxValue, double.MinValue, double.MaxValue, double.MinValue)]
        public void Constructor_WithExtremeValues_SetsPropertiesCorrectly(double angleX, double angleY, double centerX, double centerY)
        {
            // Act
            var skewTransform = new SkewTransform(angleX, angleY, centerX, centerY);

            // Assert
            Assert.Equal(angleX, skewTransform.AngleX);
            Assert.Equal(angleY, skewTransform.AngleY);
            Assert.Equal(centerX, skewTransform.CenterX);
            Assert.Equal(centerY, skewTransform.CenterY);
        }

        /// <summary>
        /// Tests the 4-parameter constructor with special floating point values.
        /// Verifies that NaN, positive infinity, and negative infinity are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0.0, 0.0, 0.0)]
        [InlineData(0.0, double.NaN, 0.0, 0.0)]
        [InlineData(0.0, 0.0, double.NaN, 0.0)]
        [InlineData(0.0, 0.0, 0.0, double.NaN)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity, double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.PositiveInfinity, double.NegativeInfinity, double.PositiveInfinity)]
        public void Constructor_WithSpecialFloatingPointValues_SetsPropertiesCorrectly(double angleX, double angleY, double centerX, double centerY)
        {
            // Act
            var skewTransform = new SkewTransform(angleX, angleY, centerX, centerY);

            // Assert
            Assert.Equal(angleX, skewTransform.AngleX);
            Assert.Equal(angleY, skewTransform.AngleY);
            Assert.Equal(centerX, skewTransform.CenterX);
            Assert.Equal(centerY, skewTransform.CenterY);
        }

        /// <summary>
        /// Tests the 4-parameter constructor with very small values close to zero.
        /// Verifies that precision is maintained for small floating point values.
        /// </summary>
        [Fact]
        public void Constructor_WithVerySmallValues_SetsPropertiesCorrectly()
        {
            // Arrange
            double angleX = double.Epsilon;
            double angleY = -double.Epsilon;
            double centerX = 1e-15;
            double centerY = -1e-15;

            // Act
            var skewTransform = new SkewTransform(angleX, angleY, centerX, centerY);

            // Assert
            Assert.Equal(angleX, skewTransform.AngleX);
            Assert.Equal(angleY, skewTransform.AngleY);
            Assert.Equal(centerX, skewTransform.CenterX);
            Assert.Equal(centerY, skewTransform.CenterY);
        }

        /// <summary>
        /// Tests the 4-parameter constructor with various mixed value combinations.
        /// Verifies that different combinations of positive, negative, and special values work correctly.
        /// </summary>
        [Theory]
        [InlineData(90.0, -45.0, 250.5, -100.75)]
        [InlineData(-180.0, 360.0, -0.0, 0.0)]
        [InlineData(123.456789, -987.654321, 0.000001, -999999.999999)]
        public void Constructor_WithMixedValues_SetsPropertiesCorrectly(double angleX, double angleY, double centerX, double centerY)
        {
            // Act
            var skewTransform = new SkewTransform(angleX, angleY, centerX, centerY);

            // Assert
            Assert.Equal(angleX, skewTransform.AngleX);
            Assert.Equal(angleY, skewTransform.AngleY);
            Assert.Equal(centerX, skewTransform.CenterX);
            Assert.Equal(centerY, skewTransform.CenterY);
        }

        /// <summary>
        /// Tests that the AngleX property can be set and retrieved with normal double values using the default constructor.
        /// </summary>
        /// <param name="angleValue">The angle value to test.</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(45.0)]
        [InlineData(-30.0)]
        [InlineData(90.0)]
        [InlineData(180.0)]
        [InlineData(-180.0)]
        [InlineData(360.0)]
        [InlineData(1.5)]
        [InlineData(-0.5)]
        public void AngleX_SetAndGetNormalValues_ReturnsCorrectValue(double angleValue)
        {
            // Arrange
            var skewTransform = new SkewTransform();

            // Act
            skewTransform.AngleX = angleValue;
            var result = skewTransform.AngleX;

            // Assert
            Assert.Equal(angleValue, result);
        }

        /// <summary>
        /// Tests that the AngleX property can be set and retrieved with edge case double values.
        /// </summary>
        /// <param name="angleValue">The edge case angle value to test.</param>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void AngleX_SetAndGetEdgeCaseValues_ReturnsCorrectValue(double angleValue)
        {
            // Arrange
            var skewTransform = new SkewTransform();

            // Act
            skewTransform.AngleX = angleValue;
            var result = skewTransform.AngleX;

            // Assert
            if (double.IsNaN(angleValue))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(angleValue, result);
            }
        }

        /// <summary>
        /// Tests that the AngleX property reflects the value set through the two-parameter constructor.
        /// </summary>
        /// <param name="angleXValue">The AngleX value to test.</param>
        /// <param name="angleYValue">The AngleY value for the constructor.</param>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(45.0, 30.0)]
        [InlineData(-15.0, 60.0)]
        [InlineData(90.0, -45.0)]
        public void AngleX_InitializedThroughTwoParameterConstructor_ReturnsCorrectValue(double angleXValue, double angleYValue)
        {
            // Arrange & Act
            var skewTransform = new SkewTransform(angleXValue, angleYValue);
            var result = skewTransform.AngleX;

            // Assert
            Assert.Equal(angleXValue, result);
        }

        /// <summary>
        /// Tests that the AngleX property reflects the value set through the four-parameter constructor.
        /// </summary>
        /// <param name="angleXValue">The AngleX value to test.</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(45.0)]
        [InlineData(-30.0)]
        [InlineData(180.0)]
        public void AngleX_InitializedThroughFourParameterConstructor_ReturnsCorrectValue(double angleXValue)
        {
            // Arrange & Act
            var skewTransform = new SkewTransform(angleXValue, 20.0, 10.0, 15.0);
            var result = skewTransform.AngleX;

            // Assert
            Assert.Equal(angleXValue, result);
        }

        /// <summary>
        /// Tests that the AngleX property can be set multiple times consecutively and always returns the last set value.
        /// </summary>
        [Fact]
        public void AngleX_SetMultipleTimes_ReturnsLastSetValue()
        {
            // Arrange
            var skewTransform = new SkewTransform();
            var firstValue = 10.0;
            var secondValue = -25.0;
            var thirdValue = 90.0;

            // Act
            skewTransform.AngleX = firstValue;
            skewTransform.AngleX = secondValue;
            skewTransform.AngleX = thirdValue;
            var result = skewTransform.AngleX;

            // Assert
            Assert.Equal(thirdValue, result);
        }

        /// <summary>
        /// Tests that the AngleX property can be modified after being initialized through a constructor.
        /// </summary>
        [Fact]
        public void AngleX_ModifiedAfterConstructorInitialization_ReturnsNewValue()
        {
            // Arrange
            var initialValue = 45.0;
            var newValue = -60.0;
            var skewTransform = new SkewTransform(initialValue, 30.0);

            // Act
            skewTransform.AngleX = newValue;
            var result = skewTransform.AngleX;

            // Assert
            Assert.Equal(newValue, result);
        }

        /// <summary>
        /// Tests that the AngleX property has the default value of 0.0 when using the default constructor.
        /// </summary>
        [Fact]
        public void AngleX_DefaultConstructor_HasDefaultValueOfZero()
        {
            // Arrange & Act
            var skewTransform = new SkewTransform();
            var result = skewTransform.AngleX;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that AngleY property can be set and retrieved with normal angle values.
        /// Verifies that the setter calls SetValue and the getter returns the correct value.
        /// </summary>
        /// <param name="angleValue">The angle value to test</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(45.0)]
        [InlineData(-45.0)]
        [InlineData(90.0)]
        [InlineData(-90.0)]
        [InlineData(180.0)]
        [InlineData(-180.0)]
        [InlineData(360.0)]
        [InlineData(-360.0)]
        [InlineData(123.456)]
        [InlineData(-123.456)]
        public void AngleY_SetNormalValues_ReturnsSetValue(double angleValue)
        {
            // Arrange
            var skewTransform = new SkewTransform();

            // Act
            skewTransform.AngleY = angleValue;

            // Assert
            Assert.Equal(angleValue, skewTransform.AngleY);
        }

        /// <summary>
        /// Tests that AngleY property can be set and retrieved with extreme double values.
        /// Verifies boundary value handling including minimum and maximum double values.
        /// </summary>
        /// <param name="angleValue">The extreme angle value to test</param>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        public void AngleY_SetExtremeValues_ReturnsSetValue(double angleValue)
        {
            // Arrange
            var skewTransform = new SkewTransform();

            // Act
            skewTransform.AngleY = angleValue;

            // Assert
            Assert.Equal(angleValue, skewTransform.AngleY);
        }

        /// <summary>
        /// Tests that AngleY property can be set to special double values like NaN and infinity.
        /// Verifies that the property handles special floating-point values correctly.
        /// </summary>
        /// <param name="angleValue">The special double value to test</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void AngleY_SetSpecialDoubleValues_ReturnsSetValue(double angleValue)
        {
            // Arrange
            var skewTransform = new SkewTransform();

            // Act
            skewTransform.AngleY = angleValue;

            // Assert
            if (double.IsNaN(angleValue))
            {
                Assert.True(double.IsNaN(skewTransform.AngleY));
            }
            else
            {
                Assert.Equal(angleValue, skewTransform.AngleY);
            }
        }

        /// <summary>
        /// Tests that AngleY property has the correct default value when the SkewTransform is created.
        /// Verifies the initial state of the bindable property.
        /// </summary>
        [Fact]
        public void AngleY_DefaultValue_ReturnsZero()
        {
            // Arrange & Act
            var skewTransform = new SkewTransform();

            // Assert
            Assert.Equal(0.0, skewTransform.AngleY);
        }

        /// <summary>
        /// Tests that AngleY property can be set multiple times with different values.
        /// Verifies that subsequent sets overwrite previous values correctly.
        /// </summary>
        [Fact]
        public void AngleY_SetMultipleTimes_ReturnsLastSetValue()
        {
            // Arrange
            var skewTransform = new SkewTransform();

            // Act & Assert
            skewTransform.AngleY = 45.0;
            Assert.Equal(45.0, skewTransform.AngleY);

            skewTransform.AngleY = -90.0;
            Assert.Equal(-90.0, skewTransform.AngleY);

            skewTransform.AngleY = 0.0;
            Assert.Equal(0.0, skewTransform.AngleY);
        }

        /// <summary>
        /// Tests that CenterX property can be set and retrieved with normal double values.
        /// Validates that the setter stores the value correctly using the underlying BindableProperty mechanism.
        /// </summary>
        /// <param name="value">The double value to set and verify</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(100.0)]
        [InlineData(-100.0)]
        [InlineData(0.5)]
        [InlineData(-0.5)]
        [InlineData(1000000.0)]
        [InlineData(-1000000.0)]
        public void CenterX_SetAndGet_ReturnsCorrectValue(double value)
        {
            // Arrange
            var skewTransform = new SkewTransform();

            // Act
            skewTransform.CenterX = value;

            // Assert
            Assert.Equal(value, skewTransform.CenterX);
        }

        /// <summary>
        /// Tests that CenterX property correctly handles extreme double values including boundaries.
        /// Ensures the property can store and retrieve double.MinValue and double.MaxValue without issues.
        /// </summary>
        /// <param name="value">The extreme double value to test</param>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void CenterX_SetExtremeValues_ReturnsCorrectValue(double value)
        {
            // Arrange
            var skewTransform = new SkewTransform();

            // Act
            skewTransform.CenterX = value;

            // Assert
            Assert.Equal(value, skewTransform.CenterX);
        }

        /// <summary>
        /// Tests that CenterX property correctly handles special double values like NaN and infinity.
        /// Validates that these special IEEE 754 values are properly stored and retrieved.
        /// </summary>
        /// <param name="value">The special double value to test</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void CenterX_SetSpecialValues_ReturnsCorrectValue(double value)
        {
            // Arrange
            var skewTransform = new SkewTransform();

            // Act
            skewTransform.CenterX = value;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(skewTransform.CenterX));
            }
            else
            {
                Assert.Equal(value, skewTransform.CenterX);
            }
        }

        /// <summary>
        /// Tests that CenterX property has the correct default value as defined by the bindable property.
        /// Verifies that a new SkewTransform instance initializes CenterX to 0.0.
        /// </summary>
        [Fact]
        public void CenterX_DefaultValue_IsZero()
        {
            // Arrange & Act
            var skewTransform = new SkewTransform();

            // Assert
            Assert.Equal(0.0, skewTransform.CenterX);
        }

        /// <summary>
        /// Tests that CenterX property can be set multiple times with different values.
        /// Ensures that subsequent property sets overwrite previous values correctly.
        /// </summary>
        [Fact]
        public void CenterX_SetMultipleTimes_UpdatesValueCorrectly()
        {
            // Arrange
            var skewTransform = new SkewTransform();

            // Act & Assert
            skewTransform.CenterX = 10.0;
            Assert.Equal(10.0, skewTransform.CenterX);

            skewTransform.CenterX = -5.0;
            Assert.Equal(-5.0, skewTransform.CenterX);

            skewTransform.CenterX = 0.0;
            Assert.Equal(0.0, skewTransform.CenterX);
        }

        /// <summary>
        /// Tests that the CenterY property setter correctly stores normal double values.
        /// Verifies that setting a standard positive value can be retrieved through the getter.
        /// </summary>
        [Fact]
        public void CenterY_SetPositiveValue_StoresAndReturnsValue()
        {
            // Arrange
            var transform = new SkewTransform();
            var expectedValue = 15.5;

            // Act
            transform.CenterY = expectedValue;

            // Assert
            Assert.Equal(expectedValue, transform.CenterY);
        }

        /// <summary>
        /// Tests that the CenterY property setter correctly stores negative double values.
        /// Verifies that setting a negative value can be retrieved through the getter.
        /// </summary>
        [Fact]
        public void CenterY_SetNegativeValue_StoresAndReturnsValue()
        {
            // Arrange
            var transform = new SkewTransform();
            var expectedValue = -25.75;

            // Act
            transform.CenterY = expectedValue;

            // Assert
            Assert.Equal(expectedValue, transform.CenterY);
        }

        /// <summary>
        /// Tests that the CenterY property setter correctly handles zero value.
        /// Verifies that setting zero can be retrieved through the getter.
        /// </summary>
        [Fact]
        public void CenterY_SetZeroValue_StoresAndReturnsValue()
        {
            // Arrange
            var transform = new SkewTransform();
            var expectedValue = 0.0;

            // Act
            transform.CenterY = expectedValue;

            // Assert
            Assert.Equal(expectedValue, transform.CenterY);
        }

        /// <summary>
        /// Tests that the CenterY property setter correctly handles boundary double values.
        /// Verifies that setting minimum and maximum double values can be stored and retrieved.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        public void CenterY_SetBoundaryValues_StoresAndReturnsValue(double value)
        {
            // Arrange
            var transform = new SkewTransform();

            // Act
            transform.CenterY = value;

            // Assert
            Assert.Equal(value, transform.CenterY);
        }

        /// <summary>
        /// Tests that the CenterY property setter correctly handles special double values.
        /// Verifies that setting NaN, positive infinity, and negative infinity can be stored and retrieved.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void CenterY_SetSpecialValues_StoresAndReturnsValue(double value)
        {
            // Arrange
            var transform = new SkewTransform();

            // Act
            transform.CenterY = value;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(transform.CenterY));
            }
            else
            {
                Assert.Equal(value, transform.CenterY);
            }
        }

        /// <summary>
        /// Tests that the CenterY property setter correctly handles very small double values.
        /// Verifies that setting epsilon and very small values can be stored and retrieved.
        /// </summary>
        [Theory]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        [InlineData(1e-300)]
        [InlineData(-1e-300)]
        public void CenterY_SetVerySmallValues_StoresAndReturnsValue(double value)
        {
            // Arrange
            var transform = new SkewTransform();

            // Act
            transform.CenterY = value;

            // Assert
            Assert.Equal(value, transform.CenterY);
        }

        /// <summary>
        /// Tests that the CenterY property setter correctly handles very large double values.
        /// Verifies that setting very large finite values can be stored and retrieved.
        /// </summary>
        [Theory]
        [InlineData(1e+300)]
        [InlineData(-1e+300)]
        [InlineData(1.7976931348623156e+308)]
        [InlineData(-1.7976931348623156e+308)]
        public void CenterY_SetVeryLargeValues_StoresAndReturnsValue(double value)
        {
            // Arrange
            var transform = new SkewTransform();

            // Act
            transform.CenterY = value;

            // Assert
            Assert.Equal(value, transform.CenterY);
        }

        /// <summary>
        /// Tests that the CenterY property setter can be called multiple times with different values.
        /// Verifies that subsequent assignments correctly update the stored value.
        /// </summary>
        [Fact]
        public void CenterY_SetMultipleValues_UpdatesCorrectly()
        {
            // Arrange
            var transform = new SkewTransform();

            // Act & Assert
            transform.CenterY = 10.0;
            Assert.Equal(10.0, transform.CenterY);

            transform.CenterY = -5.5;
            Assert.Equal(-5.5, transform.CenterY);

            transform.CenterY = 0.0;
            Assert.Equal(0.0, transform.CenterY);

            transform.CenterY = double.PositiveInfinity;
            Assert.Equal(double.PositiveInfinity, transform.CenterY);
        }

        /// <summary>
        /// Tests that the default constructor creates a SkewTransform instance with all properties set to their default values.
        /// Verifies that AngleX, AngleY, CenterX, and CenterY are all initialized to 0.0.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesInstanceWithDefaultValues()
        {
            // Arrange & Act
            var skewTransform = new SkewTransform();

            // Assert
            Assert.NotNull(skewTransform);
            Assert.IsType<SkewTransform>(skewTransform);
            Assert.IsAssignableFrom<Transform>(skewTransform);
            Assert.Equal(0.0, skewTransform.AngleX);
            Assert.Equal(0.0, skewTransform.AngleY);
            Assert.Equal(0.0, skewTransform.CenterX);
            Assert.Equal(0.0, skewTransform.CenterY);
        }
    }
}