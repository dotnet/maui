#nullable disable

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests.Shapes
{
    public class PolyLineSegmentTests
    {
        /// <summary>
        /// Tests that the parameterless constructor properly initializes the Points property with an empty PointCollection.
        /// Verifies that the Points property is not null and contains an empty collection after construction.
        /// </summary>
        [Fact]
        public void Constructor_Default_InitializesPointsWithEmptyCollection()
        {
            // Act
            var polyLineSegment = new PolyLineSegment();

            // Assert
            Assert.NotNull(polyLineSegment.Points);
            Assert.IsType<PointCollection>(polyLineSegment.Points);
            Assert.Empty(polyLineSegment.Points);
        }
    }
}
