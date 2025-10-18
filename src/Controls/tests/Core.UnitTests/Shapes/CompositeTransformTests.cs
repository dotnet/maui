#nullable disable

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public sealed class CompositeTransformTests
    {
        /// <summary>
        /// Tests that the CenterY property can be set and retrieved correctly for various double values.
        /// Verifies that the setter calls SetValue and the getter returns the correct value.
        /// </summary>
        /// <param name="value">The double value to set and verify.</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(100.5)]
        [InlineData(-100.5)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        public void CenterY_SetValue_ReturnsCorrectValue(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.CenterY = value;

            // Assert
            Assert.Equal(value, transform.CenterY);
        }

        /// <summary>
        /// Tests that the CenterY property correctly handles special double values like NaN and infinities.
        /// Verifies that the property can store and retrieve these special floating-point values.
        /// </summary>
        /// <param name="value">The special double value to test.</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void CenterY_SetSpecialDoubleValues_ReturnsCorrectValue(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

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
        /// Tests that the CenterY property has the correct default value.
        /// Verifies that a newly created CompositeTransform has CenterY initialized to 0.0.
        /// </summary>
        [Fact]
        public void CenterY_DefaultValue_IsZero()
        {
            // Arrange & Act
            var transform = new CompositeTransform();

            // Assert
            Assert.Equal(0.0, transform.CenterY);
        }

        /// <summary>
        /// Tests that the CenterY property can be set multiple times with different values.
        /// Verifies that subsequent sets correctly overwrite the previous value.
        /// </summary>
        [Fact]
        public void CenterY_SetMultipleTimes_LastValueIsPreserved()
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.CenterY = 10.0;
            transform.CenterY = 20.0;
            transform.CenterY = 30.0;

            // Assert
            Assert.Equal(30.0, transform.CenterY);
        }

        /// <summary>
        /// Tests that the ScaleX property returns the default value when not explicitly set.
        /// </summary>
        [Fact]
        public void ScaleX_DefaultValue_Returns1Point0()
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            var result = transform.ScaleX;

            // Assert
            Assert.Equal(1.0, result);
        }

        /// <summary>
        /// Tests that the ScaleX property correctly stores and retrieves normal positive values.
        /// </summary>
        /// <param name="value">The ScaleX value to test.</param>
        /// <param name="expected">The expected returned value.</param>
        [Theory]
        [InlineData(2.0, 2.0)]
        [InlineData(0.5, 0.5)]
        [InlineData(10.0, 10.0)]
        [InlineData(0.1, 0.1)]
        [InlineData(100.0, 100.0)]
        public void ScaleX_SetValidPositiveValue_ReturnsCorrectValue(double value, double expected)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.ScaleX = value;
            var result = transform.ScaleX;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that the ScaleX property correctly handles zero value.
        /// </summary>
        [Fact]
        public void ScaleX_SetZeroValue_ReturnsZero()
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.ScaleX = 0.0;
            var result = transform.ScaleX;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that the ScaleX property correctly stores and retrieves negative values.
        /// </summary>
        /// <param name="value">The negative ScaleX value to test.</param>
        /// <param name="expected">The expected returned value.</param>
        [Theory]
        [InlineData(-1.0, -1.0)]
        [InlineData(-2.5, -2.5)]
        [InlineData(-0.5, -0.5)]
        [InlineData(-10.0, -10.0)]
        public void ScaleX_SetNegativeValue_ReturnsCorrectValue(double value, double expected)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.ScaleX = value;
            var result = transform.ScaleX;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that the ScaleX property correctly handles extreme boundary values.
        /// </summary>
        /// <param name="value">The extreme boundary value to test.</param>
        /// <param name="expected">The expected returned value.</param>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.Epsilon, double.Epsilon)]
        public void ScaleX_SetExtremeValues_ReturnsCorrectValue(double value, double expected)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.ScaleX = value;
            var result = transform.ScaleX;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that the ScaleX property correctly handles special double values including NaN and infinity.
        /// </summary>
        /// <param name="value">The special double value to test.</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ScaleX_SetSpecialDoubleValues_ReturnsCorrectValue(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.ScaleX = value;
            var result = transform.ScaleX;

            // Assert
            if (double.IsNaN(value))
                Assert.True(double.IsNaN(result));
            else if (double.IsPositiveInfinity(value))
                Assert.True(double.IsPositiveInfinity(result));
            else if (double.IsNegativeInfinity(value))
                Assert.True(double.IsNegativeInfinity(result));
        }

        /// <summary>
        /// Tests that multiple assignments to ScaleX property work correctly and the last value is retained.
        /// </summary>
        [Fact]
        public void ScaleX_MultipleAssignments_ReturnsLastAssignedValue()
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.ScaleX = 1.5;
            transform.ScaleX = 2.0;
            transform.ScaleX = 3.5;
            var result = transform.ScaleX;

            // Assert
            Assert.Equal(3.5, result);
        }

        /// <summary>
        /// Tests that the ScaleX property correctly handles very small decimal values.
        /// </summary>
        [Fact]
        public void ScaleX_SetVerySmallDecimalValue_ReturnsCorrectValue()
        {
            // Arrange
            var transform = new CompositeTransform();
            var verySmallValue = 1e-15;

            // Act
            transform.ScaleX = verySmallValue;
            var result = transform.ScaleX;

            // Assert
            Assert.Equal(verySmallValue, result);
        }

        /// <summary>
        /// Tests that the ScaleX property correctly handles very large decimal values.
        /// </summary>
        [Fact]
        public void ScaleX_SetVeryLargeDecimalValue_ReturnsCorrectValue()
        {
            // Arrange
            var transform = new CompositeTransform();
            var veryLargeValue = 1e15;

            // Act
            transform.ScaleX = veryLargeValue;
            var result = transform.ScaleX;

            // Assert
            Assert.Equal(veryLargeValue, result);
        }

        /// <summary>
        /// Tests that ScaleY property returns the default value when not explicitly set.
        /// Input: New CompositeTransform instance.
        /// Expected: ScaleY should return 1.0 (the default value).
        /// </summary>
        [Fact]
        public void ScaleY_DefaultValue_ReturnsOne()
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            var result = transform.ScaleY;

            // Assert
            Assert.Equal(1.0, result);
        }

        /// <summary>
        /// Tests that ScaleY property can be set and retrieved with various valid double values.
        /// Input: Various double values including positive, negative, zero, and decimal values.
        /// Expected: ScaleY should return the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(2.0)]
        [InlineData(-1.0)]
        [InlineData(0.5)]
        [InlineData(10.0)]
        [InlineData(-5.5)]
        [InlineData(0.001)]
        [InlineData(1000.0)]
        public void ScaleY_SetValidValues_ReturnsSetValue(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.ScaleY = value;
            var result = transform.ScaleY;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that ScaleY property can handle extreme double values.
        /// Input: Double boundary values including MinValue and MaxValue.
        /// Expected: ScaleY should return the exact extreme value that was set.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void ScaleY_SetExtremeValues_ReturnsSetValue(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.ScaleY = value;
            var result = transform.ScaleY;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that ScaleY property can handle special double values like NaN and infinities.
        /// Input: Special double values (NaN, PositiveInfinity, NegativeInfinity).
        /// Expected: ScaleY should return the special value that was set.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ScaleY_SetSpecialValues_ReturnsSetValue(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

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
        /// Tests that ScaleY property can be set multiple times with different values.
        /// Input: Sequential setting of different double values.
        /// Expected: ScaleY should always return the most recently set value.
        /// </summary>
        [Fact]
        public void ScaleY_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act & Assert
            transform.ScaleY = 2.0;
            Assert.Equal(2.0, transform.ScaleY);

            transform.ScaleY = -1.5;
            Assert.Equal(-1.5, transform.ScaleY);

            transform.ScaleY = 0.0;
            Assert.Equal(0.0, transform.ScaleY);

            transform.ScaleY = 100.0;
            Assert.Equal(100.0, transform.ScaleY);
        }

        /// <summary>
        /// Tests that the SkewX property has the correct default value of 0.0.
        /// </summary>
        [Fact]
        public void SkewX_DefaultValue_ReturnsZero()
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            var result = transform.SkewX;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that the SkewX property can be set and retrieved with various valid double values.
        /// </summary>
        /// <param name="value">The double value to set and test.</param>
        /// <param name="expectedValue">The expected value to be returned.</param>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(1.0, 1.0)]
        [InlineData(-1.0, -1.0)]
        [InlineData(45.0, 45.0)]
        [InlineData(-45.0, -45.0)]
        [InlineData(90.0, 90.0)]
        [InlineData(180.0, 180.0)]
        [InlineData(360.0, 360.0)]
        [InlineData(0.123456789, 0.123456789)]
        [InlineData(-0.123456789, -0.123456789)]
        public void SkewX_SetValidValues_ReturnsSetValue(double value, double expectedValue)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.SkewX = value;
            var result = transform.SkewX;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the SkewX property handles boundary double values correctly.
        /// </summary>
        /// <param name="value">The boundary double value to test.</param>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void SkewX_SetBoundaryValues_ReturnsSetValue(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.SkewX = value;
            var result = transform.SkewX;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the SkewX property handles special double values (NaN and Infinity) correctly.
        /// </summary>
        /// <param name="value">The special double value to test.</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void SkewX_SetSpecialValues_ReturnsSetValue(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.SkewX = value;
            var result = transform.SkewX;

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
        /// Tests that multiple consecutive sets and gets of the SkewX property work correctly.
        /// </summary>
        [Fact]
        public void SkewX_MultipleSetAndGet_ReturnsCorrectValues()
        {
            // Arrange
            var transform = new CompositeTransform();
            var values = new double[] { 0.0, 45.0, -30.0, 90.0, 0.0 };

            foreach (var value in values)
            {
                // Act
                transform.SkewX = value;
                var result = transform.SkewX;

                // Assert
                Assert.Equal(value, result);
            }
        }

        /// <summary>
        /// Tests that the SkewX property setter works correctly when the transform already has other properties set.
        /// </summary>
        [Fact]
        public void SkewX_SetWithOtherPropertiesSet_ReturnsCorrectValue()
        {
            // Arrange
            var transform = new CompositeTransform
            {
                CenterX = 10.0,
                CenterY = 20.0,
                ScaleX = 2.0,
                ScaleY = 3.0,
                Rotation = 45.0,
                TranslateX = 5.0,
                TranslateY = 15.0
            };
            var skewXValue = 30.0;

            // Act
            transform.SkewX = skewXValue;
            var result = transform.SkewX;

            // Assert
            Assert.Equal(skewXValue, result);
            // Verify other properties are not affected
            Assert.Equal(10.0, transform.CenterX);
            Assert.Equal(20.0, transform.CenterY);
            Assert.Equal(2.0, transform.ScaleX);
            Assert.Equal(3.0, transform.ScaleY);
            Assert.Equal(45.0, transform.Rotation);
            Assert.Equal(5.0, transform.TranslateX);
            Assert.Equal(15.0, transform.TranslateY);
        }

        /// <summary>
        /// Tests that SkewY property can be set and retrieved with normal double values.
        /// Verifies basic get/set functionality works correctly.
        /// </summary>
        /// <param name="value">The double value to test</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(45.0)]
        [InlineData(-45.0)]
        [InlineData(123.456)]
        [InlineData(-123.456)]
        public void SkewY_SetAndGet_ReturnsExpectedValue(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.SkewY = value;
            var result = transform.SkewY;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that SkewY property handles special double values correctly.
        /// Verifies edge cases like NaN, infinity values, and extreme boundary values.
        /// </summary>
        /// <param name="value">The special double value to test</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void SkewY_SetSpecialDoubleValues_ReturnsExpectedValue(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.SkewY = value;
            var result = transform.SkewY;

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
        /// Tests that SkewY property maintains consistency across multiple set operations.
        /// Verifies that setting different values in sequence works correctly.
        /// </summary>
        [Fact]
        public void SkewY_MultipleSetOperations_MaintainsConsistency()
        {
            // Arrange
            var transform = new CompositeTransform();
            var values = new[] { 0.0, 15.5, -30.0, 90.0, -180.0 };

            // Act & Assert
            foreach (var value in values)
            {
                transform.SkewY = value;
                Assert.Equal(value, transform.SkewY);
            }
        }

        /// <summary>
        /// Tests that SkewY property has the correct default value.
        /// Verifies the initial state of a new CompositeTransform instance.
        /// </summary>
        [Fact]
        public void SkewY_DefaultValue_IsZero()
        {
            // Arrange & Act
            var transform = new CompositeTransform();
            var defaultValue = transform.SkewY;

            // Assert
            Assert.Equal(0.0, defaultValue);
        }

        /// <summary>
        /// Tests that the Rotation property setter accepts various double values and the getter returns the same values.
        /// Tests the property with normal rotation angles, boundary values, and special double values.
        /// Verifies that both setter and getter work correctly for all valid double inputs.
        /// </summary>
        /// <param name="rotationValue">The rotation value to test.</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(45.0)]
        [InlineData(90.0)]
        [InlineData(180.0)]
        [InlineData(360.0)]
        [InlineData(-45.0)]
        [InlineData(-90.0)]
        [InlineData(-180.0)]
        [InlineData(-360.0)]
        [InlineData(720.0)]
        [InlineData(-720.0)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(1e-10)]
        [InlineData(-1e-10)]
        public void Rotation_SetAndGet_ReturnsExpectedValue(double rotationValue)
        {
            // Arrange
            var compositeTransform = new CompositeTransform();

            // Act
            compositeTransform.Rotation = rotationValue;
            var result = compositeTransform.Rotation;

            // Assert
            Assert.Equal(rotationValue, result);
        }

        /// <summary>
        /// Tests that the Rotation property setter and getter handle special double values correctly.
        /// Tests with NaN, PositiveInfinity, and NegativeInfinity values.
        /// Verifies that the property can store and retrieve these special floating-point values.
        /// </summary>
        /// <param name="specialValue">The special double value to test.</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void Rotation_SetSpecialDoubleValues_ReturnsExpectedValue(double specialValue)
        {
            // Arrange
            var compositeTransform = new CompositeTransform();

            // Act
            compositeTransform.Rotation = specialValue;
            var result = compositeTransform.Rotation;

            // Assert
            if (double.IsNaN(specialValue))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(specialValue, result);
            }
        }

        /// <summary>
        /// Tests that the Rotation property returns the default value when not explicitly set.
        /// Verifies that the initial rotation value matches the default specified in RotationProperty.
        /// Expected result is 0.0 based on the BindableProperty.Create call with default value 0.0.
        /// </summary>
        [Fact]
        public void Rotation_DefaultValue_ReturnsZero()
        {
            // Arrange
            var compositeTransform = new CompositeTransform();

            // Act
            var result = compositeTransform.Rotation;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that TranslateX property returns the default value of 0.0 when not explicitly set.
        /// </summary>
        [Fact]
        public void TranslateX_DefaultValue_ReturnsZero()
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            var result = transform.TranslateX;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that TranslateX property correctly stores and retrieves various double values.
        /// </summary>
        /// <param name="value">The double value to test setting and getting.</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(123.456)]
        [InlineData(-987.654)]
        [InlineData(1.7976931348623157E+308)] // double.MaxValue
        [InlineData(-1.7976931348623157E+308)] // double.MinValue
        [InlineData(4.94065645841247E-324)] // double.Epsilon
        public void TranslateX_SetAndGet_StoresValueCorrectly(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.TranslateX = value;
            var result = transform.TranslateX;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that TranslateX property correctly handles special double values like NaN and infinity.
        /// </summary>
        /// <param name="specialValue">The special double value to test.</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void TranslateX_SpecialDoubleValues_StoresValueCorrectly(double specialValue)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.TranslateX = specialValue;
            var result = transform.TranslateX;

            // Assert
            if (double.IsNaN(specialValue))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(specialValue, result);
            }
        }

        /// <summary>
        /// Tests that multiple sets to TranslateX property correctly update the stored value.
        /// </summary>
        [Fact]
        public void TranslateX_MultipleSet_UpdatesValueCorrectly()
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act & Assert
            transform.TranslateX = 10.0;
            Assert.Equal(10.0, transform.TranslateX);

            transform.TranslateX = -5.5;
            Assert.Equal(-5.5, transform.TranslateX);

            transform.TranslateX = 0.0;
            Assert.Equal(0.0, transform.TranslateX);
        }

        /// <summary>
        /// Tests that TranslateX property works correctly with boundary values close to zero.
        /// </summary>
        [Theory]
        [InlineData(1E-10)]
        [InlineData(-1E-10)]
        [InlineData(1E-15)]
        [InlineData(-1E-15)]
        public void TranslateX_BoundaryValuesNearZero_StoresValueCorrectly(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.TranslateX = value;
            var result = transform.TranslateX;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that TranslateX property works correctly with very large values.
        /// </summary>
        [Theory]
        [InlineData(1E100)]
        [InlineData(-1E100)]
        [InlineData(1E200)]
        [InlineData(-1E200)]
        public void TranslateX_VeryLargeValues_StoresValueCorrectly(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.TranslateX = value;
            var result = transform.TranslateX;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that setting the TranslateY property with various valid double values works correctly.
        /// This test verifies that the setter properly calls SetValue with the correct bindable property and value.
        /// Expected result: The value should be set without throwing exceptions and be retrievable via the getter.
        /// </summary>
        /// <param name="value">The double value to set for TranslateY property.</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(100.5)]
        [InlineData(-100.5)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(1.7976931348623157E+308)]
        [InlineData(-1.7976931348623157E+308)]
        [InlineData(4.94065645841247E-324)]
        [InlineData(-4.94065645841247E-324)]
        public void TranslateY_SetValidValue_SetsAndRetrievesCorrectly(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.TranslateY = value;
            var result = transform.TranslateY;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that setting the TranslateY property with special double values (NaN, Infinity) works correctly.
        /// This test verifies that the setter handles special floating-point values without exceptions.
        /// Expected result: Special values should be set and retrieved correctly, preserving their special nature.
        /// </summary>
        /// <param name="specialValue">The special double value to test.</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void TranslateY_SetSpecialDoubleValues_HandlesCorrectly(double specialValue)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.TranslateY = specialValue;
            var result = transform.TranslateY;

            // Assert
            if (double.IsNaN(specialValue))
            {
                Assert.True(double.IsNaN(result));
            }
            else if (double.IsPositiveInfinity(specialValue))
            {
                Assert.True(double.IsPositiveInfinity(result));
            }
            else if (double.IsNegativeInfinity(specialValue))
            {
                Assert.True(double.IsNegativeInfinity(result));
            }
        }

        /// <summary>
        /// Tests that the TranslateY property returns the default value when not explicitly set.
        /// This test verifies that the getter properly retrieves the default value from the bindable property.
        /// Expected result: The default value (0.0) should be returned.
        /// </summary>
        [Fact]
        public void TranslateY_GetDefaultValue_ReturnsZero()
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            var result = transform.TranslateY;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that multiple consecutive sets and gets of the TranslateY property work correctly.
        /// This test verifies that the property setter and getter maintain consistency across multiple operations.
        /// Expected result: Each set value should be correctly retrieved by subsequent get operations.
        /// </summary>
        [Fact]
        public void TranslateY_MultipleSetAndGet_MaintainsConsistency()
        {
            // Arrange
            var transform = new CompositeTransform();
            var testValues = new[] { 10.5, -20.3, 0.0, 999.999, -999.999 };

            foreach (var testValue in testValues)
            {
                // Act
                transform.TranslateY = testValue;
                var result = transform.TranslateY;

                // Assert
                Assert.Equal(testValue, result);
            }
        }

        /// <summary>
        /// Tests that setting the TranslateY property with boundary values works correctly.
        /// This test verifies that the setter handles edge case numeric values properly.
        /// Expected result: Boundary values should be set and retrieved without data loss or exceptions.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(-0.0)]
        [InlineData(1E-10)]
        [InlineData(-1E-10)]
        [InlineData(1E10)]
        [InlineData(-1E10)]
        public void TranslateY_SetBoundaryValues_HandlesCorrectly(double boundaryValue)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.TranslateY = boundaryValue;
            var result = transform.TranslateY;

            // Assert
            Assert.Equal(boundaryValue, result);
        }

        /// <summary>
        /// Tests that CenterX property can be set and retrieved with normal double values.
        /// </summary>
        /// <param name="value">The double value to set and verify.</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(100.5)]
        [InlineData(-100.5)]
        [InlineData(0.5)]
        [InlineData(1000000.123)]
        [InlineData(-1000000.123)]
        public void CenterX_SetAndGet_ReturnsExpectedValue(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.CenterX = value;
            var result = transform.CenterX;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that CenterX property can be set and retrieved with extreme double values.
        /// </summary>
        /// <param name="value">The extreme double value to set and verify.</param>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void CenterX_SetAndGetExtremeValues_ReturnsExpectedValue(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.CenterX = value;
            var result = transform.CenterX;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that CenterX property can be set and retrieved with special double values like NaN and infinity.
        /// </summary>
        /// <param name="value">The special double value to set and verify.</param>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void CenterX_SetAndGetSpecialValues_ReturnsExpectedValue(double value)
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            transform.CenterX = value;
            var result = transform.CenterX;

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
        /// Tests that CenterX property returns the default value when not explicitly set.
        /// The default value for CenterX should be 0.0 based on the BindableProperty definition.
        /// </summary>
        [Fact]
        public void CenterX_DefaultValue_ReturnsZero()
        {
            // Arrange
            var transform = new CompositeTransform();

            // Act
            var result = transform.CenterX;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that CenterX property can be set multiple times with different values.
        /// </summary>
        [Fact]
        public void CenterX_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var transform = new CompositeTransform();
            var firstValue = 10.5;
            var secondValue = -20.3;
            var thirdValue = 0.0;

            // Act & Assert
            transform.CenterX = firstValue;
            Assert.Equal(firstValue, transform.CenterX);

            transform.CenterX = secondValue;
            Assert.Equal(secondValue, transform.CenterX);

            transform.CenterX = thirdValue;
            Assert.Equal(thirdValue, transform.CenterX);
        }
    }
}