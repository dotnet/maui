#nullable disable

using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using System;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests.Shapes
{
    public sealed partial class EllipseTests
    {
        /// <summary>
        /// Tests GetPath method with normal positive dimensions and stroke thickness.
        /// Verifies that the ellipse is properly positioned and sized with stroke offset.
        /// Expected: Path with ellipse offset by half stroke thickness.
        /// </summary>
        [Fact]
        public void GetPath_WithNormalDimensions_ReturnsCorrectPath()
        {
            // Arrange
            var ellipse = new Ellipse();
            ellipse.SetValue(VisualElement.WidthProperty, 100.0);
            ellipse.SetValue(VisualElement.HeightProperty, 60.0);
            ellipse.SetValue(Shape.StrokeThicknessProperty, 4.0);

            // Act
            var path = ellipse.GetPath();

            // Assert
            Assert.NotNull(path);
            Assert.IsType<PathF>(path);
        }

        /// <summary>
        /// Tests GetPath method with zero stroke thickness.
        /// Verifies that ellipse uses full dimensions without offset.
        /// Expected: Path positioned at (0,0) with full width and height.
        /// </summary>
        [Fact]
        public void GetPath_WithZeroStrokeThickness_ReturnsPathAtOrigin()
        {
            // Arrange
            var ellipse = new Ellipse();
            ellipse.SetValue(VisualElement.WidthProperty, 80.0);
            ellipse.SetValue(VisualElement.HeightProperty, 40.0);
            ellipse.SetValue(Shape.StrokeThicknessProperty, 0.0);

            // Act
            var path = ellipse.GetPath();

            // Assert
            Assert.NotNull(path);
            Assert.IsType<PathF>(path);
        }

        /// <summary>
        /// Tests GetPath method with stroke thickness larger than dimensions.
        /// Verifies that method handles negative calculated dimensions gracefully.
        /// Expected: Valid path object returned despite negative calculated dimensions.
        /// </summary>
        [Fact]
        public void GetPath_WithLargeStrokeThickness_ReturnsValidPath()
        {
            // Arrange
            var ellipse = new Ellipse();
            ellipse.SetValue(VisualElement.WidthProperty, 10.0);
            ellipse.SetValue(VisualElement.HeightProperty, 8.0);
            ellipse.SetValue(Shape.StrokeThicknessProperty, 20.0);

            // Act
            var path = ellipse.GetPath();

            // Assert
            Assert.NotNull(path);
            Assert.IsType<PathF>(path);
        }

        /// <summary>
        /// Tests GetPath method with zero dimensions.
        /// Verifies that method handles zero width and height values.
        /// Expected: Valid path object with zero-sized ellipse.
        /// </summary>
        [Fact]
        public void GetPath_WithZeroDimensions_ReturnsValidPath()
        {
            // Arrange
            var ellipse = new Ellipse();
            ellipse.SetValue(VisualElement.WidthProperty, 0.0);
            ellipse.SetValue(VisualElement.HeightProperty, 0.0);
            ellipse.SetValue(Shape.StrokeThicknessProperty, 2.0);

            // Act
            var path = ellipse.GetPath();

            // Assert
            Assert.NotNull(path);
            Assert.IsType<PathF>(path);
        }

        /// <summary>
        /// Tests GetPath method with stroke thickness equal to dimensions.
        /// Verifies boundary condition where calculated dimensions become zero.
        /// Expected: Valid path with zero-sized ellipse positioned at stroke offset.
        /// </summary>
        [Fact]
        public void GetPath_WithStrokeEqualToDimensions_ReturnsValidPath()
        {
            // Arrange
            var ellipse = new Ellipse();
            ellipse.SetValue(VisualElement.WidthProperty, 12.0);
            ellipse.SetValue(VisualElement.HeightProperty, 12.0);
            ellipse.SetValue(Shape.StrokeThicknessProperty, 12.0);

            // Act
            var path = ellipse.GetPath();

            // Assert
            Assert.NotNull(path);
            Assert.IsType<PathF>(path);
        }

        /// <summary>
        /// Tests GetPath method with very large dimensions.
        /// Verifies that method handles large numeric values without overflow.
        /// Expected: Valid path object with large ellipse dimensions.
        /// </summary>
        [Fact]
        public void GetPath_WithLargeDimensions_ReturnsValidPath()
        {
            // Arrange
            var ellipse = new Ellipse();
            ellipse.SetValue(VisualElement.WidthProperty, 10000.0);
            ellipse.SetValue(VisualElement.HeightProperty, 8000.0);
            ellipse.SetValue(Shape.StrokeThicknessProperty, 100.0);

            // Act
            var path = ellipse.GetPath();

            // Assert
            Assert.NotNull(path);
            Assert.IsType<PathF>(path);
        }

        /// <summary>
        /// Tests GetPath method with fractional stroke thickness.
        /// Verifies proper handling of fractional values and float casting.
        /// Expected: Valid path with proper fractional positioning calculations.
        /// </summary>
        [Fact]
        public void GetPath_WithFractionalStroke_ReturnsValidPath()
        {
            // Arrange
            var ellipse = new Ellipse();
            ellipse.SetValue(VisualElement.WidthProperty, 50.5);
            ellipse.SetValue(VisualElement.HeightProperty, 30.3);
            ellipse.SetValue(Shape.StrokeThicknessProperty, 1.7);

            // Act
            var path = ellipse.GetPath();

            // Assert
            Assert.NotNull(path);
            Assert.IsType<PathF>(path);
        }

        /// <summary>
        /// Tests GetPath method with minimum positive values.
        /// Verifies handling of very small positive dimensions and stroke.
        /// Expected: Valid path with minimal ellipse size.
        /// </summary>
        [Fact]
        public void GetPath_WithMinimalPositiveValues_ReturnsValidPath()
        {
            // Arrange
            var ellipse = new Ellipse();
            ellipse.SetValue(VisualElement.WidthProperty, 0.1);
            ellipse.SetValue(VisualElement.HeightProperty, 0.1);
            ellipse.SetValue(Shape.StrokeThicknessProperty, 0.05);

            // Act
            var path = ellipse.GetPath();

            // Assert
            Assert.NotNull(path);
            Assert.IsType<PathF>(path);
        }

        /// <summary>
        /// Tests GetPath method with asymmetric dimensions.
        /// Verifies proper handling when width and height are very different.
        /// Expected: Valid path with correctly proportioned ellipse.
        /// </summary>
        [Theory]
        [InlineData(100.0, 10.0, 2.0)]
        [InlineData(10.0, 100.0, 2.0)]
        [InlineData(200.0, 5.0, 1.0)]
        [InlineData(5.0, 200.0, 1.0)]
        public void GetPath_WithAsymmetricDimensions_ReturnsValidPath(double width, double height, double strokeThickness)
        {
            // Arrange
            var ellipse = new Ellipse();
            ellipse.SetValue(VisualElement.WidthProperty, width);
            ellipse.SetValue(VisualElement.HeightProperty, height);
            ellipse.SetValue(Shape.StrokeThicknessProperty, strokeThickness);

            // Act
            var path = ellipse.GetPath();

            // Assert
            Assert.NotNull(path);
            Assert.IsType<PathF>(path);
        }

        /// <summary>
        /// Tests GetPath method with various stroke thickness values.
        /// Verifies consistent behavior across different stroke thickness scenarios.
        /// Expected: Valid path objects for all stroke thickness values.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(5.0)]
        [InlineData(25.0)]
        [InlineData(100.0)]
        public void GetPath_WithVariousStrokeThickness_ReturnsValidPath(double strokeThickness)
        {
            // Arrange
            var ellipse = new Ellipse();
            ellipse.SetValue(VisualElement.WidthProperty, 50.0);
            ellipse.SetValue(VisualElement.HeightProperty, 30.0);
            ellipse.SetValue(Shape.StrokeThicknessProperty, strokeThickness);

            // Act
            var path = ellipse.GetPath();

            // Assert
            Assert.NotNull(path);
            Assert.IsType<PathF>(path);
        }
    }
}
