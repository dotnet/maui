using System;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Graphics.Tests
{
    public class PathBoundsTests
    {
        private const float FloatComparisonDelta = 0.001f;

        [Fact]
        public void EmptyPath_ShouldReturnZeroBounds()
        {
            // Arrange
            var path = new PathF();

            // Act
            var bounds = path.CalculateTightBounds();

            // Assert
            Assert.Equal(0, bounds.X);
            Assert.Equal(0, bounds.Y);
            Assert.Equal(0, bounds.Width);
            Assert.Equal(0, bounds.Height);
        }

        [Fact]
        public void SinglePoint_ShouldReturnZeroSizeBounds()
        {
            // Arrange
            var path = new PathF();
            path.MoveTo(10, 20);

            // Act
            var bounds = path.CalculateTightBounds();

            // Assert
            Assert.Equal(10, bounds.X);
            Assert.Equal(20, bounds.Y);
            Assert.Equal(0, bounds.Width);
            Assert.Equal(0, bounds.Height);
        }

        [Fact]
        public void LinePath_ShouldReturnCorrectBounds()
        {
            // Arrange
            var path = new PathF();
            path.MoveTo(10, 20);
            path.LineTo(100, 200);

            // Act
            var bounds = path.CalculateTightBounds();

            // Assert
            Assert.Equal(10, bounds.X);
            Assert.Equal(20, bounds.Y);
            Assert.Equal(90, bounds.Width);
            Assert.Equal(180, bounds.Height);
        }

        [Fact]
        public void RectanglePath_ShouldReturnCorrectBounds()
        {
            // Arrange
            var path = new PathF();
            path.AppendRectangle(0, 0, 200, 200);

            // Act
            var bounds = path.CalculateTightBounds();

            // Assert
            Assert.Equal(0, bounds.X);
            Assert.Equal(0, bounds.Y);
            Assert.Equal(200, bounds.Width);
            Assert.Equal(200, bounds.Height);
        }

        [Fact]
        public void QuadBezierPath_ShouldReturnCorrectBounds_WithControlPointInside()
        {
            // Arrange
            var path = new PathF();
            path.MoveTo(0, 0);
            path.QuadTo(50, 50, 100, 0);  // Control point inside the start and end points' box

            // Act
            var bounds = path.CalculateTightBounds();

            // Assert
            Assert.Equal(0, bounds.X);
            Assert.Equal(0, bounds.Y);
            Assert.Equal(100, bounds.Width);
            Assert.Equal(25, bounds.Height, FloatComparisonDelta); // Highest point is at y=25 when t=0.5
        }

        [Fact]
        public void QuadBezierPath_ShouldReturnCorrectBounds_WithControlPointOutside()
        {
            // Arrange
            var path = new PathF();
            path.MoveTo(0, 0);
            path.QuadTo(50, 100, 100, 0);  // Control point well outside the start and end points' box

            // Act
            var bounds = path.CalculateTightBounds();

            // Assert
            Assert.Equal(0, bounds.X);
            Assert.Equal(0, bounds.Y, FloatComparisonDelta);
            Assert.Equal(100, bounds.Width);
            Assert.Equal(50, bounds.Height, FloatComparisonDelta); // Highest point is at y=50 when t=0.5
        }

        [Fact]
        public void CubicBezierPath_ShouldReturnCorrectBounds_WithControlPointsInside()
        {
            // Arrange
            var path = new PathF();
            path.MoveTo(0, 0);
            path.CurveTo(25, 25, 75, 25, 100, 0);  // Control points inside

            // Act
            var bounds = path.CalculateTightBounds();

            // Assert
            Assert.Equal(0, bounds.X);
            Assert.Equal(0, bounds.Y);
            Assert.Equal(100, bounds.Width);
            // Maximum height occurs somewhere around t=0.5, but exact value depends on control points
            Assert.True(bounds.Height > 0 && bounds.Height < 25);
        }

        [Fact]
        public void CubicBezierPath_ShouldReturnCorrectBounds_WithControlPointsOutside()
        {
            // Arrange
            var path = new PathF();
            path.MoveTo(0, 0);
            path.CurveTo(25, 100, 75, 100, 100, 0);  // Control points far outside

            // Act
            var bounds = path.CalculateTightBounds();

            // Assert
            Assert.Equal(0, bounds.X);
            Assert.Equal(0, bounds.Y);
            Assert.Equal(100, bounds.Width);
            Assert.True(bounds.Height > 50 && bounds.Height < 100); // Highest point depends on control points
        }

        [Fact]
        public void CubicBezierPath_CompareWithFlattening_ShouldBeMoreAccurate()
        {
            // Arrange - Create a path with a cubic bezier curve with control points far outside the actual curve
            var path = new PathF();
            path.MoveTo(0, 0);
            path.CurveTo(0, 500, 444, 500, 444, 0);

            // Act
            var tightBounds = path.CalculateTightBounds();
            var flattenedBounds = path.GetBoundsByFlattening();

            // Assert
            // The tight bounds should be within the expected range
            Assert.Equal(0, tightBounds.X);
            Assert.Equal(0, tightBounds.Y);
            Assert.Equal(444, tightBounds.Width);
            Assert.True(tightBounds.Height < flattenedBounds.Height);
            
            // The height of tight bounds should be significantly less than the 
            // height of flattened bounds which would include control points
            Assert.True(tightBounds.Height < 400);
        }

        [Fact]
        public void ComplexPath_WithMultipleSegments_ShouldReturnCorrectBounds()
        {
            // Arrange
            var path = new PathF();
            path.MoveTo(100, 100);
            path.LineTo(200, 100);
            path.QuadTo(300, 50, 300, 200);
            path.CurveTo(300, 300, 200, 350, 100, 300);
            path.Close();

            // Act
            var bounds = path.CalculateTightBounds();

            // Assert
            Assert.True(bounds.X >= 100);
            Assert.True(bounds.Y >= 50);
            Assert.True(bounds.Width <= 200);
            Assert.True(bounds.Height <= 300);

            // Compare with flattened bounds which might be less accurate
            var flattenedBounds = path.GetBoundsByFlattening();
            Assert.True(bounds.Width <= flattenedBounds.Width);
            Assert.True(bounds.Height <= flattenedBounds.Height);
        }

        [Fact]
        public void Arc_ShouldReturnCorrectBounds()
        {
            // Arrange
            var path = new PathF();
            path.MoveTo(100, 100);
            path.AddArc(50, 50, 150, 150, 0, 90, true);

            // Act
            var bounds = path.CalculateTightBounds();

            // Assert
            Assert.True(bounds.X >= 50);
            Assert.True(bounds.Y >= 50);
            Assert.True(bounds.Width <= 100);
            Assert.True(bounds.Height <= 100);
        }

        [Fact]
        public void CirclePath_ShouldReturnCorrectBounds()
        {
            // Arrange
            var path = new PathF();
            path.AppendCircle(100, 100, 50);

            // Act
            var bounds = path.CalculateTightBounds();

            // Assert
            Assert.Equal(50, bounds.X);
            Assert.Equal(50, bounds.Y);
            Assert.Equal(100, bounds.Width);
            Assert.Equal(100, bounds.Height);
        }
    }
}