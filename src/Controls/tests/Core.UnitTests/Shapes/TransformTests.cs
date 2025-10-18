#nullable disable

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System.ComponentModel;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the Transform class.
    /// </summary>
    public class TransformTests
    {
        /// <summary>
        /// Tests that setting the Value property with a custom matrix stores the value correctly.
        /// Input: Custom Matrix with specific transformation values.
        /// Expected: The getter returns the same matrix values that were set.
        /// </summary>
        [Fact]
        public void Value_SetCustomMatrix_StoresAndRetrievesCorrectly()
        {
            // Arrange
            var transform = new Transform();
            var customMatrix = new Matrix(1.5, 2.0, 3.0, 4.0, 5.0, 6.0);

            // Act
            transform.Value = customMatrix;

            // Assert
            Assert.Equal(customMatrix, transform.Value);
        }

        /// <summary>
        /// Tests that setting the Value property with Matrix.Identity stores the identity matrix correctly.
        /// Input: Matrix.Identity static property value.
        /// Expected: The getter returns the identity matrix.
        /// </summary>
        [Fact]
        public void Value_SetIdentityMatrix_StoresAndRetrievesCorrectly()
        {
            // Arrange
            var transform = new Transform();
            var identityMatrix = Matrix.Identity;

            // Act
            transform.Value = identityMatrix;

            // Assert
            Assert.Equal(identityMatrix, transform.Value);
        }

        /// <summary>
        /// Tests that setting the Value property with extreme double values handles them correctly.
        /// Input: Matrix with various extreme double values including infinity and NaN.
        /// Expected: The getter returns the same extreme values that were set.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MinValue, 0.0, 1.0, double.PositiveInfinity, double.NegativeInfinity)]
        [InlineData(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN)]
        [InlineData(0.0, 0.0, 0.0, 0.0, 0.0, 0.0)]
        [InlineData(-1.0, -2.0, -3.0, -4.0, -5.0, -6.0)]
        public void Value_SetMatrixWithExtremeValues_StoresAndRetrievesCorrectly(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var transform = new Transform();
            var extremeMatrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            transform.Value = extremeMatrix;

            // Assert
            Assert.Equal(extremeMatrix, transform.Value);
        }

        /// <summary>
        /// Tests that setting the Value property multiple times with different matrices updates the stored value correctly.
        /// Input: Sequential setting of different Matrix values.
        /// Expected: Each set operation updates the stored value, and the getter returns the most recently set value.
        /// </summary>
        [Fact]
        public void Value_SetMultipleDifferentMatrices_UpdatesValueCorrectly()
        {
            // Arrange
            var transform = new Transform();
            var firstMatrix = new Matrix(1.0, 0.0, 0.0, 1.0, 10.0, 20.0);
            var secondMatrix = new Matrix(2.0, 0.0, 0.0, 2.0, 30.0, 40.0);
            var thirdMatrix = Matrix.Identity;

            // Act & Assert - First matrix
            transform.Value = firstMatrix;
            Assert.Equal(firstMatrix, transform.Value);

            // Act & Assert - Second matrix
            transform.Value = secondMatrix;
            Assert.Equal(secondMatrix, transform.Value);

            // Act & Assert - Third matrix
            transform.Value = thirdMatrix;
            Assert.Equal(thirdMatrix, transform.Value);
        }

        /// <summary>
        /// Tests that setting the Value property with the same matrix value multiple times maintains consistency.
        /// Input: Same Matrix value set multiple times.
        /// Expected: The getter consistently returns the same matrix value after each set operation.
        /// </summary>
        [Fact]
        public void Value_SetSameMatrixMultipleTimes_MaintainsConsistency()
        {
            // Arrange
            var transform = new Transform();
            var matrix = new Matrix(1.2, 2.3, 3.4, 4.5, 5.6, 6.7);

            // Act & Assert - Set and verify multiple times
            transform.Value = matrix;
            Assert.Equal(matrix, transform.Value);

            transform.Value = matrix;
            Assert.Equal(matrix, transform.Value);

            transform.Value = matrix;
            Assert.Equal(matrix, transform.Value);
        }

        /// <summary>
        /// Tests that the Value property starts with a default matrix value when not explicitly set.
        /// Input: Newly created Transform instance without setting Value.
        /// Expected: The getter returns a default Matrix (equivalent to new Matrix()).
        /// </summary>
        [Fact]
        public void Value_DefaultValue_ReturnsDefaultMatrix()
        {
            // Arrange
            var transform = new Transform();
            var expectedDefaultMatrix = new Matrix();

            // Act
            var actualValue = transform.Value;

            // Assert
            Assert.Equal(expectedDefaultMatrix, actualValue);
        }
    }
}
