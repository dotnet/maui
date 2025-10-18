#nullable disable

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public sealed class PolyBezierSegmentTests
    {
        /// <summary>
        /// Tests that the parameterless constructor successfully creates a PolyBezierSegment instance
        /// without throwing any exceptions.
        /// </summary>
        [Fact]
        public void Constructor_InitializesSuccessfully_CreatesInstance()
        {
            // Arrange & Act
            var result = new PolyBezierSegment();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PolyBezierSegment>(result);
        }

        /// <summary>
        /// Tests that the parameterless constructor initializes the Points property to a non-null
        /// PointCollection instance.
        /// </summary>
        [Fact]
        public void Constructor_InitializesPointsProperty_PointsIsNotNull()
        {
            // Arrange & Act
            var segment = new PolyBezierSegment();

            // Assert
            Assert.NotNull(segment.Points);
        }

        /// <summary>
        /// Tests that the parameterless constructor initializes the Points property to an empty
        /// PointCollection, ensuring no points are present initially.
        /// </summary>
        [Fact]
        public void Constructor_InitializesPointsProperty_PointsIsEmpty()
        {
            // Arrange & Act
            var segment = new PolyBezierSegment();

            // Assert
            Assert.Empty(segment.Points);
        }

        /// <summary>
        /// Tests that the parameterless constructor initializes the Points property to the correct
        /// type (PointCollection).
        /// </summary>
        [Fact]
        public void Constructor_InitializesPointsProperty_PointsIsCorrectType()
        {
            // Arrange & Act
            var segment = new PolyBezierSegment();

            // Assert
            Assert.IsType<PointCollection>(segment.Points);
        }
    }
}
