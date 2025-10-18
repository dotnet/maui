#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class RegionTests : BaseTestFixture
    {
        [Fact]
        public void RegionOneLineConstruction()
        {
            double[] lineHeights = { 20 };
            double maxWidth = 200;
            double startX = 90;
            double endX = 180;
            double startY = 80;

            var region = Region.FromLines(lineHeights, maxWidth, startX, endX, startY);

            // Top Left start character
            Assert.True(region.Contains(new Point(90, 80)));
            Assert.True(region.Contains(new Point(90, 99)));

            // Top Right end character
            Assert.True(region.Contains(new Point(179, 80)));
            Assert.True(region.Contains(new Point(179, 99)));

            //** Outside Container **//
            // Top Left start character
            Assert.False(region.Contains(new Point(89, 80)));
            Assert.False(region.Contains(new Point(89, 99)));

            // Top Right end character
            Assert.False(region.Contains(new Point(180, 80)));
            Assert.False(region.Contains(new Point(180, 99)));

        }

        [Fact]
        public void RegionTwoLineConstruction()
        {
            double[] lineHeights = { 20, 20 };
            double maxWidth = 200;
            double startX = 90;
            double endX = 40;
            double startY = 80;

            var region = Region.FromLines(lineHeights, maxWidth, startX, endX, startY);

            // Top Left start character
            Assert.True(region.Contains(new Point(90, 80)));
            Assert.True(region.Contains(new Point(90, 99)));

            // Top Right end character
            Assert.True(region.Contains(new Point(199, 80)));
            Assert.True(region.Contains(new Point(199, 99)));


            // End Left end character
            Assert.True(region.Contains(new Point(0, 100)));
            Assert.True(region.Contains(new Point(0, 119)));

            // End Right end character
            Assert.True(region.Contains(new Point(39, 100)));
            Assert.True(region.Contains(new Point(39, 119)));

            //** Outside Container **//
            // Top Left start character
            Assert.False(region.Contains(new Point(89, 80)));
            Assert.False(region.Contains(new Point(89, 99)));

            // Top Right end character
            Assert.False(region.Contains(new Point(200, 80)));
            Assert.False(region.Contains(new Point(200, 99)));

            // End Left end character
            Assert.False(region.Contains(new Point(-1, 100)));
            Assert.False(region.Contains(new Point(-1, 119)));

            // End Right end character
            Assert.False(region.Contains(new Point(40, 100)));
            Assert.False(region.Contains(new Point(40, 119)));
        }

        [Fact]
        public void RegionThreeLineConstruction()
        {
            double[] lineHeights = { 20, 20, 20 };
            double maxWidth = 200;
            double startX = 90;
            double endX = 40;
            double startY = 80;

            var region = Region.FromLines(lineHeights, maxWidth, startX, endX, startY);

            // Top Left start character
            Assert.True(region.Contains(new Point(90, 80)));
            Assert.True(region.Contains(new Point(90, 99)));

            // Top Right end character
            Assert.True(region.Contains(new Point(199, 80)));
            Assert.True(region.Contains(new Point(199, 99)));

            // Middle Left end character
            Assert.True(region.Contains(new Point(0, 100)));
            Assert.True(region.Contains(new Point(0, 119)));

            // Middle Right end character
            Assert.True(region.Contains(new Point(199, 100)));
            Assert.True(region.Contains(new Point(199, 119)));

            // End Left end character
            Assert.True(region.Contains(new Point(0, 120)));
            Assert.True(region.Contains(new Point(0, 139)));

            // End Right end character
            Assert.True(region.Contains(new Point(39, 120)));
            Assert.True(region.Contains(new Point(39, 139)));

            //** Outside Container **//
            // Top Left start character
            Assert.False(region.Contains(new Point(89, 80)));
            Assert.False(region.Contains(new Point(89, 99)));

            // Top Right end character
            Assert.False(region.Contains(new Point(200, 80)));
            Assert.False(region.Contains(new Point(200, 99)));

            // End Left end character
            Assert.False(region.Contains(new Point(-1, 120)));
            Assert.False(region.Contains(new Point(-1, 139)));

            // End Right end character
            Assert.False(region.Contains(new Point(40, 120)));
            Assert.False(region.Contains(new Point(40, 139)));
        }

        [Fact]
        public void RegionInflate()
        {
            double[] lineHeights = { 20 };
            double maxWidth = 200;
            double startX = 90;
            double endX = 180;
            double startY = 80;

            var region = Region.FromLines(lineHeights, maxWidth, startX, endX, startY).Inflate(10);

            // Top Left start character
            Assert.True(region.Contains(new Point(90, 80)));
            Assert.True(region.Contains(new Point(90, 99)));

            // Top Right end character
            Assert.True(region.Contains(new Point(179, 80)));
            Assert.True(region.Contains(new Point(179, 99)));

            //** Inflated Container **//
            // Top Left start character
            Assert.True(region.Contains(new Point(89, 80)));
            Assert.True(region.Contains(new Point(89, 99)));

            // Top Right end character
            Assert.True(region.Contains(new Point(180, 80)));
            Assert.True(region.Contains(new Point(180, 99)));

            //** Outside Container **//
            // Top Left start character
            Assert.False(region.Contains(new Point(79, 80)));
            Assert.False(region.Contains(new Point(79, 99)));

            // Top Right end character
            Assert.False(region.Contains(new Point(190, 80)));
            Assert.False(region.Contains(new Point(190, 99)));
        }

        [Fact]
        public void RegionDeflate()
        {
            double[] lineHeights = { 20 };
            double maxWidth = 200;
            double startX = 90;
            double endX = 180;
            double startY = 80;

            var region = Region.FromLines(lineHeights, maxWidth, startX, endX, startY).Inflate(10).Deflate();

            // Top Left start character
            Assert.True(region.Contains(new Point(90, 80)));
            Assert.True(region.Contains(new Point(90, 99)));

            // Top Right end character
            Assert.True(region.Contains(new Point(179, 80)));
            Assert.True(region.Contains(new Point(179, 99)));

            //** Outside Container **//
            // Top Left start character
            Assert.False(region.Contains(new Point(89, 80)));
            Assert.False(region.Contains(new Point(89, 99)));

            // Top Right end character
            Assert.False(region.Contains(new Point(180, 80)));
            Assert.False(region.Contains(new Point(180, 99)));

        }

        /// <summary>
        /// Tests Contains method with default/uninitialized Region where Regions property is null.
        /// Should return false for any coordinates when Regions is null.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(100, 100)]
        [InlineData(-50, -50)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        public void Contains_WithNullRegions_ReturnsFalse(double x, double y)
        {
            // Arrange - Create default Region struct where Regions will be null
            var region = default(Region);

            // Act
            var result = region.Contains(x, y);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests Contains method with empty regions collection.
        /// Should return false when no rectangles are present.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(10, 10)]
        [InlineData(-5, -5)]
        public void Contains_WithEmptyRegions_ReturnsFalse(double x, double y)
        {
            // Arrange
            var region = Region.FromRectangles(new List<Rect>());

            // Act
            var result = region.Contains(x, y);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests Contains method with single rectangle region.
        /// Tests points inside, outside, and on boundaries of the rectangle.
        /// </summary>
        [Theory]
        [InlineData(10, 10, true)]   // Inside rectangle
        [InlineData(15, 15, true)]   // Inside rectangle
        [InlineData(0, 0, true)]     // Top-left corner (inclusive)
        [InlineData(19.9, 19.9, true)] // Just inside bottom-right
        [InlineData(20, 20, false)]  // Bottom-right corner (exclusive)
        [InlineData(-1, 10, false)]  // Left of rectangle
        [InlineData(21, 10, false)]  // Right of rectangle
        [InlineData(10, -1, false)]  // Above rectangle
        [InlineData(10, 21, false)]  // Below rectangle
        [InlineData(-10, -10, false)] // Far outside
        public void Contains_WithSingleRectangle_ReturnsExpectedResult(double x, double y, bool expected)
        {
            // Arrange - Create region with single 20x20 rectangle at origin
            var rectangles = new List<Rect> { new Rect(0, 0, 20, 20) };
            var region = Region.FromRectangles(rectangles);

            // Act
            var result = region.Contains(x, y);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests Contains method with multiple non-overlapping rectangles.
        /// Tests that method returns true if point is in any rectangle.
        /// </summary>
        [Theory]
        [InlineData(5, 5, true)]     // In first rectangle
        [InlineData(35, 35, true)]   // In second rectangle
        [InlineData(65, 65, true)]   // In third rectangle
        [InlineData(25, 25, false)]  // Between rectangles
        [InlineData(15, 35, false)]  // Between first and second
        [InlineData(100, 100, false)] // Outside all rectangles
        public void Contains_WithMultipleRectangles_ReturnsExpectedResult(double x, double y, bool expected)
        {
            // Arrange - Create region with three separate 10x10 rectangles
            var rectangles = new List<Rect>
            {
                new Rect(0, 0, 10, 10),     // First rectangle
                new Rect(30, 30, 10, 10),   // Second rectangle  
                new Rect(60, 60, 10, 10)    // Third rectangle
            };
            var region = Region.FromRectangles(rectangles);

            // Act
            var result = region.Contains(x, y);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests Contains method with overlapping rectangles.
        /// Verifies that overlapping areas are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(5, 5, true)]     // In first rectangle only
        [InlineData(15, 15, true)]   // In overlapping area
        [InlineData(25, 25, true)]   // In second rectangle only
        [InlineData(35, 35, false)]  // Outside both rectangles
        public void Contains_WithOverlappingRectangles_ReturnsExpectedResult(double x, double y, bool expected)
        {
            // Arrange - Create region with two overlapping rectangles
            var rectangles = new List<Rect>
            {
                new Rect(0, 0, 20, 20),     // First rectangle
                new Rect(10, 10, 20, 20)    // Second rectangle overlapping first
            };
            var region = Region.FromRectangles(rectangles);

            // Act
            var result = region.Contains(x, y);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests Contains method with extreme coordinate values.
        /// Ensures method handles edge cases with special double values appropriately.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0, false)]
        [InlineData(0, double.NaN, false)]
        [InlineData(double.NaN, double.NaN, false)]
        [InlineData(double.PositiveInfinity, 0, false)]
        [InlineData(0, double.PositiveInfinity, false)]
        [InlineData(double.NegativeInfinity, 0, false)]
        [InlineData(0, double.NegativeInfinity, false)]
        [InlineData(double.MaxValue, double.MaxValue, false)]
        [InlineData(double.MinValue, double.MinValue, false)]
        public void Contains_WithExtremeValues_ReturnsFalse(double x, double y, bool expected)
        {
            // Arrange - Create region with normal rectangle
            var rectangles = new List<Rect> { new Rect(0, 0, 100, 100) };
            var region = Region.FromRectangles(rectangles);

            // Act
            var result = region.Contains(x, y);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests Contains method with very large rectangle to test extreme but valid coordinates.
        /// </summary>
        [Theory]
        [InlineData(1000000, 1000000, true)]
        [InlineData(-1000000, -1000000, true)]
        [InlineData(0, 0, true)]
        public void Contains_WithLargeRectangle_ReturnsExpectedResult(double x, double y, bool expected)
        {
            // Arrange - Create region with very large rectangle
            var rectangles = new List<Rect> { new Rect(-2000000, -2000000, 4000000, 4000000) };
            var region = Region.FromRectangles(rectangles);

            // Act
            var result = region.Contains(x, y);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests Contains method with zero-sized rectangles.
        /// Zero-sized rectangles should not contain any points.
        /// </summary>
        [Theory]
        [InlineData(10, 10, false)]
        [InlineData(0, 0, false)]
        [InlineData(5, 5, false)]
        public void Contains_WithZeroSizedRectangles_ReturnsFalse(double x, double y, bool expected)
        {
            // Arrange - Create region with zero-width and zero-height rectangles
            var rectangles = new List<Rect>
            {
                new Rect(10, 10, 0, 0),    // Zero-sized rectangle
                new Rect(0, 0, 0, 10),     // Zero-width rectangle
                new Rect(5, 5, 10, 0)      // Zero-height rectangle
            };
            var region = Region.FromRectangles(rectangles);

            // Act
            var result = region.Contains(x, y);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that Deflate returns the same region when inflation is null.
        /// This test verifies the not-covered branch where _inflation == null.
        /// </summary>
        [Fact]
        public void Deflate_WhenInflationIsNull_ReturnsSameRegion()
        {
            // Arrange - Create a region without inflation using FromRectangles
            var rectangles = new List<Rect>
            {
                new Rect(10, 20, 100, 50)
            };
            var originalRegion = Region.FromRectangles(rectangles);

            // Act
            var deflatedRegion = originalRegion.Deflate();

            // Assert - Verify the deflated region contains the same points as original
            Assert.True(deflatedRegion.Contains(new Point(10, 20))); // Top-left corner
            Assert.True(deflatedRegion.Contains(new Point(109, 69))); // Bottom-right corner (inclusive)
            Assert.True(deflatedRegion.Contains(new Point(50, 40))); // Middle point

            // Verify boundaries - should be the same as original
            Assert.False(deflatedRegion.Contains(new Point(9, 20))); // Just outside left
            Assert.False(deflatedRegion.Contains(new Point(110, 20))); // Just outside right
            Assert.False(deflatedRegion.Contains(new Point(10, 19))); // Just outside top  
            Assert.False(deflatedRegion.Contains(new Point(10, 70))); // Just outside bottom
        }

        /// <summary>
        /// Tests that Deflate with null inflation works correctly for multiple rectangles.
        /// This test verifies the not-covered branch with a more complex region.
        /// </summary>
        [Fact]
        public void Deflate_WhenInflationIsNullWithMultipleRectangles_ReturnsSameRegion()
        {
            // Arrange - Create a region with multiple rectangles
            var rectangles = new List<Rect>
            {
                new Rect(0, 0, 50, 30),
                new Rect(60, 0, 40, 30)
            };
            var originalRegion = Region.FromRectangles(rectangles);

            // Act
            var deflatedRegion = originalRegion.Deflate();

            // Assert - Verify both rectangles are preserved
            // First rectangle
            Assert.True(deflatedRegion.Contains(new Point(0, 0)));
            Assert.True(deflatedRegion.Contains(new Point(49, 29)));
            Assert.False(deflatedRegion.Contains(new Point(50, 0)));

            // Second rectangle  
            Assert.True(deflatedRegion.Contains(new Point(60, 0)));
            Assert.True(deflatedRegion.Contains(new Point(99, 29)));
            Assert.False(deflatedRegion.Contains(new Point(100, 0)));

            // Gap between rectangles should not be contained
            Assert.False(deflatedRegion.Contains(new Point(55, 15)));
        }

        /// <summary>
        /// Tests that Deflate with null inflation works correctly for empty region.
        /// This test verifies the not-covered branch with edge case of empty rectangles.
        /// </summary>
        [Fact]
        public void Deflate_WhenInflationIsNullWithEmptyRectangles_ReturnsSameRegion()
        {
            // Arrange - Create a region with empty rectangles list
            var rectangles = new List<Rect>();
            var originalRegion = Region.FromRectangles(rectangles);

            // Act
            var deflatedRegion = originalRegion.Deflate();

            // Assert - Should not contain any points
            Assert.False(deflatedRegion.Contains(new Point(0, 0)));
            Assert.False(deflatedRegion.Contains(new Point(10, 10)));
            Assert.False(deflatedRegion.Contains(new Point(-10, -10)));
        }

        /// <summary>
        /// Tests that Deflate with null inflation preserves region with zero-sized rectangle.
        /// This test verifies the not-covered branch with edge case of zero dimensions.
        /// </summary>
        [Fact]
        public void Deflate_WhenInflationIsNullWithZeroSizedRectangle_ReturnsSameRegion()
        {
            // Arrange - Create a region with zero-sized rectangle
            var rectangles = new List<Rect>
            {
                new Rect(50, 50, 0, 0)
            };
            var originalRegion = Region.FromRectangles(rectangles);

            // Act
            var deflatedRegion = originalRegion.Deflate();

            // Assert - Zero-sized rectangle should not contain any points
            Assert.False(deflatedRegion.Contains(new Point(50, 50)));
            Assert.False(deflatedRegion.Contains(new Point(49, 49)));
            Assert.False(deflatedRegion.Contains(new Point(51, 51)));
        }

        /// <summary>
        /// Tests that Deflate with null inflation works with extreme coordinate values.
        /// This test verifies the not-covered branch with boundary value conditions.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, double.MinValue, 100, 100)]
        [InlineData(double.MaxValue - 100, double.MaxValue - 100, 50, 50)]
        [InlineData(-1000, -1000, 2000, 2000)]
        [InlineData(0, 0, double.MaxValue, double.MaxValue)]
        public void Deflate_WhenInflationIsNullWithExtremeValues_ReturnsSameRegion(double x, double y, double width, double height)
        {
            // Arrange - Create a region with extreme values
            var rectangles = new List<Rect>
            {
                new Rect(x, y, width, height)
            };
            var originalRegion = Region.FromRectangles(rectangles);

            // Act
            var deflatedRegion = originalRegion.Deflate();

            // Assert - Should contain points within the rectangle bounds
            if (width > 0 && height > 0 && x < double.MaxValue && y < double.MaxValue)
            {
                Assert.True(deflatedRegion.Contains(new Point(x, y)));

                // Test a point that should be outside if dimensions are reasonable
                if (x > double.MinValue + 1)
                {
                    Assert.False(deflatedRegion.Contains(new Point(x - 1, y)));
                }
            }
        }

        /// <summary>
        /// Tests that deflating an inflated region properly reverses the inflation.
        /// This test verifies that the Inflate method is called with negated values.
        /// </summary>
        [Fact]
        public void Deflate_WhenInflationExists_ReversesInflation()
        {
            // Arrange - Create a region and inflate it
            var rectangles = new List<Rect>
            {
                new Rect(50, 50, 100, 80)
            };
            var originalRegion = Region.FromRectangles(rectangles);
            var inflatedRegion = originalRegion.Inflate(10, 15, 20, 25);

            // Act
            var deflatedRegion = inflatedRegion.Deflate();

            // Assert - Should match the original region boundaries
            Assert.True(deflatedRegion.Contains(new Point(50, 50))); // Original top-left
            Assert.True(deflatedRegion.Contains(new Point(149, 129))); // Original bottom-right

            // Should not contain the inflated area
            Assert.False(deflatedRegion.Contains(new Point(49, 50))); // Outside original left
            Assert.False(deflatedRegion.Contains(new Point(150, 129))); // Outside original right
            Assert.False(deflatedRegion.Contains(new Point(50, 49))); // Outside original top
            Assert.False(deflatedRegion.Contains(new Point(50, 130))); // Outside original bottom
        }

        /// <summary>
        /// Tests that deflating with different inflation values works correctly.
        /// This test verifies the behavior with various inflation scenarios.
        /// </summary>
        [Theory]
        [InlineData(5, 5, 5, 5)]
        [InlineData(10, 20, 30, 40)]
        [InlineData(0, 0, 0, 0)]
        [InlineData(1, 0, 0, 1)]
        [InlineData(100, 200, 300, 400)]
        public void Deflate_WhenInflationExistsWithVariousValues_ReversesInflationCorrectly(double left, double top, double right, double bottom)
        {
            // Arrange
            var rectangles = new List<Rect>
            {
                new Rect(100, 100, 200, 150)
            };
            var originalRegion = Region.FromRectangles(rectangles);
            var inflatedRegion = originalRegion.Inflate(left, top, right, bottom);

            // Act
            var deflatedRegion = inflatedRegion.Deflate();

            // Assert - Should match original region
            Assert.True(deflatedRegion.Contains(new Point(100, 100))); // Original top-left
            Assert.True(deflatedRegion.Contains(new Point(299, 249))); // Original bottom-right
            Assert.True(deflatedRegion.Contains(new Point(200, 175))); // Original center

            // Verify boundaries - should exclude inflated areas
            if (left > 0)
                Assert.False(deflatedRegion.Contains(new Point(100 - 1, 100)));
            if (top > 0)
                Assert.False(deflatedRegion.Contains(new Point(100, 100 - 1)));
            if (right > 0)
                Assert.False(deflatedRegion.Contains(new Point(300, 100)));
            if (bottom > 0)
                Assert.False(deflatedRegion.Contains(new Point(100, 250)));
        }

        /// <summary>
        /// Tests that a Region equals itself (reflexive property).
        /// Input: Same Region instance compared to itself.
        /// Expected: Returns true.
        /// </summary>
        [Fact]
        public void Equals_SameInstance_ReturnsTrue()
        {
            // Arrange
            var rectangles = new List<Rect> { new Rect(0, 0, 100, 50) };
            var region = Region.FromRectangles(rectangles);

            // Act
            var result = region.Equals(region);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that two different Region instances with identical rectangle data are not equal 
        /// due to reference equality comparison of Regions property.
        /// Input: Two different Region instances created with same rectangle data.
        /// Expected: Returns false due to different ReadOnlyCollection references.
        /// </summary>
        [Fact]
        public void Equals_DifferentInstancesSameRectangles_ReturnsFalse()
        {
            // Arrange
            var rectangles1 = new List<Rect> { new Rect(0, 0, 100, 50) };
            var rectangles2 = new List<Rect> { new Rect(0, 0, 100, 50) };
            var region1 = Region.FromRectangles(rectangles1);
            var region2 = Region.FromRectangles(rectangles2);

            // Act
            var result = region1.Equals(region2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that two different Region instances with different rectangle data are not equal.
        /// Input: Two Region instances with completely different rectangles.
        /// Expected: Returns false.
        /// </summary>
        [Fact]
        public void Equals_DifferentInstancesDifferentRectangles_ReturnsFalse()
        {
            // Arrange
            var rectangles1 = new List<Rect> { new Rect(0, 0, 100, 50) };
            var rectangles2 = new List<Rect> { new Rect(10, 10, 200, 100) };
            var region1 = Region.FromRectangles(rectangles1);
            var region2 = Region.FromRectangles(rectangles2);

            // Act
            var result = region1.Equals(region2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that two Region instances with empty rectangle collections are not equal 
        /// due to reference equality comparison.
        /// Input: Two Region instances created with empty rectangle collections.
        /// Expected: Returns false due to different ReadOnlyCollection references.
        /// </summary>
        [Fact]
        public void Equals_EmptyRectangleCollections_ReturnsFalse()
        {
            // Arrange
            var region1 = Region.FromRectangles(new List<Rect>());
            var region2 = Region.FromRectangles(new List<Rect>());

            // Act
            var result = region1.Equals(region2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that two Region instances with multiple identical rectangles are not equal 
        /// due to reference equality comparison.
        /// Input: Two Region instances with multiple identical rectangles.
        /// Expected: Returns false due to different ReadOnlyCollection references.
        /// </summary>
        [Fact]
        public void Equals_MultipleIdenticalRectangles_ReturnsFalse()
        {
            // Arrange
            var rectangles1 = new List<Rect>
            {
                new Rect(0, 0, 100, 50),
                new Rect(100, 50, 150, 75),
                new Rect(250, 125, 300, 200)
            };
            var rectangles2 = new List<Rect>
            {
                new Rect(0, 0, 100, 50),
                new Rect(100, 50, 150, 75),
                new Rect(250, 125, 300, 200)
            };
            var region1 = Region.FromRectangles(rectangles1);
            var region2 = Region.FromRectangles(rectangles2);

            // Act
            var result = region1.Equals(region2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests equality comparison with rectangles containing extreme coordinate values.
        /// Input: Two Region instances with rectangles using extreme double values.
        /// Expected: Returns false due to different ReadOnlyCollection references.
        /// </summary>
        [Fact]
        public void Equals_ExtremeCoordinateValues_ReturnsFalse()
        {
            // Arrange
            var rectangles1 = new List<Rect>
            {
                new Rect(double.MinValue, double.MinValue, double.MaxValue, double.MaxValue),
                new Rect(0, 0, double.PositiveInfinity, double.PositiveInfinity)
            };
            var rectangles2 = new List<Rect>
            {
                new Rect(double.MinValue, double.MinValue, double.MaxValue, double.MaxValue),
                new Rect(0, 0, double.PositiveInfinity, double.PositiveInfinity)
            };
            var region1 = Region.FromRectangles(rectangles1);
            var region2 = Region.FromRectangles(rectangles2);

            // Act
            var result = region1.Equals(region2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests equality comparison with rectangles containing NaN values.
        /// Input: Two Region instances with rectangles using NaN coordinate values.
        /// Expected: Returns false due to different ReadOnlyCollection references.
        /// </summary>
        [Fact]
        public void Equals_NaNCoordinateValues_ReturnsFalse()
        {
            // Arrange
            var rectangles1 = new List<Rect>
            {
                new Rect(double.NaN, double.NaN, double.NaN, double.NaN)
            };
            var rectangles2 = new List<Rect>
            {
                new Rect(double.NaN, double.NaN, double.NaN, double.NaN)
            };
            var region1 = Region.FromRectangles(rectangles1);
            var region2 = Region.FromRectangles(rectangles2);

            // Act
            var result = region1.Equals(region2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals(object) returns false when passed null.
        /// </summary>
        [Fact]
        public void Equals_WithNullObject_ReturnsFalse()
        {
            // Arrange
            var rectangles = new[] { new Rect(0, 0, 100, 50) };
            var region = Region.FromRectangles(rectangles);

            // Act
            bool result = region.Equals((object)null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals(object) returns false when passed objects of different types.
        /// Validates the type checking behavior of the method.
        /// </summary>
        /// <param name="obj">The object of different type to compare against</param>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        public void Equals_WithDifferentTypeObjects_ReturnsFalse(object obj)
        {
            // Arrange
            var rectangles = new[] { new Rect(0, 0, 100, 50) };
            var region = Region.FromRectangles(rectangles);

            // Act
            bool result = region.Equals(obj);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals(object) returns false when passed a different struct type.
        /// Validates that different struct types are correctly identified.
        /// </summary>
        [Fact]
        public void Equals_WithDifferentStructType_ReturnsFalse()
        {
            // Arrange
            var rectangles = new[] { new Rect(0, 0, 100, 50) };
            var region = Region.FromRectangles(rectangles);
            var differentStruct = new Rect(0, 0, 100, 50);

            // Act
            bool result = region.Equals((object)differentStruct);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals(object) returns true when comparing a Region with itself (boxed).
        /// Validates that the same Region instance equals itself when boxed to object.
        /// </summary>
        [Fact]
        public void Equals_WithSameRegionBoxed_ReturnsTrue()
        {
            // Arrange
            var rectangles = new[] { new Rect(10, 20, 100, 50) };
            var region = Region.FromRectangles(rectangles);

            // Act
            bool result = region.Equals((object)region);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals(object) returns true when comparing Regions with identical rectangles.
        /// Validates that two Regions created with the same rectangle data are considered equal.
        /// </summary>
        [Fact]
        public void Equals_WithEqualRegions_ReturnsTrue()
        {
            // Arrange
            var rectangles1 = new[] { new Rect(0, 0, 100, 50) };
            var rectangles2 = new[] { new Rect(0, 0, 100, 50) };
            var region1 = Region.FromRectangles(rectangles1);
            var region2 = Region.FromRectangles(rectangles2);

            // Act
            bool result = region1.Equals((object)region2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals(object) returns false when comparing Regions with different rectangles.
        /// Validates that Regions with different rectangle data are not considered equal.
        /// </summary>
        [Fact]
        public void Equals_WithDifferentRectangles_ReturnsFalse()
        {
            // Arrange
            var rectangles1 = new[] { new Rect(0, 0, 100, 50) };
            var rectangles2 = new[] { new Rect(10, 10, 100, 50) };
            var region1 = Region.FromRectangles(rectangles1);
            var region2 = Region.FromRectangles(rectangles2);

            // Act
            bool result = region1.Equals((object)region2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals(object) returns false when comparing Regions with different inflation values.
        /// Validates that Regions with same rectangles but different inflation are not considered equal.
        /// </summary>
        [Fact]
        public void Equals_WithDifferentInflation_ReturnsFalse()
        {
            // Arrange
            var rectangles = new[] { new Rect(0, 0, 100, 50) };
            var region1 = Region.FromRectangles(rectangles);
            var region2 = Region.FromRectangles(rectangles).Inflate(5);

            // Act
            bool result = region1.Equals((object)region2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals(object) returns true when comparing Regions with same rectangles and same inflation.
        /// Validates that Regions with identical rectangles and inflation values are considered equal.
        /// </summary>
        [Fact]
        public void Equals_WithSameRectanglesAndInflation_ReturnsTrue()
        {
            // Arrange
            var rectangles = new[] { new Rect(0, 0, 100, 50) };
            var region1 = Region.FromRectangles(rectangles).Inflate(10);
            var region2 = Region.FromRectangles(rectangles).Inflate(10);

            // Act
            bool result = region1.Equals((object)region2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals(object) works correctly with empty regions.
        /// Validates equality behavior when no rectangles are present.
        /// </summary>
        [Fact]
        public void Equals_WithEmptyRegions_ReturnsTrue()
        {
            // Arrange
            var emptyRectangles = new Rect[0];
            var region1 = Region.FromRectangles(emptyRectangles);
            var region2 = Region.FromRectangles(emptyRectangles);

            // Act
            bool result = region1.Equals((object)region2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals(object) works correctly with multiple rectangles.
        /// Validates equality behavior when multiple rectangles are present.
        /// </summary>
        [Fact]
        public void Equals_WithMultipleRectangles_ReturnsTrue()
        {
            // Arrange
            var rectangles1 = new[] {
                new Rect(0, 0, 100, 50),
                new Rect(100, 50, 200, 100)
            };
            var rectangles2 = new[] {
                new Rect(0, 0, 100, 50),
                new Rect(100, 50, 200, 100)
            };
            var region1 = Region.FromRectangles(rectangles1);
            var region2 = Region.FromRectangles(rectangles2);

            // Act
            bool result = region1.Equals((object)region2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals(object) returns false when one region is empty and another is not.
        /// Validates inequality between empty and non-empty regions.
        /// </summary>
        [Fact]
        public void Equals_WithEmptyAndNonEmptyRegions_ReturnsFalse()
        {
            // Arrange
            var emptyRectangles = new Rect[0];
            var nonEmptyRectangles = new[] { new Rect(0, 0, 100, 50) };
            var emptyRegion = Region.FromRectangles(emptyRectangles);
            var nonEmptyRegion = Region.FromRectangles(nonEmptyRectangles);

            // Act
            bool result = emptyRegion.Equals((object)nonEmptyRegion);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetHashCode returns consistent values for regions without inflation.
        /// Expected result: Same hash code for identical regions with no inflation.
        /// </summary>
        [Fact]
        public void GetHashCode_RegionWithoutInflation_ReturnsConsistentHashCode()
        {
            // Arrange
            var rectangles = new List<Rect>
            {
                new Rect(0, 0, 100, 50),
                new Rect(0, 50, 100, 50)
            };
            var region = Region.FromRectangles(rectangles);

            // Act
            var hashCode1 = region.GetHashCode();
            var hashCode2 = region.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that GetHashCode returns consistent values for regions with inflation.
        /// Expected result: Same hash code for identical regions with same inflation.
        /// </summary>
        [Fact]
        public void GetHashCode_RegionWithInflation_ReturnsConsistentHashCode()
        {
            // Arrange
            var rectangles = new List<Rect>
            {
                new Rect(10, 10, 80, 40)
            };
            var region = Region.FromRectangles(rectangles).Inflate(5);

            // Act
            var hashCode1 = region.GetHashCode();
            var hashCode2 = region.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that equal regions produce equal hash codes without inflation.
        /// Expected result: Identical regions without inflation have identical hash codes.
        /// </summary>
        [Fact]
        public void GetHashCode_EqualRegionsWithoutInflation_ReturnEqualHashCodes()
        {
            // Arrange
            var rectangles1 = new List<Rect> { new Rect(0, 0, 100, 50) };
            var rectangles2 = new List<Rect> { new Rect(0, 0, 100, 50) };
            var region1 = Region.FromRectangles(rectangles1);
            var region2 = Region.FromRectangles(rectangles2);

            // Act
            var hashCode1 = region1.GetHashCode();
            var hashCode2 = region2.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that equal regions produce equal hash codes with inflation.
        /// Expected result: Identical regions with identical inflation have identical hash codes.
        /// </summary>
        [Fact]
        public void GetHashCode_EqualRegionsWithInflation_ReturnEqualHashCodes()
        {
            // Arrange
            var rectangles1 = new List<Rect> { new Rect(10, 10, 80, 40) };
            var rectangles2 = new List<Rect> { new Rect(10, 10, 80, 40) };
            var region1 = Region.FromRectangles(rectangles1).Inflate(3);
            var region2 = Region.FromRectangles(rectangles2).Inflate(3);

            // Act
            var hashCode1 = region1.GetHashCode();
            var hashCode2 = region2.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that regions with different inflation produce different hash codes.
        /// Expected result: Same regions with different inflation values have different hash codes.
        /// </summary>
        [Fact]
        public void GetHashCode_SameRegionDifferentInflation_ReturnDifferentHashCodes()
        {
            // Arrange
            var rectangles = new List<Rect> { new Rect(20, 20, 60, 30) };
            var regionWithoutInflation = Region.FromRectangles(rectangles);
            var regionWithInflation = Region.FromRectangles(rectangles).Inflate(2);

            // Act
            var hashCodeWithoutInflation = regionWithoutInflation.GetHashCode();
            var hashCodeWithInflation = regionWithInflation.GetHashCode();

            // Assert
            Assert.NotEqual(hashCodeWithoutInflation, hashCodeWithInflation);
        }

        /// <summary>
        /// Tests that regions with different inflation amounts produce different hash codes.
        /// Expected result: Same regions with different inflation amounts have different hash codes.
        /// </summary>
        [Fact]
        public void GetHashCode_SameRegionDifferentInflationAmounts_ReturnDifferentHashCodes()
        {
            // Arrange
            var rectangles = new List<Rect> { new Rect(15, 15, 70, 35) };
            var region1 = Region.FromRectangles(rectangles).Inflate(1);
            var region2 = Region.FromRectangles(rectangles).Inflate(5);

            // Act
            var hashCode1 = region1.GetHashCode();
            var hashCode2 = region2.GetHashCode();

            // Assert
            Assert.NotEqual(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that different regions produce different hash codes.
        /// Expected result: Different regions should ideally have different hash codes.
        /// </summary>
        [Fact]
        public void GetHashCode_DifferentRegions_ReturnDifferentHashCodes()
        {
            // Arrange
            var rectangles1 = new List<Rect> { new Rect(0, 0, 100, 50) };
            var rectangles2 = new List<Rect> { new Rect(10, 10, 80, 40) };
            var region1 = Region.FromRectangles(rectangles1);
            var region2 = Region.FromRectangles(rectangles2);

            // Act
            var hashCode1 = region1.GetHashCode();
            var hashCode2 = region2.GetHashCode();

            // Assert
            Assert.NotEqual(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that empty regions produce consistent hash codes.
        /// Expected result: Empty regions without inflation have consistent hash codes.
        /// </summary>
        [Fact]
        public void GetHashCode_EmptyRegion_ReturnsConsistentHashCode()
        {
            // Arrange
            var emptyRegion = Region.FromRectangles(new List<Rect>());

            // Act
            var hashCode1 = emptyRegion.GetHashCode();
            var hashCode2 = emptyRegion.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests hash code calculation with asymmetric inflation values.
        /// Expected result: Regions with asymmetric inflation produce consistent hash codes.
        /// </summary>
        [Fact]
        public void GetHashCode_RegionWithAsymmetricInflation_ReturnsConsistentHashCode()
        {
            // Arrange
            var rectangles = new List<Rect> { new Rect(25, 25, 50, 25) };
            var region = Region.FromRectangles(rectangles).Inflate(1, 2, 3, 4);

            // Act
            var hashCode1 = region.GetHashCode();
            var hashCode2 = region.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that regions with multiple rectangles and inflation produce consistent hash codes.
        /// Expected result: Complex regions with multiple rectangles and inflation have consistent hash codes.
        /// </summary>
        [Fact]
        public void GetHashCode_MultipleRectanglesWithInflation_ReturnsConsistentHashCode()
        {
            // Arrange
            var rectangles = new List<Rect>
            {
                new Rect(0, 0, 50, 25),
                new Rect(0, 25, 50, 25),
                new Rect(50, 0, 50, 50)
            };
            var region = Region.FromRectangles(rectangles).Inflate(2);

            // Act
            var hashCode1 = region.GetHashCode();
            var hashCode2 = region.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that FromRectangles throws ArgumentNullException when rectangles parameter is null.
        /// </summary>
        [Fact]
        public void FromRectangles_NullRectangles_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable<Rect> rectangles = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Region.FromRectangles(rectangles));
        }

        /// <summary>
        /// Tests that FromRectangles creates a valid Region when given an empty collection of rectangles.
        /// </summary>
        [Fact]
        public void FromRectangles_EmptyCollection_ReturnsValidRegion()
        {
            // Arrange
            var rectangles = new Rect[0];

            // Act
            var result = Region.FromRectangles(rectangles);

            // Assert
            Assert.NotEqual(default(Region), result);
        }

        /// <summary>
        /// Tests that FromRectangles creates a valid Region when given an empty List of rectangles.
        /// </summary>
        [Fact]
        public void FromRectangles_EmptyList_ReturnsValidRegion()
        {
            // Arrange
            var rectangles = new List<Rect>();

            // Act
            var result = Region.FromRectangles(rectangles);

            // Assert
            Assert.NotEqual(default(Region), result);
        }

        /// <summary>
        /// Tests that FromRectangles creates a valid Region when given a single rectangle.
        /// </summary>
        [Fact]
        public void FromRectangles_SingleRectangle_ReturnsValidRegion()
        {
            // Arrange
            var rectangles = new[] { new Rect(10, 20, 100, 50) };

            // Act
            var result = Region.FromRectangles(rectangles);

            // Assert
            Assert.NotEqual(default(Region), result);
        }

        /// <summary>
        /// Tests that FromRectangles creates a valid Region when given multiple rectangles.
        /// </summary>
        [Fact]
        public void FromRectangles_MultipleRectangles_ReturnsValidRegion()
        {
            // Arrange
            var rectangles = new[]
            {
                new Rect(0, 0, 100, 50),
                new Rect(50, 25, 75, 100),
                new Rect(200, 150, 300, 200)
            };

            // Act
            var result = Region.FromRectangles(rectangles);

            // Assert
            Assert.NotEqual(default(Region), result);
        }

        /// <summary>
        /// Tests that FromRectangles handles rectangles with zero dimensions.
        /// </summary>
        [Fact]
        public void FromRectangles_ZeroDimensionRectangles_ReturnsValidRegion()
        {
            // Arrange
            var rectangles = new[]
            {
                new Rect(0, 0, 0, 0),
                new Rect(10, 10, 0, 50),
                new Rect(20, 20, 100, 0)
            };

            // Act
            var result = Region.FromRectangles(rectangles);

            // Assert
            Assert.NotEqual(default(Region), result);
        }

        /// <summary>
        /// Tests that FromRectangles handles rectangles with negative dimensions.
        /// </summary>
        [Fact]
        public void FromRectangles_NegativeDimensionRectangles_ReturnsValidRegion()
        {
            // Arrange
            var rectangles = new[]
            {
                new Rect(-10, -20, 100, 50),
                new Rect(0, 0, -50, 30),
                new Rect(10, 20, 100, -40)
            };

            // Act
            var result = Region.FromRectangles(rectangles);

            // Assert
            Assert.NotEqual(default(Region), result);
        }

        /// <summary>
        /// Tests that FromRectangles handles rectangles with extreme values.
        /// </summary>
        [Fact]
        public void FromRectangles_ExtremeValueRectangles_ReturnsValidRegion()
        {
            // Arrange
            var rectangles = new[]
            {
                new Rect(double.MinValue, double.MinValue, double.MaxValue, double.MaxValue),
                new Rect(0, 0, double.PositiveInfinity, double.PositiveInfinity),
                new Rect(double.NaN, double.NaN, double.NaN, double.NaN)
            };

            // Act
            var result = Region.FromRectangles(rectangles);

            // Assert
            Assert.NotEqual(default(Region), result);
        }

        /// <summary>
        /// Tests that FromRectangles works with different IEnumerable implementations.
        /// </summary>
        [Fact]
        public void FromRectangles_DifferentEnumerableTypes_ReturnsValidRegion()
        {
            // Arrange
            var rectanglesList = new List<Rect> { new Rect(0, 0, 50, 50) };
            var rectanglesHashSet = new HashSet<Rect> { new Rect(10, 10, 60, 60) };
            var rectanglesLinq = rectanglesList.Where(r => r.Width > 0);

            // Act
            var resultFromList = Region.FromRectangles(rectanglesList);
            var resultFromHashSet = Region.FromRectangles(rectanglesHashSet);
            var resultFromLinq = Region.FromRectangles(rectanglesLinq);

            // Assert
            Assert.NotEqual(default(Region), resultFromList);
            Assert.NotEqual(default(Region), resultFromHashSet);
            Assert.NotEqual(default(Region), resultFromLinq);
        }

        /// <summary>
        /// Tests that FromRectangles creates equal regions when given the same rectangles.
        /// </summary>
        [Fact]
        public void FromRectangles_SameRectangles_CreatesEqualRegions()
        {
            // Arrange
            var rectangles1 = new[] { new Rect(10, 20, 100, 50), new Rect(50, 75, 80, 60) };
            var rectangles2 = new[] { new Rect(10, 20, 100, 50), new Rect(50, 75, 80, 60) };

            // Act
            var region1 = Region.FromRectangles(rectangles1);
            var region2 = Region.FromRectangles(rectangles2);

            // Assert
            Assert.Equal(region1, region2);
            Assert.True(region1 == region2);
            Assert.False(region1 != region2);
        }
    }


    public partial class RegionInflateTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that Inflate returns the same region when Regions property is null (default constructor).
        /// This test covers the uncovered null check in the Inflate method.
        /// </summary>
        [Fact]
        public void Inflate_NullRegions_ReturnsThis()
        {
            // Arrange
            var region = new Region(); // Default constructor creates region with null Regions

            // Act
            var result = region.Inflate(10, 20, 30, 40);

            // Assert
            Assert.Equal(region, result);
        }

        /// <summary>
        /// Tests that Inflate works correctly with an empty regions collection.
        /// </summary>
        [Fact]
        public void Inflate_EmptyRegions_ReturnsNewRegionWithCorrectInflation()
        {
            // Arrange
            var region = Region.FromRectangles(new List<Rect>());

            // Act
            var result = region.Inflate(5, 10, 15, 20);

            // Assert
            Assert.NotEqual(region, result);
            // The result should have no regions since input was empty
        }

        /// <summary>
        /// Tests that Inflate correctly inflates a single rectangle region.
        /// </summary>
        [Fact]
        public void Inflate_SingleRectangle_InflatesCorrectly()
        {
            // Arrange
            var originalRect = new Rect(10, 20, 30, 40);
            var region = Region.FromRectangles(new[] { originalRect });

            // Act
            var result = region.Inflate(5, 8, 12, 15);

            // Assert
            Assert.NotEqual(region, result);
            // Verify the inflated region contains points that should be inside after inflation
            Assert.True(result.Contains(5, 12)); // Left-top corner (10-5, 20-8)
            Assert.True(result.Contains(51, 74)); // Right-bottom corner (10+30+12-1, 20+40+15-1)
        }

        /// <summary>
        /// Tests that Inflate correctly inflates multiple rectangles.
        /// </summary>
        [Fact]
        public void Inflate_MultipleRectangles_InflatesAllCorrectly()
        {
            // Arrange
            var rect1 = new Rect(0, 0, 10, 10);
            var rect2 = new Rect(20, 30, 15, 25);
            var region = Region.FromRectangles(new[] { rect1, rect2 });

            // Act
            var result = region.Inflate(2, 3, 4, 5);

            // Assert
            Assert.NotEqual(region, result);
            // First rectangle: original (0,0,10,10) -> inflated (-2,-3,16,18)
            Assert.True(result.Contains(-1, -2));
            Assert.True(result.Contains(13, 14));
            // Second rectangle: original (20,30,15,25) -> inflated (18,27,21,33)
            Assert.True(result.Contains(19, 28));
            Assert.True(result.Contains(38, 59));
        }

        /// <summary>
        /// Tests that Inflate with zero values returns a region with identical rectangles but different inflation.
        /// </summary>
        [Fact]
        public void Inflate_ZeroValues_ReturnsNewRegionWithZeroInflation()
        {
            // Arrange
            var originalRect = new Rect(10, 20, 30, 40);
            var region = Region.FromRectangles(new[] { originalRect });

            // Act
            var result = region.Inflate(0, 0, 0, 0);

            // Assert
            Assert.NotEqual(region, result);
            // Should contain the same points as original
            Assert.True(result.Contains(10, 20));
            Assert.True(result.Contains(39, 59));
            Assert.False(result.Contains(9, 19));
            Assert.False(result.Contains(40, 60));
        }

        /// <summary>
        /// Tests that Inflate with negative values deflates the region correctly.
        /// </summary>
        [Fact]
        public void Inflate_NegativeValues_DeflatesRegion()
        {
            // Arrange
            var originalRect = new Rect(0, 0, 100, 100);
            var region = Region.FromRectangles(new[] { originalRect });

            // Act
            var result = region.Inflate(-10, -15, -20, -25);

            // Assert
            Assert.NotEqual(region, result);
            // Deflated rectangle: (0,0,100,100) -> (10,15,70,60)
            Assert.True(result.Contains(10, 15));
            Assert.True(result.Contains(79, 74));
            Assert.False(result.Contains(9, 14));
            Assert.False(result.Contains(80, 75));
        }

        /// <summary>
        /// Tests that Inflate adds to existing inflation when the region already has inflation.
        /// </summary>
        [Fact]
        public void Inflate_ExistingInflation_AddsToExistingInflation()
        {
            // Arrange
            var originalRect = new Rect(10, 10, 20, 20);
            var region = Region.FromRectangles(new[] { originalRect }).Inflate(5, 5, 5, 5);

            // Act
            var result = region.Inflate(3, 4, 6, 7);

            // Assert
            Assert.NotEqual(region, result);
            // Should contain points that account for both inflations
            Assert.True(result.Contains(2, 1)); // 10-5-3, 10-5-4
            Assert.True(result.Contains(43, 45)); // 10+20+5+6+3-1, 10+20+5+7+4-1
        }

        /// <summary>
        /// Tests Inflate with extreme double values to ensure robustness.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, 0, 0, 0)]
        [InlineData(0, double.MaxValue, 0, 0)]
        [InlineData(0, 0, double.MaxValue, 0)]
        [InlineData(0, 0, 0, double.MaxValue)]
        [InlineData(double.MinValue, 0, 0, 0)]
        [InlineData(0, double.MinValue, 0, 0)]
        [InlineData(0, 0, double.MinValue, 0)]
        [InlineData(0, 0, 0, double.MinValue)]
        public void Inflate_ExtremeValues_HandlesCorrectly(double left, double top, double right, double bottom)
        {
            // Arrange
            var originalRect = new Rect(100, 100, 50, 50);
            var region = Region.FromRectangles(new[] { originalRect });

            // Act & Assert - Should not throw exception
            var result = region.Inflate(left, top, right, bottom);
            Assert.NotEqual(region, result);
        }

        /// <summary>
        /// Tests Inflate with special double values (NaN, Infinity).
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0, 0, 0)]
        [InlineData(0, double.NaN, 0, 0)]
        [InlineData(0, 0, double.NaN, 0)]
        [InlineData(0, 0, 0, double.NaN)]
        [InlineData(double.PositiveInfinity, 0, 0, 0)]
        [InlineData(0, double.PositiveInfinity, 0, 0)]
        [InlineData(0, 0, double.PositiveInfinity, 0)]
        [InlineData(0, 0, 0, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 0, 0, 0)]
        [InlineData(0, double.NegativeInfinity, 0, 0)]
        [InlineData(0, 0, double.NegativeInfinity, 0)]
        [InlineData(0, 0, 0, double.NegativeInfinity)]
        public void Inflate_SpecialDoubleValues_HandlesCorrectly(double left, double top, double right, double bottom)
        {
            // Arrange
            var originalRect = new Rect(10, 10, 20, 20);
            var region = Region.FromRectangles(new[] { originalRect });

            // Act & Assert - Should not throw exception
            var result = region.Inflate(left, top, right, bottom);
            Assert.NotEqual(region, result);
        }

        /// <summary>
        /// Tests that Inflate creates rectangles with correct dimensions and positions.
        /// </summary>
        [Fact]
        public void Inflate_ValidParameters_CreatesCorrectRectangleDimensions()
        {
            // Arrange
            var originalRect = new Rect(50, 60, 40, 30);
            var region = Region.FromRectangles(new[] { originalRect });

            // Act
            var result = region.Inflate(10, 5, 15, 20);

            // Assert
            // Original: x=50, y=60, width=40, height=30
            // After inflation: x=40, y=55, width=65, height=55
            Assert.True(result.Contains(40, 55)); // New top-left
            Assert.True(result.Contains(104, 109)); // New bottom-right (40+65-1, 55+55-1)
            Assert.False(result.Contains(39, 54)); // Just outside top-left
            Assert.False(result.Contains(105, 110)); // Just outside bottom-right
        }
    }
}