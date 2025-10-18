#nullable disable

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Xunit;


namespace Microsoft.Maui.Controls.Shapes.UnitTests
{
    public class PolyQuadraticBezierSegmentTests
    {
        /// <summary>
        /// Tests that the default constructor initializes the Points property with a new PointCollection.
        /// Verifies that the Points property is not null, is of the correct type, and is initially empty.
        /// </summary>
        [Fact]
        public void Constructor_Default_InitializesPointsProperty()
        {
            // Act
            var segment = new PolyQuadraticBezierSegment();

            // Assert
            Assert.NotNull(segment.Points);
            Assert.IsType<PointCollection>(segment.Points);
            Assert.Empty(segment.Points);
        }

        /// <summary>
        /// Tests that the default constructor creates a valid instance that can be accessed without exceptions.
        /// Verifies that the object is properly constructed and the Points property is accessible.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesValidInstance()
        {
            // Act
            var segment = new PolyQuadraticBezierSegment();

            // Assert
            Assert.NotNull(segment);
            Assert.IsType<PolyQuadraticBezierSegment>(segment);

            // Verify Points property can be accessed multiple times without issues
            var points1 = segment.Points;
            var points2 = segment.Points;
            Assert.Same(points1, points2);
        }
    }
}
