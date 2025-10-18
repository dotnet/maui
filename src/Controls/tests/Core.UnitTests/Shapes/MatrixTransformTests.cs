#nullable disable

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using Xunit;


namespace Microsoft.Maui.Controls.Shapes.UnitTests
{
    /// <summary>
    /// Unit tests for MatrixTransform class.
    /// </summary>
    public class MatrixTransformTests
    {
        /// <summary>
        /// Tests that the Matrix property setter stores the provided value correctly
        /// and the getter retrieves the same value for normal matrix values.
        /// </summary>
        /// <param name="m11">Matrix element m11</param>
        /// <param name="m12">Matrix element m12</param>
        /// <param name="m21">Matrix element m21</param>
        /// <param name="m22">Matrix element m22</param>
        /// <param name="offsetX">Matrix offset X</param>
        /// <param name="offsetY">Matrix offset Y</param>
        [Theory]
        [InlineData(1.0, 0.0, 0.0, 1.0, 0.0, 0.0)] // Identity matrix
        [InlineData(2.0, 0.0, 0.0, 2.0, 0.0, 0.0)] // Scale matrix
        [InlineData(1.0, 0.0, 0.0, 1.0, 10.0, 20.0)] // Translation matrix
        [InlineData(-1.0, 0.0, 0.0, -1.0, 0.0, 0.0)] // Negative scale
        [InlineData(0.5, 0.5, -0.5, 0.5, 0.0, 0.0)] // Rotation matrix
        [InlineData(0.0, 0.0, 0.0, 0.0, 0.0, 0.0)] // Zero matrix
        public void Matrix_SetAndGet_NormalValues_ReturnsCorrectValue(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            // Arrange
            var matrixTransform = new MatrixTransform();
            var expectedMatrix = new Matrix(m11, m12, m21, m22, offsetX, offsetY);

            // Act
            matrixTransform.Matrix = expectedMatrix;
            var actualMatrix = matrixTransform.Matrix;

            // Assert
            Assert.Equal(expectedMatrix, actualMatrix);
        }

        /// <summary>
        /// Tests that the Matrix property setter handles extreme double values correctly
        /// and the getter retrieves the same values.
        /// </summary>
        /// <param name="value">The extreme double value to test</param>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.Epsilon)]
        [InlineData(-double.Epsilon)]
        public void Matrix_SetAndGet_ExtremeDoubleValues_ReturnsCorrectValue(double value)
        {
            // Arrange
            var matrixTransform = new MatrixTransform();
            var expectedMatrix = new Matrix(value, value, value, value, value, value);

            // Act
            matrixTransform.Matrix = expectedMatrix;
            var actualMatrix = matrixTransform.Matrix;

            // Assert
            Assert.Equal(expectedMatrix, actualMatrix);
        }

        /// <summary>
        /// Tests that the Matrix property correctly handles the static Identity matrix.
        /// </summary>
        [Fact]
        public void Matrix_SetAndGet_IdentityMatrix_ReturnsCorrectValue()
        {
            // Arrange
            var matrixTransform = new MatrixTransform();
            var expectedMatrix = Matrix.Identity;

            // Act
            matrixTransform.Matrix = expectedMatrix;
            var actualMatrix = matrixTransform.Matrix;

            // Assert
            Assert.Equal(expectedMatrix, actualMatrix);
        }

        /// <summary>
        /// Tests that the Matrix property correctly handles the default Matrix value.
        /// </summary>
        [Fact]
        public void Matrix_SetAndGet_DefaultMatrix_ReturnsCorrectValue()
        {
            // Arrange
            var matrixTransform = new MatrixTransform();
            var expectedMatrix = default(Matrix);

            // Act
            matrixTransform.Matrix = expectedMatrix;
            var actualMatrix = matrixTransform.Matrix;

            // Assert
            Assert.Equal(expectedMatrix, actualMatrix);
        }

        /// <summary>
        /// Tests that the Matrix property has an initial default value when created.
        /// </summary>
        [Fact]
        public void Matrix_Get_InitialValue_ReturnsDefaultMatrix()
        {
            // Arrange
            var matrixTransform = new MatrixTransform();

            // Act
            var actualMatrix = matrixTransform.Matrix;

            // Assert
            Assert.Equal(default(Matrix), actualMatrix);
        }

        /// <summary>
        /// Tests that multiple set operations on the Matrix property work correctly.
        /// </summary>
        [Fact]
        public void Matrix_SetMultipleTimes_ReturnsLastSetValue()
        {
            // Arrange
            var matrixTransform = new MatrixTransform();
            var firstMatrix = new Matrix(1.0, 2.0, 3.0, 4.0, 5.0, 6.0);
            var secondMatrix = new Matrix(6.0, 5.0, 4.0, 3.0, 2.0, 1.0);

            // Act
            matrixTransform.Matrix = firstMatrix;
            matrixTransform.Matrix = secondMatrix;
            var actualMatrix = matrixTransform.Matrix;

            // Assert
            Assert.Equal(secondMatrix, actualMatrix);
        }

        /// <summary>
        /// Tests that the Matrix property handles mixed extreme values correctly.
        /// </summary>
        [Fact]
        public void Matrix_SetAndGet_MixedExtremeValues_ReturnsCorrectValue()
        {
            // Arrange
            var matrixTransform = new MatrixTransform();
            var expectedMatrix = new Matrix(
                double.MaxValue,
                double.MinValue,
                double.NaN,
                double.PositiveInfinity,
                double.NegativeInfinity,
                0.0);

            // Act
            matrixTransform.Matrix = expectedMatrix;
            var actualMatrix = matrixTransform.Matrix;

            // Assert
            Assert.Equal(expectedMatrix, actualMatrix);
        }
    }
}
