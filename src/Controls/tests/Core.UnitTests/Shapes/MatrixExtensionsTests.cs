#nullable disable

using System;
using System.Numerics;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class MatrixExtensionsTests
    {
        /// <summary>
        /// Tests that ToMatrix3X2 throws NullReferenceException when matrix parameter is null.
        /// This test validates the method's behavior with null input.
        /// Expected result: NullReferenceException should be thrown.
        /// </summary>
        [Fact]
        public void ToMatrix3X2_NullMatrix_ThrowsNullReferenceException()
        {
            // Arrange
            Matrix matrix = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => matrix.ToMatrix3X2());
        }

        /// <summary>
        /// Tests that ToMatrix3X2 correctly converts normal double values to Matrix3x2 with proper float casting and field mapping.
        /// This test validates the basic conversion functionality with typical matrix values.
        /// Expected result: All values should be correctly converted from double to float and mapped to appropriate Matrix3x2 fields.
        /// </summary>
        [Fact]
        public void ToMatrix3X2_NormalValues_ConvertsCorrectly()
        {
            // Arrange
            var matrix = new Matrix(1.5, 2.7, 3.3, 4.8, 5.2, 6.9);

            // Act
            var result = matrix.ToMatrix3X2();

            // Assert
            Assert.Equal(1.5f, result.M11);
            Assert.Equal(2.7f, result.M12);
            Assert.Equal(3.3f, result.M21);
            Assert.Equal(4.8f, result.M22);
            Assert.Equal(5.2f, result.M31); // OffsetX maps to M31
            Assert.Equal(6.9f, result.M32); // OffsetY maps to M32
        }

        /// <summary>
        /// Tests that ToMatrix3X2 correctly handles zero values in all matrix components.
        /// This test validates the conversion behavior with zero values.
        /// Expected result: All Matrix3x2 fields should be zero.
        /// </summary>
        [Fact]
        public void ToMatrix3X2_ZeroValues_ConvertsCorrectly()
        {
            // Arrange
            var matrix = new Matrix(0.0, 0.0, 0.0, 0.0, 0.0, 0.0);

            // Act
            var result = matrix.ToMatrix3X2();

            // Assert
            Assert.Equal(0.0f, result.M11);
            Assert.Equal(0.0f, result.M12);
            Assert.Equal(0.0f, result.M21);
            Assert.Equal(0.0f, result.M22);
            Assert.Equal(0.0f, result.M31);
            Assert.Equal(0.0f, result.M32);
        }

        /// <summary>
        /// Tests that ToMatrix3X2 correctly handles identity matrix values.
        /// This test validates the conversion behavior with standard identity matrix values.
        /// Expected result: Matrix3x2 should represent an identity matrix with offsets.
        /// </summary>
        [Fact]
        public void ToMatrix3X2_IdentityMatrix_ConvertsCorrectly()
        {
            // Arrange
            var matrix = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);

            // Act
            var result = matrix.ToMatrix3X2();

            // Assert
            Assert.Equal(1.0f, result.M11);
            Assert.Equal(0.0f, result.M12);
            Assert.Equal(0.0f, result.M21);
            Assert.Equal(1.0f, result.M22);
            Assert.Equal(0.0f, result.M31);
            Assert.Equal(0.0f, result.M32);
        }

        /// <summary>
        /// Tests that ToMatrix3X2 correctly handles negative values in all matrix components.
        /// This test validates the conversion behavior with negative values.
        /// Expected result: All Matrix3x2 fields should correctly represent the negative values as floats.
        /// </summary>
        [Fact]
        public void ToMatrix3X2_NegativeValues_ConvertsCorrectly()
        {
            // Arrange
            var matrix = new Matrix(-1.5, -2.7, -3.3, -4.8, -5.2, -6.9);

            // Act
            var result = matrix.ToMatrix3X2();

            // Assert
            Assert.Equal(-1.5f, result.M11);
            Assert.Equal(-2.7f, result.M12);
            Assert.Equal(-3.3f, result.M21);
            Assert.Equal(-4.8f, result.M22);
            Assert.Equal(-5.2f, result.M31);
            Assert.Equal(-6.9f, result.M32);
        }

        /// <summary>
        /// Tests that ToMatrix3X2 correctly handles double.NaN values in all matrix components.
        /// This test validates the conversion behavior with NaN values.
        /// Expected result: All Matrix3x2 fields should be float.NaN.
        /// </summary>
        [Fact]
        public void ToMatrix3X2_NaNValues_ConvertsCorrectly()
        {
            // Arrange
            var matrix = new Matrix(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN);

            // Act
            var result = matrix.ToMatrix3X2();

            // Assert
            Assert.True(float.IsNaN(result.M11));
            Assert.True(float.IsNaN(result.M12));
            Assert.True(float.IsNaN(result.M21));
            Assert.True(float.IsNaN(result.M22));
            Assert.True(float.IsNaN(result.M31));
            Assert.True(float.IsNaN(result.M32));
        }

        /// <summary>
        /// Tests that ToMatrix3X2 correctly handles double.PositiveInfinity values in all matrix components.
        /// This test validates the conversion behavior with positive infinity values.
        /// Expected result: All Matrix3x2 fields should be float.PositiveInfinity.
        /// </summary>
        [Fact]
        public void ToMatrix3X2_PositiveInfinityValues_ConvertsCorrectly()
        {
            // Arrange
            var matrix = new Matrix(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                                    double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);

            // Act
            var result = matrix.ToMatrix3X2();

            // Assert
            Assert.True(float.IsPositiveInfinity(result.M11));
            Assert.True(float.IsPositiveInfinity(result.M12));
            Assert.True(float.IsPositiveInfinity(result.M21));
            Assert.True(float.IsPositiveInfinity(result.M22));
            Assert.True(float.IsPositiveInfinity(result.M31));
            Assert.True(float.IsPositiveInfinity(result.M32));
        }

        /// <summary>
        /// Tests that ToMatrix3X2 correctly handles double.NegativeInfinity values in all matrix components.
        /// This test validates the conversion behavior with negative infinity values.
        /// Expected result: All Matrix3x2 fields should be float.NegativeInfinity.
        /// </summary>
        [Fact]
        public void ToMatrix3X2_NegativeInfinityValues_ConvertsCorrectly()
        {
            // Arrange
            var matrix = new Matrix(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity,
                                    double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);

            // Act
            var result = matrix.ToMatrix3X2();

            // Assert
            Assert.True(float.IsNegativeInfinity(result.M11));
            Assert.True(float.IsNegativeInfinity(result.M12));
            Assert.True(float.IsNegativeInfinity(result.M21));
            Assert.True(float.IsNegativeInfinity(result.M22));
            Assert.True(float.IsNegativeInfinity(result.M31));
            Assert.True(float.IsNegativeInfinity(result.M32));
        }

        /// <summary>
        /// Tests that ToMatrix3X2 correctly handles extreme values near double.MaxValue.
        /// This test validates the conversion behavior with very large values and potential precision loss.
        /// Expected result: Values should be converted to float, potentially with precision loss for values outside float range.
        /// </summary>
        [Fact]
        public void ToMatrix3X2_ExtremeMaxValues_ConvertsCorrectly()
        {
            // Arrange
            var matrix = new Matrix(double.MaxValue, double.MaxValue, double.MaxValue,
                                    double.MaxValue, double.MaxValue, double.MaxValue);

            // Act
            var result = matrix.ToMatrix3X2();

            // Assert
            Assert.True(float.IsPositiveInfinity(result.M11)); // double.MaxValue exceeds float.MaxValue
            Assert.True(float.IsPositiveInfinity(result.M12));
            Assert.True(float.IsPositiveInfinity(result.M21));
            Assert.True(float.IsPositiveInfinity(result.M22));
            Assert.True(float.IsPositiveInfinity(result.M31));
            Assert.True(float.IsPositiveInfinity(result.M32));
        }

        /// <summary>
        /// Tests that ToMatrix3X2 correctly handles extreme values near double.MinValue.
        /// This test validates the conversion behavior with very large negative values and potential precision loss.
        /// Expected result: Values should be converted to float, potentially with precision loss for values outside float range.
        /// </summary>
        [Fact]
        public void ToMatrix3X2_ExtremeMinValues_ConvertsCorrectly()
        {
            // Arrange
            var matrix = new Matrix(double.MinValue, double.MinValue, double.MinValue,
                                    double.MinValue, double.MinValue, double.MinValue);

            // Act
            var result = matrix.ToMatrix3X2();

            // Assert
            Assert.True(float.IsNegativeInfinity(result.M11)); // double.MinValue exceeds float.MinValue
            Assert.True(float.IsNegativeInfinity(result.M12));
            Assert.True(float.IsNegativeInfinity(result.M21));
            Assert.True(float.IsNegativeInfinity(result.M22));
            Assert.True(float.IsNegativeInfinity(result.M31));
            Assert.True(float.IsNegativeInfinity(result.M32));
        }

        /// <summary>
        /// Tests that ToMatrix3X2 correctly handles precision conversion from double to float with values within float range.
        /// This test validates the precision conversion behavior with high-precision double values.
        /// Expected result: Values should be converted to float with appropriate precision loss.
        /// </summary>
        [Fact]
        public void ToMatrix3X2_HighPrecisionValues_ConvertsWithExpectedPrecisionLoss()
        {
            // Arrange
            var matrix = new Matrix(1.123456789012345, 2.987654321098765, 3.555555555555555,
                                    4.777777777777777, 5.999999999999999, 6.111111111111111);

            // Act
            var result = matrix.ToMatrix3X2();

            // Assert
            // Values should be converted to float precision
            Assert.Equal(1.1234568f, result.M11, 6); // Allow for float precision
            Assert.Equal(2.9876542f, result.M12, 6);
            Assert.Equal(3.5555556f, result.M21, 6);
            Assert.Equal(4.7777777f, result.M22, 6);
            Assert.Equal(6.0f, result.M31, 6);
            Assert.Equal(6.111111f, result.M32, 6);
        }

        /// <summary>
        /// Tests ToMatrix extension method with normal positive values.
        /// Verifies that all Matrix3x2 fields are correctly mapped to Matrix properties.
        /// </summary>
        [Fact]
        public void ToMatrix_NormalPositiveValues_CorrectlyMapsAllFields()
        {
            // Arrange
            var matrix3x2 = new Matrix3x2(1.1f, 2.2f, 3.3f, 4.4f, 5.5f, 6.6f);

            // Act
            var result = matrix3x2.ToMatrix();

            // Assert
            Assert.Equal(1.1, result.M11, 10);
            Assert.Equal(2.2, result.M12, 10);
            Assert.Equal(3.3, result.M21, 10);
            Assert.Equal(4.4, result.M22, 10);
            Assert.Equal(5.5, result.OffsetX, 10);
            Assert.Equal(6.6, result.OffsetY, 10);
        }

        /// <summary>
        /// Tests ToMatrix extension method with negative values.
        /// Verifies that negative float values are correctly converted to double properties.
        /// </summary>
        [Fact]
        public void ToMatrix_NegativeValues_CorrectlyConvertsAllFields()
        {
            // Arrange
            var matrix3x2 = new Matrix3x2(-1.5f, -2.7f, -3.9f, -4.1f, -5.3f, -6.8f);

            // Act
            var result = matrix3x2.ToMatrix();

            // Assert
            Assert.Equal(-1.5, result.M11, 10);
            Assert.Equal(-2.7, result.M12, 10);
            Assert.Equal(-3.9, result.M21, 10);
            Assert.Equal(-4.1, result.M22, 10);
            Assert.Equal(-5.3, result.OffsetX, 10);
            Assert.Equal(-6.8, result.OffsetY, 10);
        }

        /// <summary>
        /// Tests ToMatrix extension method with zero values.
        /// Verifies that zero Matrix3x2 creates a zero Matrix with all properties set to zero.
        /// </summary>
        [Fact]
        public void ToMatrix_ZeroValues_ReturnsMatrixWithZeroProperties()
        {
            // Arrange
            var matrix3x2 = new Matrix3x2(0f, 0f, 0f, 0f, 0f, 0f);

            // Act
            var result = matrix3x2.ToMatrix();

            // Assert
            Assert.Equal(0, result.M11);
            Assert.Equal(0, result.M12);
            Assert.Equal(0, result.M21);
            Assert.Equal(0, result.M22);
            Assert.Equal(0, result.OffsetX);
            Assert.Equal(0, result.OffsetY);
        }

        /// <summary>
        /// Tests ToMatrix extension method with identity matrix values.
        /// Verifies that identity Matrix3x2 converts to corresponding Matrix values.
        /// </summary>
        [Fact]
        public void ToMatrix_IdentityMatrix_CorrectlyConvertsIdentityValues()
        {
            // Arrange
            var matrix3x2 = Matrix3x2.Identity; // M11=1, M12=0, M21=0, M22=1, M31=0, M32=0

            // Act
            var result = matrix3x2.ToMatrix();

            // Assert
            Assert.Equal(1, result.M11);
            Assert.Equal(0, result.M12);
            Assert.Equal(0, result.M21);
            Assert.Equal(1, result.M22);
            Assert.Equal(0, result.OffsetX);
            Assert.Equal(0, result.OffsetY);
        }

        /// <summary>
        /// Tests ToMatrix extension method with float boundary values.
        /// Verifies that minimum and maximum float values are correctly converted to double.
        /// </summary>
        [Fact]
        public void ToMatrix_FloatBoundaryValues_CorrectlyConvertsToDouble()
        {
            // Arrange
            var matrix3x2 = new Matrix3x2(float.MinValue, float.MaxValue, -1f, 1f, 0f, 0.5f);

            // Act
            var result = matrix3x2.ToMatrix();

            // Assert
            Assert.Equal((double)float.MinValue, result.M11);
            Assert.Equal((double)float.MaxValue, result.M12);
            Assert.Equal(-1, result.M21);
            Assert.Equal(1, result.M22);
            Assert.Equal(0, result.OffsetX);
            Assert.Equal(0.5, result.OffsetY, 10);
        }

        /// <summary>
        /// Tests ToMatrix extension method with special float values including NaN.
        /// Verifies that NaN values are preserved during conversion to Matrix.
        /// </summary>
        [Fact]
        public void ToMatrix_NaNValues_PreservesNaNInResult()
        {
            // Arrange
            var matrix3x2 = new Matrix3x2(float.NaN, 1f, 2f, float.NaN, 3f, float.NaN);

            // Act
            var result = matrix3x2.ToMatrix();

            // Assert
            Assert.True(double.IsNaN(result.M11));
            Assert.Equal(1, result.M12);
            Assert.Equal(2, result.M21);
            Assert.True(double.IsNaN(result.M22));
            Assert.Equal(3, result.OffsetX);
            Assert.True(double.IsNaN(result.OffsetY));
        }

        /// <summary>
        /// Tests ToMatrix extension method with positive infinity values.
        /// Verifies that positive infinity float values convert to positive infinity double values.
        /// </summary>
        [Fact]
        public void ToMatrix_PositiveInfinityValues_PreservesInfinityInResult()
        {
            // Arrange
            var matrix3x2 = new Matrix3x2(float.PositiveInfinity, 0f, 1f, float.PositiveInfinity, 0f, float.PositiveInfinity);

            // Act
            var result = matrix3x2.ToMatrix();

            // Assert
            Assert.True(double.IsPositiveInfinity(result.M11));
            Assert.Equal(0, result.M12);
            Assert.Equal(1, result.M21);
            Assert.True(double.IsPositiveInfinity(result.M22));
            Assert.Equal(0, result.OffsetX);
            Assert.True(double.IsPositiveInfinity(result.OffsetY));
        }

        /// <summary>
        /// Tests ToMatrix extension method with negative infinity values.
        /// Verifies that negative infinity float values convert to negative infinity double values.
        /// </summary>
        [Fact]
        public void ToMatrix_NegativeInfinityValues_PreservesNegativeInfinityInResult()
        {
            // Arrange
            var matrix3x2 = new Matrix3x2(float.NegativeInfinity, 1f, 0f, float.NegativeInfinity, float.NegativeInfinity, 2f);

            // Act
            var result = matrix3x2.ToMatrix();

            // Assert
            Assert.True(double.IsNegativeInfinity(result.M11));
            Assert.Equal(1, result.M12);
            Assert.Equal(0, result.M21);
            Assert.True(double.IsNegativeInfinity(result.M22));
            Assert.True(double.IsNegativeInfinity(result.OffsetX));
            Assert.Equal(2, result.OffsetY);
        }

        /// <summary>
        /// Tests ToMatrix extension method with very small float values.
        /// Verifies that precision is maintained during float to double conversion.
        /// </summary>
        [Fact]
        public void ToMatrix_VerySmallValues_MaintainsPrecision()
        {
            // Arrange
            var matrix3x2 = new Matrix3x2(1e-7f, -1e-7f, 1e-6f, -1e-6f, 1e-5f, -1e-5f);

            // Act
            var result = matrix3x2.ToMatrix();

            // Assert
            Assert.Equal(1e-7, result.M11, 10);
            Assert.Equal(-1e-7, result.M12, 10);
            Assert.Equal(1e-6, result.M21, 10);
            Assert.Equal(-1e-6, result.M22, 10);
            Assert.Equal(1e-5, result.OffsetX, 10);
            Assert.Equal(-1e-5, result.OffsetY, 10);
        }
    }
}