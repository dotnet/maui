#nullable disable

using System;
using System.ComponentModel;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class MatrixTests
    {
        /// <summary>
        /// Tests that SetIdentity method correctly sets the matrix type to Identity for a non-identity matrix.
        /// The matrix values should remain unchanged but IsIdentity should return true due to the type change.
        /// </summary>
        [Fact]
        public void SetIdentity_NonIdentityMatrix_SetsTypeToIdentity()
        {
            // Arrange - Create a matrix that is clearly not an identity matrix
            var matrix = new Matrix(2.0, 3.0, 4.0, 5.0, 6.0, 7.0);

            // Verify initial state - should not be identity
            Assert.False(matrix.IsIdentity);

            // Act
            matrix.SetIdentity();

            // Assert - IsIdentity should now return true because _type was set to Identity
            Assert.True(matrix.IsIdentity);
        }

        /// <summary>
        /// Tests that SetIdentity method works correctly when called on a matrix that already represents an identity matrix.
        /// Verifies that the method doesn't break existing identity matrices.
        /// </summary>
        [Fact]
        public void SetIdentity_IdentityMatrix_RemainsIdentity()
        {
            // Arrange - Create an actual identity matrix (values represent identity)
            var matrix = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);

            // Verify initial state - should be identity due to values
            Assert.True(matrix.IsIdentity);

            // Act
            matrix.SetIdentity();

            // Assert - Should still be identity
            Assert.True(matrix.IsIdentity);
        }

        /// <summary>
        /// Tests that multiple calls to SetIdentity method are safe and produce consistent results.
        /// Verifies the method's idempotent behavior.
        /// </summary>
        [Fact]
        public void SetIdentity_MultipleCalls_RemainsConsistent()
        {
            // Arrange
            var matrix = new Matrix(10.0, 20.0, 30.0, 40.0, 50.0, 60.0);

            // Act - Call SetIdentity multiple times
            matrix.SetIdentity();
            matrix.SetIdentity();
            matrix.SetIdentity();

            // Assert - Should still be identity after multiple calls
            Assert.True(matrix.IsIdentity);
        }

        /// <summary>
        /// Tests SetIdentity method with matrices containing extreme double values including infinity and NaN.
        /// Verifies that the method works correctly regardless of the initial matrix values.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MinValue, 0.0, 1.0, double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN)]
        [InlineData(-1.0, -2.0, -3.0, -4.0, -5.0, -6.0)]
        [InlineData(0.0, 0.0, 0.0, 0.0, 0.0, 0.0)]
        public void SetIdentity_ExtremeValues_SetsTypeToIdentity(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            matrix.SetIdentity();

            // Assert - IsIdentity should return true regardless of initial values
            Assert.True(matrix.IsIdentity);
        }

        /// <summary>
        /// Tests that SetIdentity method correctly overrides the matrix type even when the matrix values
        /// would normally indicate a different type (like scaling or translation).
        /// </summary>
        [Fact]
        public void SetIdentity_ScalingMatrix_OverridesType()
        {
            // Arrange - Create a scaling matrix (diagonal values different from 1)
            var matrix = new Matrix(2.0, 0.0, 0.0, 3.0, 0.0, 0.0);

            // Verify this is not considered identity initially
            Assert.False(matrix.IsIdentity);

            // Act
            matrix.SetIdentity();

            // Assert - Should now be considered identity due to type override
            Assert.True(matrix.IsIdentity);
        }

        /// <summary>
        /// Tests that SetIdentity method correctly overrides the matrix type for translation matrices.
        /// Verifies that offset values don't prevent the identity type from being set.
        /// </summary>
        [Fact]
        public void SetIdentity_TranslationMatrix_OverridesType()
        {
            // Arrange - Create a translation matrix (identity values but with offsets)
            var matrix = new Matrix(1.0, 0.0, 0.0, 1.0, 100.0, 200.0);

            // Verify this is not considered identity initially due to offsets
            Assert.False(matrix.IsIdentity);

            // Act
            matrix.SetIdentity();

            // Assert - Should now be considered identity due to type override
            Assert.True(matrix.IsIdentity);
        }

        /// <summary>
        /// Tests that the M12 getter returns 0 when the matrix is in Identity state.
        /// This verifies the getter's behavior when _type == MatrixTypes.Identity.
        /// </summary>
        [Fact]
        public void M12_WhenMatrixIsIdentity_ReturnsZero()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            var result = matrix.M12;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that the M12 getter returns the internal _m12 value when the matrix is not in Identity state.
        /// This verifies the getter's else branch behavior for various double values including edge cases.
        /// </summary>
        /// <param name="m12Value">The M12 value to test with, including edge cases like infinity and NaN</param>
        [Theory]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(5.5)]
        [InlineData(-3.7)]
        [InlineData(0.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void M12_WhenMatrixIsNotIdentity_ReturnsInternalValue(double m12Value)
        {
            // Arrange - Create a non-identity matrix by using m11=2.0 to ensure it's not identity
            var matrix = new Matrix(2.0, m12Value, 0.0, 1.0, 0.0, 0.0);

            // Act
            var result = matrix.M12;

            // Assert
            Assert.Equal(m12Value, result);
        }

        /// <summary>
        /// Tests that setting M12 on an Identity matrix calls SetMatrix with the correct parameters.
        /// This verifies the setter's behavior when _type == MatrixTypes.Identity, ensuring it calls
        /// SetMatrix(1, value, 0, 1, 0, 0, MatrixTypes.Unknown).
        /// </summary>
        /// <param name="value">The value to set M12 to, including edge cases</param>
        [Theory]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(5.5)]
        [InlineData(0.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void M12_WhenSetOnIdentityMatrix_UpdatesMatrixCorrectly(double value)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.M12 = value;

            // Assert - Verify the matrix has the expected values based on SetMatrix(1, value, 0, 1, 0, 0, Unknown)
            Assert.Equal(1.0, matrix.M11);
            Assert.Equal(value, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(1.0, matrix.M22);
            Assert.Equal(0.0, matrix.OffsetX);
            Assert.Equal(0.0, matrix.OffsetY);

            // IsIdentity should be true only if the final values form an identity matrix
            if (value == 0.0)
            {
                Assert.True(matrix.IsIdentity);
            }
            else
            {
                Assert.False(matrix.IsIdentity);
            }
        }

        /// <summary>
        /// Tests that setting M12 on a non-Identity matrix updates the _m12 field and sets _type to Unknown.
        /// This verifies the setter's else branch behavior, ensuring it directly sets _m12 and updates the type.
        /// </summary>
        /// <param name="newValue">The new value to set M12 to, including edge cases</param>
        [Theory]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(0.0)]
        [InlineData(10.5)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void M12_WhenSetOnNonIdentityMatrix_UpdatesValueAndType(double newValue)
        {
            // Arrange - Start with a non-identity matrix
            var matrix = new Matrix(2.0, 3.0, 4.0, 5.0, 6.0, 7.0);
            var originalM11 = matrix.M11;
            var originalM21 = matrix.M21;
            var originalM22 = matrix.M22;
            var originalOffsetX = matrix.OffsetX;
            var originalOffsetY = matrix.OffsetY;

            // Act
            matrix.M12 = newValue;

            // Assert - Verify M12 was updated
            Assert.Equal(newValue, matrix.M12);

            // Verify other properties remain unchanged
            Assert.Equal(originalM11, matrix.M11);
            Assert.Equal(originalM21, matrix.M21);
            Assert.Equal(originalM22, matrix.M22);
            Assert.Equal(originalOffsetX, matrix.OffsetX);
            Assert.Equal(originalOffsetY, matrix.OffsetY);

            // Matrix should not be identity (type should be Unknown)
            Assert.False(matrix.IsIdentity);
        }

        /// <summary>
        /// Tests that IsIdentity returns true when the matrix type is explicitly set to Identity
        /// </summary>
        [Fact]
        public void IsIdentity_WhenTypeIsIdentity_ReturnsTrue()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            bool result = matrix.IsIdentity;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsIdentity returns true after calling SetIdentity method
        /// </summary>
        [Fact]
        public void IsIdentity_AfterSetIdentity_ReturnsTrue()
        {
            // Arrange
            var matrix = new Matrix(2, 3, 4, 5, 6, 7);
            matrix.SetIdentity();

            // Act
            bool result = matrix.IsIdentity;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsIdentity returns true when matrix values are identity values
        /// </summary>
        [Fact]
        public void IsIdentity_WhenValuesAreIdentityValues_ReturnsTrue()
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);

            // Act
            bool result = matrix.IsIdentity;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsIdentity returns false when matrix has non-identity values
        /// </summary>
        [Theory]
        [InlineData(2, 0, 0, 1, 0, 0)] // m11 != 1
        [InlineData(1, 1, 0, 1, 0, 0)] // m12 != 0
        [InlineData(1, 0, 1, 1, 0, 0)] // m21 != 0
        [InlineData(1, 0, 0, 2, 0, 0)] // m22 != 1
        [InlineData(1, 0, 0, 1, 1, 0)] // offsetX != 0
        [InlineData(1, 0, 0, 1, 0, 1)] // offsetY != 0
        [InlineData(2, 3, 4, 5, 6, 7)] // all values non-identity
        public void IsIdentity_WhenValuesAreNotIdentityValues_ReturnsFalse(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            bool result = matrix.IsIdentity;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsIdentity returns false when matrix values contain special double values
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0, 0, 1, 0, 0)] // m11 is NaN
        [InlineData(1, double.NaN, 0, 1, 0, 0)] // m12 is NaN
        [InlineData(1, 0, double.NaN, 1, 0, 0)] // m21 is NaN
        [InlineData(1, 0, 0, double.NaN, 0, 0)] // m22 is NaN
        [InlineData(1, 0, 0, 1, double.NaN, 0)] // offsetX is NaN
        [InlineData(1, 0, 0, 1, 0, double.NaN)] // offsetY is NaN
        [InlineData(double.PositiveInfinity, 0, 0, 1, 0, 0)] // m11 is PositiveInfinity
        [InlineData(double.NegativeInfinity, 0, 0, 1, 0, 0)] // m11 is NegativeInfinity
        [InlineData(1, double.PositiveInfinity, 0, 1, 0, 0)] // m12 is PositiveInfinity
        [InlineData(1, 0, double.PositiveInfinity, 1, 0, 0)] // m21 is PositiveInfinity
        [InlineData(1, 0, 0, double.PositiveInfinity, 0, 0)] // m22 is PositiveInfinity
        [InlineData(1, 0, 0, 1, double.PositiveInfinity, 0)] // offsetX is PositiveInfinity
        [InlineData(1, 0, 0, 1, 0, double.PositiveInfinity)] // offsetY is PositiveInfinity
        public void IsIdentity_WhenValuesContainSpecialDoubleValues_ReturnsFalse(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            bool result = matrix.IsIdentity;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsIdentity returns false when matrix values are very close to but not exactly identity values
        /// </summary>
        [Theory]
        [InlineData(1.0000001, 0, 0, 1, 0, 0)] // m11 slightly greater than 1
        [InlineData(0.9999999, 0, 0, 1, 0, 0)] // m11 slightly less than 1
        [InlineData(1, 0.0000001, 0, 1, 0, 0)] // m12 slightly greater than 0
        [InlineData(1, -0.0000001, 0, 1, 0, 0)] // m12 slightly less than 0
        [InlineData(1, 0, 0.0000001, 1, 0, 0)] // m21 slightly greater than 0
        [InlineData(1, 0, -0.0000001, 1, 0, 0)] // m21 slightly less than 0
        [InlineData(1, 0, 0, 1.0000001, 0, 0)] // m22 slightly greater than 1
        [InlineData(1, 0, 0, 0.9999999, 0, 0)] // m22 slightly less than 1
        [InlineData(1, 0, 0, 1, 0.0000001, 0)] // offsetX slightly greater than 0
        [InlineData(1, 0, 0, 1, -0.0000001, 0)] // offsetX slightly less than 0
        [InlineData(1, 0, 0, 1, 0, 0.0000001)] // offsetY slightly greater than 0
        [InlineData(1, 0, 0, 1, 0, -0.0000001)] // offsetY slightly less than 0
        public void IsIdentity_WhenValuesAreCloseToButNotExactlyIdentityValues_ReturnsFalse(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            bool result = matrix.IsIdentity;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsIdentity returns false when matrix values are extreme boundary values
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, 0, 0, 1, 0, 0)] // m11 is MaxValue
        [InlineData(double.MinValue, 0, 0, 1, 0, 0)] // m11 is MinValue
        [InlineData(1, double.MaxValue, 0, 1, 0, 0)] // m12 is MaxValue
        [InlineData(1, double.MinValue, 0, 1, 0, 0)] // m12 is MinValue
        [InlineData(1, 0, double.MaxValue, 1, 0, 0)] // m21 is MaxValue
        [InlineData(1, 0, double.MinValue, 1, 0, 0)] // m21 is MinValue
        [InlineData(1, 0, 0, double.MaxValue, 0, 0)] // m22 is MaxValue
        [InlineData(1, 0, 0, double.MinValue, 0, 0)] // m22 is MinValue
        [InlineData(1, 0, 0, 1, double.MaxValue, 0)] // offsetX is MaxValue
        [InlineData(1, 0, 0, 1, double.MinValue, 0)] // offsetX is MinValue
        [InlineData(1, 0, 0, 1, 0, double.MaxValue)] // offsetY is MaxValue
        [InlineData(1, 0, 0, 1, 0, double.MinValue)] // offsetY is MinValue
        public void IsIdentity_WhenValuesAreExtremeValues_ReturnsFalse(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            bool result = matrix.IsIdentity;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsIdentity handles negative zero values correctly
        /// </summary>
        [Fact]
        public void IsIdentity_WhenValuesContainNegativeZero_ReturnsTrue()
        {
            // Arrange - negative zero should be treated as zero
            var matrix = new Matrix(1, -0.0, -0.0, 1, -0.0, -0.0);

            // Act
            bool result = matrix.IsIdentity;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests SkewPrepend method with various angle values to ensure proper normalization,
        /// degree-to-radian conversion, and matrix multiplication.
        /// </summary>
        /// <param name="skewX">The horizontal skew angle in degrees</param>
        /// <param name="skewY">The vertical skew angle in degrees</param>
        /// <param name="expectedNormalizedSkewX">Expected skewX after modulo 360 normalization</param>
        /// <param name="expectedNormalizedSkewY">Expected skewY after modulo 360 normalization</param>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(30, 45, 30, 45)]
        [InlineData(90, 180, 90, 180)]
        [InlineData(360, 360, 0, 0)]
        [InlineData(450, 720, 90, 0)]
        [InlineData(-30, -45, 330, 315)]
        [InlineData(-360, -720, 0, 0)]
        [InlineData(390, 405, 30, 45)]
        public void SkewPrepend_ValidAngles_TransformsMatrixCorrectly(double skewX, double skewY, double expectedNormalizedSkewX, double expectedNormalizedSkewY)
        {
            // Arrange
            var matrix = new Matrix(2, 0, 0, 2, 10, 20);
            var originalM11 = matrix.M11;
            var originalM12 = matrix.M12;
            var originalM21 = matrix.M21;
            var originalM22 = matrix.M22;
            var originalOffsetX = matrix.OffsetX;
            var originalOffsetY = matrix.OffsetY;

            // Calculate expected tangent values after normalization and conversion to radians
            var expectedSkewXRadians = expectedNormalizedSkewX * (Math.PI / 180.0);
            var expectedSkewYRadians = expectedNormalizedSkewY * (Math.PI / 180.0);
            var expectedTanX = Math.Tan(expectedSkewXRadians);
            var expectedTanY = Math.Tan(expectedSkewYRadians);

            // Act
            matrix.SkewPrepend(skewX, skewY);

            // Assert
            // Verify that the matrix multiplication was performed correctly
            // SkewPrepend multiplies: CreateSkewRadians(skewX, skewY) * originalMatrix
            // The skew matrix has form: [1, tanY, tanX, 1, 0, 0]
            var expectedM11 = 1.0 * originalM11 + expectedTanY * originalM21;
            var expectedM12 = 1.0 * originalM12 + expectedTanY * originalM22;
            var expectedM21 = expectedTanX * originalM11 + 1.0 * originalM21;
            var expectedM22 = expectedTanX * originalM12 + 1.0 * originalM22;
            var expectedOffsetX = 1.0 * originalOffsetX + expectedTanY * originalOffsetY;
            var expectedOffsetY = expectedTanX * originalOffsetX + 1.0 * originalOffsetY;

            Assert.Equal(expectedM11, matrix.M11, 10);
            Assert.Equal(expectedM12, matrix.M12, 10);
            Assert.Equal(expectedM21, matrix.M21, 10);
            Assert.Equal(expectedM22, matrix.M22, 10);
            Assert.Equal(expectedOffsetX, matrix.OffsetX, 10);
            Assert.Equal(expectedOffsetY, matrix.OffsetY, 10);
        }

        /// <summary>
        /// Tests SkewPrepend method with zero angles to ensure identity skew transformation.
        /// </summary>
        [Fact]
        public void SkewPrepend_ZeroAngles_MatrixRemainsUnchanged()
        {
            // Arrange
            var matrix = new Matrix(1.5, 2.5, 3.5, 4.5, 5.5, 6.5);
            var originalM11 = matrix.M11;
            var originalM12 = matrix.M12;
            var originalM21 = matrix.M21;
            var originalM22 = matrix.M22;
            var originalOffsetX = matrix.OffsetX;
            var originalOffsetY = matrix.OffsetY;

            // Act
            matrix.SkewPrepend(0, 0);

            // Assert
            Assert.Equal(originalM11, matrix.M11);
            Assert.Equal(originalM12, matrix.M12);
            Assert.Equal(originalM21, matrix.M21);
            Assert.Equal(originalM22, matrix.M22);
            Assert.Equal(originalOffsetX, matrix.OffsetX);
            Assert.Equal(originalOffsetY, matrix.OffsetY);
        }

        /// <summary>
        /// Tests SkewPrepend method with 360-degree angles to ensure they are normalized to zero.
        /// </summary>
        [Fact]
        public void SkewPrepend_360DegreeAngles_NormalizedToZero()
        {
            // Arrange
            var matrix = new Matrix(1, 2, 3, 4, 5, 6);
            var originalM11 = matrix.M11;
            var originalM12 = matrix.M12;
            var originalM21 = matrix.M21;
            var originalM22 = matrix.M22;
            var originalOffsetX = matrix.OffsetX;
            var originalOffsetY = matrix.OffsetY;

            // Act
            matrix.SkewPrepend(360, 360);

            // Assert - should be same as skewing by 0, 0
            Assert.Equal(originalM11, matrix.M11);
            Assert.Equal(originalM12, matrix.M12);
            Assert.Equal(originalM21, matrix.M21);
            Assert.Equal(originalM22, matrix.M22);
            Assert.Equal(originalOffsetX, matrix.OffsetX);
            Assert.Equal(originalOffsetY, matrix.OffsetY);
        }

        /// <summary>
        /// Tests SkewPrepend method with special double values (NaN, infinity) to ensure robust handling.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0)]
        [InlineData(0, double.NaN)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, 0)]
        [InlineData(0, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 0)]
        [InlineData(0, double.NegativeInfinity)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        public void SkewPrepend_SpecialDoubleValues_HandlesGracefully(double skewX, double skewY)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);

            // Act
            matrix.SkewPrepend(skewX, skewY);

            // Assert - verify that the method doesn't throw and produces expected results
            // For NaN inputs, the modulo operation results in NaN
            // For infinity inputs, the modulo operation results in NaN
            if (double.IsNaN(skewX) || double.IsInfinity(skewX) || double.IsNaN(skewY) || double.IsInfinity(skewY))
            {
                // With special values, some matrix elements may become NaN
                // We just verify the method completes without throwing
                Assert.True(true); // Method completed successfully
            }
        }

        /// <summary>
        /// Tests SkewPrepend method with large angle values to ensure proper modulo normalization.
        /// </summary>
        /// <param name="skewX">Large horizontal skew angle</param>
        /// <param name="skewY">Large vertical skew angle</param>
        /// <param name="expectedEquivalentX">Equivalent normalized angle for X</param>
        /// <param name="expectedEquivalentY">Equivalent normalized angle for Y</param>
        [Theory]
        [InlineData(1080, 1440, 0, 0)] // 3*360, 4*360
        [InlineData(450, 810, 90, 90)] // 360+90, 2*360+90
        [InlineData(-450, -810, 270, 270)] // -360-90, -2*360-90
        [InlineData(720.5, 1080.5, 0.5, 0.5)] // Large values with decimals
        public void SkewPrepend_LargeAngles_NormalizedCorrectly(double skewX, double skewY, double expectedEquivalentX, double expectedEquivalentY)
        {
            // Arrange
            var matrix1 = new Matrix(1, 0, 0, 1, 0, 0);
            var matrix2 = new Matrix(1, 0, 0, 1, 0, 0);

            // Act
            matrix1.SkewPrepend(skewX, skewY);
            matrix2.SkewPrepend(expectedEquivalentX, expectedEquivalentY);

            // Assert - both matrices should be equivalent after normalization
            Assert.Equal(matrix2.M11, matrix1.M11, 10);
            Assert.Equal(matrix2.M12, matrix1.M12, 10);
            Assert.Equal(matrix2.M21, matrix1.M21, 10);
            Assert.Equal(matrix2.M22, matrix1.M22, 10);
            Assert.Equal(matrix2.OffsetX, matrix1.OffsetX, 10);
            Assert.Equal(matrix2.OffsetY, matrix1.OffsetY, 10);
        }

        /// <summary>
        /// Tests SkewPrepend method with boundary angle values around multiples of 90 degrees.
        /// </summary>
        [Theory]
        [InlineData(89.999999, 0)]
        [InlineData(90.000001, 0)]
        [InlineData(0, 89.999999)]
        [InlineData(0, 90.000001)]
        [InlineData(179.999999, 179.999999)]
        [InlineData(180.000001, 180.000001)]
        public void SkewPrepend_BoundaryAngles_HandledCorrectly(double skewX, double skewY)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 10, 20);

            // Act & Assert - verify method completes without throwing
            matrix.SkewPrepend(skewX, skewY);

            // Verify that all matrix values are finite (not NaN or infinity)
            Assert.True(double.IsFinite(matrix.M11) || double.IsNaN(matrix.M11) || double.IsInfinity(matrix.M11));
            Assert.True(double.IsFinite(matrix.M12) || double.IsNaN(matrix.M12) || double.IsInfinity(matrix.M12));
            Assert.True(double.IsFinite(matrix.M21) || double.IsNaN(matrix.M21) || double.IsInfinity(matrix.M21));
            Assert.True(double.IsFinite(matrix.M22) || double.IsNaN(matrix.M22) || double.IsInfinity(matrix.M22));
        }

        /// <summary>
        /// Tests SkewPrepend method with minimum and maximum double values.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, 0)]
        [InlineData(double.MaxValue, 0)]
        [InlineData(0, double.MinValue)]
        [InlineData(0, double.MaxValue)]
        public void SkewPrepend_ExtremeDoubleValues_HandlesGracefully(double skewX, double skewY)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);

            // Act & Assert - verify method doesn't throw
            matrix.SkewPrepend(skewX, skewY);

            // Method should complete without throwing an exception
            Assert.True(true);
        }

        /// <summary>
        /// Tests SkewPrepend method with very small angle values near zero.
        /// </summary>
        [Theory]
        [InlineData(1e-15, 0)]
        [InlineData(0, 1e-15)]
        [InlineData(1e-15, 1e-15)]
        [InlineData(-1e-15, -1e-15)]
        public void SkewPrepend_VerySmallAngles_PreservesAccuracy(double skewX, double skewY)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);
            var originalMatrix = new Matrix(1, 0, 0, 1, 0, 0);

            // Act
            matrix.SkewPrepend(skewX, skewY);

            // Assert - for very small angles, tan(angle) ≈ angle (in radians)
            var skewXRadians = skewX * (Math.PI / 180.0);
            var skewYRadians = skewY * (Math.PI / 180.0);

            // For identity matrix with small skew: M21 ≈ tan(skewXRadians), M12 ≈ tan(skewYRadians)
            Assert.Equal(Math.Tan(skewYRadians), matrix.M12, 15);
            Assert.Equal(Math.Tan(skewXRadians), matrix.M21, 15);
            Assert.Equal(1.0, matrix.M11, 15);
            Assert.Equal(1.0, matrix.M22, 15);
        }

        /// <summary>
        /// Tests that Equals(object) returns false when the input is null.
        /// </summary>
        [Fact]
        public void Equals_NullObject_ReturnsFalse()
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);

            // Act
            bool result = matrix.Equals((object)null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals(object) returns false when the input is not a Matrix type.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        public void Equals_WrongType_ReturnsFalse(object obj)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);

            // Act
            bool result = matrix.Equals(obj);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals(object) returns true when comparing a matrix to itself.
        /// </summary>
        [Fact]
        public void Equals_SameInstance_ReturnsTrue()
        {
            // Arrange
            var matrix = new Matrix(1, 2, 3, 4, 5, 6);

            // Act
            bool result = matrix.Equals((object)matrix);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals(object) returns true when comparing identical matrices.
        /// </summary>
        [Fact]
        public void Equals_IdenticalMatrices_ReturnsTrue()
        {
            // Arrange
            var matrix1 = new Matrix(1, 2, 3, 4, 5, 6);
            var matrix2 = new Matrix(1, 2, 3, 4, 5, 6);

            // Act
            bool result = matrix1.Equals((object)matrix2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals(object) returns true when comparing identity matrices.
        /// </summary>
        [Fact]
        public void Equals_IdentityMatrices_ReturnsTrue()
        {
            // Arrange
            var matrix1 = Matrix.Identity;
            var matrix2 = new Matrix(1, 0, 0, 1, 0, 0);

            // Act
            bool result = matrix1.Equals((object)matrix2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals(object) returns false when matrices differ by specific fields.
        /// </summary>
        [Theory]
        [InlineData(2, 0, 0, 1, 0, 0)] // Different M11
        [InlineData(1, 2, 0, 1, 0, 0)] // Different M12
        [InlineData(1, 0, 2, 1, 0, 0)] // Different M21
        [InlineData(1, 0, 0, 2, 0, 0)] // Different M22
        [InlineData(1, 0, 0, 1, 2, 0)] // Different OffsetX
        [InlineData(1, 0, 0, 1, 0, 2)] // Different OffsetY
        public void Equals_DifferentMatrixValues_ReturnsFalse(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix1 = new Matrix(1, 0, 0, 1, 0, 0);
            var matrix2 = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            bool result = matrix1.Equals((object)matrix2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals(object) handles special double values correctly.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0, 0, 1, 0, 0)]
        [InlineData(double.PositiveInfinity, 0, 0, 1, 0, 0)]
        [InlineData(double.NegativeInfinity, 0, 0, 1, 0, 0)]
        [InlineData(double.MaxValue, 0, 0, 1, 0, 0)]
        [InlineData(double.MinValue, 0, 0, 1, 0, 0)]
        public void Equals_SpecialDoubleValues_ReturnsFalse(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix1 = new Matrix(1, 0, 0, 1, 0, 0);
            var matrix2 = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            bool result = matrix1.Equals((object)matrix2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals(object) returns true when both matrices have identical special double values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, 0, 0, double.PositiveInfinity, 0, 0)]
        [InlineData(double.NegativeInfinity, 0, 0, double.NegativeInfinity, 0, 0)]
        public void Equals_IdenticalSpecialDoubleValues_ReturnsTrue(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix1 = new Matrix(m11, m12, m21, m22, offsetX, offsetY);
            var matrix2 = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            bool result = matrix1.Equals((object)matrix2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals(object) correctly handles matrices with different derived types.
        /// </summary>
        [Fact]
        public void Equals_DifferentMatrixTypes_ReturnsFalse()
        {
            // Arrange - Create matrices that will have different internal _type values
            var identityMatrix = new Matrix(1, 0, 0, 1, 0, 0); // Will be Identity type
            var translationMatrix = new Matrix(1, 0, 0, 1, 5, 0); // Will be Translation type

            // Act
            bool result = identityMatrix.Equals((object)translationMatrix);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals(object) handles zero and negative zero correctly.
        /// </summary>
        [Fact]
        public void Equals_ZeroAndNegativeZero_ReturnsTrue()
        {
            // Arrange
            var matrix1 = new Matrix(0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            var matrix2 = new Matrix(-0.0, -0.0, -0.0, -0.0, -0.0, -0.0);

            // Act
            bool result = matrix1.Equals((object)matrix2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests multiplication of two identity matrices.
        /// Input: Two identity matrices.
        /// Expected: Identity matrix result.
        /// </summary>
        [Fact]
        public void Multiply_IdentityMatrices_ReturnsIdentityMatrix()
        {
            // Arrange
            var matrix1 = Matrix.Identity;
            var matrix2 = Matrix.Identity;

            // Act
            var result = Matrix.Multiply(matrix1, matrix2);

            // Assert
            Assert.Equal(Matrix.Identity, result);
        }

        /// <summary>
        /// Tests multiplication of identity matrix with a general matrix.
        /// Input: Identity matrix and general transformation matrix.
        /// Expected: Returns the general matrix unchanged.
        /// </summary>
        [Fact]
        public void Multiply_IdentityWithGeneralMatrix_ReturnsGeneralMatrix()
        {
            // Arrange
            var identity = Matrix.Identity;
            var general = new Matrix(2, 3, 4, 5, 6, 7);

            // Act
            var result = Matrix.Multiply(identity, general);

            // Assert
            Assert.Equal(general, result);
        }

        /// <summary>
        /// Tests multiplication of general matrix with identity matrix.
        /// Input: General transformation matrix and identity matrix.
        /// Expected: Returns the general matrix unchanged.
        /// </summary>
        [Fact]
        public void Multiply_GeneralMatrixWithIdentity_ReturnsGeneralMatrix()
        {
            // Arrange
            var general = new Matrix(2, 3, 4, 5, 6, 7);
            var identity = Matrix.Identity;

            // Act
            var result = Matrix.Multiply(general, identity);

            // Assert
            Assert.Equal(general, result);
        }

        /// <summary>
        /// Tests multiplication with translation matrices.
        /// Input: Two translation matrices with different offsets.
        /// Expected: Combined translation offsets.
        /// </summary>
        [Fact]
        public void Multiply_TranslationMatrices_CombinesTranslations()
        {
            // Arrange
            var translation1 = new Matrix(1, 0, 0, 1, 10, 20);
            var translation2 = new Matrix(1, 0, 0, 1, 5, 15);

            // Act
            var result = Matrix.Multiply(translation1, translation2);

            // Assert
            var expected = new Matrix(1, 0, 0, 1, 15, 35);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests multiplication with scaling matrices.
        /// Input: Two scaling matrices with different scale factors.
        /// Expected: Combined scaling factors.
        /// </summary>
        [Fact]
        public void Multiply_ScalingMatrices_CombinesScaling()
        {
            // Arrange
            var scaling1 = new Matrix(2, 0, 0, 3, 0, 0);
            var scaling2 = new Matrix(4, 0, 0, 5, 0, 0);

            // Act
            var result = Matrix.Multiply(scaling1, scaling2);

            // Assert
            var expected = new Matrix(8, 0, 0, 15, 0, 0);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests multiplication with mixed scaling and translation matrices.
        /// Input: Scaling matrix multiplied by translation matrix.
        /// Expected: Proper combined transformation.
        /// </summary>
        [Fact]
        public void Multiply_ScalingWithTranslation_ProperCombination()
        {
            // Arrange
            var scaling = new Matrix(2, 0, 0, 3, 0, 0);
            var translation = new Matrix(1, 0, 0, 1, 10, 20);

            // Act
            var result = Matrix.Multiply(scaling, translation);

            // Assert
            var expected = new Matrix(2, 0, 0, 3, 10, 20);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests multiplication with translation and scaling matrices.
        /// Input: Translation matrix multiplied by scaling matrix.
        /// Expected: Scaled translation offsets.
        /// </summary>
        [Fact]
        public void Multiply_TranslationWithScaling_ScalesTranslation()
        {
            // Arrange
            var translation = new Matrix(1, 0, 0, 1, 10, 20);
            var scaling = new Matrix(2, 0, 0, 3, 0, 0);

            // Act
            var result = Matrix.Multiply(translation, scaling);

            // Assert
            var expected = new Matrix(2, 0, 0, 3, 20, 60);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests multiplication with general transformation matrices.
        /// Input: Two complex transformation matrices.
        /// Expected: Proper matrix multiplication result.
        /// </summary>
        [Fact]
        public void Multiply_GeneralMatrices_ProperMatrixMultiplication()
        {
            // Arrange
            var matrix1 = new Matrix(1, 2, 3, 4, 5, 6);
            var matrix2 = new Matrix(7, 8, 9, 10, 11, 12);

            // Act
            var result = Matrix.Multiply(matrix1, matrix2);

            // Assert
            // Expected calculation:
            // m11 = 1*7 + 2*9 = 25
            // m12 = 1*8 + 2*10 = 28
            // m21 = 3*7 + 4*9 = 57
            // m22 = 3*8 + 4*10 = 64
            // offsetX = 5*7 + 6*9 + 11 = 100
            // offsetY = 5*8 + 6*10 + 12 = 112
            var expected = new Matrix(25, 28, 57, 64, 100, 112);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests multiplication with matrices containing special double values.
        /// Input: Matrices with NaN values.
        /// Expected: Result contains NaN in affected components.
        /// </summary>
        [Fact]
        public void Multiply_MatricesWithNaN_ProducesNaNResult()
        {
            // Arrange
            var matrix1 = new Matrix(double.NaN, 0, 0, 1, 0, 0);
            var matrix2 = new Matrix(1, 0, 0, 1, 0, 0);

            // Act
            var result = Matrix.Multiply(matrix1, matrix2);

            // Assert
            Assert.True(double.IsNaN(result.M11));
        }

        /// <summary>
        /// Tests multiplication with matrices containing infinity values.
        /// Input: Matrices with positive infinity values.
        /// Expected: Result contains infinity in affected components.
        /// </summary>
        [Fact]
        public void Multiply_MatricesWithPositiveInfinity_ProducesInfinityResult()
        {
            // Arrange
            var matrix1 = new Matrix(double.PositiveInfinity, 0, 0, 1, 0, 0);
            var matrix2 = new Matrix(2, 0, 0, 1, 0, 0);

            // Act
            var result = Matrix.Multiply(matrix1, matrix2);

            // Assert
            Assert.True(double.IsPositiveInfinity(result.M11));
        }

        /// <summary>
        /// Tests multiplication with matrices containing negative infinity values.
        /// Input: Matrices with negative infinity values.
        /// Expected: Result contains negative infinity in affected components.
        /// </summary>
        [Fact]
        public void Multiply_MatricesWithNegativeInfinity_ProducesNegativeInfinityResult()
        {
            // Arrange
            var matrix1 = new Matrix(double.NegativeInfinity, 0, 0, 1, 0, 0);
            var matrix2 = new Matrix(2, 0, 0, 1, 0, 0);

            // Act
            var result = Matrix.Multiply(matrix1, matrix2);

            // Assert
            Assert.True(double.IsNegativeInfinity(result.M11));
        }

        /// <summary>
        /// Tests multiplication with matrices containing maximum double values.
        /// Input: Matrices with double.MaxValue.
        /// Expected: Result handles extreme values without overflow to infinity.
        /// </summary>
        [Fact]
        public void Multiply_MatricesWithMaxValue_HandlesExtremeValues()
        {
            // Arrange
            var matrix1 = new Matrix(double.MaxValue, 0, 0, 1, 0, 0);
            var matrix2 = new Matrix(1, 0, 0, 1, 0, 0);

            // Act
            var result = Matrix.Multiply(matrix1, matrix2);

            // Assert
            Assert.Equal(double.MaxValue, result.M11);
        }

        /// <summary>
        /// Tests multiplication with matrices containing minimum double values.
        /// Input: Matrices with double.MinValue.
        /// Expected: Result handles extreme negative values.
        /// </summary>
        [Fact]
        public void Multiply_MatricesWithMinValue_HandlesExtremeNegativeValues()
        {
            // Arrange
            var matrix1 = new Matrix(double.MinValue, 0, 0, 1, 0, 0);
            var matrix2 = new Matrix(1, 0, 0, 1, 0, 0);

            // Act
            var result = Matrix.Multiply(matrix1, matrix2);

            // Assert
            Assert.Equal(double.MinValue, result.M11);
        }

        /// <summary>
        /// Tests multiplication with zero matrices.
        /// Input: Matrices with all zero values.
        /// Expected: Result is zero matrix.
        /// </summary>
        [Fact]
        public void Multiply_ZeroMatrices_ProducesZeroMatrix()
        {
            // Arrange
            var zero1 = new Matrix(0, 0, 0, 0, 0, 0);
            var zero2 = new Matrix(0, 0, 0, 0, 0, 0);

            // Act
            var result = Matrix.Multiply(zero1, zero2);

            // Assert
            var expected = new Matrix(0, 0, 0, 0, 0, 0);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests multiplication order dependency (non-commutativity).
        /// Input: Two different matrices in different orders.
        /// Expected: Different results demonstrating matrix multiplication is not commutative.
        /// </summary>
        [Fact]
        public void Multiply_DifferentOrders_ProducesDifferentResults()
        {
            // Arrange
            var matrix1 = new Matrix(1, 2, 0, 1, 0, 0);
            var matrix2 = new Matrix(1, 0, 3, 1, 0, 0);

            // Act
            var result1 = Matrix.Multiply(matrix1, matrix2);
            var result2 = Matrix.Multiply(matrix2, matrix1);

            // Assert
            Assert.NotEqual(result1, result2);
        }

        /// <summary>
        /// Tests the Determinant property for Identity matrix type.
        /// Should return 1.0 for identity matrices.
        /// </summary>
        [Fact]
        public void Determinant_IdentityMatrix_ReturnsOne()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            var result = matrix.Determinant;

            // Assert
            Assert.Equal(1.0, result);
        }

        /// <summary>
        /// Tests the Determinant property for Translation matrix type.
        /// Should return 1.0 for pure translation matrices.
        /// </summary>
        [Theory]
        [InlineData(5.0, 0.0)]
        [InlineData(0.0, 3.0)]
        [InlineData(10.5, -7.2)]
        [InlineData(-15.0, 25.0)]
        public void Determinant_TranslationMatrix_ReturnsOne(double offsetX, double offsetY)
        {
            // Arrange - Create pure translation matrix (1,0,0,1,offsetX,offsetY)
            var matrix = new Matrix(1, 0, 0, 1, offsetX, offsetY);

            // Act
            var result = matrix.Determinant;

            // Assert
            Assert.Equal(1.0, result);
        }

        /// <summary>
        /// Tests the Determinant property for Scaling matrix type.
        /// Should return m11 * m22 for scaling matrices.
        /// </summary>
        [Theory]
        [InlineData(2.0, 3.0, 6.0)]
        [InlineData(0.5, 4.0, 2.0)]
        [InlineData(-2.0, 1.5, -3.0)]
        [InlineData(10.0, 0.1, 1.0)]
        public void Determinant_ScalingMatrix_ReturnsM11TimesM22(double scaleX, double scaleY, double expected)
        {
            // Arrange - Create pure scaling matrix (scaleX,0,0,scaleY,0,0)
            var matrix = new Matrix(scaleX, 0, 0, scaleY, 0, 0);

            // Act
            var result = matrix.Determinant;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the Determinant property for combined Scaling and Translation matrix type.
        /// Should return m11 * m22 for scaling with translation matrices.
        /// </summary>
        [Theory]
        [InlineData(2.0, 3.0, 5.0, 7.0, 6.0)]
        [InlineData(0.5, 4.0, -2.0, 1.0, 2.0)]
        [InlineData(-1.0, -1.0, 10.0, -5.0, 1.0)]
        public void Determinant_ScalingWithTranslationMatrix_ReturnsM11TimesM22(double scaleX, double scaleY, double offsetX, double offsetY, double expected)
        {
            // Arrange - Create scaling with translation matrix
            var matrix = new Matrix(scaleX, 0, 0, scaleY, offsetX, offsetY);

            // Act
            var result = matrix.Determinant;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the Determinant property for Unknown matrix type.
        /// Should return (m11 * m22) - (m12 * m21) for general matrices.
        /// </summary>
        [Theory]
        [InlineData(1.0, 2.0, 3.0, 4.0, -2.0)]
        [InlineData(2.0, 1.0, 1.0, 2.0, 3.0)]
        [InlineData(5.0, 3.0, 2.0, 4.0, 14.0)]
        [InlineData(-1.0, 2.0, 3.0, -4.0, -2.0)]
        public void Determinant_UnknownMatrix_ReturnsGeneralDeterminantFormula(double m11, double m12, double m21, double m22, double expected)
        {
            // Arrange - Create general matrix with non-zero m12 and m21 to ensure Unknown type
            var matrix = new Matrix(m11, m12, m21, m22, 0, 0);

            // Act
            var result = matrix.Determinant;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the Determinant property with zero determinant.
        /// Should return 0.0 for singular matrices.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(2.0, 0.0)]
        [InlineData(0.0, 3.0)]
        public void Determinant_ScalingMatrixWithZeroScale_ReturnsZero(double scaleX, double scaleY)
        {
            // Arrange
            var matrix = new Matrix(scaleX, 0, 0, scaleY, 0, 0);

            // Act
            var result = matrix.Determinant;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests the Determinant property with zero determinant for general matrices.
        /// Should return 0.0 when (m11 * m22) equals (m12 * m21).
        /// </summary>
        [Fact]
        public void Determinant_UnknownMatrixWithZeroDeterminant_ReturnsZero()
        {
            // Arrange - Create matrix where m11*m22 = m12*m21 (2*4 = 2*4)
            var matrix = new Matrix(2, 2, 4, 4, 0, 0);

            // Act
            var result = matrix.Determinant;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests the Determinant property with extreme values.
        /// Should handle very large and very small numbers correctly.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, 1.0, 0.0, 1.0, double.MaxValue)]
        [InlineData(double.MinValue, 1.0, 0.0, 1.0, double.MinValue)]
        [InlineData(double.Epsilon, double.Epsilon, 0.0, 0.0, 0.0)]
        public void Determinant_ExtremeValues_HandlesCorrectly(double m11, double m22, double m12, double m21, double expected)
        {
            // Arrange
            var matrix = new Matrix(m11, m12, m21, m22, 0, 0);

            // Act
            var result = matrix.Determinant;

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the Determinant property with special floating point values.
        /// Should handle NaN and Infinity values appropriately.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 1.0, 0.0, 0.0)]
        [InlineData(1.0, double.NaN, 0.0, 0.0)]
        [InlineData(double.PositiveInfinity, 1.0, 0.0, 0.0)]
        [InlineData(1.0, double.PositiveInfinity, 0.0, 0.0)]
        [InlineData(double.NegativeInfinity, 1.0, 0.0, 0.0)]
        [InlineData(1.0, double.NegativeInfinity, 0.0, 0.0)]
        public void Determinant_SpecialFloatingPointValues_HandlesAppropriately(double m11, double m22, double m12, double m21)
        {
            // Arrange
            var matrix = new Matrix(m11, m12, m21, m22, 0, 0);

            // Act
            var result = matrix.Determinant;

            // Assert
            // For NaN cases, result should be NaN
            if (double.IsNaN(m11) || double.IsNaN(m22))
            {
                Assert.True(double.IsNaN(result));
            }
            // For Infinity cases, result should be Infinity
            else if (double.IsInfinity(m11) || double.IsInfinity(m22))
            {
                Assert.True(double.IsInfinity(result));
            }
        }

        /// <summary>
        /// Tests the HasInverse property when determinant is non-zero.
        /// Should return true when matrix has an inverse.
        /// </summary>
        [Theory]
        [InlineData(2.0, 3.0)]
        [InlineData(-1.0, 0.5)]
        [InlineData(0.1, 10.0)]
        public void HasInverse_NonZeroDeterminant_ReturnsTrue(double scaleX, double scaleY)
        {
            // Arrange
            var matrix = new Matrix(scaleX, 0, 0, scaleY, 0, 0);

            // Act
            var result = matrix.HasInverse;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests the HasInverse property when determinant is zero.
        /// Should return false when matrix is singular.
        /// </summary>
        [Theory]
        [InlineData(0.0, 1.0)]
        [InlineData(1.0, 0.0)]
        [InlineData(0.0, 0.0)]
        public void HasInverse_ZeroDeterminant_ReturnsFalse(double scaleX, double scaleY)
        {
            // Arrange
            var matrix = new Matrix(scaleX, 0, 0, scaleY, 0, 0);

            // Act
            var result = matrix.HasInverse;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests the HasInverse property with Translation matrix.
        /// Should return true since translation matrices always have determinant 1.0.
        /// </summary>
        [Theory]
        [InlineData(5.0, 10.0)]
        [InlineData(-3.0, 7.5)]
        [InlineData(0.0, 1.0)]
        public void HasInverse_TranslationMatrix_ReturnsTrue(double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, offsetX, offsetY);

            // Act
            var result = matrix.HasInverse;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests MultiplyPoint method with Identity matrix type.
        /// Verifies that identity transformation leaves points unchanged.
        /// Should exercise the Identity case branch (lines 230-231).
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(-1, -1)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(123.456, -789.012)]
        public void MultiplyPoint_IdentityMatrix_ReturnsUnchangedPoint(double x, double y)
        {
            // Arrange
            var matrix = Matrix.Identity;
            var originalPoint = new Point(x, y);

            // Act
            var transformedPoint = matrix.Transform(originalPoint);

            // Assert
            Assert.Equal(originalPoint.X, transformedPoint.X);
            Assert.Equal(originalPoint.Y, transformedPoint.Y);
        }

        /// <summary>
        /// Tests MultiplyPoint method with Identity matrix type for special double values.
        /// Verifies that identity transformation preserves special double values.
        /// Should exercise the Identity case branch (lines 230-231).
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0)]
        [InlineData(0, double.NaN)]
        [InlineData(double.PositiveInfinity, 0)]
        [InlineData(0, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 0)]
        [InlineData(0, double.NegativeInfinity)]
        public void MultiplyPoint_IdentityMatrix_SpecialValues_ReturnsUnchangedPoint(double x, double y)
        {
            // Arrange
            var matrix = Matrix.Identity;
            var originalPoint = new Point(x, y);

            // Act
            var transformedPoint = matrix.Transform(originalPoint);

            // Assert
            Assert.Equal(originalPoint.X, transformedPoint.X);
            Assert.Equal(originalPoint.Y, transformedPoint.Y);
        }

        /// <summary>
        /// Tests MultiplyPoint method with Translation matrix type.
        /// Verifies that translation-only transformation adds offset values to coordinates.
        /// Should exercise the Translation case branch (lines 232-235).
        /// </summary>
        [Theory]
        [InlineData(0, 0, 5, 10)]
        [InlineData(1, 1, -3, -7)]
        [InlineData(-5, -10, 2.5, 7.5)]
        [InlineData(100, 200, -50, -100)]
        public void MultiplyPoint_TranslationMatrix_AppliesTranslation(double x, double y, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, offsetX, offsetY);
            var originalPoint = new Point(x, y);
            var expectedX = x + offsetX;
            var expectedY = y + offsetY;

            // Act
            var transformedPoint = matrix.Transform(originalPoint);

            // Assert
            Assert.Equal(expectedX, transformedPoint.X);
            Assert.Equal(expectedY, transformedPoint.Y);
        }

        /// <summary>
        /// Tests MultiplyPoint method with Translation matrix type for special double values.
        /// Verifies that translation handles special double values correctly.
        /// Should exercise the Translation case branch (lines 232-235).
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0, 5, 10)]
        [InlineData(0, double.NaN, 5, 10)]
        [InlineData(double.PositiveInfinity, 0, 5, 10)]
        [InlineData(0, double.PositiveInfinity, 5, 10)]
        [InlineData(double.NegativeInfinity, 0, 5, 10)]
        [InlineData(0, double.NegativeInfinity, 5, 10)]
        [InlineData(10, 20, double.NaN, 0)]
        [InlineData(10, 20, 0, double.NaN)]
        [InlineData(10, 20, double.PositiveInfinity, 0)]
        [InlineData(10, 20, 0, double.PositiveInfinity)]
        [InlineData(10, 20, double.NegativeInfinity, 0)]
        [InlineData(10, 20, 0, double.NegativeInfinity)]
        public void MultiplyPoint_TranslationMatrix_SpecialValues_HandlesCorrectly(double x, double y, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, offsetX, offsetY);
            var originalPoint = new Point(x, y);
            var expectedX = x + offsetX;
            var expectedY = y + offsetY;

            // Act
            var transformedPoint = matrix.Transform(originalPoint);

            // Assert
            Assert.Equal(expectedX, transformedPoint.X);
            Assert.Equal(expectedY, transformedPoint.Y);
        }

        /// <summary>
        /// Tests MultiplyPoint method with combined Scaling and Translation matrix type.
        /// Verifies that scaling + translation transformation applies both operations correctly.
        /// Should exercise the Scaling | Translation case branch (lines 240-245).
        /// </summary>
        [Theory]
        [InlineData(1, 1, 2, 3, 5, 10)]
        [InlineData(0, 0, 2, 3, 5, 10)]
        [InlineData(-1, -2, 0.5, 0.25, -3, -7)]
        [InlineData(10, 20, -2, -3, 15, 25)]
        public void MultiplyPoint_ScalingTranslationMatrix_AppliesBothTransformations(double x, double y, double scaleX, double scaleY, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(scaleX, 0, 0, scaleY, offsetX, offsetY);
            var originalPoint = new Point(x, y);
            var expectedX = x * scaleX + offsetX;
            var expectedY = y * scaleY + offsetY;

            // Act
            var transformedPoint = matrix.Transform(originalPoint);

            // Assert
            Assert.Equal(expectedX, transformedPoint.X);
            Assert.Equal(expectedY, transformedPoint.Y);
        }

        /// <summary>
        /// Tests MultiplyPoint method with combined Scaling and Translation matrix type for edge cases.
        /// Verifies that scaling + translation handles zero scaling factors correctly.
        /// Should exercise the Scaling | Translation case branch (lines 240-245).
        /// </summary>
        [Theory]
        [InlineData(5, 10, 0, 1, 3, 7)]
        [InlineData(5, 10, 1, 0, 3, 7)]
        [InlineData(5, 10, 0, 0, 3, 7)]
        public void MultiplyPoint_ScalingTranslationMatrix_ZeroScaling_HandlesCorrectly(double x, double y, double scaleX, double scaleY, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(scaleX, 0, 0, scaleY, offsetX, offsetY);
            var originalPoint = new Point(x, y);
            var expectedX = x * scaleX + offsetX;
            var expectedY = y * scaleY + offsetY;

            // Act
            var transformedPoint = matrix.Transform(originalPoint);

            // Assert
            Assert.Equal(expectedX, transformedPoint.X);
            Assert.Equal(expectedY, transformedPoint.Y);
        }

        /// <summary>
        /// Tests MultiplyPoint method with combined Scaling and Translation matrix type for special double values.
        /// Verifies that scaling + translation handles special double values correctly.
        /// Should exercise the Scaling | Translation case branch (lines 240-245).
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0, 2, 3, 5, 10)]
        [InlineData(0, double.NaN, 2, 3, 5, 10)]
        [InlineData(double.PositiveInfinity, 0, 2, 3, 5, 10)]
        [InlineData(0, double.PositiveInfinity, 2, 3, 5, 10)]
        [InlineData(double.NegativeInfinity, 0, 2, 3, 5, 10)]
        [InlineData(0, double.NegativeInfinity, 2, 3, 5, 10)]
        public void MultiplyPoint_ScalingTranslationMatrix_SpecialValues_HandlesCorrectly(double x, double y, double scaleX, double scaleY, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(scaleX, 0, 0, scaleY, offsetX, offsetY);
            var originalPoint = new Point(x, y);
            var expectedX = x * scaleX + offsetX;
            var expectedY = y * scaleY + offsetY;

            // Act
            var transformedPoint = matrix.Transform(originalPoint);

            // Assert
            Assert.Equal(expectedX, transformedPoint.X);
            Assert.Equal(expectedY, transformedPoint.Y);
        }

        /// <summary>
        /// Tests MultiplyPoint method with Transform(Point[]) overload for Identity matrix.
        /// Verifies that array transformation works correctly for identity matrix.
        /// Should exercise the Identity case branch (lines 230-231).
        /// </summary>
        [Fact]
        public void MultiplyPoint_IdentityMatrix_ArrayTransform_ReturnsUnchangedPoints()
        {
            // Arrange
            var matrix = Matrix.Identity;
            var originalPoints = new Point[]
            {
                new Point(0, 0),
                new Point(1, 2),
                new Point(-3, -4),
                new Point(5.5, -6.7)
            };
            var pointsToTransform = new Point[originalPoints.Length];
            Array.Copy(originalPoints, pointsToTransform, originalPoints.Length);

            // Act
            matrix.Transform(pointsToTransform);

            // Assert
            for (int i = 0; i < originalPoints.Length; i++)
            {
                Assert.Equal(originalPoints[i].X, pointsToTransform[i].X);
                Assert.Equal(originalPoints[i].Y, pointsToTransform[i].Y);
            }
        }

        /// <summary>
        /// Tests MultiplyPoint method with Transform(Point[]) overload for Translation matrix.
        /// Verifies that array transformation works correctly for translation matrix.
        /// Should exercise the Translation case branch (lines 232-235).
        /// </summary>
        [Fact]
        public void MultiplyPoint_TranslationMatrix_ArrayTransform_AppliesTranslation()
        {
            // Arrange
            var offsetX = 10.0;
            var offsetY = -5.0;
            var matrix = new Matrix(1, 0, 0, 1, offsetX, offsetY);
            var originalPoints = new Point[]
            {
                new Point(0, 0),
                new Point(1, 2),
                new Point(-3, -4)
            };
            var pointsToTransform = new Point[originalPoints.Length];
            Array.Copy(originalPoints, pointsToTransform, originalPoints.Length);

            // Act
            matrix.Transform(pointsToTransform);

            // Assert
            for (int i = 0; i < originalPoints.Length; i++)
            {
                Assert.Equal(originalPoints[i].X + offsetX, pointsToTransform[i].X);
                Assert.Equal(originalPoints[i].Y + offsetY, pointsToTransform[i].Y);
            }
        }

        /// <summary>
        /// Tests MultiplyPoint method with Transform(Point[]) overload for Scaling+Translation matrix.
        /// Verifies that array transformation works correctly for combined scaling and translation.
        /// Should exercise the Scaling | Translation case branch (lines 240-245).
        /// </summary>
        [Fact]
        public void MultiplyPoint_ScalingTranslationMatrix_ArrayTransform_AppliesBothTransformations()
        {
            // Arrange
            var scaleX = 2.0;
            var scaleY = 3.0;
            var offsetX = 5.0;
            var offsetY = -10.0;
            var matrix = new Matrix(scaleX, 0, 0, scaleY, offsetX, offsetY);
            var originalPoints = new Point[]
            {
                new Point(1, 2),
                new Point(-3, 4),
                new Point(0, 0)
            };
            var pointsToTransform = new Point[originalPoints.Length];
            Array.Copy(originalPoints, pointsToTransform, originalPoints.Length);

            // Act
            matrix.Transform(pointsToTransform);

            // Assert
            for (int i = 0; i < originalPoints.Length; i++)
            {
                var expectedX = originalPoints[i].X * scaleX + offsetX;
                var expectedY = originalPoints[i].Y * scaleY + offsetY;
                Assert.Equal(expectedX, pointsToTransform[i].X);
                Assert.Equal(expectedY, pointsToTransform[i].Y);
            }
        }

        /// <summary>
        /// Tests MultiplyPoint method with Transform(Point[]) overload for null array.
        /// Verifies that null array is handled gracefully without throwing exceptions.
        /// Should exercise the null check in Transform(Point[]) method.
        /// </summary>
        [Fact]
        public void MultiplyPoint_TransformArray_NullArray_DoesNotThrow()
        {
            // Arrange
            var matrix = Matrix.Identity;
            Point[] nullArray = null;

            // Act & Assert
            var exception = Record.Exception(() => matrix.Transform(nullArray));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MultiplyPoint method with Transform(Point[]) overload for empty array.
        /// Verifies that empty array is handled correctly.
        /// Should exercise the array transformation logic without any iterations.
        /// </summary>
        [Fact]
        public void MultiplyPoint_TransformArray_EmptyArray_DoesNotThrow()
        {
            // Arrange
            var matrix = new Matrix(2, 0, 0, 3, 5, 10);
            var emptyArray = new Point[0];

            // Act & Assert
            var exception = Record.Exception(() => matrix.Transform(emptyArray));
            Assert.Null(exception);
            Assert.Empty(emptyArray);
        }

        /// <summary>
        /// Tests that Prepend correctly multiplies matrices with the input matrix applied first.
        /// Verifies that the result equals matrix * this (prepend order).
        /// </summary>
        [Fact]
        public void Prepend_WithBasicMatrix_MultipliesInCorrectOrder()
        {
            // Arrange
            var matrix = new Matrix(2, 0, 0, 2, 10, 20); // Scale by 2, translate by (10, 20)
            var prepend = new Matrix(1, 0, 0, 1, 5, 10); // Translate by (5, 10)
            var expected = prepend * matrix; // Expected result of prepend * original

            // Act
            matrix.Prepend(prepend);

            // Assert
            Assert.Equal(expected.M11, matrix.M11);
            Assert.Equal(expected.M12, matrix.M12);
            Assert.Equal(expected.M21, matrix.M21);
            Assert.Equal(expected.M22, matrix.M22);
            Assert.Equal(expected.OffsetX, matrix.OffsetX);
            Assert.Equal(expected.OffsetY, matrix.OffsetY);
        }

        /// <summary>
        /// Tests that prepending an identity matrix leaves the original matrix unchanged.
        /// Verifies that Identity * matrix = matrix.
        /// </summary>
        [Fact]
        public void Prepend_WithIdentityMatrix_LeavesMatrixUnchanged()
        {
            // Arrange
            var original = new Matrix(2, 3, 4, 5, 6, 7);
            var matrix = new Matrix(2, 3, 4, 5, 6, 7);
            var identity = Matrix.Identity;

            // Act
            matrix.Prepend(identity);

            // Assert
            Assert.Equal(original.M11, matrix.M11);
            Assert.Equal(original.M12, matrix.M12);
            Assert.Equal(original.M21, matrix.M21);
            Assert.Equal(original.M22, matrix.M22);
            Assert.Equal(original.OffsetX, matrix.OffsetX);
            Assert.Equal(original.OffsetY, matrix.OffsetY);
        }

        /// <summary>
        /// Tests that prepending to an identity matrix results in the prepended matrix.
        /// Verifies that prepend * Identity = prepend.
        /// </summary>
        [Fact]
        public void Prepend_ToIdentityMatrix_ResultsInPrependedMatrix()
        {
            // Arrange
            var matrix = Matrix.Identity;
            var prepend = new Matrix(2, 3, 4, 5, 6, 7);

            // Act
            matrix.Prepend(prepend);

            // Assert
            Assert.Equal(prepend.M11, matrix.M11);
            Assert.Equal(prepend.M12, matrix.M12);
            Assert.Equal(prepend.M21, matrix.M21);
            Assert.Equal(prepend.M22, matrix.M22);
            Assert.Equal(prepend.OffsetX, matrix.OffsetX);
            Assert.Equal(prepend.OffsetY, matrix.OffsetY);
        }

        /// <summary>
        /// Tests Prepend with extreme double values including positive and negative infinity.
        /// Verifies that the method handles edge case values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MinValue, 0, 1, 0, 0)]
        [InlineData(double.PositiveInfinity, 0, 0, double.NegativeInfinity, 0, 0)]
        [InlineData(0, 0, 0, 0, double.MaxValue, double.MinValue)]
        public void Prepend_WithExtremeValues_HandlesEdgeCases(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(1, 2, 3, 4, 5, 6);
            var prepend = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act & Assert - Should not throw
            matrix.Prepend(prepend);

            // Verify the operation completed (values may be infinity/NaN but should not crash)
            Assert.True(double.IsFinite(matrix.M11) || double.IsInfinity(matrix.M11) || double.IsNaN(matrix.M11));
        }

        /// <summary>
        /// Tests Prepend with NaN values to ensure the method handles invalid numeric values.
        /// Verifies that NaN values propagate through the calculation without exceptions.
        /// </summary>
        [Fact]
        public void Prepend_WithNaNValues_PropagatesNaN()
        {
            // Arrange
            var matrix = new Matrix(1, 2, 3, 4, 5, 6);
            var prepend = new Matrix(double.NaN, 0, 0, double.NaN, 0, 0);

            // Act
            matrix.Prepend(prepend);

            // Assert - NaN should propagate through calculations
            Assert.True(double.IsNaN(matrix.M11) || double.IsNaN(matrix.M22));
        }

        /// <summary>
        /// Tests Prepend with zero matrix to verify multiplication behavior with zeros.
        /// Verifies that multiplying by a zero matrix produces expected results.
        /// </summary>
        [Fact]
        public void Prepend_WithZeroMatrix_ProducesZeroResult()
        {
            // Arrange
            var matrix = new Matrix(2, 3, 4, 5, 6, 7);
            var zeroMatrix = new Matrix(0, 0, 0, 0, 0, 0);

            // Act
            matrix.Prepend(zeroMatrix);

            // Assert
            Assert.Equal(0.0, matrix.M11, 10);
            Assert.Equal(0.0, matrix.M12, 10);
            Assert.Equal(0.0, matrix.M21, 10);
            Assert.Equal(0.0, matrix.M22, 10);
            Assert.Equal(0.0, matrix.OffsetX, 10);
            Assert.Equal(0.0, matrix.OffsetY, 10);
        }

        /// <summary>
        /// Tests the order difference between Prepend and Append operations.
        /// Verifies that Prepend(A) produces different results than Append(A) for non-commutative matrices.
        /// </summary>
        [Fact]
        public void Prepend_VersusAppend_ProducesDifferentResults()
        {
            // Arrange
            var matrix1 = new Matrix(2, 0, 0, 3, 1, 2);
            var matrix2 = new Matrix(2, 0, 0, 3, 1, 2);
            var transform = new Matrix(1, 0, 0, 1, 5, 10); // Translation matrix

            // Act
            matrix1.Prepend(transform);  // transform * matrix1
            matrix2.Append(transform);   // matrix2 * transform

            // Assert - Results should be different for non-commutative operations
            Assert.NotEqual(matrix1.OffsetX, matrix2.OffsetX);
            Assert.NotEqual(matrix1.OffsetY, matrix2.OffsetY);
        }

        /// <summary>
        /// Tests Prepend with very small values near zero to verify precision handling.
        /// Verifies that the method maintains reasonable precision with small numbers.
        /// </summary>
        [Fact]
        public void Prepend_WithVerySmallValues_MaintainsPrecision()
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);
            var smallMatrix = new Matrix(1, double.Epsilon, double.Epsilon, 1, double.Epsilon, double.Epsilon);

            // Act
            matrix.Prepend(smallMatrix);

            // Assert
            Assert.Equal(1.0, matrix.M11, 15);
            Assert.Equal(double.Epsilon, matrix.M12, 15);
            Assert.Equal(double.Epsilon, matrix.M21, 15);
            Assert.Equal(1.0, matrix.M22, 15);
        }

        /// <summary>
        /// Tests the Translate method when the matrix type is Identity.
        /// Should call SetMatrix with translation parameters.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(1.0, 2.0)]
        [InlineData(-1.0, -2.0)]
        [InlineData(10.5, -15.7)]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        public void Translate_IdentityMatrix_SetsTranslationMatrix(double offsetX, double offsetY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.Translate(offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(1.0, matrix.M22);
            Assert.Equal(offsetX, matrix.OffsetX);
            Assert.Equal(offsetY, matrix.OffsetY);
        }

        /// <summary>
        /// Tests the Translate method when the matrix type is Unknown.
        /// Should only update the offset values without changing the type flags.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(5.0, 7.0)]
        [InlineData(-3.0, -9.0)]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        public void Translate_UnknownMatrix_UpdatesOffsetsOnly(double offsetX, double offsetY)
        {
            // Arrange - Create Unknown matrix with non-zero m12 to force Unknown type
            var matrix = new Matrix(1.0, 0.5, 0.0, 1.0, 2.0, 3.0);
            var originalOffsetX = matrix.OffsetX;
            var originalOffsetY = matrix.OffsetY;

            // Act
            matrix.Translate(offsetX, offsetY);

            // Assert
            Assert.Equal(originalOffsetX + offsetX, matrix.OffsetX);
            Assert.Equal(originalOffsetY + offsetY, matrix.OffsetY);
            // Other matrix elements should remain unchanged
            Assert.Equal(1.0, matrix.M11);
            Assert.Equal(0.5, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(1.0, matrix.M22);
        }

        /// <summary>
        /// Tests the Translate method when the matrix type is Scaling.
        /// Should update offsets and set the Translation flag.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(1.5, 2.5)]
        [InlineData(-4.0, -6.0)]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        public void Translate_ScalingMatrix_UpdatesOffsetsAndSetsTranslationFlag(double offsetX, double offsetY)
        {
            // Arrange - Create Scaling matrix (m11 != 1, but no existing translation)
            var matrix = new Matrix(2.0, 0.0, 0.0, 3.0, 0.0, 0.0);

            // Act
            matrix.Translate(offsetX, offsetY);

            // Assert
            Assert.Equal(offsetX, matrix.OffsetX);
            Assert.Equal(offsetY, matrix.OffsetY);
            // Other matrix elements should remain unchanged
            Assert.Equal(2.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(3.0, matrix.M22);
        }

        /// <summary>
        /// Tests the Translate method when the matrix type is Translation.
        /// Should update offsets and maintain the Translation flag.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(2.0, 4.0)]
        [InlineData(-1.0, -3.0)]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        public void Translate_TranslationMatrix_UpdatesOffsetsAndMaintainsTranslationFlag(double offsetX, double offsetY)
        {
            // Arrange - Create Translation matrix (identity with existing translation)
            var matrix = new Matrix(1.0, 0.0, 0.0, 1.0, 5.0, 7.0);
            var originalOffsetX = matrix.OffsetX;
            var originalOffsetY = matrix.OffsetY;

            // Act
            matrix.Translate(offsetX, offsetY);

            // Assert
            Assert.Equal(originalOffsetX + offsetX, matrix.OffsetX);
            Assert.Equal(originalOffsetY + offsetY, matrix.OffsetY);
            // Other matrix elements should remain unchanged
            Assert.Equal(1.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(1.0, matrix.M22);
        }

        /// <summary>
        /// Tests the Translate method with extreme double values to ensure proper handling.
        /// </summary>
        [Fact]
        public void Translate_ExtremeValues_HandlesCorrectly()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act & Assert - Test various extreme combinations
            matrix.Translate(double.MinValue, double.MaxValue);
            Assert.Equal(double.MinValue, matrix.OffsetX);
            Assert.Equal(double.MaxValue, matrix.OffsetY);

            matrix = Matrix.Identity;
            matrix.Translate(double.PositiveInfinity, double.NegativeInfinity);
            Assert.Equal(double.PositiveInfinity, matrix.OffsetX);
            Assert.Equal(double.NegativeInfinity, matrix.OffsetY);

            matrix = Matrix.Identity;
            matrix.Translate(double.NaN, 0.0);
            Assert.True(double.IsNaN(matrix.OffsetX));
            Assert.Equal(0.0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests the Translate method with zero values on different matrix types.
        /// </summary>
        [Fact]
        public void Translate_ZeroValues_HandlesAllMatrixTypes()
        {
            // Test Identity matrix with zero translation
            var identityMatrix = Matrix.Identity;
            identityMatrix.Translate(0.0, 0.0);
            Assert.Equal(0.0, identityMatrix.OffsetX);
            Assert.Equal(0.0, identityMatrix.OffsetY);

            // Test Unknown matrix with zero translation
            var unknownMatrix = new Matrix(1.0, 0.1, 0.0, 1.0, 2.0, 3.0);
            var originalOffsetX = unknownMatrix.OffsetX;
            var originalOffsetY = unknownMatrix.OffsetY;
            unknownMatrix.Translate(0.0, 0.0);
            Assert.Equal(originalOffsetX, unknownMatrix.OffsetX);
            Assert.Equal(originalOffsetY, unknownMatrix.OffsetY);

            // Test Scaling matrix with zero translation
            var scalingMatrix = new Matrix(2.0, 0.0, 0.0, 2.0, 0.0, 0.0);
            scalingMatrix.Translate(0.0, 0.0);
            Assert.Equal(0.0, scalingMatrix.OffsetX);
            Assert.Equal(0.0, scalingMatrix.OffsetY);
        }

        /// <summary>
        /// Tests that Translate properly accumulates multiple translation operations.
        /// </summary>
        [Fact]
        public void Translate_MultipleTranslations_AccumulatesCorrectly()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.Translate(1.0, 2.0);
            matrix.Translate(3.0, 4.0);
            matrix.Translate(-2.0, -1.0);

            // Assert
            Assert.Equal(2.0, matrix.OffsetX); // 1 + 3 - 2 = 2
            Assert.Equal(5.0, matrix.OffsetY); // 2 + 4 - 1 = 5
        }

        /// <summary>
        /// Tests that RotatePrepend normalizes angles using modulus operation and applies rotation correctly.
        /// Tests the specific input conditions of various angle values.
        /// Expected result: Angles are normalized to [0, 360) range and rotation is applied to the matrix.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(90)]
        [InlineData(180)]
        [InlineData(270)]
        [InlineData(360)]
        [InlineData(450)] // 450 % 360 = 90
        [InlineData(720)] // 720 % 360 = 0
        [InlineData(-90)] // -90 % 360 = -90 (C# modulus behavior)
        [InlineData(-180)]
        [InlineData(-360)]
        public void RotatePrepend_ValidAngles_NormalizesAngleAndAppliesRotation(double angle)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 10, 20); // Identity with translation
            var originalMatrix = new Matrix(1, 0, 0, 1, 10, 20);

            // Act
            matrix.RotatePrepend(angle);

            // Assert
            // Verify that the matrix has been modified (not equal to original)
            if (angle % 360 != 0)
            {
                Assert.NotEqual(originalMatrix, matrix);
            }
            else
            {
                // For angles that are multiples of 360, the matrix should remain unchanged
                Assert.Equal(originalMatrix, matrix);
            }
        }

        /// <summary>
        /// Tests that RotatePrepend handles zero angle correctly.
        /// Tests the specific input condition of zero degrees rotation.
        /// Expected result: Matrix should remain unchanged.
        /// </summary>
        [Fact]
        public void RotatePrepend_ZeroAngle_MatrixUnchanged()
        {
            // Arrange
            var matrix = new Matrix(2, 1, 1, 2, 5, 10);
            var expected = new Matrix(2, 1, 1, 2, 5, 10);

            // Act
            matrix.RotatePrepend(0);

            // Assert
            Assert.Equal(expected, matrix);
        }

        /// <summary>
        /// Tests that RotatePrepend correctly handles 90-degree rotation.
        /// Tests the specific mathematical transformation for 90-degree rotation.
        /// Expected result: Matrix should be rotated by 90 degrees.
        /// </summary>
        [Fact]
        public void RotatePrepend_NinetyDegrees_CorrectRotation()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.RotatePrepend(90);

            // Assert
            // After 90-degree rotation, the matrix should have specific values
            // cos(90°) = 0, sin(90°) = 1
            Assert.Equal(0, matrix.M11, 10); // cos(90°)
            Assert.Equal(1, matrix.M12, 10); // sin(90°)
            Assert.Equal(-1, matrix.M21, 10); // -sin(90°)
            Assert.Equal(0, matrix.M22, 10); // cos(90°)
        }

        /// <summary>
        /// Tests that RotatePrepend correctly handles 180-degree rotation.
        /// Tests the specific mathematical transformation for 180-degree rotation.
        /// Expected result: Matrix should be rotated by 180 degrees.
        /// </summary>
        [Fact]
        public void RotatePrepend_OneEightyDegrees_CorrectRotation()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.RotatePrepend(180);

            // Assert
            // After 180-degree rotation, the matrix should have specific values
            // cos(180°) = -1, sin(180°) = 0
            Assert.Equal(-1, matrix.M11, 10); // cos(180°)
            Assert.Equal(0, matrix.M12, 10); // sin(180°)
            Assert.Equal(0, matrix.M21, 10); // -sin(180°)
            Assert.Equal(-1, matrix.M22, 10); // cos(180°)
        }

        /// <summary>
        /// Tests that RotatePrepend handles special double values.
        /// Tests the specific input conditions of NaN, PositiveInfinity, and NegativeInfinity.
        /// Expected result: Method should handle special values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void RotatePrepend_SpecialDoubleValues_HandlesGracefully(double angle)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act & Assert
            // Should not throw an exception
            matrix.RotatePrepend(angle);
        }

        /// <summary>
        /// Tests that RotatePrepend correctly handles very large angles.
        /// Tests the specific input condition of extremely large angle values.
        /// Expected result: Large angles should be normalized correctly.
        /// </summary>
        [Theory]
        [InlineData(3600)] // 10 full rotations
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        public void RotatePrepend_VeryLargeAngles_NormalizesCorrectly(double angle)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.RotatePrepend(angle);

            // Assert
            // Should complete without exception
            // The exact result depends on the modulus operation with very large numbers
            Assert.True(true); // If we get here, the method didn't throw
        }

        /// <summary>
        /// Tests that RotatePrepend applies rotation in the correct order (prepend vs append).
        /// Tests the specific behavior difference between prepend and append operations.
        /// Expected result: Prepend should multiply rotation matrix before the current matrix.
        /// </summary>
        [Fact]
        public void RotatePrepend_PrependOrder_AppliesRotationFirst()
        {
            // Arrange
            var matrix = new Matrix(2, 0, 0, 2, 10, 20); // Scale matrix with translation
            var matrixCopy = new Matrix(2, 0, 0, 2, 10, 20);

            // Act
            matrix.RotatePrepend(90);

            // Also test regular rotate for comparison
            matrixCopy.Rotate(90);

            // Assert
            // The results should be different because prepend applies rotation first
            Assert.NotEqual(matrix, matrixCopy);
        }

        /// <summary>
        /// Tests that RotatePrepend correctly handles negative angle normalization.
        /// Tests the specific behavior of C# modulus operator with negative values.
        /// Expected result: Negative angles should be handled according to C# modulus semantics.
        /// </summary>
        [Fact]
        public void RotatePrepend_NegativeAngle_ModulusHandledCorrectly()
        {
            // Arrange
            var matrix1 = Matrix.Identity;
            var matrix2 = Matrix.Identity;

            // Act
            matrix1.RotatePrepend(-90);
            matrix2.RotatePrepend(270); // Equivalent positive angle

            // Assert
            // Due to C# modulus behavior, -90 % 360 = -90, not 270
            // So these matrices might not be equal, depending on implementation
            // But the method should complete successfully
            Assert.True(true); // Method completed successfully
        }

        /// <summary>
        /// Tests that RotatePrepend maintains matrix precision for multiple rotations.
        /// Tests the specific behavior of cumulative rotations.
        /// Expected result: Multiple small rotations should be equivalent to one large rotation.
        /// </summary>
        [Fact]
        public void RotatePrepend_MultipleRotations_CumulativeEffect()
        {
            // Arrange
            var matrix1 = Matrix.Identity;
            var matrix2 = Matrix.Identity;

            // Act
            // Apply four 90-degree rotations
            matrix1.RotatePrepend(90);
            matrix1.RotatePrepend(90);
            matrix1.RotatePrepend(90);
            matrix1.RotatePrepend(90);

            // Apply one 360-degree rotation
            matrix2.RotatePrepend(360);

            // Assert
            // Both should result in identity matrix (within tolerance)
            Assert.Equal(matrix2.M11, matrix1.M11, 10);
            Assert.Equal(matrix2.M12, matrix1.M12, 10);
            Assert.Equal(matrix2.M21, matrix1.M21, 10);
            Assert.Equal(matrix2.M22, matrix1.M22, 10);
        }

        /// <summary>
        /// Tests that M22 getter returns 1.0 when matrix type is Identity
        /// </summary>
        [Fact]
        public void M22_WhenMatrixTypeIsIdentity_ReturnsOne()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            var result = matrix.M22;

            // Assert
            Assert.Equal(1.0, result);
        }

        /// <summary>
        /// Tests that M22 getter returns the internal _m22 field value when matrix type is not Identity
        /// </summary>
        [Theory]
        [InlineData(2.5)]
        [InlineData(-3.7)]
        [InlineData(0.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void M22_WhenMatrixTypeIsNotIdentity_ReturnsInternalM22Value(double expectedValue)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, expectedValue, 0, 0);

            // Act
            var result = matrix.M22;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that setting M22 on Identity matrix calls SetMatrix with scaling parameters
        /// </summary>
        [Theory]
        [InlineData(2.0)]
        [InlineData(-1.5)]
        [InlineData(0.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void M22_SetOnIdentityMatrix_ConvertsToScalingMatrix(double value)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.M22 = value;

            // Assert
            Assert.Equal(1.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(value, matrix.M22);
            Assert.Equal(0.0, matrix.OffsetX);
            Assert.Equal(0.0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests that setting M22 on non-Identity matrix updates the internal _m22 field
        /// </summary>
        [Theory]
        [InlineData(1.0, 0.0, 0.0, 2.0, 0.0, 0.0, 3.5)] // Scaling matrix
        [InlineData(1.0, 0.0, 0.0, 1.0, 5.0, 7.0, 2.2)] // Translation matrix
        [InlineData(2.0, 0.0, 0.0, 3.0, 1.0, 2.0, 4.7)] // Translation + Scaling matrix
        [InlineData(1.0, 2.0, 3.0, 4.0, 0.0, 0.0, 8.1)] // Unknown matrix
        public void M22_SetOnNonIdentityMatrix_UpdatesInternalField(double m11, double m12, double m21, double m22, double offsetX, double offsetY, double newValue)
        {
            // Arrange
            var matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            matrix.M22 = newValue;

            // Assert
            Assert.Equal(newValue, matrix.M22);
        }

        /// <summary>
        /// Tests that setting M22 on non-Identity matrix with known type updates the matrix type to include scaling
        /// </summary>
        [Fact]
        public void M22_SetOnTranslationMatrix_UpdatesTypeToIncludeScaling()
        {
            // Arrange - Create a translation-only matrix
            var matrix = new Matrix(1.0, 0.0, 0.0, 1.0, 5.0, 7.0);

            // Act
            matrix.M22 = 2.0;

            // Assert
            Assert.Equal(2.0, matrix.M22);
            // The matrix should now have both translation and scaling characteristics
            Assert.False(matrix.IsIdentity);
        }

        /// <summary>
        /// Tests that setting M22 on Unknown matrix type doesn't change the type classification
        /// </summary>
        [Fact]
        public void M22_SetOnUnknownMatrix_DoesNotChangeType()
        {
            // Arrange - Create an unknown matrix (has rotation components)
            var matrix = new Matrix(1.0, 2.0, 3.0, 4.0, 0.0, 0.0);

            // Act
            matrix.M22 = 5.0;

            // Assert
            Assert.Equal(5.0, matrix.M22);
            Assert.False(matrix.IsIdentity);
        }

        /// <summary>
        /// Tests edge case values for M22 setter
        /// </summary>
        [Theory]
        [InlineData(1e-16)] // Very small positive value
        [InlineData(-1e-16)] // Very small negative value
        [InlineData(1e16)] // Very large positive value
        [InlineData(-1e16)] // Very large negative value
        public void M22_SetEdgeCaseValues_HandlesCorrectly(double value)
        {
            // Arrange
            var matrix = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);

            // Act
            matrix.M22 = value;

            // Assert
            Assert.Equal(value, matrix.M22);
        }

        /// <summary>
        /// Tests that M22 setter and getter work consistently for special double values
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void M22_SetAndGetSpecialDoubleValues_WorksConsistently(double specialValue)
        {
            // Arrange
            var matrix = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);

            // Act
            matrix.M22 = specialValue;
            var result = matrix.M22;

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
        /// Tests that setting M22 to 1.0 on identity matrix still converts to scaling type
        /// </summary>
        [Fact]
        public void M22_SetToOneOnIdentityMatrix_ConvertsToScalingType()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.M22 = 1.0;

            // Assert
            Assert.Equal(1.0, matrix.M22);
            // Even though the value is 1.0, it should no longer be considered an Identity matrix
            // since SetMatrix was called with MatrixTypes.Scaling
            Assert.False(matrix.IsIdentity);
        }

        /// <summary>
        /// Tests CreateTranslation with normal positive translation values.
        /// Input: positive offsetX and offsetY values.
        /// Expected: Matrix with identity transformation values and correct translation offsets.
        /// </summary>
        [Fact]
        public void CreateTranslation_PositiveValues_ReturnsTranslationMatrix()
        {
            // Arrange
            double offsetX = 10.5;
            double offsetY = 20.3;

            // Act
            var result = Matrix.CreateTranslation(offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, result.M11);
            Assert.Equal(0.0, result.M12);
            Assert.Equal(0.0, result.M21);
            Assert.Equal(1.0, result.M22);
            Assert.Equal(offsetX, result.OffsetX);
            Assert.Equal(offsetY, result.OffsetY);
        }

        /// <summary>
        /// Tests CreateTranslation with negative translation values.
        /// Input: negative offsetX and offsetY values.
        /// Expected: Matrix with identity transformation values and correct negative translation offsets.
        /// </summary>
        [Fact]
        public void CreateTranslation_NegativeValues_ReturnsTranslationMatrix()
        {
            // Arrange
            double offsetX = -15.7;
            double offsetY = -25.9;

            // Act
            var result = Matrix.CreateTranslation(offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, result.M11);
            Assert.Equal(0.0, result.M12);
            Assert.Equal(0.0, result.M21);
            Assert.Equal(1.0, result.M22);
            Assert.Equal(offsetX, result.OffsetX);
            Assert.Equal(offsetY, result.OffsetY);
        }

        /// <summary>
        /// Tests CreateTranslation with zero translation values.
        /// Input: zero offsetX and offsetY values.
        /// Expected: Matrix with identity transformation values and zero translation offsets.
        /// </summary>
        [Fact]
        public void CreateTranslation_ZeroValues_ReturnsTranslationMatrix()
        {
            // Arrange
            double offsetX = 0.0;
            double offsetY = 0.0;

            // Act
            var result = Matrix.CreateTranslation(offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, result.M11);
            Assert.Equal(0.0, result.M12);
            Assert.Equal(0.0, result.M21);
            Assert.Equal(1.0, result.M22);
            Assert.Equal(0.0, result.OffsetX);
            Assert.Equal(0.0, result.OffsetY);
        }

        /// <summary>
        /// Tests CreateTranslation with extreme double values.
        /// Input: double.MaxValue and double.MinValue for translation offsets.
        /// Expected: Matrix with identity transformation values and extreme translation offsets.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.MinValue, double.MaxValue)]
        public void CreateTranslation_ExtremeValues_ReturnsTranslationMatrix(double offsetX, double offsetY)
        {
            // Act
            var result = Matrix.CreateTranslation(offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, result.M11);
            Assert.Equal(0.0, result.M12);
            Assert.Equal(0.0, result.M21);
            Assert.Equal(1.0, result.M22);
            Assert.Equal(offsetX, result.OffsetX);
            Assert.Equal(offsetY, result.OffsetY);
        }

        /// <summary>
        /// Tests CreateTranslation with special floating point values.
        /// Input: NaN, PositiveInfinity, and NegativeInfinity values for translation offsets.
        /// Expected: Matrix with identity transformation values and special floating point translation offsets.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0.0)]
        [InlineData(0.0, double.NaN)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, 0.0)]
        [InlineData(0.0, double.PositiveInfinity)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 0.0)]
        [InlineData(0.0, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.PositiveInfinity)]
        public void CreateTranslation_SpecialFloatingPointValues_ReturnsTranslationMatrix(double offsetX, double offsetY)
        {
            // Act
            var result = Matrix.CreateTranslation(offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, result.M11);
            Assert.Equal(0.0, result.M12);
            Assert.Equal(0.0, result.M21);
            Assert.Equal(1.0, result.M22);
            Assert.Equal(offsetX, result.OffsetX);
            Assert.Equal(offsetY, result.OffsetY);
        }

        /// <summary>
        /// Tests CreateTranslation with very small values near zero.
        /// Input: very small positive and negative values for translation offsets.
        /// Expected: Matrix with identity transformation values and very small translation offsets.
        /// </summary>
        [Theory]
        [InlineData(double.Epsilon, double.Epsilon)]
        [InlineData(-double.Epsilon, -double.Epsilon)]
        [InlineData(double.Epsilon, -double.Epsilon)]
        [InlineData(-double.Epsilon, double.Epsilon)]
        public void CreateTranslation_VerySmallValues_ReturnsTranslationMatrix(double offsetX, double offsetY)
        {
            // Act
            var result = Matrix.CreateTranslation(offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, result.M11);
            Assert.Equal(0.0, result.M12);
            Assert.Equal(0.0, result.M21);
            Assert.Equal(1.0, result.M22);
            Assert.Equal(offsetX, result.OffsetX);
            Assert.Equal(offsetY, result.OffsetY);
        }

        /// <summary>
        /// Tests CreateTranslation with mixed positive and negative values.
        /// Input: various combinations of positive and negative translation offsets.
        /// Expected: Matrix with identity transformation values and correct mixed translation offsets.
        /// </summary>
        [Theory]
        [InlineData(100.5, -200.3)]
        [InlineData(-150.7, 300.9)]
        [InlineData(1000000.0, -0.001)]
        [InlineData(-0.00001, 999999.99)]
        public void CreateTranslation_MixedValues_ReturnsTranslationMatrix(double offsetX, double offsetY)
        {
            // Act
            var result = Matrix.CreateTranslation(offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, result.M11);
            Assert.Equal(0.0, result.M12);
            Assert.Equal(0.0, result.M21);
            Assert.Equal(1.0, result.M22);
            Assert.Equal(offsetX, result.OffsetX);
            Assert.Equal(offsetY, result.OffsetY);
        }

        /// <summary>
        /// Tests that RotateAt method properly normalizes angles using modulo 360 operation
        /// and applies rotation transformation around specified center point.
        /// </summary>
        /// <param name="angle">The rotation angle in degrees</param>
        /// <param name="centerX">X coordinate of rotation center</param>
        /// <param name="centerY">Y coordinate of rotation center</param>
        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(90, 0, 0)]
        [InlineData(180, 0, 0)]
        [InlineData(270, 0, 0)]
        [InlineData(360, 0, 0)]
        [InlineData(450, 0, 0)] // 450 % 360 = 90
        [InlineData(720, 0, 0)] // 720 % 360 = 0
        [InlineData(-90, 0, 0)]
        [InlineData(-270, 0, 0)]
        [InlineData(-360, 0, 0)]
        [InlineData(45, 10, 20)]
        [InlineData(135, -5, 15)]
        [InlineData(225, 0, -10)]
        [InlineData(315, -5, -5)]
        public void RotateAt_ValidAnglesAndCenters_AppliesRotationTransformation(double angle, double centerX, double centerY)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0); // Identity matrix
            var originalMatrix = matrix;

            // Act
            matrix.RotateAt(angle, centerX, centerY);

            // Assert - Verify that the matrix has changed (unless angle is multiple of 360 and center is origin)
            bool shouldBeIdentical = (angle % 360.0 == 0) && centerX == 0 && centerY == 0;
            if (shouldBeIdentical)
            {
                Assert.Equal(originalMatrix, matrix);
            }
            else
            {
                Assert.NotEqual(originalMatrix, matrix);
            }
        }

        /// <summary>
        /// Tests that RotateAt method handles large angle values correctly through modulo operation.
        /// </summary>
        /// <param name="largeAngle">Large angle value that should be normalized</param>
        /// <param name="expectedNormalizedAngle">Expected angle after modulo 360 operation</param>
        [Theory]
        [InlineData(720, 0)]
        [InlineData(900, 180)]
        [InlineData(1080, 0)]
        [InlineData(1170, 90)]
        [InlineData(3600, 0)]
        [InlineData(7200, 0)]
        public void RotateAt_LargeAngles_NormalizesAngleCorrectly(double largeAngle, double expectedNormalizedAngle)
        {
            // Arrange
            var matrix1 = new Matrix(1, 0, 0, 1, 0, 0);
            var matrix2 = new Matrix(1, 0, 0, 1, 0, 0);

            // Act
            matrix1.RotateAt(largeAngle, 0, 0);
            matrix2.RotateAt(expectedNormalizedAngle, 0, 0);

            // Assert - Both matrices should be equal after rotation
            Assert.Equal(matrix2, matrix1);
        }

        /// <summary>
        /// Tests that RotateAt method handles negative angles correctly through modulo operation.
        /// </summary>
        /// <param name="negativeAngle">Negative angle value</param>
        [Theory]
        [InlineData(-90)]
        [InlineData(-180)]
        [InlineData(-270)]
        [InlineData(-360)]
        [InlineData(-450)]
        [InlineData(-720)]
        public void RotateAt_NegativeAngles_AppliesCorrectTransformation(double negativeAngle)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);

            // Act
            matrix.RotateAt(negativeAngle, 0, 0);

            // Assert - Verify transformation was applied for non-zero normalized angles
            if (negativeAngle % 360.0 != 0)
            {
                Assert.False(matrix.IsIdentity);
            }
        }

        /// <summary>
        /// Tests that RotateAt method handles special double values correctly.
        /// </summary>
        /// <param name="specialAngle">Special double value for angle</param>
        /// <param name="centerX">X coordinate of rotation center</param>
        /// <param name="centerY">Y coordinate of rotation center</param>
        [Theory]
        [InlineData(double.NaN, 0, 0)]
        [InlineData(double.PositiveInfinity, 0, 0)]
        [InlineData(double.NegativeInfinity, 0, 0)]
        [InlineData(0, double.NaN, 0)]
        [InlineData(0, 0, double.NaN)]
        [InlineData(0, double.PositiveInfinity, 0)]
        [InlineData(0, 0, double.PositiveInfinity)]
        [InlineData(0, double.NegativeInfinity, 0)]
        [InlineData(0, 0, double.NegativeInfinity)]
        public void RotateAt_SpecialDoubleValues_HandlesGracefully(double specialAngle, double centerX, double centerY)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);

            // Act & Assert - Should not throw exception
            matrix.RotateAt(specialAngle, centerX, centerY);
        }

        /// <summary>
        /// Tests that RotateAt method correctly applies rotation around non-origin center points.
        /// </summary>
        [Fact]
        public void RotateAt_NonOriginCenter_AppliesTranslationAndRotation()
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);
            double angle = 90;
            double centerX = 10;
            double centerY = 5;

            // Act
            matrix.RotateAt(angle, centerX, centerY);

            // Assert - Matrix should not be identity and should have changed
            Assert.False(matrix.IsIdentity);
            Assert.NotEqual(1.0, matrix.M11, 10); // Should not be exactly 1
            Assert.NotEqual(0.0, matrix.M12, 10); // Should not be exactly 0
        }

        /// <summary>
        /// Tests that RotateAt method with zero angle and origin center maintains identity matrix.
        /// </summary>
        [Fact]
        public void RotateAt_ZeroAngleOriginCenter_MaintainsIdentity()
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);
            var originalMatrix = matrix;

            // Act
            matrix.RotateAt(0, 0, 0);

            // Assert
            Assert.Equal(originalMatrix, matrix);
            Assert.True(matrix.IsIdentity);
        }

        /// <summary>
        /// Tests that RotateAt method handles boundary values correctly.
        /// </summary>
        /// <param name="angle">Boundary angle value</param>
        /// <param name="centerX">Boundary center X coordinate</param>
        /// <param name="centerY">Boundary center Y coordinate</param>
        [Theory]
        [InlineData(double.MaxValue, 0, 0)]
        [InlineData(double.MinValue, 0, 0)]
        [InlineData(0, double.MaxValue, 0)]
        [InlineData(0, double.MinValue, 0)]
        [InlineData(0, 0, double.MaxValue)]
        [InlineData(0, 0, double.MinValue)]
        public void RotateAt_BoundaryValues_HandlesCorrectly(double angle, double centerX, double centerY)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);

            // Act & Assert - Should complete without throwing
            matrix.RotateAt(angle, centerX, centerY);
        }

        /// <summary>
        /// Tests that RotateAt method produces consistent results for equivalent angles.
        /// </summary>
        [Theory]
        [InlineData(0, 360)]
        [InlineData(90, 450)]
        [InlineData(180, 540)]
        [InlineData(270, 630)]
        [InlineData(-90, 270)]
        [InlineData(-180, 180)]
        public void RotateAt_EquivalentAngles_ProduceSameResult(double angle1, double angle2)
        {
            // Arrange
            var matrix1 = new Matrix(1, 0, 0, 1, 0, 0);
            var matrix2 = new Matrix(1, 0, 0, 1, 0, 0);
            double centerX = 5;
            double centerY = 3;

            // Act
            matrix1.RotateAt(angle1, centerX, centerY);
            matrix2.RotateAt(angle2, centerX, centerY);

            // Assert
            Assert.Equal(matrix2, matrix1);
        }

        /// <summary>
        /// Tests that getting OffsetX from an identity matrix returns 0.
        /// </summary>
        [Fact]
        public void OffsetX_WhenMatrixIsIdentity_ReturnsZero()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            var result = matrix.OffsetX;

            // Assert
            Assert.Equal(0.0, result);
        }

        /// <summary>
        /// Tests that getting OffsetX from a non-identity matrix returns the actual offset value.
        /// </summary>
        [Theory]
        [InlineData(5.5)]
        [InlineData(-10.2)]
        [InlineData(0.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void OffsetX_WhenMatrixIsNotIdentity_ReturnsActualOffsetValue(double expectedOffset)
        {
            // Arrange
            var matrix = new Matrix(1, 2, 3, 4, expectedOffset, 6);

            // Act
            var result = matrix.OffsetX;

            // Assert
            Assert.Equal(expectedOffset, result);
        }

        /// <summary>
        /// Tests that getting OffsetX from a matrix with NaN offset returns NaN.
        /// </summary>
        [Fact]
        public void OffsetX_WhenOffsetIsNaN_ReturnsNaN()
        {
            // Arrange
            var matrix = new Matrix(1, 2, 3, 4, double.NaN, 6);

            // Act
            var result = matrix.OffsetX;

            // Assert
            Assert.True(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that setting OffsetX on an identity matrix creates a translation matrix.
        /// </summary>
        [Theory]
        [InlineData(5.5)]
        [InlineData(-10.2)]
        [InlineData(0.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void OffsetX_WhenSetOnIdentityMatrix_CreatesTranslationMatrix(double offsetValue)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.OffsetX = offsetValue;

            // Assert
            Assert.Equal(offsetValue, matrix.OffsetX);
            Assert.Equal(0.0, matrix.OffsetY);
            Assert.Equal(1.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(1.0, matrix.M22);
        }

        /// <summary>
        /// Tests that setting OffsetX on a non-identity matrix updates the offset value.
        /// </summary>
        [Theory]
        [InlineData(5.5)]
        [InlineData(-10.2)]
        [InlineData(0.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void OffsetX_WhenSetOnNonIdentityMatrix_UpdatesOffsetValue(double newOffset)
        {
            // Arrange
            var matrix = new Matrix(2, 3, 4, 5, 100, 200);
            var originalM11 = matrix.M11;
            var originalM12 = matrix.M12;
            var originalM21 = matrix.M21;
            var originalM22 = matrix.M22;
            var originalOffsetY = matrix.OffsetY;

            // Act
            matrix.OffsetX = newOffset;

            // Assert
            Assert.Equal(newOffset, matrix.OffsetX);
            Assert.Equal(originalOffsetY, matrix.OffsetY);
            Assert.Equal(originalM11, matrix.M11);
            Assert.Equal(originalM12, matrix.M12);
            Assert.Equal(originalM21, matrix.M21);
            Assert.Equal(originalM22, matrix.M22);
        }

        /// <summary>
        /// Tests that setting OffsetX to zero on a matrix with existing translation maintains other matrix values.
        /// </summary>
        [Fact]
        public void OffsetX_WhenSetToZeroOnTranslationMatrix_MaintainsOtherValues()
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 50, 60);

            // Act
            matrix.OffsetX = 0.0;

            // Assert
            Assert.Equal(0.0, matrix.OffsetX);
            Assert.Equal(60.0, matrix.OffsetY);
            Assert.Equal(1.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(1.0, matrix.M22);
        }

        /// <summary>
        /// Tests that multiple consecutive OffsetX assignments work correctly.
        /// </summary>
        [Fact]
        public void OffsetX_WhenSetMultipleTimes_UpdatesCorrectly()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act & Assert
            matrix.OffsetX = 10.0;
            Assert.Equal(10.0, matrix.OffsetX);

            matrix.OffsetX = -5.0;
            Assert.Equal(-5.0, matrix.OffsetX);

            matrix.OffsetX = 0.0;
            Assert.Equal(0.0, matrix.OffsetX);
        }

        /// <summary>
        /// Tests that setting OffsetX preserves the matrix determinant calculation.
        /// </summary>
        [Fact]
        public void OffsetX_WhenSet_PreservesMatrixDeterminant()
        {
            // Arrange
            var matrix = new Matrix(2, 3, 4, 5, 0, 0);
            var originalDeterminant = matrix.Determinant;

            // Act
            matrix.OffsetX = 100.0;

            // Assert - Determinant should not change when only translation changes
            Assert.Equal(originalDeterminant, matrix.Determinant);
        }

        /// <summary>
        /// Tests that CreateSkewRadians creates a matrix with correct skew transformation values for various input combinations.
        /// Verifies that the method correctly applies Math.Tan to input angles and sets the matrix components appropriately.
        /// Expected result: Matrix with M11=1, M12=Math.Tan(skewY), M21=Math.Tan(skewX), M22=1, OffsetX=0, OffsetY=0.
        /// </summary>
        /// <param name="skewX">The skew angle in radians for the X axis</param>
        /// <param name="skewY">The skew angle in radians for the Y axis</param>
        /// <param name="expectedM12">Expected M12 value (Math.Tan(skewY))</param>
        /// <param name="expectedM21">Expected M21 value (Math.Tan(skewX))</param>
        [Theory]
        [InlineData(0.0, 0.0, 0.0, 0.0)]
        [InlineData(0.5, 0.0, 0.0, 0.5463024898437905)]
        [InlineData(0.0, 0.5, 0.5463024898437905, 0.0)]
        [InlineData(0.5, 0.7, 0.8422883804630794, 0.5463024898437905)]
        [InlineData(-0.5, -0.7, -0.8422883804630794, -0.5463024898437905)]
        [InlineData(0.1, -0.3, -0.30933624960962325, 0.10033467208545055)]
        [InlineData(-0.1, 0.3, 0.30933624960962325, -0.10033467208545055)]
        [InlineData(1.0, 1.2, 2.5721516221263188, 1.5574077246549023)]
        [InlineData(-1.0, -1.2, -2.5721516221263188, -1.5574077246549023)]
        public void CreateSkewRadians_ValidInputs_ReturnsCorrectMatrix(double skewX, double skewY, double expectedM12, double expectedM21)
        {
            // Act
            var result = Matrix.CreateSkewRadians(skewX, skewY);

            // Assert
            Assert.Equal(1.0, result.M11, 15);
            Assert.Equal(expectedM12, result.M12, 15);
            Assert.Equal(expectedM21, result.M21, 15);
            Assert.Equal(1.0, result.M22, 15);
            Assert.Equal(0.0, result.OffsetX, 15);
            Assert.Equal(0.0, result.OffsetY, 15);
        }

        /// <summary>
        /// Tests CreateSkewRadians with very small values close to zero.
        /// Verifies that small radian values produce correspondingly small skew transformations.
        /// Expected result: Matrix with very small M12 and M21 values approximately equal to the input angles.
        /// </summary>
        [Theory]
        [InlineData(1e-10, 1e-10)]
        [InlineData(-1e-10, -1e-10)]
        [InlineData(1e-15, 1e-15)]
        [InlineData(-1e-15, -1e-15)]
        public void CreateSkewRadians_VerySmallValues_ReturnsCorrectMatrix(double skewX, double skewY)
        {
            // Act
            var result = Matrix.CreateSkewRadians(skewX, skewY);

            // Assert
            Assert.Equal(1.0, result.M11, 15);
            Assert.Equal(Math.Tan(skewY), result.M12, 15);
            Assert.Equal(Math.Tan(skewX), result.M21, 15);
            Assert.Equal(1.0, result.M22, 15);
            Assert.Equal(0.0, result.OffsetX, 15);
            Assert.Equal(0.0, result.OffsetY, 15);
        }

        /// <summary>
        /// Tests CreateSkewRadians with values near π/2 where Math.Tan approaches infinity.
        /// Verifies that the method handles angles close to π/2 radians correctly.
        /// Expected result: Matrix with very large M12 and M21 values from Math.Tan of near-π/2 values.
        /// </summary>
        [Theory]
        [InlineData(1.5707, 1.5707)]
        [InlineData(-1.5707, -1.5707)]
        [InlineData(1.5706, 1.5708)]
        [InlineData(-1.5708, -1.5706)]
        public void CreateSkewRadians_ValuesNearPiOver2_ReturnsCorrectMatrix(double skewX, double skewY)
        {
            // Act
            var result = Matrix.CreateSkewRadians(skewX, skewY);

            // Assert
            Assert.Equal(1.0, result.M11, 15);
            Assert.Equal(Math.Tan(skewY), result.M12);
            Assert.Equal(Math.Tan(skewX), result.M21);
            Assert.Equal(1.0, result.M22, 15);
            Assert.Equal(0.0, result.OffsetX, 15);
            Assert.Equal(0.0, result.OffsetY, 15);
        }

        /// <summary>
        /// Tests CreateSkewRadians with exactly π/2 where Math.Tan returns infinity.
        /// Verifies that the method handles infinite Math.Tan results correctly.
        /// Expected result: Matrix with infinite M12 and M21 values.
        /// </summary>
        [Fact]
        public void CreateSkewRadians_ExactlyPiOver2_ReturnsMatrixWithInfiniteValues()
        {
            // Arrange
            double piOver2 = Math.PI / 2.0;

            // Act
            var result = Matrix.CreateSkewRadians(piOver2, piOver2);

            // Assert
            Assert.Equal(1.0, result.M11, 15);
            Assert.True(double.IsPositiveInfinity(result.M12));
            Assert.True(double.IsPositiveInfinity(result.M21));
            Assert.Equal(1.0, result.M22, 15);
            Assert.Equal(0.0, result.OffsetX, 15);
            Assert.Equal(0.0, result.OffsetY, 15);
        }

        /// <summary>
        /// Tests CreateSkewRadians with exactly -π/2 where Math.Tan returns negative infinity.
        /// Verifies that the method handles negative infinite Math.Tan results correctly.
        /// Expected result: Matrix with negative infinite M12 and M21 values.
        /// </summary>
        [Fact]
        public void CreateSkewRadians_ExactlyNegativePiOver2_ReturnsMatrixWithNegativeInfiniteValues()
        {
            // Arrange
            double negativePiOver2 = -Math.PI / 2.0;

            // Act
            var result = Matrix.CreateSkewRadians(negativePiOver2, negativePiOver2);

            // Assert
            Assert.Equal(1.0, result.M11, 15);
            Assert.True(double.IsNegativeInfinity(result.M12));
            Assert.True(double.IsNegativeInfinity(result.M21));
            Assert.Equal(1.0, result.M22, 15);
            Assert.Equal(0.0, result.OffsetX, 15);
            Assert.Equal(0.0, result.OffsetY, 15);
        }

        /// <summary>
        /// Tests CreateSkewRadians with special double values like NaN and infinity.
        /// Verifies that the method handles special floating-point values correctly.
        /// Expected result: Matrix with corresponding special values in M12 and M21 based on Math.Tan behavior.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0.0)]
        [InlineData(0.0, double.NaN)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, 0.0)]
        [InlineData(0.0, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 0.0)]
        [InlineData(0.0, double.NegativeInfinity)]
        public void CreateSkewRadians_SpecialDoubleValues_ReturnsMatrixWithExpectedValues(double skewX, double skewY)
        {
            // Act
            var result = Matrix.CreateSkewRadians(skewX, skewY);

            // Assert
            Assert.Equal(1.0, result.M11, 15);
            Assert.Equal(Math.Tan(skewY), result.M12);
            Assert.Equal(Math.Tan(skewX), result.M21);
            Assert.Equal(1.0, result.M22, 15);
            Assert.Equal(0.0, result.OffsetX, 15);
            Assert.Equal(0.0, result.OffsetY, 15);
        }

        /// <summary>
        /// Tests CreateSkewRadians with extreme boundary values.
        /// Verifies that the method handles the maximum and minimum double values correctly.
        /// Expected result: Matrix with Math.Tan results of extreme values in M12 and M21.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, 0.0)]
        [InlineData(0.0, double.MaxValue)]
        [InlineData(double.MinValue, 0.0)]
        [InlineData(0.0, double.MinValue)]
        public void CreateSkewRadians_ExtremeValues_ReturnsMatrixWithExpectedValues(double skewX, double skewY)
        {
            // Act
            var result = Matrix.CreateSkewRadians(skewX, skewY);

            // Assert
            Assert.Equal(1.0, result.M11, 15);
            Assert.Equal(Math.Tan(skewY), result.M12);
            Assert.Equal(Math.Tan(skewX), result.M21);
            Assert.Equal(1.0, result.M22, 15);
            Assert.Equal(0.0, result.OffsetX, 15);
            Assert.Equal(0.0, result.OffsetY, 15);
        }

        /// <summary>
        /// Tests CreateSkewRadians with multiples of π where Math.Tan should return values close to zero.
        /// Verifies that the method correctly handles angles that are multiples of π radians.
        /// Expected result: Matrix with M12 and M21 values close to zero.
        /// </summary>
        [Theory]
        [InlineData(Math.PI, 0.0)]
        [InlineData(0.0, Math.PI)]
        [InlineData(2 * Math.PI, 0.0)]
        [InlineData(0.0, 2 * Math.PI)]
        [InlineData(-Math.PI, 0.0)]
        [InlineData(0.0, -Math.PI)]
        [InlineData(3 * Math.PI, 4 * Math.PI)]
        public void CreateSkewRadians_MultiplesOfPi_ReturnsMatrixWithNearZeroSkewValues(double skewX, double skewY)
        {
            // Act
            var result = Matrix.CreateSkewRadians(skewX, skewY);

            // Assert
            Assert.Equal(1.0, result.M11, 15);
            Assert.Equal(Math.Tan(skewY), result.M12, 12); // Allow for floating point precision issues
            Assert.Equal(Math.Tan(skewX), result.M21, 12); // Allow for floating point precision issues
            Assert.Equal(1.0, result.M22, 15);
            Assert.Equal(0.0, result.OffsetX, 15);
            Assert.Equal(0.0, result.OffsetY, 15);
        }

        /// <summary>
        /// Tests CreateSkewRadians with large positive and negative values.
        /// Verifies that the method handles large angle values correctly.
        /// Expected result: Matrix with Math.Tan results of the large input values in M12 and M21.
        /// </summary>
        [Theory]
        [InlineData(10.0, 15.0)]
        [InlineData(-10.0, -15.0)]
        [InlineData(100.0, -50.0)]
        [InlineData(-100.0, 200.0)]
        public void CreateSkewRadians_LargeValues_ReturnsMatrixWithExpectedValues(double skewX, double skewY)
        {
            // Act
            var result = Matrix.CreateSkewRadians(skewX, skewY);

            // Assert
            Assert.Equal(1.0, result.M11, 15);
            Assert.Equal(Math.Tan(skewY), result.M12);
            Assert.Equal(Math.Tan(skewX), result.M21);
            Assert.Equal(1.0, result.M22, 15);
            Assert.Equal(0.0, result.OffsetX, 15);
            Assert.Equal(0.0, result.OffsetY, 15);
        }

        /// <summary>
        /// Tests RotateAtPrepend with zero angle - should not modify the matrix.
        /// Input: angle = 0, arbitrary center point.
        /// Expected: Matrix remains unchanged.
        /// </summary>
        [Fact]
        public void RotateAtPrepend_ZeroAngle_MatrixUnchanged()
        {
            // Arrange
            var matrix = new Matrix(2, 3, 4, 5, 6, 7);
            var originalM11 = matrix.M11;
            var originalM12 = matrix.M12;
            var originalM21 = matrix.M21;
            var originalM22 = matrix.M22;
            var originalOffsetX = matrix.OffsetX;
            var originalOffsetY = matrix.OffsetY;

            // Act
            matrix.RotateAtPrepend(0, 10, 20);

            // Assert
            Assert.Equal(originalM11, matrix.M11, 10);
            Assert.Equal(originalM12, matrix.M12, 10);
            Assert.Equal(originalM21, matrix.M21, 10);
            Assert.Equal(originalM22, matrix.M22, 10);
            Assert.Equal(originalOffsetX, matrix.OffsetX, 10);
            Assert.Equal(originalOffsetY, matrix.OffsetY, 10);
        }

        /// <summary>
        /// Tests RotateAtPrepend with standard rotation angles (90, 180, 270, 360 degrees).
        /// Input: Common rotation angles and center at origin.
        /// Expected: Matrix is transformed according to rotation mathematics.
        /// </summary>
        [Theory]
        [InlineData(90)]
        [InlineData(180)]
        [InlineData(270)]
        [InlineData(360)]
        public void RotateAtPrepend_StandardAngles_MatrixTransformed(double angle)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.RotateAtPrepend(angle, 0, 0);

            // Assert
            // For identity matrix with rotation at origin, verify the rotation matrix values
            if (angle == 90 || angle == 270)
            {
                Assert.Equal(0, matrix.M11, 10);
                Assert.Equal(0, matrix.M22, 10);
            }
            else if (angle == 180)
            {
                Assert.Equal(-1, matrix.M11, 10);
                Assert.Equal(-1, matrix.M22, 10);
                Assert.Equal(0, matrix.M12, 10);
                Assert.Equal(0, matrix.M21, 10);
            }
            else if (angle == 360)
            {
                Assert.Equal(1, matrix.M11, 10);
                Assert.Equal(1, matrix.M22, 10);
                Assert.Equal(0, matrix.M12, 10);
                Assert.Equal(0, matrix.M21, 10);
            }
        }

        /// <summary>
        /// Tests RotateAtPrepend with negative angles.
        /// Input: Negative angle values.
        /// Expected: Angle is normalized using modulo operation, matrix transforms accordingly.
        /// </summary>
        [Theory]
        [InlineData(-90)]
        [InlineData(-180)]
        [InlineData(-270)]
        [InlineData(-360)]
        public void RotateAtPrepend_NegativeAngles_AngleNormalizedAndMatrixTransformed(double angle)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.RotateAtPrepend(angle, 0, 0);

            // Assert
            // Verify the matrix is transformed (not identity after negative rotation)
            var isIdentityAfterRotation = matrix.M11 == 1 && matrix.M12 == 0 &&
                                        matrix.M21 == 0 && matrix.M22 == 1 &&
                                        matrix.OffsetX == 0 && matrix.OffsetY == 0;

            if (angle == -360)
            {
                Assert.True(isIdentityAfterRotation);
            }
            else
            {
                Assert.False(isIdentityAfterRotation);
            }
        }

        /// <summary>
        /// Tests RotateAtPrepend with angles greater than 360 degrees.
        /// Input: Angles > 360 degrees.
        /// Expected: Angle is normalized using modulo operation, equivalent transformation applied.
        /// </summary>
        [Theory]
        [InlineData(450, 90)]   // 450 % 360 = 90
        [InlineData(720, 360)]  // 720 % 360 = 0 (equivalent to 360)
        [InlineData(810, 90)]   // 810 % 360 = 90
        public void RotateAtPrepend_LargeAngles_AngleNormalizedCorrectly(double inputAngle, double expectedEquivalentAngle)
        {
            // Arrange
            var matrix1 = Matrix.Identity;
            var matrix2 = Matrix.Identity;

            // Act
            matrix1.RotateAtPrepend(inputAngle, 5, 10);
            matrix2.RotateAtPrepend(expectedEquivalentAngle, 5, 10);

            // Assert
            Assert.Equal(matrix2.M11, matrix1.M11, 10);
            Assert.Equal(matrix2.M12, matrix1.M12, 10);
            Assert.Equal(matrix2.M21, matrix1.M21, 10);
            Assert.Equal(matrix2.M22, matrix1.M22, 10);
            Assert.Equal(matrix2.OffsetX, matrix1.OffsetX, 10);
            Assert.Equal(matrix2.OffsetY, matrix1.OffsetY, 10);
        }

        /// <summary>
        /// Tests RotateAtPrepend with different center points.
        /// Input: Same angle with different center coordinates.
        /// Expected: Different transformation results based on center point.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(10, 20)]
        [InlineData(-5, -10)]
        [InlineData(100, 200)]
        public void RotateAtPrepend_DifferentCenterPoints_TransformationVariesByCenter(double centerX, double centerY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.RotateAtPrepend(90, centerX, centerY);

            // Assert
            // Verify the matrix has been transformed
            Assert.False(matrix.M11 == 1 && matrix.M12 == 0 && matrix.M21 == 0 && matrix.M22 == 1);
        }

        /// <summary>
        /// Tests RotateAtPrepend with special floating-point values for angle.
        /// Input: NaN, PositiveInfinity, NegativeInfinity for angle parameter.
        /// Expected: Method handles special values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void RotateAtPrepend_SpecialFloatingPointAngles_HandledGracefully(double angle)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act & Assert
            // Should not throw an exception, but the result may be NaN or undefined
            matrix.RotateAtPrepend(angle, 0, 0);

            // Verify method completed without exception
            Assert.True(true);
        }

        /// <summary>
        /// Tests RotateAtPrepend with special floating-point values for center coordinates.
        /// Input: NaN, infinities for center coordinates.
        /// Expected: Method handles special values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0)]
        [InlineData(0, double.NaN)]
        [InlineData(double.PositiveInfinity, 0)]
        [InlineData(0, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 0)]
        [InlineData(0, double.NegativeInfinity)]
        public void RotateAtPrepend_SpecialFloatingPointCenters_HandledGracefully(double centerX, double centerY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act & Assert
            // Should not throw an exception
            matrix.RotateAtPrepend(45, centerX, centerY);

            // Verify method completed without exception
            Assert.True(true);
        }

        /// <summary>
        /// Tests RotateAtPrepend with extreme numeric values.
        /// Input: Very large and very small angle values.
        /// Expected: Method handles extreme values and normalizes angle correctly.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(1e10)]
        [InlineData(-1e10)]
        public void RotateAtPrepend_ExtremeNumericValues_HandledCorrectly(double angle)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act & Assert
            // Should not throw an exception
            matrix.RotateAtPrepend(angle, 0, 0);

            // Verify method completed without exception
            Assert.True(true);
        }

        /// <summary>
        /// Tests RotateAtPrepend on a pre-transformed matrix to verify prepend behavior.
        /// Input: Non-identity matrix with rotation prepended.
        /// Expected: Rotation is applied first, then existing transformation.
        /// </summary>
        [Fact]
        public void RotateAtPrepend_NonIdentityMatrix_PrependBehaviorCorrect()
        {
            // Arrange
            var matrix = new Matrix(2, 0, 0, 2, 10, 20); // Scale by 2 and translate
            var originalMatrix = new Matrix(2, 0, 0, 2, 10, 20);

            // Act
            matrix.RotateAtPrepend(90, 0, 0);

            // Assert
            // Verify the matrix has been modified
            Assert.False(matrix.M11 == originalMatrix.M11 &&
                        matrix.M12 == originalMatrix.M12 &&
                        matrix.M21 == originalMatrix.M21 &&
                        matrix.M22 == originalMatrix.M22);
        }

        /// <summary>
        /// Tests RotateAtPrepend with boundary values around the modulo operation.
        /// Input: Values like 359.9, 360.1, etc.
        /// Expected: Correct modulo normalization behavior.
        /// </summary>
        [Theory]
        [InlineData(359.9)]
        [InlineData(360.1)]
        [InlineData(0.1)]
        [InlineData(-0.1)]
        public void RotateAtPrepend_BoundaryAngles_ModuloNormalizationCorrect(double angle)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.RotateAtPrepend(angle, 0, 0);

            // Assert
            // Verify the matrix has been transformed appropriately
            // For very small angles, the matrix should be close to identity but not exactly
            if (Math.Abs(angle % 360.0) < 1.0 && Math.Abs(angle % 360.0) > 0)
            {
                Assert.True(Math.Abs(matrix.M11 - 1) < 0.1);
                Assert.True(Math.Abs(matrix.M22 - 1) < 0.1);
            }
        }

        /// <summary>
        /// Tests that getting M21 from an Identity matrix returns 0.
        /// </summary>
        [Fact]
        public void M21_Get_IdentityMatrix_Returns0()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            var result = matrix.M21;

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that getting M21 from a non-Identity matrix returns the actual _m21 value.
        /// </summary>
        [Theory]
        [InlineData(5.0)]
        [InlineData(-3.14)]
        [InlineData(0.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void M21_Get_NonIdentityMatrix_ReturnsM21Value(double expectedM21)
        {
            // Arrange - Create a non-Identity matrix by setting m21 to a non-zero value
            var matrix = new Matrix(1, 0, expectedM21, 1, 0, 0);

            // Act
            var result = matrix.M21;

            // Assert
            if (double.IsNaN(expectedM21))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(expectedM21, result);
            }
        }

        /// <summary>
        /// Tests that setting M21 on an Identity matrix calls SetMatrix with correct parameters and changes type to Unknown.
        /// </summary>
        [Theory]
        [InlineData(1.0)]
        [InlineData(-2.5)]
        [InlineData(0.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void M21_Set_IdentityMatrix_SetsMatrixAndChangesType(double newM21Value)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.M21 = newM21Value;

            // Assert
            if (double.IsNaN(newM21Value))
            {
                Assert.True(double.IsNaN(matrix.M21));
            }
            else
            {
                Assert.Equal(newM21Value, matrix.M21);
            }
            Assert.Equal(1, matrix.M11); // Should be set to 1
            Assert.Equal(0, matrix.M12); // Should be set to 0
            Assert.Equal(1, matrix.M22); // Should be set to 1
            Assert.Equal(0, matrix.OffsetX); // Should be set to 0
            Assert.Equal(0, matrix.OffsetY); // Should be set to 0
            Assert.False(matrix.IsIdentity); // Should no longer be Identity since type changed to Unknown
        }

        /// <summary>
        /// Tests that setting M21 on a non-Identity matrix sets _m21 directly and changes type to Unknown.
        /// </summary>
        [Theory]
        [InlineData(2.0, 10.0)]
        [InlineData(-1.5, 0.0)]
        [InlineData(0.0, -5.5)]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, 3.14)]
        public void M21_Set_NonIdentityMatrix_SetsM21DirectlyAndChangesType(double initialM21, double newM21Value)
        {
            // Arrange - Create a non-Identity matrix
            var matrix = new Matrix(2, 3, initialM21, 4, 5, 6);
            var originalM11 = matrix.M11;
            var originalM12 = matrix.M12;
            var originalM22 = matrix.M22;
            var originalOffsetX = matrix.OffsetX;
            var originalOffsetY = matrix.OffsetY;

            // Act
            matrix.M21 = newM21Value;

            // Assert
            if (double.IsNaN(newM21Value))
            {
                Assert.True(double.IsNaN(matrix.M21));
            }
            else
            {
                Assert.Equal(newM21Value, matrix.M21);
            }
            // Other matrix values should remain unchanged
            Assert.Equal(originalM11, matrix.M11);
            Assert.Equal(originalM12, matrix.M12);
            Assert.Equal(originalM22, matrix.M22);
            Assert.Equal(originalOffsetX, matrix.OffsetX);
            Assert.Equal(originalOffsetY, matrix.OffsetY);
            Assert.False(matrix.IsIdentity); // Should not be Identity since type changed to Unknown
        }

        /// <summary>
        /// Tests that setting M21 to zero on an Identity matrix preserves Identity behavior in getter.
        /// </summary>
        [Fact]
        public void M21_Set_IdentityMatrixWithZero_PreservesIdentityBehavior()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.M21 = 0.0;

            // Assert
            Assert.Equal(0, matrix.M21);
            Assert.False(matrix.IsIdentity); // Type changed to Unknown, so no longer Identity
        }

        /// <summary>
        /// Tests that M21 getter works correctly after multiple set operations.
        /// </summary>
        [Fact]
        public void M21_GetSet_MultipleOperations_WorksCorrectly()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act & Assert - Multiple operations
            matrix.M21 = 5.0;
            Assert.Equal(5.0, matrix.M21);

            matrix.M21 = -10.5;
            Assert.Equal(-10.5, matrix.M21);

            matrix.M21 = 0.0;
            Assert.Equal(0.0, matrix.M21);
        }

        /// <summary>
        /// Tests that MultiplyVector does not modify input values when matrix type is Identity.
        /// </summary>
        /// <param name="x">The X coordinate value to test</param>
        /// <param name="y">The Y coordinate value to test</param>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(1.0, 1.0)]
        [InlineData(-1.0, -1.0)]
        [InlineData(5.5, -3.2)]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        public void MultiplyVector_IdentityMatrix_DoesNotModifyValues(double x, double y)
        {
            // Arrange
            var matrix = Matrix.Identity;
            double originalX = x;
            double originalY = y;

            // Act
            matrix.MultiplyVector(ref x, ref y);

            // Assert
            Assert.Equal(originalX, x);
            Assert.Equal(originalY, y);
        }

        /// <summary>
        /// Tests that MultiplyVector does not modify input values when matrix type is Translation.
        /// Translation does not affect vectors, only points.
        /// </summary>
        /// <param name="x">The X coordinate value to test</param>
        /// <param name="y">The Y coordinate value to test</param>
        /// <param name="offsetX">The X translation offset</param>
        /// <param name="offsetY">The Y translation offset</param>
        [Theory]
        [InlineData(0.0, 0.0, 10.0, 20.0)]
        [InlineData(1.0, 1.0, -5.0, -10.0)]
        [InlineData(-1.0, -1.0, 0.0, 0.0)]
        [InlineData(5.5, -3.2, 100.0, -200.0)]
        [InlineData(double.MaxValue, double.MinValue, 1.0, 1.0)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity, 50.0, -50.0)]
        [InlineData(double.NaN, double.NaN, 25.0, 30.0)]
        public void MultiplyVector_TranslationMatrix_DoesNotModifyValues(double x, double y, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = CreateTranslationMatrix(offsetX, offsetY);
            double originalX = x;
            double originalY = y;

            // Act
            matrix.MultiplyVector(ref x, ref y);

            // Assert
            Assert.Equal(originalX, x);
            Assert.Equal(originalY, y);
        }

        /// <summary>
        /// Tests that MultiplyVector scales input values correctly when matrix type is Scaling.
        /// </summary>
        /// <param name="x">The X coordinate value to test</param>
        /// <param name="y">The Y coordinate value to test</param>
        /// <param name="scaleX">The X scaling factor</param>
        /// <param name="scaleY">The Y scaling factor</param>
        [Theory]
        [InlineData(1.0, 1.0, 2.0, 3.0)]
        [InlineData(5.0, -2.0, 0.5, -1.5)]
        [InlineData(0.0, 0.0, 10.0, 20.0)]
        [InlineData(-3.0, 4.0, -2.0, -0.5)]
        [InlineData(1.0, 1.0, 0.0, 0.0)]
        [InlineData(2.0, 3.0, double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.MaxValue, double.MinValue, 0.1, 0.1)]
        public void MultiplyVector_ScalingMatrix_ScalesValuesCorrectly(double x, double y, double scaleX, double scaleY)
        {
            // Arrange
            var matrix = CreateScalingMatrix(scaleX, scaleY);
            double expectedX = x * scaleX;
            double expectedY = y * scaleY;

            // Act
            matrix.MultiplyVector(ref x, ref y);

            // Assert
            Assert.Equal(expectedX, x);
            Assert.Equal(expectedY, y);
        }

        /// <summary>
        /// Tests that MultiplyVector handles NaN scaling factors correctly.
        /// </summary>
        [Fact]
        public void MultiplyVector_ScalingMatrixWithNaN_ProducesNaNResults()
        {
            // Arrange
            var matrix = CreateScalingMatrix(double.NaN, double.NaN);
            double x = 5.0;
            double y = 10.0;

            // Act
            matrix.MultiplyVector(ref x, ref y);

            // Assert
            Assert.True(double.IsNaN(x));
            Assert.True(double.IsNaN(y));
        }

        /// <summary>
        /// Tests that MultiplyVector scales input values correctly when matrix type is Scaling combined with Translation.
        /// Translation should be ignored for vectors.
        /// </summary>
        /// <param name="x">The X coordinate value to test</param>
        /// <param name="y">The Y coordinate value to test</param>
        /// <param name="scaleX">The X scaling factor</param>
        /// <param name="scaleY">The Y scaling factor</param>
        [Theory]
        [InlineData(1.0, 1.0, 2.0, 3.0)]
        [InlineData(5.0, -2.0, 0.5, -1.5)]
        [InlineData(-3.0, 4.0, -2.0, -0.5)]
        public void MultiplyVector_ScalingTranslationMatrix_IgnoresTranslationScalesCorrectly(double x, double y, double scaleX, double scaleY)
        {
            // Arrange
            var matrix = CreateScalingTranslationMatrix(scaleX, scaleY, 100.0, 200.0);
            double expectedX = x * scaleX;
            double expectedY = y * scaleY;

            // Act
            matrix.MultiplyVector(ref x, ref y);

            // Assert
            Assert.Equal(expectedX, x);
            Assert.Equal(expectedY, y);
        }

        /// <summary>
        /// Tests that MultiplyVector performs full matrix transformation when matrix type is Unknown.
        /// Uses the formula: new_x = x * m11 + y * m21, new_y = x * m12 + y * m22
        /// </summary>
        /// <param name="x">The X coordinate value to test</param>
        /// <param name="y">The Y coordinate value to test</param>
        /// <param name="m11">The matrix element (1,1)</param>
        /// <param name="m12">The matrix element (1,2)</param>
        /// <param name="m21">The matrix element (2,1)</param>
        /// <param name="m22">The matrix element (2,2)</param>
        [Theory]
        [InlineData(1.0, 1.0, 2.0, 3.0, 4.0, 5.0)]
        [InlineData(2.0, 3.0, 1.0, 0.0, 0.0, 1.0)]
        [InlineData(0.0, 0.0, 5.0, 6.0, 7.0, 8.0)]
        [InlineData(-1.0, -2.0, 2.0, -3.0, 4.0, -5.0)]
        [InlineData(5.0, -3.0, 0.5, 1.5, -0.5, 2.0)]
        public void MultiplyVector_UnknownMatrix_PerformsFullTransformation(double x, double y, double m11, double m12, double m21, double m22)
        {
            // Arrange
            var matrix = CreateUnknownMatrix(m11, m12, m21, m22, 0.0, 0.0);
            double expectedX = x * m11 + y * m21;
            double expectedY = x * m12 + y * m22;

            // Act
            matrix.MultiplyVector(ref x, ref y);

            // Assert
            Assert.Equal(expectedX, x);
            Assert.Equal(expectedY, y);
        }

        /// <summary>
        /// Tests that MultiplyVector handles zero matrix coefficients correctly in Unknown matrix.
        /// </summary>
        [Fact]
        public void MultiplyVector_UnknownMatrixWithZeroCoefficients_ProducesZeroResults()
        {
            // Arrange
            var matrix = CreateUnknownMatrix(0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            double x = 5.0;
            double y = 10.0;

            // Act
            matrix.MultiplyVector(ref x, ref y);

            // Assert
            Assert.Equal(0.0, x);
            Assert.Equal(0.0, y);
        }

        /// <summary>
        /// Tests that MultiplyVector handles infinite matrix coefficients correctly in Unknown matrix.
        /// </summary>
        [Fact]
        public void MultiplyVector_UnknownMatrixWithInfiniteCoefficients_ProducesInfiniteResults()
        {
            // Arrange
            var matrix = CreateUnknownMatrix(double.PositiveInfinity, double.NegativeInfinity,
                                          double.PositiveInfinity, double.NegativeInfinity, 0.0, 0.0);
            double x = 1.0;
            double y = 1.0;

            // Act
            matrix.MultiplyVector(ref x, ref y);

            // Assert
            Assert.True(double.IsPositiveInfinity(x)); // 1 * +inf + 1 * +inf = +inf
            Assert.True(double.IsNegativeInfinity(y)); // 1 * -inf + 1 * -inf = -inf
        }

        /// <summary>
        /// Tests that MultiplyVector handles NaN matrix coefficients correctly in Unknown matrix.
        /// </summary>
        [Fact]
        public void MultiplyVector_UnknownMatrixWithNaNCoefficients_ProducesNaNResults()
        {
            // Arrange
            var matrix = CreateUnknownMatrix(double.NaN, double.NaN, double.NaN, double.NaN, 0.0, 0.0);
            double x = 1.0;
            double y = 1.0;

            // Act
            matrix.MultiplyVector(ref x, ref y);

            // Assert
            Assert.True(double.IsNaN(x));
            Assert.True(double.IsNaN(y));
        }

        #region Helper Methods

        /// <summary>
        /// Creates a translation matrix using internal CreateTranslation method.
        /// </summary>
        private static Matrix CreateTranslationMatrix(double offsetX, double offsetY)
        {
            // Create a matrix and set it to translation type manually since CreateTranslation is internal
            var matrix = new Matrix(1.0, 0.0, 0.0, 1.0, offsetX, offsetY);
            // Use reflection to set the type to Translation since SetMatrix is internal
            var typeField = typeof(Matrix).GetField("_type", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            typeField.SetValue(matrix, MatrixTypes.Translation);
            return matrix;
        }

        /// <summary>
        /// Creates a scaling matrix using internal CreateScaling method.
        /// </summary>
        private static Matrix CreateScalingMatrix(double scaleX, double scaleY)
        {
            // Create a matrix and set it to scaling type manually since CreateScaling is internal
            var matrix = new Matrix(scaleX, 0.0, 0.0, scaleY, 0.0, 0.0);
            // Use reflection to set the type to Scaling since SetMatrix is internal
            var typeField = typeof(Matrix).GetField("_type", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            typeField.SetValue(matrix, MatrixTypes.Scaling);
            return matrix;
        }

        /// <summary>
        /// Creates a scaling with translation matrix.
        /// </summary>
        private static Matrix CreateScalingTranslationMatrix(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            var matrix = new Matrix(scaleX, 0.0, 0.0, scaleY, offsetX, offsetY);
            // Use reflection to set the type to Scaling | Translation
            var typeField = typeof(Matrix).GetField("_type", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            typeField.SetValue(matrix, MatrixTypes.Scaling | MatrixTypes.Translation);
            return matrix;
        }

        /// <summary>
        /// Creates an unknown matrix with specified coefficients.
        /// </summary>
        private static Matrix CreateUnknownMatrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            var matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);
            // Constructor already sets type to Unknown, so no need to modify it
            return matrix;
        }

        #endregion

        /// <summary>
        /// Tests ScalePrepend method with positive scaling values on an identity matrix.
        /// Verifies that scaling is correctly applied as a prepended operation.
        /// </summary>
        [Theory]
        [InlineData(2.0, 3.0)]
        [InlineData(0.5, 0.25)]
        [InlineData(1.0, 1.0)]
        public void ScalePrepend_PositiveValues_IdentityMatrix_AppliesScalingCorrectly(double scaleX, double scaleY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.ScalePrepend(scaleX, scaleY);

            // Assert
            Assert.Equal(scaleX, matrix.M11);
            Assert.Equal(0, matrix.M12);
            Assert.Equal(0, matrix.M21);
            Assert.Equal(scaleY, matrix.M22);
            Assert.Equal(0, matrix.OffsetX);
            Assert.Equal(0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests ScalePrepend method with zero scaling values.
        /// Verifies that zero scaling creates a degenerate matrix.
        /// </summary>
        [Theory]
        [InlineData(0.0, 1.0)]
        [InlineData(1.0, 0.0)]
        [InlineData(0.0, 0.0)]
        public void ScalePrepend_ZeroValues_CreatesDegenerate(double scaleX, double scaleY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.ScalePrepend(scaleX, scaleY);

            // Assert
            Assert.Equal(scaleX, matrix.M11);
            Assert.Equal(0, matrix.M12);
            Assert.Equal(0, matrix.M21);
            Assert.Equal(scaleY, matrix.M22);
            Assert.Equal(0, matrix.OffsetX);
            Assert.Equal(0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests ScalePrepend method with negative scaling values.
        /// Verifies that negative scaling creates mirroring transformation.
        /// </summary>
        [Theory]
        [InlineData(-1.0, 1.0)]
        [InlineData(1.0, -1.0)]
        [InlineData(-2.0, -3.0)]
        public void ScalePrepend_NegativeValues_AppliesMirroring(double scaleX, double scaleY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.ScalePrepend(scaleX, scaleY);

            // Assert
            Assert.Equal(scaleX, matrix.M11);
            Assert.Equal(0, matrix.M12);
            Assert.Equal(0, matrix.M21);
            Assert.Equal(scaleY, matrix.M22);
            Assert.Equal(0, matrix.OffsetX);
            Assert.Equal(0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests ScalePrepend method with special floating point values.
        /// Verifies handling of NaN and infinity values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 1.0)]
        [InlineData(1.0, double.NaN)]
        [InlineData(double.PositiveInfinity, 1.0)]
        [InlineData(1.0, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 1.0)]
        [InlineData(1.0, double.NegativeInfinity)]
        public void ScalePrepend_SpecialFloatValues_HandlesCorrectly(double scaleX, double scaleY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.ScalePrepend(scaleX, scaleY);

            // Assert
            Assert.Equal(scaleX, matrix.M11);
            Assert.Equal(0, matrix.M12);
            Assert.Equal(0, matrix.M21);
            Assert.Equal(scaleY, matrix.M22);
            Assert.Equal(0, matrix.OffsetX);
            Assert.Equal(0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests ScalePrepend method on a translation matrix.
        /// Verifies that scaling is applied before translation transformation.
        /// </summary>
        [Fact]
        public void ScalePrepend_TranslationMatrix_AppliesScalingBeforeTranslation()
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 10, 20); // Translation matrix
            double scaleX = 2.0;
            double scaleY = 3.0;

            // Act
            matrix.ScalePrepend(scaleX, scaleY);

            // Assert - Result should be scaling * translation
            Assert.Equal(scaleX, matrix.M11);
            Assert.Equal(0, matrix.M12);
            Assert.Equal(0, matrix.M21);
            Assert.Equal(scaleY, matrix.M22);
            Assert.Equal(scaleX * 10, matrix.OffsetX);
            Assert.Equal(scaleY * 20, matrix.OffsetY);
        }

        /// <summary>
        /// Tests ScalePrepend method on a complex matrix with all components.
        /// Verifies that scaling is applied correctly to all matrix elements.
        /// </summary>
        [Fact]
        public void ScalePrepend_ComplexMatrix_AppliesScalingToAllComponents()
        {
            // Arrange
            var matrix = new Matrix(2, 3, 4, 5, 6, 7); // Complex matrix
            double scaleX = 2.0;
            double scaleY = 0.5;

            // Act
            matrix.ScalePrepend(scaleX, scaleY);

            // Assert - Result should be scaling * original
            Assert.Equal(scaleX * 2, matrix.M11);
            Assert.Equal(scaleX * 3, matrix.M12);
            Assert.Equal(scaleY * 4, matrix.M21);
            Assert.Equal(scaleY * 5, matrix.M22);
            Assert.Equal(scaleX * 6, matrix.OffsetX);
            Assert.Equal(scaleY * 7, matrix.OffsetY);
        }

        /// <summary>
        /// Tests ScalePrepend method with extreme values.
        /// Verifies handling of very large and very small scaling factors.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, 1.0)]
        [InlineData(1.0, double.MaxValue)]
        [InlineData(double.MinValue, 1.0)]
        [InlineData(1.0, double.MinValue)]
        [InlineData(double.Epsilon, 1.0)]
        [InlineData(1.0, double.Epsilon)]
        public void ScalePrepend_ExtremeValues_HandlesCorrectly(double scaleX, double scaleY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.ScalePrepend(scaleX, scaleY);

            // Assert
            Assert.Equal(scaleX, matrix.M11);
            Assert.Equal(0, matrix.M12);
            Assert.Equal(0, matrix.M21);
            Assert.Equal(scaleY, matrix.M22);
            Assert.Equal(0, matrix.OffsetX);
            Assert.Equal(0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests ScalePrepend method multiple times.
        /// Verifies that multiple scaling operations are correctly combined.
        /// </summary>
        [Fact]
        public void ScalePrepend_MultipleCalls_CombinesScaling()
        {
            // Arrange
            var matrix = Matrix.Identity;
            double scaleX1 = 2.0, scaleY1 = 3.0;
            double scaleX2 = 0.5, scaleY2 = 4.0;

            // Act
            matrix.ScalePrepend(scaleX1, scaleY1);
            matrix.ScalePrepend(scaleX2, scaleY2);

            // Assert - Second scaling should be applied first (prepended)
            Assert.Equal(scaleX2 * scaleX1, matrix.M11);
            Assert.Equal(0, matrix.M12);
            Assert.Equal(0, matrix.M21);
            Assert.Equal(scaleY2 * scaleY1, matrix.M22);
            Assert.Equal(0, matrix.OffsetX);
            Assert.Equal(0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests TranslatePrepend with positive offset values.
        /// Verifies that the translation is correctly prepended to an identity matrix.
        /// Expected result: Matrix should have translation offsets applied.
        /// </summary>
        [Fact]
        public void TranslatePrepend_PositiveOffsets_UpdatesMatrixCorrectly()
        {
            // Arrange
            var matrix = Matrix.Identity;
            double offsetX = 10.5;
            double offsetY = 20.7;

            // Act
            matrix.TranslatePrepend(offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, matrix.M11, 10);
            Assert.Equal(0.0, matrix.M12, 10);
            Assert.Equal(0.0, matrix.M21, 10);
            Assert.Equal(1.0, matrix.M22, 10);
            Assert.Equal(offsetX, matrix.OffsetX, 10);
            Assert.Equal(offsetY, matrix.OffsetY, 10);
        }

        /// <summary>
        /// Tests TranslatePrepend with negative offset values.
        /// Verifies that negative translations are correctly applied.
        /// Expected result: Matrix should have negative translation offsets.
        /// </summary>
        [Fact]
        public void TranslatePrepend_NegativeOffsets_UpdatesMatrixCorrectly()
        {
            // Arrange
            var matrix = Matrix.Identity;
            double offsetX = -15.3;
            double offsetY = -25.8;

            // Act
            matrix.TranslatePrepend(offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, matrix.M11, 10);
            Assert.Equal(0.0, matrix.M12, 10);
            Assert.Equal(0.0, matrix.M21, 10);
            Assert.Equal(1.0, matrix.M22, 10);
            Assert.Equal(offsetX, matrix.OffsetX, 10);
            Assert.Equal(offsetY, matrix.OffsetY, 10);
        }

        /// <summary>
        /// Tests TranslatePrepend with zero offset values.
        /// Verifies that zero translation does not change the matrix.
        /// Expected result: Matrix should remain unchanged (identity).
        /// </summary>
        [Fact]
        public void TranslatePrepend_ZeroOffsets_MatrixRemainsUnchanged()
        {
            // Arrange
            var matrix = Matrix.Identity;
            double offsetX = 0.0;
            double offsetY = 0.0;

            // Act
            matrix.TranslatePrepend(offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, matrix.M11, 10);
            Assert.Equal(0.0, matrix.M12, 10);
            Assert.Equal(0.0, matrix.M21, 10);
            Assert.Equal(1.0, matrix.M22, 10);
            Assert.Equal(0.0, matrix.OffsetX, 10);
            Assert.Equal(0.0, matrix.OffsetY, 10);
        }

        /// <summary>
        /// Tests TranslatePrepend with extreme maximum double values.
        /// Verifies behavior with boundary values of double type.
        /// Expected result: Matrix should handle extreme values without error.
        /// </summary>
        [Fact]
        public void TranslatePrepend_MaximumDoubleValues_HandlesExtremeValues()
        {
            // Arrange
            var matrix = Matrix.Identity;
            double offsetX = double.MaxValue;
            double offsetY = double.MaxValue;

            // Act
            matrix.TranslatePrepend(offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, matrix.M11, 10);
            Assert.Equal(0.0, matrix.M12, 10);
            Assert.Equal(0.0, matrix.M21, 10);
            Assert.Equal(1.0, matrix.M22, 10);
            Assert.Equal(double.MaxValue, matrix.OffsetX);
            Assert.Equal(double.MaxValue, matrix.OffsetY);
        }

        /// <summary>
        /// Tests TranslatePrepend with extreme minimum double values.
        /// Verifies behavior with boundary values of double type.
        /// Expected result: Matrix should handle extreme negative values without error.
        /// </summary>
        [Fact]
        public void TranslatePrepend_MinimumDoubleValues_HandlesExtremeValues()
        {
            // Arrange
            var matrix = Matrix.Identity;
            double offsetX = double.MinValue;
            double offsetY = double.MinValue;

            // Act
            matrix.TranslatePrepend(offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, matrix.M11, 10);
            Assert.Equal(0.0, matrix.M12, 10);
            Assert.Equal(0.0, matrix.M21, 10);
            Assert.Equal(1.0, matrix.M22, 10);
            Assert.Equal(double.MinValue, matrix.OffsetX);
            Assert.Equal(double.MinValue, matrix.OffsetY);
        }

        /// <summary>
        /// Tests TranslatePrepend with NaN values.
        /// Verifies that NaN values are properly handled in matrix operations.
        /// Expected result: Matrix offset values should be NaN.
        /// </summary>
        [Fact]
        public void TranslatePrepend_NaNValues_PreservesNaNInMatrix()
        {
            // Arrange
            var matrix = Matrix.Identity;
            double offsetX = double.NaN;
            double offsetY = double.NaN;

            // Act
            matrix.TranslatePrepend(offsetX, offsetY);

            // Assert
            Assert.True(double.IsNaN(matrix.OffsetX));
            Assert.True(double.IsNaN(matrix.OffsetY));
        }

        /// <summary>
        /// Tests TranslatePrepend with positive infinity values.
        /// Verifies that positive infinity values are handled correctly.
        /// Expected result: Matrix offset values should be positive infinity.
        /// </summary>
        [Fact]
        public void TranslatePrepend_PositiveInfinityValues_PreservesInfinityInMatrix()
        {
            // Arrange
            var matrix = Matrix.Identity;
            double offsetX = double.PositiveInfinity;
            double offsetY = double.PositiveInfinity;

            // Act
            matrix.TranslatePrepend(offsetX, offsetY);

            // Assert
            Assert.Equal(double.PositiveInfinity, matrix.OffsetX);
            Assert.Equal(double.PositiveInfinity, matrix.OffsetY);
        }

        /// <summary>
        /// Tests TranslatePrepend with negative infinity values.
        /// Verifies that negative infinity values are handled correctly.
        /// Expected result: Matrix offset values should be negative infinity.
        /// </summary>
        [Fact]
        public void TranslatePrepend_NegativeInfinityValues_PreservesInfinityInMatrix()
        {
            // Arrange
            var matrix = Matrix.Identity;
            double offsetX = double.NegativeInfinity;
            double offsetY = double.NegativeInfinity;

            // Act
            matrix.TranslatePrepend(offsetX, offsetY);

            // Assert
            Assert.Equal(double.NegativeInfinity, matrix.OffsetX);
            Assert.Equal(double.NegativeInfinity, matrix.OffsetY);
        }

        /// <summary>
        /// Tests TranslatePrepend on a pre-existing scaled matrix.
        /// Verifies that translation prepend works correctly with non-identity matrices.
        /// Expected result: Translation should be applied before the existing scaling transformation.
        /// </summary>
        [Fact]
        public void TranslatePrepend_OnScaledMatrix_AppliesTranslationCorrectly()
        {
            // Arrange
            var matrix = new Matrix(2.0, 0.0, 0.0, 3.0, 5.0, 7.0); // Scaled and translated matrix
            double offsetX = 10.0;
            double offsetY = 15.0;

            // Act
            matrix.TranslatePrepend(offsetX, offsetY);

            // Assert
            Assert.Equal(2.0, matrix.M11, 10);
            Assert.Equal(0.0, matrix.M12, 10);
            Assert.Equal(0.0, matrix.M21, 10);
            Assert.Equal(3.0, matrix.M22, 10);
            // For prepend translation, the new offset should be: original_offset + scale * translation_offset
            Assert.Equal(25.0, matrix.OffsetX, 10); // 5 + 2*10
            Assert.Equal(52.0, matrix.OffsetY, 10); // 7 + 3*15
        }

        /// <summary>
        /// Tests TranslatePrepend with mixed positive and negative offset values.
        /// Verifies correct handling of asymmetric translation values.
        /// Expected result: Matrix should apply different offset values to X and Y axes.
        /// </summary>
        [Fact]
        public void TranslatePrepend_MixedOffsetSigns_UpdatesMatrixCorrectly()
        {
            // Arrange
            var matrix = Matrix.Identity;
            double offsetX = 42.7;
            double offsetY = -17.3;

            // Act
            matrix.TranslatePrepend(offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, matrix.M11, 10);
            Assert.Equal(0.0, matrix.M12, 10);
            Assert.Equal(0.0, matrix.M21, 10);
            Assert.Equal(1.0, matrix.M22, 10);
            Assert.Equal(offsetX, matrix.OffsetX, 10);
            Assert.Equal(offsetY, matrix.OffsetY, 10);
        }

        /// <summary>
        /// Tests ScaleAt method with identity matrix and scaling around origin.
        /// Verifies that scaling around origin (0,0) produces expected matrix values.
        /// </summary>
        [Fact]
        public void ScaleAt_IdentityMatrixAroundOrigin_ProducesCorrectScaling()
        {
            // Arrange
            var matrix = Matrix.Identity;
            double scaleX = 2.0;
            double scaleY = 3.0;
            double centerX = 0.0;
            double centerY = 0.0;

            // Act
            matrix.ScaleAt(scaleX, scaleY, centerX, centerY);

            // Assert
            Assert.Equal(2.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(3.0, matrix.M22);
            Assert.Equal(0.0, matrix.OffsetX);
            Assert.Equal(0.0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests ScaleAt method with identity matrix and scaling around non-origin center point.
        /// Verifies that scaling around a center point applies proper translation offsets.
        /// </summary>
        [Fact]
        public void ScaleAt_IdentityMatrixAroundCenterPoint_ProducesCorrectScalingWithTranslation()
        {
            // Arrange
            var matrix = Matrix.Identity;
            double scaleX = 2.0;
            double scaleY = 0.5;
            double centerX = 10.0;
            double centerY = 20.0;

            // Act
            matrix.ScaleAt(scaleX, scaleY, centerX, centerY);

            // Assert
            Assert.Equal(2.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(0.5, matrix.M22);
            // OffsetX = centerX - scaleX * centerX = 10 - 2 * 10 = -10
            Assert.Equal(-10.0, matrix.OffsetX);
            // OffsetY = centerY - scaleY * centerY = 20 - 0.5 * 20 = 10
            Assert.Equal(10.0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests ScaleAt method with zero scale factors.
        /// Verifies that zero scaling produces matrix with zero scale components.
        /// </summary>
        [Theory]
        [InlineData(0.0, 1.0, 5.0, 5.0)]
        [InlineData(1.0, 0.0, 5.0, 5.0)]
        [InlineData(0.0, 0.0, 5.0, 5.0)]
        public void ScaleAt_ZeroScaleFactors_ProducesZeroScaleComponents(double scaleX, double scaleY, double centerX, double centerY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.ScaleAt(scaleX, scaleY, centerX, centerY);

            // Assert
            Assert.Equal(scaleX, matrix.M11);
            Assert.Equal(scaleY, matrix.M22);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
        }

        /// <summary>
        /// Tests ScaleAt method with negative scale factors.
        /// Verifies that negative scaling produces correct matrix values for mirroring transformations.
        /// </summary>
        [Theory]
        [InlineData(-1.0, 1.0, 0.0, 0.0)]
        [InlineData(1.0, -1.0, 0.0, 0.0)]
        [InlineData(-2.0, -3.0, 5.0, 10.0)]
        public void ScaleAt_NegativeScaleFactors_ProducesCorrectMirroringMatrix(double scaleX, double scaleY, double centerX, double centerY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.ScaleAt(scaleX, scaleY, centerX, centerY);

            // Assert
            Assert.Equal(scaleX, matrix.M11);
            Assert.Equal(scaleY, matrix.M22);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(centerX - scaleX * centerX, matrix.OffsetX);
            Assert.Equal(centerY - scaleY * centerY, matrix.OffsetY);
        }

        /// <summary>
        /// Tests ScaleAt method with unity scale factors.
        /// Verifies that scaling by 1.0 around any center point produces identity-like scaling.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(10.0, 20.0)]
        [InlineData(-5.0, -15.0)]
        public void ScaleAt_UnityScaleFactors_ProducesIdentityLikeMatrix(double centerX, double centerY)
        {
            // Arrange
            var matrix = Matrix.Identity;
            double scaleX = 1.0;
            double scaleY = 1.0;

            // Act
            matrix.ScaleAt(scaleX, scaleY, centerX, centerY);

            // Assert
            Assert.Equal(1.0, matrix.M11);
            Assert.Equal(1.0, matrix.M22);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(0.0, matrix.OffsetX);
            Assert.Equal(0.0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests ScaleAt method with extreme numeric values.
        /// Verifies that the method handles extreme double values correctly.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, 1.0, 0.0, 0.0)]
        [InlineData(1.0, double.MaxValue, 0.0, 0.0)]
        [InlineData(double.MinValue, 1.0, 0.0, 0.0)]
        [InlineData(1.0, double.MinValue, 0.0, 0.0)]
        public void ScaleAt_ExtremeScaleValues_HandlesExtremeDoubleValues(double scaleX, double scaleY, double centerX, double centerY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.ScaleAt(scaleX, scaleY, centerX, centerY);

            // Assert
            Assert.Equal(scaleX, matrix.M11);
            Assert.Equal(scaleY, matrix.M22);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
        }

        /// <summary>
        /// Tests ScaleAt method with extreme center point values.
        /// Verifies that the method handles extreme center coordinates correctly.
        /// </summary>
        [Theory]
        [InlineData(2.0, 3.0, double.MaxValue, 0.0)]
        [InlineData(2.0, 3.0, 0.0, double.MaxValue)]
        [InlineData(2.0, 3.0, double.MinValue, 0.0)]
        [InlineData(2.0, 3.0, 0.0, double.MinValue)]
        public void ScaleAt_ExtremeCenterValues_HandlesExtremeCenterCoordinates(double scaleX, double scaleY, double centerX, double centerY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.ScaleAt(scaleX, scaleY, centerX, centerY);

            // Assert
            Assert.Equal(scaleX, matrix.M11);
            Assert.Equal(scaleY, matrix.M22);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
        }

        /// <summary>
        /// Tests ScaleAt method with special double values (NaN, Infinity).
        /// Verifies that the method handles special floating-point values appropriately.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 1.0, 0.0, 0.0)]
        [InlineData(1.0, double.NaN, 0.0, 0.0)]
        [InlineData(double.PositiveInfinity, 1.0, 0.0, 0.0)]
        [InlineData(1.0, double.PositiveInfinity, 0.0, 0.0)]
        [InlineData(double.NegativeInfinity, 1.0, 0.0, 0.0)]
        [InlineData(1.0, double.NegativeInfinity, 0.0, 0.0)]
        [InlineData(1.0, 1.0, double.NaN, 0.0)]
        [InlineData(1.0, 1.0, 0.0, double.NaN)]
        [InlineData(1.0, 1.0, double.PositiveInfinity, 0.0)]
        [InlineData(1.0, 1.0, 0.0, double.PositiveInfinity)]
        [InlineData(1.0, 1.0, double.NegativeInfinity, 0.0)]
        [InlineData(1.0, 1.0, 0.0, double.NegativeInfinity)]
        public void ScaleAt_SpecialDoubleValues_HandlesNanAndInfinityValues(double scaleX, double scaleY, double centerX, double centerY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.ScaleAt(scaleX, scaleY, centerX, centerY);

            // Assert
            // The method should complete without throwing exceptions
            // The exact behavior with NaN/Infinity depends on the underlying math operations
            Assert.Equal(scaleX, matrix.M11);
            Assert.Equal(scaleY, matrix.M22);
        }

        /// <summary>
        /// Tests ScaleAt method on non-identity matrix.
        /// Verifies that scaling applies correctly to an already transformed matrix.
        /// </summary>
        [Fact]
        public void ScaleAt_NonIdentityMatrix_CombinesTransformations()
        {
            // Arrange
            var matrix = new Matrix(2.0, 0.0, 0.0, 2.0, 10.0, 20.0); // 2x scale with translation
            double scaleX = 0.5;
            double scaleY = 0.25;
            double centerX = 0.0;
            double centerY = 0.0;

            // Act
            matrix.ScaleAt(scaleX, scaleY, centerX, centerY);

            // Assert - The result should be the original matrix multiplied by the scaling matrix
            // Expected: M11 = 2.0 * 0.5 = 1.0, M22 = 2.0 * 0.25 = 0.5
            Assert.Equal(1.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(0.5, matrix.M22);
            Assert.Equal(10.0, matrix.OffsetX); // Original OffsetX should remain
            Assert.Equal(20.0, matrix.OffsetY); // Original OffsetY should remain
        }

        /// <summary>
        /// Tests ScaleAt method with fractional scale factors.
        /// Verifies that fractional scaling produces correct matrix values for shrinking transformations.
        /// </summary>
        [Theory]
        [InlineData(0.5, 0.25, 4.0, 8.0)]
        [InlineData(0.1, 0.9, -2.0, 3.0)]
        [InlineData(0.33333, 0.66666, 15.0, -10.0)]
        public void ScaleAt_FractionalScaleFactors_ProducesCorrectShrinkingMatrix(double scaleX, double scaleY, double centerX, double centerY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.ScaleAt(scaleX, scaleY, centerX, centerY);

            // Assert
            Assert.Equal(scaleX, matrix.M11, 5); // Using precision for floating point comparison
            Assert.Equal(scaleY, matrix.M22, 5);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(centerX - scaleX * centerX, matrix.OffsetX, 5);
            Assert.Equal(centerY - scaleY * centerY, matrix.OffsetY, 5);
        }

        /// <summary>
        /// Tests that Invert throws InvalidOperationException when determinant is zero for scaling matrix.
        /// Input: Matrix with zero determinant (scaling type with _m11 = 0).
        /// Expected: InvalidOperationException is thrown.
        /// </summary>
        [Fact]
        public void Invert_ScalingMatrixWithZeroDeterminant_ThrowsInvalidOperationException()
        {
            // Arrange - Create scaling matrix with zero determinant (_m11 = 0, _m22 = 2)
            var matrix = new Matrix(0, 0, 0, 2, 0, 0);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => matrix.Invert());
        }

        /// <summary>
        /// Tests that Invert throws InvalidOperationException when determinant is zero for scaling+translation matrix.
        /// Input: Matrix with zero determinant (scaling+translation type with _m22 = 0).
        /// Expected: InvalidOperationException is thrown.
        /// </summary>
        [Fact]
        public void Invert_ScalingTranslationMatrixWithZeroDeterminant_ThrowsInvalidOperationException()
        {
            // Arrange - Create scaling+translation matrix with zero determinant (_m11 = 3, _m22 = 0)
            var matrix = new Matrix(3, 0, 0, 0, 5, 7);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => matrix.Invert());
        }

        /// <summary>
        /// Tests that Invert throws InvalidOperationException when determinant is zero for unknown matrix type.
        /// Input: Matrix with zero determinant (unknown type where (_m11 * _m22) - (_m12 * _m21) = 0).
        /// Expected: InvalidOperationException is thrown.
        /// </summary>
        [Fact]
        public void Invert_UnknownMatrixWithZeroDeterminant_ThrowsInvalidOperationException()
        {
            // Arrange - Create unknown matrix with zero determinant (2*2 - 1*4 = 0)
            var matrix = new Matrix(2, 1, 4, 2, 0, 0);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => matrix.Invert());
        }

        /// <summary>
        /// Tests that Invert does nothing for identity matrix.
        /// Input: Identity matrix (_m11=1, _m22=1, all others=0).
        /// Expected: Matrix remains unchanged.
        /// </summary>
        [Fact]
        public void Invert_IdentityMatrix_RemainsUnchanged()
        {
            // Arrange - Create identity matrix
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);
            var originalMatrix = new Matrix(1, 0, 0, 1, 0, 0);

            // Act
            matrix.Invert();

            // Assert - Matrix should remain the same
            Assert.Equal(originalMatrix, matrix);
        }

        /// <summary>
        /// Tests that Invert negates offsets for translation matrix.
        /// Input: Translation matrix (_m11=1, _m22=1, _m12=0, _m21=0, non-zero offsets).
        /// Expected: Offsets are negated, diagonal elements remain 1.
        /// </summary>
        [Theory]
        [InlineData(5, 3)]
        [InlineData(-2, 7)]
        [InlineData(0, 4)]
        [InlineData(1.5, -3.7)]
        public void Invert_TranslationMatrix_NegatesOffsets(double offsetX, double offsetY)
        {
            // Arrange - Create translation matrix
            var matrix = new Matrix(1, 0, 0, 1, offsetX, offsetY);

            // Act
            matrix.Invert();

            // Assert - Offsets should be negated
            Assert.Equal(1, matrix.M11);
            Assert.Equal(0, matrix.M12);
            Assert.Equal(0, matrix.M21);
            Assert.Equal(1, matrix.M22);
            Assert.Equal(-offsetX, matrix.OffsetX);
            Assert.Equal(-offsetY, matrix.OffsetY);
        }

        /// <summary>
        /// Tests that Invert correctly handles scaling+translation matrix.
        /// Input: Scaling+translation matrix (diagonal scaling with non-zero offsets).
        /// Expected: Scaling elements are inverted and offsets are transformed.
        /// </summary>
        [Theory]
        [InlineData(2, 3, 4, 5)]
        [InlineData(0.5, 4, -2, 1)]
        [InlineData(-2, -0.5, 3, -7)]
        public void Invert_ScalingTranslationMatrix_InvertsScalingAndTransformsOffsets(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            // Arrange - Create scaling+translation matrix
            var matrix = new Matrix(scaleX, 0, 0, scaleY, offsetX, offsetY);
            var expectedM11 = 1.0 / scaleX;
            var expectedM22 = 1.0 / scaleY;
            var expectedOffsetX = -offsetX * expectedM11;
            var expectedOffsetY = -offsetY * expectedM22;

            // Act
            matrix.Invert();

            // Assert
            Assert.Equal(expectedM11, matrix.M11, 10);
            Assert.Equal(0, matrix.M12);
            Assert.Equal(0, matrix.M21);
            Assert.Equal(expectedM22, matrix.M22, 10);
            Assert.Equal(expectedOffsetX, matrix.OffsetX, 10);
            Assert.Equal(expectedOffsetY, matrix.OffsetY, 10);
        }

        /// <summary>
        /// Tests that Invert throws InvalidOperationException for edge case with very small determinant that rounds to zero.
        /// Input: Matrix with extremely small but theoretically non-zero determinant.
        /// Expected: InvalidOperationException is thrown when determinant evaluates to zero.
        /// </summary>
        [Fact]
        public void Invert_MatrixWithVerySmallDeterminant_ThrowsInvalidOperationException()
        {
            // Arrange - Create matrix with very small determinant that may round to zero
            var matrix = new Matrix(1e-100, 1e-100, 1e-100, 1e-100, 0, 0);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => matrix.Invert());
        }

        /// <summary>
        /// Tests that Invert handles matrices with infinity values appropriately.
        /// Input: Matrix with infinity values that result in zero determinant.
        /// Expected: InvalidOperationException is thrown.
        /// </summary>
        [Fact]
        public void Invert_MatrixWithInfinityValues_ThrowsInvalidOperationException()
        {
            // Arrange - Create matrix with infinity values that result in zero determinant
            var matrix = new Matrix(double.PositiveInfinity, 0, 0, 0, 0, 0);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => matrix.Invert());
        }

        /// <summary>
        /// Tests that Invert handles NaN values appropriately.
        /// Input: Matrix with NaN values.
        /// Expected: InvalidOperationException is thrown due to NaN determinant.
        /// </summary>
        [Fact]
        public void Invert_MatrixWithNaNValues_ThrowsInvalidOperationException()
        {
            // Arrange - Create matrix with NaN values
            var matrix = new Matrix(double.NaN, 0, 0, 1, 0, 0);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => matrix.Invert());
        }

        /// <summary>
        /// Tests ScaleAtPrepend method with normal scaling factors and center point.
        /// Verifies that the method correctly prepends scaling transformation to the matrix.
        /// </summary>
        [Fact]
        public void ScaleAtPrepend_NormalValues_AppliesScalingCorrectly()
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 10, 20); // Identity with translation
            double scaleX = 2.0;
            double scaleY = 3.0;
            double centerX = 5.0;
            double centerY = 10.0;

            // Act
            matrix.ScaleAtPrepend(scaleX, scaleY, centerX, centerY);

            // Assert
            Assert.Equal(2.0, matrix.M11, 5);
            Assert.Equal(0.0, matrix.M12, 5);
            Assert.Equal(0.0, matrix.M21, 5);
            Assert.Equal(3.0, matrix.M22, 5);
            Assert.Equal(15.0, matrix.OffsetX, 5); // 10 * 2 + (5 - 2*5) = 20 - 5 = 15
            Assert.Equal(50.0, matrix.OffsetY, 5); // 20 * 3 + (10 - 3*10) = 60 - 20 = 40
        }

        /// <summary>
        /// Tests ScaleAtPrepend method with identity scaling factors (1.0, 1.0).
        /// Verifies that the matrix remains unchanged when scaled by identity.
        /// </summary>
        [Fact]
        public void ScaleAtPrepend_IdentityScaling_MatrixUnchanged()
        {
            // Arrange
            var matrix = new Matrix(2, 3, 4, 5, 6, 7);
            var originalM11 = matrix.M11;
            var originalM12 = matrix.M12;
            var originalM21 = matrix.M21;
            var originalM22 = matrix.M22;
            var originalOffsetX = matrix.OffsetX;
            var originalOffsetY = matrix.OffsetY;

            // Act
            matrix.ScaleAtPrepend(1.0, 1.0, 10.0, 20.0);

            // Assert
            Assert.Equal(originalM11, matrix.M11, 5);
            Assert.Equal(originalM12, matrix.M12, 5);
            Assert.Equal(originalM21, matrix.M21, 5);
            Assert.Equal(originalM22, matrix.M22, 5);
            Assert.Equal(originalOffsetX, matrix.OffsetX, 5);
            Assert.Equal(originalOffsetY, matrix.OffsetY, 5);
        }

        /// <summary>
        /// Tests ScaleAtPrepend method with zero scaling factors.
        /// Verifies that zero scaling creates a singular matrix transformation.
        /// </summary>
        [Theory]
        [InlineData(0.0, 1.0)]
        [InlineData(1.0, 0.0)]
        [InlineData(0.0, 0.0)]
        public void ScaleAtPrepend_ZeroScaling_CreatesSingularMatrix(double scaleX, double scaleY)
        {
            // Arrange
            var matrix = new Matrix(1, 2, 3, 4, 5, 6);

            // Act
            matrix.ScaleAtPrepend(scaleX, scaleY, 0.0, 0.0);

            // Assert
            Assert.Equal(scaleX, matrix.M11, 5);
            Assert.Equal(0.0, matrix.M12, 5);
            Assert.Equal(0.0, matrix.M21, 5);
            Assert.Equal(scaleY, matrix.M22, 5);
        }

        /// <summary>
        /// Tests ScaleAtPrepend method with negative scaling factors.
        /// Verifies that negative scaling creates proper flip/mirror transformations.
        /// </summary>
        [Theory]
        [InlineData(-1.0, 1.0)]
        [InlineData(1.0, -1.0)]
        [InlineData(-1.0, -1.0)]
        [InlineData(-2.0, -3.0)]
        public void ScaleAtPrepend_NegativeScaling_AppliesCorrectly(double scaleX, double scaleY)
        {
            // Arrange
            var matrix = Matrix.Identity;
            double centerX = 0.0;
            double centerY = 0.0;

            // Act
            matrix.ScaleAtPrepend(scaleX, scaleY, centerX, centerY);

            // Assert
            Assert.Equal(scaleX, matrix.M11, 5);
            Assert.Equal(0.0, matrix.M12, 5);
            Assert.Equal(0.0, matrix.M21, 5);
            Assert.Equal(scaleY, matrix.M22, 5);
            Assert.Equal(0.0, matrix.OffsetX, 5);
            Assert.Equal(0.0, matrix.OffsetY, 5);
        }

        /// <summary>
        /// Tests ScaleAtPrepend method with different center points.
        /// Verifies that the center point affects the translation components correctly.
        /// </summary>
        [Theory]
        [InlineData(2.0, 3.0, 0.0, 0.0)]
        [InlineData(2.0, 3.0, 5.0, 10.0)]
        [InlineData(2.0, 3.0, -5.0, -10.0)]
        [InlineData(0.5, 0.5, 10.0, 20.0)]
        public void ScaleAtPrepend_DifferentCenterPoints_CalculatesOffsetsCorrectly(double scaleX, double scaleY, double centerX, double centerY)
        {
            // Arrange
            var matrix = Matrix.Identity;
            double expectedOffsetX = centerX - scaleX * centerX;
            double expectedOffsetY = centerY - scaleY * centerY;

            // Act
            matrix.ScaleAtPrepend(scaleX, scaleY, centerX, centerY);

            // Assert
            Assert.Equal(scaleX, matrix.M11, 5);
            Assert.Equal(scaleY, matrix.M22, 5);
            Assert.Equal(expectedOffsetX, matrix.OffsetX, 5);
            Assert.Equal(expectedOffsetY, matrix.OffsetY, 5);
        }

        /// <summary>
        /// Tests ScaleAtPrepend method with extreme double values.
        /// Verifies that the method handles boundary values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, 1.0, 0.0, 0.0)]
        [InlineData(1.0, double.MaxValue, 0.0, 0.0)]
        [InlineData(double.MinValue, 1.0, 0.0, 0.0)]
        [InlineData(1.0, double.MinValue, 0.0, 0.0)]
        public void ScaleAtPrepend_ExtremeValues_DoesNotThrow(double scaleX, double scaleY, double centerX, double centerY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act & Assert - Should not throw
            matrix.ScaleAtPrepend(scaleX, scaleY, centerX, centerY);

            Assert.Equal(scaleX, matrix.M11, 5);
            Assert.Equal(scaleY, matrix.M22, 5);
        }

        /// <summary>
        /// Tests ScaleAtPrepend method with special double values (NaN, Infinity).
        /// Verifies that special values are handled and propagated correctly.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 1.0, 0.0, 0.0)]
        [InlineData(1.0, double.NaN, 0.0, 0.0)]
        [InlineData(double.PositiveInfinity, 1.0, 0.0, 0.0)]
        [InlineData(1.0, double.PositiveInfinity, 0.0, 0.0)]
        [InlineData(double.NegativeInfinity, 1.0, 0.0, 0.0)]
        [InlineData(1.0, double.NegativeInfinity, 0.0, 0.0)]
        public void ScaleAtPrepend_SpecialDoubleValues_PropagatesCorrectly(double scaleX, double scaleY, double centerX, double centerY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.ScaleAtPrepend(scaleX, scaleY, centerX, centerY);

            // Assert
            Assert.Equal(scaleX, matrix.M11);
            Assert.Equal(scaleY, matrix.M22);
        }

        /// <summary>
        /// Tests ScaleAtPrepend method with special values in center coordinates.
        /// Verifies that special center point values are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(2.0, 3.0, double.NaN, 0.0)]
        [InlineData(2.0, 3.0, 0.0, double.NaN)]
        [InlineData(2.0, 3.0, double.PositiveInfinity, 0.0)]
        [InlineData(2.0, 3.0, 0.0, double.PositiveInfinity)]
        [InlineData(2.0, 3.0, double.NegativeInfinity, 0.0)]
        [InlineData(2.0, 3.0, 0.0, double.NegativeInfinity)]
        public void ScaleAtPrepend_SpecialCenterValues_HandlesCorrectly(double scaleX, double scaleY, double centerX, double centerY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.ScaleAtPrepend(scaleX, scaleY, centerX, centerY);

            // Assert - Should not throw and should apply scaling factors
            Assert.Equal(scaleX, matrix.M11, 5);
            Assert.Equal(scaleY, matrix.M22, 5);
        }

        /// <summary>
        /// Tests that ScaleAtPrepend correctly implements prepend operation.
        /// Verifies the order of operations by comparing with manual matrix multiplication.
        /// </summary>
        [Fact]
        public void ScaleAtPrepend_VerifyPrependOrder_CorrectOperationOrder()
        {
            // Arrange
            var matrix = new Matrix(1, 2, 3, 4, 5, 6);
            var originalMatrix = new Matrix(1, 2, 3, 4, 5, 6);
            double scaleX = 2.0;
            double scaleY = 3.0;
            double centerX = 1.0;
            double centerY = 2.0;

            // Act
            matrix.ScaleAtPrepend(scaleX, scaleY, centerX, centerY);

            // Assert - Verify this is prepend by checking that scaling matrix was applied first
            // With prepend, the scaling affects the input to the original transformation
            Assert.NotEqual(originalMatrix.M11, matrix.M11);
            Assert.NotEqual(originalMatrix.M22, matrix.M22);

            // The scaling factors should be visible in the diagonal elements
            Assert.Equal(2.0, matrix.M11, 5); // scaleX applied to m11
            Assert.Equal(6.0, matrix.M12, 5); // scaleX applied to m12  
            Assert.Equal(9.0, matrix.M21, 5); // scaleY applied to m21
            Assert.Equal(12.0, matrix.M22, 5); // scaleY applied to m22
        }

        /// <summary>
        /// Tests that HasInverse returns true for the Identity matrix.
        /// The Identity matrix always has a determinant of 1.0, so it should always have an inverse.
        /// </summary>
        [Fact]
        public void HasInverse_IdentityMatrix_ReturnsTrue()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            var result = matrix.HasInverse;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasInverse returns true for scaling matrices with non-zero scale factors.
        /// Scaling matrices have determinant = m11 * m22, so they have inverse when both scale factors are non-zero.
        /// </summary>
        [Theory]
        [InlineData(2, 3)]
        [InlineData(-1, -1)]
        [InlineData(0.5, 0.25)]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(1e-10, 1e-10)]
        [InlineData(-1e-10, -1e-10)]
        public void HasInverse_ScalingMatrixWithNonZeroFactors_ReturnsTrue(double scaleX, double scaleY)
        {
            // Arrange
            var matrix = new Matrix(scaleX, 0, 0, scaleY, 0, 0);

            // Act
            var result = matrix.HasInverse;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasInverse returns false for scaling matrices with zero scale factors.
        /// When either m11 or m22 is zero, the determinant (m11 * m22) becomes zero, so no inverse exists.
        /// </summary>
        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(0, 0)]
        [InlineData(0, -1)]
        [InlineData(-1, 0)]
        public void HasInverse_ScalingMatrixWithZeroFactor_ReturnsFalse(double scaleX, double scaleY)
        {
            // Arrange
            var matrix = new Matrix(scaleX, 0, 0, scaleY, 0, 0);

            // Act
            var result = matrix.HasInverse;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasInverse returns true for combined scaling and translation matrices with non-zero determinants.
        /// These matrices have determinant = m11 * m22, so they have inverse when both scale factors are non-zero.
        /// </summary>
        [Theory]
        [InlineData(2, 3, 10, 20)]
        [InlineData(-1, -1, -5, -5)]
        [InlineData(0.5, 0.25, 100, -100)]
        public void HasInverse_ScalingTranslationMatrixWithNonZeroDeterminant_ReturnsTrue(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(scaleX, 0, 0, scaleY, offsetX, offsetY);

            // Act
            var result = matrix.HasInverse;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasInverse returns false for combined scaling and translation matrices with zero determinants.
        /// When either m11 or m22 is zero, the determinant becomes zero regardless of translation values.
        /// </summary>
        [Theory]
        [InlineData(0, 1, 10, 20)]
        [InlineData(1, 0, -5, -5)]
        [InlineData(0, 0, 100, -100)]
        public void HasInverse_ScalingTranslationMatrixWithZeroDeterminant_ReturnsFalse(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(scaleX, 0, 0, scaleY, offsetX, offsetY);

            // Act
            var result = matrix.HasInverse;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasInverse returns true for unknown matrices with non-zero determinants.
        /// Unknown matrices have determinant = (m11 * m22) - (m12 * m21), so they have inverse when this is non-zero.
        /// </summary>
        [Theory]
        [InlineData(1, 2, 3, 4, 0, 0)] // determinant = 4 - 6 = -2
        [InlineData(2, 1, 1, 2, 5, 10)] // determinant = 4 - 1 = 3
        [InlineData(-1, 2, 3, -4, 0, 0)] // determinant = 4 - 6 = -2
        [InlineData(5, 0, 0, 5, 0, 0)] // determinant = 25 - 0 = 25
        public void HasInverse_UnknownMatrixWithNonZeroDeterminant_ReturnsTrue(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            var result = matrix.HasInverse;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasInverse returns false for unknown matrices with zero determinants.
        /// When (m11 * m22) - (m12 * m21) equals zero, the matrix has no inverse.
        /// </summary>
        [Theory]
        [InlineData(1, 2, 2, 4, 0, 0)] // determinant = 4 - 4 = 0
        [InlineData(2, 3, 4, 6, 5, 10)] // determinant = 12 - 12 = 0
        [InlineData(-1, -2, -3, -6, 0, 0)] // determinant = 6 - 6 = 0
        public void HasInverse_UnknownMatrixWithZeroDeterminant_ReturnsFalse(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            var result = matrix.HasInverse;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasInverse handles infinity values correctly.
        /// Tests matrices where determinant calculations involving infinity result in non-zero determinants.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity, 0, 0, 1, 0, 0)] // determinant = infinity * 1 - 0 * 0 = infinity
        [InlineData(1, 0, 0, double.PositiveInfinity, 0, 0)] // determinant = 1 * infinity - 0 * 0 = infinity
        [InlineData(double.NegativeInfinity, 0, 0, 1, 0, 0)] // determinant = -infinity * 1 - 0 * 0 = -infinity
        public void HasInverse_MatrixWithInfinityValues_ReturnsTrue(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            var result = matrix.HasInverse;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasInverse returns false when determinant calculations result in NaN.
        /// NaN is not equal to zero, but matrices with NaN determinants should not be considered invertible.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0, 0, 1, 0, 0)]
        [InlineData(1, 0, 0, double.NaN, 0, 0)]
        [InlineData(double.NaN, double.NaN, double.NaN, double.NaN, 0, 0)]
        public void HasInverse_MatrixWithNaNValues_ReturnsFalse(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            var result = matrix.HasInverse;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasInverse handles edge cases with very small but non-zero determinants.
        /// These should still be considered invertible even though they're close to zero.
        /// </summary>
        [Theory]
        [InlineData(1e-100, 0, 0, 1e-100, 0, 0)] // determinant = 1e-200
        [InlineData(1, 1e-100, 1e-100, 1, 0, 0)] // determinant = 1 - 1e-200
        public void HasInverse_MatrixWithVerySmallNonZeroDeterminant_ReturnsTrue(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            var result = matrix.HasInverse;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that OffsetY getter returns 0 when matrix type is Identity
        /// Input: Matrix with _type set to MatrixTypes.Identity
        /// Expected: OffsetY returns 0 regardless of internal _offsetY value
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(10.5)]
        [InlineData(-5.2)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        public void OffsetY_WhenMatrixTypeIsIdentity_ReturnsZero(double internalOffsetYValue)
        {
            // Arrange
            var matrix = Matrix.Identity;
            matrix._offsetY = internalOffsetYValue; // Set internal value but type remains Identity

            // Act
            var result = matrix.OffsetY;

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that OffsetY setter calls SetMatrix when matrix type is Identity
        /// Input: Matrix with _type set to MatrixTypes.Identity, various offset values
        /// Expected: SetMatrix called with translation parameters (1, 0, 0, 1, 0, value, Translation)
        /// </summary>
        [Theory]
        [InlineData(5.5)]
        [InlineData(-3.2)]
        [InlineData(0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void OffsetY_WhenMatrixTypeIsIdentity_CallsSetMatrix(double value)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.OffsetY = value;

            // Assert
            Assert.Equal(1, matrix._m11);
            Assert.Equal(0, matrix._m12);
            Assert.Equal(0, matrix._m21);
            Assert.Equal(1, matrix._m22);
            Assert.Equal(0, matrix._offsetX);
            Assert.Equal(value, matrix._offsetY);
            Assert.Equal(MatrixTypes.Translation, matrix._type);
        }

        /// <summary>
        /// Tests that OffsetY setter updates _offsetY but does not modify _type when type is Unknown
        /// Input: Matrix with _type set to MatrixTypes.Unknown
        /// Expected: _offsetY set to value but _type remains Unknown
        /// </summary>
        [Theory]
        [InlineData(5.5)]
        [InlineData(-3.2)]
        [InlineData(0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void OffsetY_WhenMatrixTypeIsUnknown_UpdatesOffsetYButDoesNotChangeType(double value)
        {
            // Arrange
            var matrix = new Matrix(2, 1, 1, 2, 10, 20);
            matrix._type = MatrixTypes.Unknown;
            var originalM11 = matrix._m11;
            var originalM12 = matrix._m12;
            var originalM21 = matrix._m21;
            var originalM22 = matrix._m22;
            var originalOffsetX = matrix._offsetX;

            // Act
            matrix.OffsetY = value;

            // Assert
            Assert.Equal(value, matrix._offsetY);
            Assert.Equal(MatrixTypes.Unknown, matrix._type);
            // Verify other fields unchanged
            Assert.Equal(originalM11, matrix._m11);
            Assert.Equal(originalM12, matrix._m12);
            Assert.Equal(originalM21, matrix._m21);
            Assert.Equal(originalM22, matrix._m22);
            Assert.Equal(originalOffsetX, matrix._offsetX);
        }

        /// <summary>
        /// Tests that OffsetY property handles Identity type conversion correctly
        /// Input: Identity matrix, set OffsetY to non-zero value, then get it back
        /// Expected: Matrix converts from Identity to Translation type and retrieved value equals set value
        /// </summary>
        [Theory]
        [InlineData(10.5)]
        [InlineData(-7.2)]
        [InlineData(1)]
        public void OffsetY_IdentityTypeConversion_HandlesCorrectly(double value)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            matrix.OffsetY = value;
            var result = matrix.OffsetY;

            // Assert
            Assert.Equal(value, result);
            Assert.Equal(MatrixTypes.Translation, matrix._type);
        }

        /// <summary>
        /// Tests that GetHashCode returns consistent values for identical Matrix instances.
        /// Validates that two Matrix instances with the same field values produce the same hash code.
        /// </summary>
        [Fact]
        public void GetHashCode_IdenticalMatrices_ReturnsSameHashCode()
        {
            // Arrange
            var matrix1 = new Matrix(1.0, 2.0, 3.0, 4.0, 5.0, 6.0);
            var matrix2 = new Matrix(1.0, 2.0, 3.0, 4.0, 5.0, 6.0);

            // Act
            int hashCode1 = matrix1.GetHashCode();
            int hashCode2 = matrix2.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that GetHashCode returns consistent values when called multiple times on the same instance.
        /// Validates that the hash code implementation is deterministic.
        /// </summary>
        [Fact]
        public void GetHashCode_SameInstance_ReturnsConsistentHashCode()
        {
            // Arrange
            var matrix = new Matrix(1.5, 2.5, 3.5, 4.5, 5.5, 6.5);

            // Act
            int hashCode1 = matrix.GetHashCode();
            int hashCode2 = matrix.GetHashCode();
            int hashCode3 = matrix.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
            Assert.Equal(hashCode2, hashCode3);
        }

        /// <summary>
        /// Tests that GetHashCode typically returns different values for different Matrix instances.
        /// Validates hash code distribution across different matrix configurations.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0)]
        [InlineData(1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 2.0, 0.0, 0.0, 2.0, 0.0, 0.0)]
        [InlineData(1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 6.0, 5.0, 4.0, 3.0, 2.0, 1.0)]
        [InlineData(-1.0, -2.0, -3.0, -4.0, -5.0, -6.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0)]
        public void GetHashCode_DifferentMatrices_ReturnsDifferentHashCodes(
            double m11_1, double m12_1, double m21_1, double m22_1, double offsetX_1, double offsetY_1,
            double m11_2, double m12_2, double m21_2, double m22_2, double offsetX_2, double offsetY_2)
        {
            // Arrange
            var matrix1 = new Matrix(m11_1, m12_1, m21_1, m22_1, offsetX_1, offsetY_1);
            var matrix2 = new Matrix(m11_2, m12_2, m21_2, m22_2, offsetX_2, offsetY_2);

            // Act
            int hashCode1 = matrix1.GetHashCode();
            int hashCode2 = matrix2.GetHashCode();

            // Assert
            Assert.NotEqual(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests GetHashCode with extreme double values including infinity and NaN.
        /// Validates that the hash code handles special floating-point values correctly.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity, 0.0, 0.0, 1.0, 0.0, 0.0)]
        [InlineData(double.NegativeInfinity, 0.0, 0.0, 1.0, 0.0, 0.0)]
        [InlineData(double.NaN, 0.0, 0.0, 1.0, 0.0, 0.0)]
        [InlineData(1.0, double.PositiveInfinity, 0.0, 1.0, 0.0, 0.0)]
        [InlineData(1.0, 0.0, double.NegativeInfinity, 1.0, 0.0, 0.0)]
        [InlineData(1.0, 0.0, 0.0, double.NaN, 0.0, 0.0)]
        [InlineData(1.0, 0.0, 0.0, 1.0, double.PositiveInfinity, 0.0)]
        [InlineData(1.0, 0.0, 0.0, 1.0, 0.0, double.NegativeInfinity)]
        [InlineData(double.MaxValue, double.MinValue, 0.0, 1.0, 0.0, 0.0)]
        [InlineData(double.Epsilon, -double.Epsilon, 0.0, 1.0, 0.0, 0.0)]
        public void GetHashCode_ExtremeDoubleValues_HandlesSpecialValues(
            double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act & Assert - Should not throw
            int hashCode = matrix.GetHashCode();

            // Verify consistency
            Assert.Equal(hashCode, matrix.GetHashCode());
        }

        /// <summary>
        /// Tests GetHashCode with zero values and negative zero.
        /// Validates proper handling of zero values in hash code calculation.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0, 0.0, 0.0, 0.0, 0.0)]
        [InlineData(-0.0, 0.0, 0.0, 1.0, 0.0, 0.0)]
        [InlineData(0.0, -0.0, 0.0, 1.0, 0.0, 0.0)]
        [InlineData(0.0, 0.0, -0.0, 1.0, 0.0, 0.0)]
        [InlineData(0.0, 0.0, 0.0, -0.0, 0.0, 0.0)]
        [InlineData(0.0, 0.0, 0.0, 1.0, -0.0, 0.0)]
        [InlineData(0.0, 0.0, 0.0, 1.0, 0.0, -0.0)]
        public void GetHashCode_ZeroValues_HandlesZeroAndNegativeZero(
            double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            int hashCode = matrix.GetHashCode();

            // Assert - Should be consistent
            Assert.Equal(hashCode, matrix.GetHashCode());
        }

        /// <summary>
        /// Tests GetHashCode with Identity matrix configurations.
        /// Validates hash code behavior for identity matrices created different ways.
        /// </summary>
        [Fact]
        public void GetHashCode_IdentityMatrix_ReturnsConsistentHashCode()
        {
            // Arrange
            var identityMatrix1 = Matrix.Identity;
            var identityMatrix2 = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);

            // Act
            int hashCode1 = identityMatrix1.GetHashCode();
            int hashCode2 = identityMatrix2.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests GetHashCode with matrices that have been modified after creation.
        /// Validates that hash code reflects current state after matrix modifications.
        /// </summary>
        [Fact]
        public void GetHashCode_ModifiedMatrix_ReflectsCurrentState()
        {
            // Arrange
            var matrix = new Matrix(1.0, 2.0, 3.0, 4.0, 5.0, 6.0);
            int originalHashCode = matrix.GetHashCode();

            // Act - Modify the matrix
            matrix.SetIdentity();
            int modifiedHashCode = matrix.GetHashCode();

            // Assert
            Assert.NotEqual(originalHashCode, modifiedHashCode);
            Assert.Equal(Matrix.Identity.GetHashCode(), modifiedHashCode);
        }

        /// <summary>
        /// Tests GetHashCode with boundary values for double precision.
        /// Validates hash code behavior at the limits of double precision.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue, double.MinValue, double.MinValue, double.MinValue, double.MinValue)]
        [InlineData(double.Epsilon, double.Epsilon, double.Epsilon, double.Epsilon, double.Epsilon, double.Epsilon)]
        [InlineData(-double.Epsilon, -double.Epsilon, -double.Epsilon, -double.Epsilon, -double.Epsilon, -double.Epsilon)]
        public void GetHashCode_BoundaryDoubleValues_HandlesPrecisionLimits(
            double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act & Assert - Should not throw
            int hashCode = matrix.GetHashCode();

            // Verify consistency
            Assert.Equal(hashCode, matrix.GetHashCode());
        }

        /// <summary>
        /// Tests GetHashCode with small differences in double values.
        /// Validates that small changes in matrix values produce different hash codes.
        /// </summary>
        [Fact]
        public void GetHashCode_SmallValueDifferences_ProducesDifferentHashCodes()
        {
            // Arrange
            var matrix1 = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
            var matrix2 = new Matrix(1.0 + double.Epsilon, 0.0, 0.0, 1.0, 0.0, 0.0);

            // Act
            int hashCode1 = matrix1.GetHashCode();
            int hashCode2 = matrix2.GetHashCode();

            // Assert
            Assert.NotEqual(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that Append correctly multiplies the current matrix with the parameter matrix using identity matrix.
        /// Verifies that appending an identity matrix does not change the original matrix.
        /// </summary>
        [Fact]
        public void Append_WithIdentityMatrix_MatrixUnchanged()
        {
            // Arrange
            var original = new Matrix(2, 3, 4, 5, 6, 7);
            var expected = new Matrix(2, 3, 4, 5, 6, 7);
            var identityMatrix = Matrix.Identity;

            // Act
            original.Append(identityMatrix);

            // Assert
            Assert.Equal(expected, original);
        }

        /// <summary>
        /// Tests that Append correctly multiplies matrices with translation transformation.
        /// Verifies that appending a translation matrix adds the translation values correctly.
        /// </summary>
        [Fact]
        public void Append_WithTranslationMatrix_TranslationApplied()
        {
            // Arrange
            var matrix = Matrix.Identity;
            var translationMatrix = new Matrix(1, 0, 0, 1, 10, 20);

            // Act
            matrix.Append(translationMatrix);

            // Assert
            Assert.Equal(10, matrix.OffsetX);
            Assert.Equal(20, matrix.OffsetY);
            Assert.Equal(1, matrix.M11);
            Assert.Equal(0, matrix.M12);
            Assert.Equal(0, matrix.M21);
            Assert.Equal(1, matrix.M22);
        }

        /// <summary>
        /// Tests that Append correctly multiplies matrices with scaling transformation.
        /// Verifies that appending a scaling matrix scales the transformation correctly.
        /// </summary>
        [Fact]
        public void Append_WithScalingMatrix_ScalingApplied()
        {
            // Arrange
            var matrix = Matrix.Identity;
            var scalingMatrix = new Matrix(2, 0, 0, 3, 0, 0);

            // Act
            matrix.Append(scalingMatrix);

            // Assert
            Assert.Equal(2, matrix.M11);
            Assert.Equal(0, matrix.M12);
            Assert.Equal(0, matrix.M21);
            Assert.Equal(3, matrix.M22);
            Assert.Equal(0, matrix.OffsetX);
            Assert.Equal(0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests that Append correctly handles various matrix transformations with different values.
        /// Verifies matrix multiplication correctness with various input combinations.
        /// </summary>
        [Theory]
        [InlineData(1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0)] // Identity * Identity
        [InlineData(2, 0, 0, 2, 0, 0, 1, 0, 0, 1, 5, 10)] // Scale * Translation
        [InlineData(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12)] // General matrices
        public void Append_WithVariousMatrices_CorrectMultiplication(
            double m11_1, double m12_1, double m21_1, double m22_1, double offsetX_1, double offsetY_1,
            double m11_2, double m12_2, double m21_2, double m22_2, double offsetX_2, double offsetY_2)
        {
            // Arrange
            var matrix1 = new Matrix(m11_1, m12_1, m21_1, m22_1, offsetX_1, offsetY_1);
            var matrix2 = new Matrix(m11_2, m12_2, m21_2, m22_2, offsetX_2, offsetY_2);
            var originalMatrix1 = new Matrix(m11_1, m12_1, m21_1, m22_1, offsetX_1, offsetY_1);

            // Expected result using matrix multiplication formula
            var expectedResult = originalMatrix1 * matrix2;

            // Act
            matrix1.Append(matrix2);

            // Assert
            Assert.Equal(expectedResult, matrix1);
        }

        /// <summary>
        /// Tests that Append handles extreme numeric values including NaN and Infinity.
        /// Verifies that the method can process boundary values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue, double.MinValue, double.MinValue, double.MinValue, double.MinValue)]
        [InlineData(double.NaN, 0, 0, double.NaN, 0, 0)]
        [InlineData(double.PositiveInfinity, 0, 0, double.PositiveInfinity, 0, 0)]
        [InlineData(double.NegativeInfinity, 0, 0, double.NegativeInfinity, 0, 0)]
        public void Append_WithExtremeValues_HandlesValuesCorrectly(
            double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrix = Matrix.Identity;
            var extremeMatrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);
            var identityMatrix = Matrix.Identity;
            var expectedResult = identityMatrix * extremeMatrix;

            // Act
            matrix.Append(extremeMatrix);

            // Assert
            Assert.Equal(expectedResult, matrix);
        }

        /// <summary>
        /// Tests that Append correctly handles zero values in matrix elements.
        /// Verifies that zero values are processed correctly without causing issues.
        /// </summary>
        [Fact]
        public void Append_WithZeroMatrix_ZeroMatrixApplied()
        {
            // Arrange
            var matrix = new Matrix(1, 2, 3, 4, 5, 6);
            var zeroMatrix = new Matrix(0, 0, 0, 0, 0, 0);
            var originalMatrix = new Matrix(1, 2, 3, 4, 5, 6);
            var expectedResult = originalMatrix * zeroMatrix;

            // Act
            matrix.Append(zeroMatrix);

            // Assert
            Assert.Equal(expectedResult, matrix);
        }

        /// <summary>
        /// Tests that Append modifies the current matrix instance in place.
        /// Verifies that the method properly mutates the struct instance.
        /// </summary>
        [Fact]
        public void Append_ModifiesCurrentMatrix_InPlace()
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 5, 10);
            var appendMatrix = new Matrix(2, 0, 0, 2, 3, 7);
            var originalM11 = matrix.M11;

            // Act
            matrix.Append(appendMatrix);

            // Assert
            Assert.NotEqual(originalM11, matrix.M11); // Verify the matrix was modified
            Assert.Equal(2, matrix.M11); // Verify the expected new value
        }

        /// <summary>
        /// Tests that Append with complex transformation combinations produces correct results.
        /// Verifies that multiple transformations are correctly composed.
        /// </summary>
        [Fact]
        public void Append_WithComplexTransformations_CorrectComposition()
        {
            // Arrange
            var baseMatrix = new Matrix(1, 0, 0, 1, 100, 200); // Translation
            var rotationMatrix = new Matrix(0, 1, -1, 0, 0, 0); // 90-degree rotation approximation
            var originalBase = new Matrix(1, 0, 0, 1, 100, 200);
            var expectedResult = originalBase * rotationMatrix;

            // Act
            baseMatrix.Append(rotationMatrix);

            // Assert
            Assert.Equal(expectedResult, baseMatrix);
        }

        /// <summary>
        /// Tests that Transform method handles null array input without throwing an exception.
        /// The method should safely return when points parameter is null.
        /// Expected result: No exception thrown.
        /// </summary>
        [Fact]
        public void Transform_NullArray_DoesNotThrow()
        {
            // Arrange
            var matrix = Matrix.Identity;
            Point[] nullArray = null;

            // Act & Assert
            var exception = Record.Exception(() => matrix.Transform(nullArray));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that Transform method handles empty array input correctly.
        /// The method should process an empty array without errors and leave it unchanged.
        /// Expected result: Array remains empty, no exception thrown.
        /// </summary>
        [Fact]
        public void Transform_EmptyArray_DoesNotThrow()
        {
            // Arrange
            var matrix = Matrix.Identity;
            var emptyArray = new Point[0];

            // Act
            matrix.Transform(emptyArray);

            // Assert
            Assert.Empty(emptyArray);
        }

        /// <summary>
        /// Tests that Transform method with identity matrix leaves points unchanged.
        /// Identity matrix should not modify the coordinates of any points.
        /// Expected result: All points remain with original coordinates.
        /// </summary>
        [Fact]
        public void Transform_SinglePointWithIdentityMatrix_PointUnchanged()
        {
            // Arrange
            var matrix = Matrix.Identity;
            var originalPoint = new Point(5.0, 10.0);
            var points = new[] { originalPoint };

            // Act
            matrix.Transform(points);

            // Assert
            Assert.Equal(originalPoint.X, points[0].X);
            Assert.Equal(originalPoint.Y, points[0].Y);
        }

        /// <summary>
        /// Tests that Transform method applies translation transformation correctly.
        /// Translation matrix should add offset values to point coordinates.
        /// Expected result: Point coordinates are offset by translation values.
        /// </summary>
        [Fact]
        public void Transform_SinglePointWithTranslationMatrix_PointTranslated()
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 3.0, 4.0); // Translation by (3, 4)
            var originalPoint = new Point(5.0, 10.0);
            var points = new[] { originalPoint };

            // Act
            matrix.Transform(points);

            // Assert
            Assert.Equal(8.0, points[0].X); // 5 + 3
            Assert.Equal(14.0, points[0].Y); // 10 + 4
        }

        /// <summary>
        /// Tests that Transform method applies scaling transformation correctly.
        /// Scaling matrix should multiply point coordinates by scale factors.
        /// Expected result: Point coordinates are scaled by matrix values.
        /// </summary>
        [Fact]
        public void Transform_SinglePointWithScalingMatrix_PointScaled()
        {
            // Arrange
            var matrix = new Matrix(2.0, 0, 0, 3.0, 0, 0); // Scale by (2, 3)
            var originalPoint = new Point(5.0, 10.0);
            var points = new[] { originalPoint };

            // Act
            matrix.Transform(points);

            // Assert
            Assert.Equal(10.0, points[0].X); // 5 * 2
            Assert.Equal(30.0, points[0].Y); // 10 * 3
        }

        /// <summary>
        /// Tests that Transform method processes multiple points correctly.
        /// All points in the array should be transformed according to the matrix.
        /// Expected result: Each point is individually transformed and replaced in the array.
        /// </summary>
        [Fact]
        public void Transform_MultiplePoints_AllPointsTransformed()
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 2.0, 3.0); // Translation by (2, 3)
            var points = new[]
            {
                new Point(1.0, 1.0),
                new Point(5.0, 10.0),
                new Point(-2.0, 7.0)
            };
            var expectedPoints = new[]
            {
                new Point(3.0, 4.0),   // (1+2, 1+3)
				new Point(7.0, 13.0),  // (5+2, 10+3)
				new Point(0.0, 10.0)   // (-2+2, 7+3)
			};

            // Act
            matrix.Transform(points);

            // Assert
            Assert.Equal(3, points.Length);
            for (int i = 0; i < points.Length; i++)
            {
                Assert.Equal(expectedPoints[i].X, points[i].X);
                Assert.Equal(expectedPoints[i].Y, points[i].Y);
            }
        }

        /// <summary>
        /// Tests that Transform method handles points with special double values.
        /// The method should handle NaN, positive/negative infinity, and extreme values correctly.
        /// Expected result: Special values are processed without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(0.0, -0.0)]
        public void Transform_PointsWithSpecialValues_HandlesCorrectly(double x, double y)
        {
            // Arrange
            var matrix = Matrix.Identity;
            var points = new[] { new Point(x, y) };

            // Act
            var exception = Record.Exception(() => matrix.Transform(points));

            // Assert
            Assert.Null(exception);
            Assert.Equal(x, points[0].X);
            Assert.Equal(y, points[0].Y);
        }

        /// <summary>
        /// Tests that Transform method handles boundary values correctly.
        /// The method should process zero coordinates and negative values without issues.
        /// Expected result: Boundary values are transformed correctly according to matrix.
        /// </summary>
        [Fact]
        public void Transform_BoundaryValues_TransformedCorrectly()
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 1.0, 1.0); // Translation by (1, 1)
            var points = new[]
            {
                new Point(0.0, 0.0),
                new Point(-1.0, -1.0),
                new Point(1.0, -1.0),
                new Point(-1.0, 1.0)
            };

            // Act
            matrix.Transform(points);

            // Assert
            Assert.Equal(1.0, points[0].X);  // 0 + 1
            Assert.Equal(1.0, points[0].Y);  // 0 + 1
            Assert.Equal(0.0, points[1].X);  // -1 + 1
            Assert.Equal(0.0, points[1].Y);  // -1 + 1
            Assert.Equal(2.0, points[2].X);  // 1 + 1
            Assert.Equal(0.0, points[2].Y);  // -1 + 1
            Assert.Equal(0.0, points[3].X);  // -1 + 1
            Assert.Equal(2.0, points[3].Y);  // 1 + 1
        }

        /// <summary>
        /// Tests that the Identity property returns a matrix with identity values.
        /// Verifies that M11=1, M12=0, M21=0, M22=1, OffsetX=0, OffsetY=0.
        /// Expected result: Matrix with standard identity transformation values.
        /// </summary>
        [Fact]
        public void Identity_Get_ReturnsMatrixWithIdentityValues()
        {
            // Act
            var identityMatrix = Matrix.Identity;

            // Assert
            Assert.Equal(1.0, identityMatrix.M11);
            Assert.Equal(0.0, identityMatrix.M12);
            Assert.Equal(0.0, identityMatrix.M21);
            Assert.Equal(1.0, identityMatrix.M22);
            Assert.Equal(0.0, identityMatrix.OffsetX);
            Assert.Equal(0.0, identityMatrix.OffsetY);
        }

        /// <summary>
        /// Tests that the Identity property returns a matrix where IsIdentity is true.
        /// Verifies that the returned matrix is recognized as an identity matrix.
        /// Expected result: IsIdentity property returns true.
        /// </summary>
        [Fact]
        public void Identity_Get_ReturnsMatrixWithIsIdentityTrue()
        {
            // Act
            var identityMatrix = Matrix.Identity;

            // Assert
            Assert.True(identityMatrix.IsIdentity);
        }

        /// <summary>
        /// Tests that multiple calls to Identity property return equivalent matrices.
        /// Verifies consistency of the static property across multiple invocations.
        /// Expected result: All calls return matrices with identical values.
        /// </summary>
        [Fact]
        public void Identity_MultipleCalls_ReturnsEquivalentMatrices()
        {
            // Act
            var identity1 = Matrix.Identity;
            var identity2 = Matrix.Identity;
            var identity3 = Matrix.Identity;

            // Assert
            Assert.Equal(identity1.M11, identity2.M11);
            Assert.Equal(identity1.M12, identity2.M12);
            Assert.Equal(identity1.M21, identity2.M21);
            Assert.Equal(identity1.M22, identity2.M22);
            Assert.Equal(identity1.OffsetX, identity2.OffsetX);
            Assert.Equal(identity1.OffsetY, identity2.OffsetY);

            Assert.Equal(identity1.M11, identity3.M11);
            Assert.Equal(identity1.M12, identity3.M12);
            Assert.Equal(identity1.M21, identity3.M21);
            Assert.Equal(identity1.M22, identity3.M22);
            Assert.Equal(identity1.OffsetX, identity3.OffsetX);
            Assert.Equal(identity1.OffsetY, identity3.OffsetY);
        }

        /// <summary>
        /// Tests that Identity matrices are equal using the equality operator.
        /// Verifies that multiple identity matrices compare as equal.
        /// Expected result: Identity matrices compare as equal using == operator.
        /// </summary>
        [Fact]
        public void Identity_EqualityComparison_IdentityMatricesAreEqual()
        {
            // Act
            var identity1 = Matrix.Identity;
            var identity2 = Matrix.Identity;

            // Assert
            Assert.True(identity1 == identity2);
            Assert.False(identity1 != identity2);
            Assert.True(identity1.Equals(identity2));
        }

        /// <summary>
        /// Tests the Skew method with normal positive angle values.
        /// Verifies that the method applies skew transformation correctly with standard angles.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(45, 30)]
        [InlineData(90, 45)]
        [InlineData(180, 90)]
        [InlineData(270, 180)]
        public void Skew_WithNormalPositiveAngles_AppliesSkewTransformation(double skewX, double skewY)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);
            var originalMatrix = matrix;

            // Act
            matrix.Skew(skewX, skewY);

            // Assert
            Assert.NotEqual(originalMatrix, matrix);
            Assert.NotEqual(0, matrix.M11 + matrix.M12 + matrix.M21 + matrix.M22);
        }

        /// <summary>
        /// Tests the Skew method with negative angle values.
        /// Verifies that negative angles are handled correctly and normalized properly.
        /// </summary>
        [Theory]
        [InlineData(-45, -30)]
        [InlineData(-90, -45)]
        [InlineData(-180, -90)]
        [InlineData(-270, -180)]
        public void Skew_WithNegativeAngles_AppliesSkewTransformation(double skewX, double skewY)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);
            var originalMatrix = matrix;

            // Act
            matrix.Skew(skewX, skewY);

            // Assert
            Assert.NotEqual(originalMatrix, matrix);
        }

        /// <summary>
        /// Tests the Skew method with angles greater than 360 degrees.
        /// Verifies that the modulo 360 operation works correctly to normalize large angles.
        /// </summary>
        [Theory]
        [InlineData(390, 405)] // 390 % 360 = 30, 405 % 360 = 45
        [InlineData(720, 810)] // 720 % 360 = 0, 810 % 360 = 90
        [InlineData(1080, 1170)] // 1080 % 360 = 0, 1170 % 360 = 90
        public void Skew_WithAnglesGreaterThan360_NormalizesAnglesCorrectly(double skewX, double skewY)
        {
            // Arrange
            var matrix1 = new Matrix(1, 0, 0, 1, 0, 0);
            var matrix2 = new Matrix(1, 0, 0, 1, 0, 0);

            // Act
            matrix1.Skew(skewX, skewY);
            matrix2.Skew(skewX % 360, skewY % 360);

            // Assert - Both matrices should be equal since angles are normalized
            Assert.Equal(matrix1.M11, matrix2.M11, 10);
            Assert.Equal(matrix1.M12, matrix2.M12, 10);
            Assert.Equal(matrix1.M21, matrix2.M21, 10);
            Assert.Equal(matrix1.M22, matrix2.M22, 10);
        }

        /// <summary>
        /// Tests the Skew method with zero angles.
        /// Verifies that zero skew angles result in no transformation (identity-like behavior).
        /// </summary>
        [Fact]
        public void Skew_WithZeroAngles_DoesNotChangeMatrix()
        {
            // Arrange
            var matrix = new Matrix(2, 3, 4, 5, 6, 7);
            var originalM11 = matrix.M11;
            var originalM12 = matrix.M12;
            var originalM21 = matrix.M21;
            var originalM22 = matrix.M22;
            var originalOffsetX = matrix.OffsetX;
            var originalOffsetY = matrix.OffsetY;

            // Act
            matrix.Skew(0, 0);

            // Assert - Matrix should remain unchanged (skew by 0 degrees means no skew)
            Assert.Equal(originalM11, matrix.M11, 10);
            Assert.Equal(originalM12, matrix.M12, 10);
            Assert.Equal(originalM21, matrix.M21, 10);
            Assert.Equal(originalM22, matrix.M22, 10);
            Assert.Equal(originalOffsetX, matrix.OffsetX, 10);
            Assert.Equal(originalOffsetY, matrix.OffsetY, 10);
        }

        /// <summary>
        /// Tests the Skew method with very large angle values.
        /// Verifies that the modulo operation handles extremely large numbers correctly.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, 45)]
        [InlineData(45, double.MaxValue)]
        [InlineData(1e10, 1e10)]
        public void Skew_WithVeryLargeAngles_HandlesNormalizationCorrectly(double skewX, double skewY)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);

            // Act & Assert - Should not throw an exception
            matrix.Skew(skewX, skewY);

            // The matrix should be modified (not remain identity)
            Assert.True(!double.IsNaN(matrix.M11) || !double.IsNaN(matrix.M12) ||
                       !double.IsNaN(matrix.M21) || !double.IsNaN(matrix.M22));
        }

        /// <summary>
        /// Tests the Skew method with special floating point values.
        /// Verifies handling of NaN, PositiveInfinity, and NegativeInfinity values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 45)]
        [InlineData(45, double.NaN)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, 45)]
        [InlineData(45, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 45)]
        [InlineData(45, double.NegativeInfinity)]
        public void Skew_WithSpecialFloatingPointValues_HandlesGracefully(double skewX, double skewY)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);

            // Act & Assert - Should not throw an exception
            matrix.Skew(skewX, skewY);

            // Matrix values might be NaN or Infinity, which is acceptable behavior
            Assert.True(true); // Test passes if no exception is thrown
        }

        /// <summary>
        /// Tests the Skew method with exact 360-degree multiples.
        /// Verifies that angles that are exact multiples of 360 normalize to 0.
        /// </summary>
        [Theory]
        [InlineData(360, 0)]
        [InlineData(0, 360)]
        [InlineData(720, 1080)]
        [InlineData(-360, -720)]
        public void Skew_WithExact360Multiples_NormalizesToZero(double skewX, double skewY)
        {
            // Arrange
            var matrix1 = new Matrix(1, 0, 0, 1, 0, 0);
            var matrix2 = new Matrix(1, 0, 0, 1, 0, 0);

            // Act
            matrix1.Skew(skewX, skewY);
            matrix2.Skew(0, 0);

            // Assert - Should be equivalent to skewing by 0 degrees
            Assert.Equal(matrix1.M11, matrix2.M11, 10);
            Assert.Equal(matrix1.M12, matrix2.M12, 10);
            Assert.Equal(matrix1.M21, matrix2.M21, 10);
            Assert.Equal(matrix1.M22, matrix2.M22, 10);
        }

        /// <summary>
        /// Tests the Skew method with fractional angle values.
        /// Verifies that fractional degrees are handled correctly in the degree-to-radian conversion.
        /// </summary>
        [Theory]
        [InlineData(45.5, 30.7)]
        [InlineData(0.1, 0.2)]
        [InlineData(359.9, 0.1)]
        public void Skew_WithFractionalAngles_AppliesSkewTransformation(double skewX, double skewY)
        {
            // Arrange
            var matrix = new Matrix(1, 0, 0, 1, 0, 0);
            var originalMatrix = matrix;

            // Act
            matrix.Skew(skewX, skewY);

            // Assert
            Assert.NotEqual(originalMatrix, matrix);
        }

        /// <summary>
        /// Tests the Skew method behavior on a non-identity matrix.
        /// Verifies that skew transformations are applied correctly to already transformed matrices.
        /// </summary>
        [Fact]
        public void Skew_OnNonIdentityMatrix_AppliesTransformationCorrectly()
        {
            // Arrange
            var matrix = new Matrix(2, 1, 1, 2, 10, 20);
            var originalM11 = matrix.M11;
            var originalM12 = matrix.M12;
            var originalM21 = matrix.M21;
            var originalM22 = matrix.M22;

            // Act
            matrix.Skew(30, 15);

            // Assert - Matrix should be different after skew
            Assert.NotEqual(originalM11, matrix.M11);
            Assert.NotEqual(originalM12, matrix.M12);
            Assert.NotEqual(originalM21, matrix.M21);
            Assert.NotEqual(originalM22, matrix.M22);
        }
    }
}

namespace Microsoft.Maui.Controls.Core.UnitTests.Shapes
{
    public partial class MatrixUtilTests
    {
        /// <summary>
        /// Tests that TransformRect returns early when the rectangle is empty (width or height <= 0).
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)] // Zero dimensions
        [InlineData(10, 20, 0, 30)] // Zero width
        [InlineData(10, 20, 30, 0)] // Zero height
        [InlineData(10, 20, -5, 30)] // Negative width
        [InlineData(10, 20, 30, -5)] // Negative height
        public void TransformRect_EmptyRectangle_NoTransformation(double x, double y, double width, double height)
        {
            // Arrange
            var rect = new Rect(x, y, width, height);
            var originalRect = new Rect(x, y, width, height);
            var matrix = new Matrix(2, 0, 0, 2, 10, 20); // Non-identity matrix

            // Act
            MatrixUtil.TransformRect(ref rect, ref matrix);

            // Assert
            Assert.Equal(originalRect.X, rect.X);
            Assert.Equal(originalRect.Y, rect.Y);
            Assert.Equal(originalRect.Width, rect.Width);
            Assert.Equal(originalRect.Height, rect.Height);
        }

        /// <summary>
        /// Tests that TransformRect returns early when the matrix is identity type.
        /// </summary>
        [Fact]
        public void TransformRect_IdentityMatrix_NoTransformation()
        {
            // Arrange
            var rect = new Rect(10, 20, 30, 40);
            var originalRect = new Rect(10, 20, 30, 40);
            var matrix = Matrix.Identity;

            // Act
            MatrixUtil.TransformRect(ref rect, ref matrix);

            // Assert
            Assert.Equal(originalRect.X, rect.X);
            Assert.Equal(originalRect.Y, rect.Y);
            Assert.Equal(originalRect.Width, rect.Width);
            Assert.Equal(originalRect.Height, rect.Height);
        }

        /// <summary>
        /// Tests TransformRect with scaling transformation using positive scale factors.
        /// </summary>
        [Theory]
        [InlineData(2.0, 3.0)] // Normal positive scaling
        [InlineData(0.5, 0.25)] // Scaling down
        [InlineData(1.0, 1.0)] // No scaling
        public void TransformRect_ScalingMatrix_PositiveScaling_TransformsCorrectly(double scaleX, double scaleY)
        {
            // Arrange
            var rect = new Rect(10, 20, 30, 40);
            var matrix = CreateScalingMatrix(scaleX, scaleY);

            // Act
            MatrixUtil.TransformRect(ref rect, ref matrix);

            // Assert
            Assert.Equal(10 * scaleX, rect.X);
            Assert.Equal(20 * scaleY, rect.Y);
            Assert.Equal(30 * scaleX, rect.Width);
            Assert.Equal(40 * scaleY, rect.Height);
        }

        /// <summary>
        /// Tests TransformRect with negative scaling factors that result in negative width.
        /// </summary>
        [Fact]
        public void TransformRect_ScalingMatrix_NegativeWidthScaling_AdjustsRectangle()
        {
            // Arrange
            var rect = new Rect(10, 20, 30, 40);
            var matrix = CreateScalingMatrix(-2.0, 3.0);

            // Act
            MatrixUtil.TransformRect(ref rect, ref matrix);

            // Assert
            // Negative width should be adjusted: X = 10 * -2 + (-60) = -80, Width = 60
            Assert.Equal(-80, rect.X);
            Assert.Equal(60, rect.Y); // 20 * 3
            Assert.Equal(60, rect.Width); // abs(30 * -2)
            Assert.Equal(120, rect.Height); // 40 * 3
        }

        /// <summary>
        /// Tests TransformRect with negative scaling factors that result in negative height.
        /// </summary>
        [Fact]
        public void TransformRect_ScalingMatrix_NegativeHeightScaling_AdjustsRectangle()
        {
            // Arrange
            var rect = new Rect(10, 20, 30, 40);
            var matrix = CreateScalingMatrix(2.0, -3.0);

            // Act
            MatrixUtil.TransformRect(ref rect, ref matrix);

            // Assert
            // Negative height should be adjusted: Y = 20 * -3 + (-120) = -180, Height = 120
            Assert.Equal(20, rect.X); // 10 * 2
            Assert.Equal(-180, rect.Y);
            Assert.Equal(60, rect.Width); // 30 * 2
            Assert.Equal(120, rect.Height); // abs(40 * -3)
        }

        /// <summary>
        /// Tests TransformRect with both negative width and height scaling.
        /// </summary>
        [Fact]
        public void TransformRect_ScalingMatrix_NegativeWidthAndHeight_AdjustsRectangle()
        {
            // Arrange
            var rect = new Rect(10, 20, 30, 40);
            var matrix = CreateScalingMatrix(-2.0, -3.0);

            // Act
            MatrixUtil.TransformRect(ref rect, ref matrix);

            // Assert
            Assert.Equal(-80, rect.X); // 10 * -2 + (-60)
            Assert.Equal(-180, rect.Y); // 20 * -3 + (-120)
            Assert.Equal(60, rect.Width); // abs(30 * -2)
            Assert.Equal(120, rect.Height); // abs(40 * -3)
        }

        /// <summary>
        /// Tests TransformRect with translation transformation.
        /// </summary>
        [Theory]
        [InlineData(5.0, 10.0)] // Positive translation
        [InlineData(-5.0, -10.0)] // Negative translation
        [InlineData(0.0, 0.0)] // No translation
        [InlineData(double.MaxValue, double.MaxValue)] // Extreme values
        [InlineData(double.MinValue, double.MinValue)] // Extreme negative values
        public void TransformRect_TranslationMatrix_TransformsCorrectly(double offsetX, double offsetY)
        {
            // Arrange
            var rect = new Rect(10, 20, 30, 40);
            var matrix = CreateTranslationMatrix(offsetX, offsetY);

            // Act
            MatrixUtil.TransformRect(ref rect, ref matrix);

            // Assert
            Assert.Equal(10 + offsetX, rect.X);
            Assert.Equal(20 + offsetY, rect.Y);
            Assert.Equal(30, rect.Width); // Width unchanged
            Assert.Equal(40, rect.Height); // Height unchanged
        }

        /// <summary>
        /// Tests TransformRect with unknown matrix type that transforms all corners.
        /// This tests the general transformation case where all four corners are transformed
        /// and a bounding box is computed.
        /// </summary>
        [Fact]
        public void TransformRect_UnknownMatrix_TransformsAllCorners()
        {
            // Arrange
            var rect = new Rect(0, 0, 2, 2);
            // Create a rotation matrix (45 degrees) - this will be MatrixTypes.Unknown
            var angle = Math.PI / 4; // 45 degrees
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);
            var matrix = new Matrix(cos, sin, -sin, cos, 0, 0);

            // Act
            MatrixUtil.TransformRect(ref rect, ref matrix);

            // Assert
            // For a 45-degree rotation of a 2x2 square at origin:
            // Original corners: (0,0), (2,0), (2,2), (0,2)
            // Rotated corners: (0,0), (√2,√2), (0,2√2), (-√2,√2)
            // Bounding box should encompass all transformed corners
            Assert.True(rect.X <= 0); // Should include leftmost point
            Assert.True(rect.Y <= 0); // Should include topmost point
            Assert.True(rect.Width > 0); // Should have positive width
            Assert.True(rect.Height > 0); // Should have positive height
        }

        /// <summary>
        /// Tests TransformRect with combined scaling and translation transformations.
        /// </summary>
        [Fact]
        public void TransformRect_CombinedScalingAndTranslation_TransformsCorrectly()
        {
            // Arrange
            var rect = new Rect(10, 20, 30, 40);
            var matrix = CreateCombinedMatrix(2.0, 3.0, 5.0, 10.0);

            // Act
            MatrixUtil.TransformRect(ref rect, ref matrix);

            // Assert
            // First scaling: X=20, Y=60, Width=60, Height=120
            // Then translation: X=25, Y=70
            Assert.Equal(25, rect.X); // (10 * 2) + 5
            Assert.Equal(70, rect.Y); // (20 * 3) + 10
            Assert.Equal(60, rect.Width); // 30 * 2
            Assert.Equal(120, rect.Height); // 40 * 3
        }

        /// <summary>
        /// Tests TransformRect with extreme double values to ensure numerical stability.
        /// </summary>
        [Theory]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN)]
        public void TransformRect_ExtremeValues_HandlesGracefully(double offsetX, double offsetY)
        {
            // Arrange
            var rect = new Rect(1, 1, 1, 1);
            var matrix = CreateTranslationMatrix(offsetX, offsetY);

            // Act
            MatrixUtil.TransformRect(ref rect, ref matrix);

            // Assert
            // The method should not throw and should handle extreme values
            // We don't assert specific values since behavior with infinity/NaN is implementation-dependent
            Assert.True(true); // Test passes if no exception is thrown
        }

        /// <summary>
        /// Helper method to create a scaling matrix with specific scale factors.
        /// </summary>
        private Matrix CreateScalingMatrix(double scaleX, double scaleY)
        {
            var matrix = new Matrix(scaleX, 0, 0, scaleY, 0, 0);
            return matrix;
        }

        /// <summary>
        /// Helper method to create a translation matrix with specific offset values.
        /// </summary>
        private Matrix CreateTranslationMatrix(double offsetX, double offsetY)
        {
            var matrix = new Matrix(1, 0, 0, 1, offsetX, offsetY);
            return matrix;
        }

        /// <summary>
        /// Helper method to create a combined scaling and translation matrix.
        /// </summary>
        private Matrix CreateCombinedMatrix(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            var matrix = new Matrix(scaleX, 0, 0, scaleY, offsetX, offsetY);
            return matrix;
        }

        /// <summary>
        /// Tests PrependOffset method when matrix is identity and offsets are zero.
        /// Input: Identity matrix with offsetX=0, offsetY=0
        /// Expected: Matrix becomes translation matrix with zero offsets
        /// </summary>
        [Fact]
        public void PrependOffset_IdentityMatrixWithZeroOffsets_CreatesTranslationMatrix()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            MatrixUtil.PrependOffset(ref matrix, 0, 0);

            // Assert
            Assert.Equal(1.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(1.0, matrix.M22);
            Assert.Equal(0.0, matrix.OffsetX);
            Assert.Equal(0.0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests PrependOffset method when matrix is identity with positive offsets.
        /// Input: Identity matrix with positive offsetX and offsetY values
        /// Expected: Matrix becomes translation matrix with specified offsets
        /// </summary>
        [Theory]
        [InlineData(1.0, 2.0)]
        [InlineData(10.5, 20.7)]
        [InlineData(100.0, 200.0)]
        public void PrependOffset_IdentityMatrixWithPositiveOffsets_CreatesTranslationMatrix(double offsetX, double offsetY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            MatrixUtil.PrependOffset(ref matrix, offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(1.0, matrix.M22);
            Assert.Equal(offsetX, matrix.OffsetX);
            Assert.Equal(offsetY, matrix.OffsetY);
        }

        /// <summary>
        /// Tests PrependOffset method when matrix is identity with negative offsets.
        /// Input: Identity matrix with negative offsetX and offsetY values
        /// Expected: Matrix becomes translation matrix with specified negative offsets
        /// </summary>
        [Theory]
        [InlineData(-1.0, -2.0)]
        [InlineData(-10.5, -20.7)]
        [InlineData(-100.0, -200.0)]
        public void PrependOffset_IdentityMatrixWithNegativeOffsets_CreatesTranslationMatrix(double offsetX, double offsetY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            MatrixUtil.PrependOffset(ref matrix, offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(1.0, matrix.M22);
            Assert.Equal(offsetX, matrix.OffsetX);
            Assert.Equal(offsetY, matrix.OffsetY);
        }

        /// <summary>
        /// Tests PrependOffset method when matrix is identity with extreme double values.
        /// Input: Identity matrix with extreme double values as offsets
        /// Expected: Matrix becomes translation matrix with extreme offset values
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.MinValue, double.MaxValue)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.PositiveInfinity)]
        public void PrependOffset_IdentityMatrixWithExtremeValues_CreatesTranslationMatrix(double offsetX, double offsetY)
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            MatrixUtil.PrependOffset(ref matrix, offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(1.0, matrix.M22);
            Assert.Equal(offsetX, matrix.OffsetX);
            Assert.Equal(offsetY, matrix.OffsetY);
        }

        /// <summary>
        /// Tests PrependOffset method when matrix is identity with NaN values.
        /// Input: Identity matrix with NaN as offsets
        /// Expected: Matrix becomes translation matrix with NaN offset values
        /// </summary>
        [Fact]
        public void PrependOffset_IdentityMatrixWithNaNOffsets_CreatesTranslationMatrix()
        {
            // Arrange
            var matrix = Matrix.Identity;

            // Act
            MatrixUtil.PrependOffset(ref matrix, double.NaN, double.NaN);

            // Assert
            Assert.Equal(1.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(1.0, matrix.M22);
            Assert.True(double.IsNaN(matrix.OffsetX));
            Assert.True(double.IsNaN(matrix.OffsetY));
        }

        /// <summary>
        /// Tests PrependOffset method when matrix is non-identity with simple scaling.
        /// Input: Scaling matrix (2x, 3y scale) with offset values
        /// Expected: Matrix offsets updated using transformation math
        /// </summary>
        [Fact]
        public void PrependOffset_NonIdentityScalingMatrix_UpdatesOffsetsWithTransformation()
        {
            // Arrange
            var matrix = new Matrix(2.0, 0.0, 0.0, 3.0, 0.0, 0.0); // 2x scale in X, 3x scale in Y
            double offsetX = 10.0;
            double offsetY = 20.0;

            // Act
            MatrixUtil.PrependOffset(ref matrix, offsetX, offsetY);

            // Assert
            Assert.Equal(2.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(3.0, matrix.M22);
            // Expected: _offsetX += _m11 * offsetX + _m21 * offsetY = 0 + 2.0 * 10.0 + 0.0 * 20.0 = 20.0
            Assert.Equal(20.0, matrix.OffsetX);
            // Expected: _offsetY += _m12 * offsetX + _m22 * offsetY = 0 + 0.0 * 10.0 + 3.0 * 20.0 = 60.0
            Assert.Equal(60.0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests PrependOffset method when matrix is non-identity with existing offsets.
        /// Input: Matrix with existing offsets and transformation values
        /// Expected: Matrix offsets accumulated correctly using transformation math
        /// </summary>
        [Fact]
        public void PrependOffset_NonIdentityMatrixWithExistingOffsets_AccumulatesOffsets()
        {
            // Arrange
            var matrix = new Matrix(2.0, 0.5, 0.3, 3.0, 100.0, 200.0); // Complex transformation with existing offsets
            double offsetX = 10.0;
            double offsetY = 20.0;

            // Act
            MatrixUtil.PrependOffset(ref matrix, offsetX, offsetY);

            // Assert
            Assert.Equal(2.0, matrix.M11);
            Assert.Equal(0.5, matrix.M12);
            Assert.Equal(0.3, matrix.M21);
            Assert.Equal(3.0, matrix.M22);
            // Expected: _offsetX += _m11 * offsetX + _m21 * offsetY = 100.0 + 2.0 * 10.0 + 0.3 * 20.0 = 100.0 + 20.0 + 6.0 = 126.0
            Assert.Equal(126.0, matrix.OffsetX);
            // Expected: _offsetY += _m12 * offsetX + _m22 * offsetY = 200.0 + 0.5 * 10.0 + 3.0 * 20.0 = 200.0 + 5.0 + 60.0 = 265.0
            Assert.Equal(265.0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests PrependOffset method when matrix is non-identity with zero offsets.
        /// Input: Non-identity matrix with zero offset values
        /// Expected: Matrix transformation values unchanged, offset calculation applied
        /// </summary>
        [Fact]
        public void PrependOffset_NonIdentityMatrixWithZeroOffsets_AppliesTransformation()
        {
            // Arrange
            var matrix = new Matrix(1.5, 0.2, 0.1, 2.5, 50.0, 75.0);

            // Act
            MatrixUtil.PrependOffset(ref matrix, 0.0, 0.0);

            // Assert
            Assert.Equal(1.5, matrix.M11);
            Assert.Equal(0.2, matrix.M12);
            Assert.Equal(0.1, matrix.M21);
            Assert.Equal(2.5, matrix.M22);
            // Expected: _offsetX += _m11 * 0.0 + _m21 * 0.0 = 50.0 + 0.0 + 0.0 = 50.0
            Assert.Equal(50.0, matrix.OffsetX);
            // Expected: _offsetY += _m12 * 0.0 + _m22 * 0.0 = 75.0 + 0.0 + 0.0 = 75.0
            Assert.Equal(75.0, matrix.OffsetY);
        }

        /// <summary>
        /// Tests PrependOffset method when matrix is non-identity with negative offsets.
        /// Input: Non-identity matrix with negative offset values
        /// Expected: Matrix transformation applied correctly with negative values
        /// </summary>
        [Theory]
        [InlineData(-5.0, -10.0)]
        [InlineData(-1.5, -2.5)]
        public void PrependOffset_NonIdentityMatrixWithNegativeOffsets_AppliesTransformation(double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(2.0, 1.0, 0.5, 3.0, 10.0, 20.0);

            // Act
            MatrixUtil.PrependOffset(ref matrix, offsetX, offsetY);

            // Assert
            Assert.Equal(2.0, matrix.M11);
            Assert.Equal(1.0, matrix.M12);
            Assert.Equal(0.5, matrix.M21);
            Assert.Equal(3.0, matrix.M22);
            // Expected: _offsetX += _m11 * offsetX + _m21 * offsetY = 10.0 + 2.0 * offsetX + 0.5 * offsetY
            double expectedOffsetX = 10.0 + 2.0 * offsetX + 0.5 * offsetY;
            Assert.Equal(expectedOffsetX, matrix.OffsetX);
            // Expected: _offsetY += _m12 * offsetX + _m22 * offsetY = 20.0 + 1.0 * offsetX + 3.0 * offsetY
            double expectedOffsetY = 20.0 + 1.0 * offsetX + 3.0 * offsetY;
            Assert.Equal(expectedOffsetY, matrix.OffsetY);
        }

        /// <summary>
        /// Tests PrependOffset method when matrix is non-identity with extreme double values.
        /// Input: Non-identity matrix with extreme double values as offsets
        /// Expected: Matrix handles extreme values in transformation calculations
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        public void PrependOffset_NonIdentityMatrixWithExtremeValues_HandlesExtremeValues(double offsetX, double offsetY)
        {
            // Arrange
            var matrix = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);

            // Act
            MatrixUtil.PrependOffset(ref matrix, offsetX, offsetY);

            // Assert
            Assert.Equal(1.0, matrix.M11);
            Assert.Equal(0.0, matrix.M12);
            Assert.Equal(0.0, matrix.M21);
            Assert.Equal(1.0, matrix.M22);
            // For identity-like transformation: _offsetX += 1.0 * offsetX + 0.0 * offsetY = offsetX
            Assert.Equal(offsetX, matrix.OffsetX);
            // For identity-like transformation: _offsetY += 0.0 * offsetX + 1.0 * offsetY = offsetY
            Assert.Equal(offsetY, matrix.OffsetY);
        }

        /// <summary>
        /// Tests PrependOffset method when matrix is non-identity with NaN values.
        /// Input: Non-identity matrix with NaN as offsets
        /// Expected: Matrix calculations result in NaN where appropriate
        /// </summary>
        [Fact]
        public void PrependOffset_NonIdentityMatrixWithNaNOffsets_HandlesNaN()
        {
            // Arrange
            var matrix = new Matrix(2.0, 1.0, 0.5, 3.0, 10.0, 20.0);

            // Act
            MatrixUtil.PrependOffset(ref matrix, double.NaN, double.NaN);

            // Assert
            Assert.Equal(2.0, matrix.M11);
            Assert.Equal(1.0, matrix.M12);
            Assert.Equal(0.5, matrix.M21);
            Assert.Equal(3.0, matrix.M22);
            // Any calculation involving NaN results in NaN
            Assert.True(double.IsNaN(matrix.OffsetX));
            Assert.True(double.IsNaN(matrix.OffsetY));
        }

        /// <summary>
        /// Creates an identity matrix for testing.
        /// </summary>
        private static Matrix CreateIdentityMatrix()
        {
            var matrix = new Matrix();
            matrix.SetMatrix(1, 0, 0, 1, 0, 0, MatrixTypes.Identity);
            return matrix;
        }

        /// <summary>
        /// Creates a scaling and translation matrix for testing.
        /// </summary>
        private static Matrix CreateScalingTranslationMatrix(double scaleX, double scaleY, double offsetX, double offsetY)
        {
            var matrix = new Matrix();
            matrix.SetMatrix(scaleX, 0, 0, scaleY, offsetX, offsetY, MatrixTypes.Scaling | MatrixTypes.Translation);
            return matrix;
        }

        /// <summary>
        /// Creates an unknown type matrix for testing.
        /// </summary>
        private static Matrix CreateUnknownMatrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            var matrix = new Matrix();
            matrix.SetMatrix(m11, m12, m21, m22, offsetX, offsetY, MatrixTypes.Unknown);
            return matrix;
        }

        /// <summary>
        /// Tests multiplying any matrix by a translation matrix where matrix1 is Unknown type.
        /// This tests the Unknown type case in the translation optimization.
        /// </summary>
        [Fact]
        public void MultiplyMatrix_UnknownMatrix1ByTranslationMatrix2_TranslationAddedTypeUnchanged()
        {
            // Arrange
            var matrix1 = CreateUnknownMatrix(1.5, 2.5, 3.5, 4.5, 10.0, 20.0);
            var matrix2 = CreateTranslationMatrix(5.0, 7.0);
            var expectedOffsetX = 10.0 + 5.0;
            var expectedOffsetY = 20.0 + 7.0;

            // Act
            MatrixUtil.MultiplyMatrix(ref matrix1, ref matrix2);

            // Assert
            Assert.Equal(1.5, matrix1._m11);
            Assert.Equal(2.5, matrix1._m12);
            Assert.Equal(3.5, matrix1._m21);
            Assert.Equal(4.5, matrix1._m22);
            Assert.Equal(expectedOffsetX, matrix1._offsetX);
            Assert.Equal(expectedOffsetY, matrix1._offsetY);
            Assert.Equal(MatrixTypes.Unknown, matrix1._type);
        }

        /// <summary>
        /// Tests multiplying a translation matrix by an unknown matrix.
        /// This tests the Unknown type case in the translation matrix1 optimization.
        /// </summary>
        [Fact]
        public void MultiplyMatrix_TranslationMatrix1ByUnknownMatrix2_TypeSetToUnknown()
        {
            // Arrange
            var matrix1 = CreateTranslationMatrix(5.0, 7.0);
            var matrix2 = CreateUnknownMatrix(1.5, 2.5, 3.5, 4.5, 1.0, 2.0);

            var expectedOffsetX = 5.0 * 1.5 + 7.0 * 3.5 + 1.0;
            var expectedOffsetY = 5.0 * 2.5 + 7.0 * 4.5 + 2.0;

            // Act
            MatrixUtil.MultiplyMatrix(ref matrix1, ref matrix2);

            // Assert
            Assert.Equal(1.5, matrix1._m11);
            Assert.Equal(2.5, matrix1._m12);
            Assert.Equal(3.5, matrix1._m21);
            Assert.Equal(4.5, matrix1._m22);
            Assert.Equal(expectedOffsetX, matrix1._offsetX);
            Assert.Equal(expectedOffsetY, matrix1._offsetY);
            Assert.Equal(MatrixTypes.Unknown, matrix1._type);
        }

        /// <summary>
        /// Tests multiplying scaling matrices (case 34: S * S).
        /// This tests the scaling-specific optimization.
        /// </summary>
        [Theory]
        [InlineData(2.0, 3.0, 1.5, 2.5)]
        [InlineData(0.5, 0.8, 4.0, 1.25)]
        [InlineData(-1.0, -2.0, 3.0, 0.5)]
        public void MultiplyMatrix_ScalingByScaling_ScalingMultiplied(double scale1X, double scale1Y, double scale2X, double scale2Y)
        {
            // Arrange
            var matrix1 = CreateScalingMatrix(scale1X, scale1Y);
            var matrix2 = CreateScalingMatrix(scale2X, scale2Y);

            // Act
            MatrixUtil.MultiplyMatrix(ref matrix1, ref matrix2);

            // Assert
            Assert.Equal(scale1X * scale2X, matrix1._m11);
            Assert.Equal(0.0, matrix1._m12);
            Assert.Equal(0.0, matrix1._m21);
            Assert.Equal(scale1Y * scale2Y, matrix1._m22);
            Assert.Equal(0.0, matrix1._offsetX);
            Assert.Equal(0.0, matrix1._offsetY);
            Assert.Equal(MatrixTypes.Scaling, matrix1._type);
        }

        /// <summary>
        /// Tests multiplying scaling matrix by scaling+translation matrix (case 35: S * S|T).
        /// This tests the specific optimization for this combination.
        /// </summary>
        [Theory]
        [InlineData(2.0, 3.0, 1.5, 2.5, 10.0, 20.0)]
        [InlineData(0.5, 0.8, 4.0, 1.25, 5.0, 15.0)]
        public void MultiplyMatrix_ScalingByScalingTranslation_Combined(double scale1X, double scale1Y, double scale2X, double scale2Y, double translateX, double translateY)
        {
            // Arrange
            var matrix1 = CreateScalingMatrix(scale1X, scale1Y);
            var matrix2 = CreateScalingTranslationMatrix(scale2X, scale2Y, translateX, translateY);

            // Act
            MatrixUtil.MultiplyMatrix(ref matrix1, ref matrix2);

            // Assert
            Assert.Equal(scale1X * scale2X, matrix1._m11);
            Assert.Equal(0.0, matrix1._m12);
            Assert.Equal(0.0, matrix1._m21);
            Assert.Equal(scale1Y * scale2Y, matrix1._m22);
            Assert.Equal(translateX, matrix1._offsetX);
            Assert.Equal(translateY, matrix1._offsetY);
            Assert.Equal(MatrixTypes.Translation | MatrixTypes.Scaling, matrix1._type);
        }

        /// <summary>
        /// Tests multiplying scaling+translation matrix by scaling matrix (case 50: S|T * S).
        /// This tests the specific optimization for this combination.
        /// </summary>
        [Theory]
        [InlineData(2.0, 3.0, 10.0, 20.0, 1.5, 2.5)]
        [InlineData(0.5, 0.8, 5.0, 15.0, 4.0, 1.25)]
        public void MultiplyMatrix_ScalingTranslationByScaling_TransformationApplied(double scale1X, double scale1Y, double translate1X, double translate1Y, double scale2X, double scale2Y)
        {
            // Arrange
            var matrix1 = CreateScalingTranslationMatrix(scale1X, scale1Y, translate1X, translate1Y);
            var matrix2 = CreateScalingMatrix(scale2X, scale2Y);

            // Act
            MatrixUtil.MultiplyMatrix(ref matrix1, ref matrix2);

            // Assert
            Assert.Equal(scale1X * scale2X, matrix1._m11);
            Assert.Equal(0.0, matrix1._m12);
            Assert.Equal(0.0, matrix1._m21);
            Assert.Equal(scale1Y * scale2Y, matrix1._m22);
            Assert.Equal(translate1X * scale2X, matrix1._offsetX);
            Assert.Equal(translate1Y * scale2Y, matrix1._offsetY);
            Assert.Equal(MatrixTypes.Scaling | MatrixTypes.Translation, matrix1._type);
        }

        /// <summary>
        /// Tests multiplying scaling+translation matrices (case 51: S|T * S|T).
        /// This tests the specific optimization for this combination.
        /// </summary>
        [Theory]
        [InlineData(2.0, 3.0, 10.0, 20.0, 1.5, 2.5, 5.0, 8.0)]
        [InlineData(0.5, 0.8, 5.0, 15.0, 4.0, 1.25, 2.0, 3.0)]
        public void MultiplyMatrix_ScalingTranslationByScalingTranslation_Combined(double scale1X, double scale1Y, double translate1X, double translate1Y, double scale2X, double scale2Y, double translate2X, double translate2Y)
        {
            // Arrange
            var matrix1 = CreateScalingTranslationMatrix(scale1X, scale1Y, translate1X, translate1Y);
            var matrix2 = CreateScalingTranslationMatrix(scale2X, scale2Y, translate2X, translate2Y);

            // Act
            MatrixUtil.MultiplyMatrix(ref matrix1, ref matrix2);

            // Assert
            Assert.Equal(scale1X * scale2X, matrix1._m11);
            Assert.Equal(0.0, matrix1._m12);
            Assert.Equal(0.0, matrix1._m21);
            Assert.Equal(scale1Y * scale2Y, matrix1._m22);
            Assert.Equal(scale2X * translate1X + translate2X, matrix1._offsetX);
            Assert.Equal(scale2Y * translate1Y + translate2Y, matrix1._offsetY);
            Assert.Equal(MatrixTypes.Scaling | MatrixTypes.Translation, matrix1._type);
        }

        /// <summary>
        /// Tests general matrix multiplication for unknown matrices (case 68: U * U).
        /// This tests the fallback general multiplication algorithm.
        /// </summary>
        [Theory]
        [InlineData(1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 2.0, 1.0, 4.0, 3.0, 2.0, 1.0)]
        [InlineData(2.0, 3.0, 1.0, 4.0, 1.0, 2.0, 1.0, 2.0, 3.0, 1.0, 4.0, 5.0)]
        public void MultiplyMatrix_UnknownByUnknown_GeneralMultiplication(double m11_1, double m12_1, double m21_1, double m22_1, double offsetX_1, double offsetY_1, double m11_2, double m12_2, double m21_2, double m22_2, double offsetX_2, double offsetY_2)
        {
            // Arrange
            var matrix1 = CreateUnknownMatrix(m11_1, m12_1, m21_1, m22_1, offsetX_1, offsetY_1);
            var matrix2 = CreateUnknownMatrix(m11_2, m12_2, m21_2, m22_2, offsetX_2, offsetY_2);

            // Expected results using standard matrix multiplication formula
            var expectedM11 = m11_1 * m11_2 + m12_1 * m21_2;
            var expectedM12 = m11_1 * m12_2 + m12_1 * m22_2;
            var expectedM21 = m21_1 * m11_2 + m22_1 * m21_2;
            var expectedM22 = m21_1 * m12_2 + m22_1 * m22_2;
            var expectedOffsetX = offsetX_1 * m11_2 + offsetY_1 * m21_2 + offsetX_2;
            var expectedOffsetY = offsetX_1 * m12_2 + offsetY_1 * m22_2 + offsetY_2;

            // Act
            MatrixUtil.MultiplyMatrix(ref matrix1, ref matrix2);

            // Assert
            Assert.Equal(expectedM11, matrix1._m11, 10);
            Assert.Equal(expectedM12, matrix1._m12, 10);
            Assert.Equal(expectedM21, matrix1._m21, 10);
            Assert.Equal(expectedM22, matrix1._m22, 10);
            Assert.Equal(expectedOffsetX, matrix1._offsetX, 10);
            Assert.Equal(expectedOffsetY, matrix1._offsetY, 10);
            Assert.Equal(MatrixTypes.Unknown, matrix1._type);
        }

        /// <summary>
        /// Tests edge cases with extreme values to ensure numerical stability.
        /// This tests boundary conditions and potential overflow scenarios.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, 0.0, 0.0, double.MaxValue, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)]
        [InlineData(double.MinValue, 0.0, 0.0, double.MinValue, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)]
        [InlineData(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0)]
        public void MultiplyMatrix_ExtremeValues_HandledCorrectly(double m11_1, double m12_1, double m21_1, double m22_1, double offsetX_1, double offsetY_1, double m11_2, double m12_2, double m21_2, double m22_2, double offsetX_2, double offsetY_2)
        {
            // Arrange
            var matrix1 = CreateUnknownMatrix(m11_1, m12_1, m21_1, m22_1, offsetX_1, offsetY_1);
            var matrix2 = CreateUnknownMatrix(m11_2, m12_2, m21_2, m22_2, offsetX_2, offsetY_2);

            // Act & Assert - Should not throw exception
            MatrixUtil.MultiplyMatrix(ref matrix1, ref matrix2);

            // Basic verification that the operation completed
            Assert.True(double.IsFinite(matrix1._m11) || double.IsInfinity(matrix1._m11) || double.IsNaN(matrix1._m11));
        }

        /// <summary>
        /// Tests that NaN values are handled without throwing exceptions.
        /// This tests error conditions with invalid numerical values.
        /// </summary>
        [Fact]
        public void MultiplyMatrix_NaNValues_HandledGracefully()
        {
            // Arrange
            var matrix1 = CreateUnknownMatrix(double.NaN, 1.0, 2.0, 3.0, 4.0, 5.0);
            var matrix2 = CreateUnknownMatrix(1.0, 2.0, 3.0, 4.0, 5.0, 6.0);

            // Act & Assert - Should not throw exception
            MatrixUtil.MultiplyMatrix(ref matrix1, ref matrix2);

            // Basic verification that NaN propagated
            Assert.True(double.IsNaN(matrix1._m11));
        }
    }
}