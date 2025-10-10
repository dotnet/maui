using System;

using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
    public partial class SwipeDirectionHelperTests
    {
        /// <summary>
        /// Tests GetAngleFromPoints with identical points.
        /// Should handle the case where both points are the same.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetAngleFromPoints_IdenticalPoints_ReturnsValidAngle()
        {
            // Arrange
            double x1 = 5.0, y1 = 5.0;
            double x2 = 5.0, y2 = 5.0;

            // Act
            double result = SwipeDirectionHelper.GetAngleFromPoints(x1, y1, x2, y2);

            // Assert
            Assert.True(result >= 0 && result < 360);
        }

        /// <summary>
        /// Tests GetAngleFromPoints with horizontal line pointing right.
        /// Input: Point (0,0) to (1,0) should represent rightward direction.
        /// Expected: Specific angle representing rightward movement.
        /// </summary>
        [Fact]
        [Trait("Category", "auto-generated")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        public void GetAngleFromPoints_HorizontalLineRight_ReturnsExpectedAngle()
        {
            // Arrange
            double x1 = 0.0, y1 = 0.0;
            double x2 = 1.0, y2 = 0.0;

            // Act
            double result = SwipeDirectionHelper.GetAngleFromPoints(x1, y1, x2, y2);

            // Assert
            Assert.Equal(0.0, result, 1);
        }

        /// <summary>
        /// Tests GetAngleFromPoints with horizontal line pointing left.
        /// Input: Point (1,0) to (0,0) should represent leftward direction.
        /// Expected: Specific angle representing leftward movement.
        /// </summary>
        [Fact]
        [Trait("Category", "auto-generated")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        public void GetAngleFromPoints_HorizontalLineLeft_ReturnsExpectedAngle()
        {
            // Arrange
            double x1 = 1.0, y1 = 0.0;
            double x2 = 0.0, y2 = 0.0;

            // Act
            double result = SwipeDirectionHelper.GetAngleFromPoints(x1, y1, x2, y2);

            // Assert
            Assert.Equal(180.0, result, 1);
        }

        /// <summary>
        /// Tests GetAngleFromPoints with vertical line pointing up.
        /// Input: Point (0,1) to (0,0) should represent upward direction.
        /// Expected: Specific angle representing upward movement.
        /// </summary>
        [Fact]
        [Trait("Category", "auto-generated")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        public void GetAngleFromPoints_VerticalLineUp_ReturnsExpectedAngle()
        {
            // Arrange
            double x1 = 0.0, y1 = 1.0;
            double x2 = 0.0, y2 = 0.0;

            // Act
            double result = SwipeDirectionHelper.GetAngleFromPoints(x1, y1, x2, y2);

            // Assert
            Assert.Equal(90.0, result, 1);
        }

        /// <summary>
        /// Tests GetAngleFromPoints with vertical line pointing down.
        /// Input: Point (0,0) to (0,1) should represent downward direction.
        /// Expected: Specific angle representing downward movement.
        /// </summary>
        [Fact]
        [Trait("Category", "auto-generated")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        public void GetAngleFromPoints_VerticalLineDown_ReturnsExpectedAngle()
        {
            // Arrange
            double x1 = 0.0, y1 = 0.0;
            double x2 = 0.0, y2 = 1.0;

            // Act
            double result = SwipeDirectionHelper.GetAngleFromPoints(x1, y1, x2, y2);

            // Assert
            Assert.Equal(270.0, result, 1);
        }

        /// <summary>
        /// Tests GetAngleFromPoints with extreme numeric values.
        /// Input: Maximum and minimum double values to test numerical stability.
        /// Expected: Method should handle extreme values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue, double.MinValue, double.MinValue)]
        [InlineData(double.MinValue, double.MinValue, double.MaxValue, double.MaxValue)]
        [InlineData(0.0, 0.0, double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue, 0.0, 0.0)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetAngleFromPoints_ExtremeValues_HandlesWithoutException(double x1, double y1, double x2, double y2)
        {
            // Act & Assert - Should not throw
            double result = SwipeDirectionHelper.GetAngleFromPoints(x1, y1, x2, y2);

            // Verify result is within expected range or is a special value
            Assert.True(double.IsNaN(result) || double.IsInfinity(result) || (result >= 0 && result < 360));
        }

        /// <summary>
        /// Tests GetAngleFromPoints with special floating point values.
        /// Input: NaN, PositiveInfinity, and NegativeInfinity values.
        /// Expected: Method should handle special values gracefully.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0.0, 1.0, 0.0)]
        [InlineData(0.0, double.NaN, 1.0, 0.0)]
        [InlineData(0.0, 0.0, double.NaN, 0.0)]
        [InlineData(0.0, 0.0, 1.0, double.NaN)]
        [InlineData(double.PositiveInfinity, 0.0, 1.0, 0.0)]
        [InlineData(0.0, double.PositiveInfinity, 1.0, 0.0)]
        [InlineData(double.NegativeInfinity, 0.0, 1.0, 0.0)]
        [InlineData(0.0, double.NegativeInfinity, 1.0, 0.0)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetAngleFromPoints_SpecialFloatingPointValues_HandlesGracefully(double x1, double y1, double x2, double y2)
        {
            // Act & Assert - Should not throw
            double result = SwipeDirectionHelper.GetAngleFromPoints(x1, y1, x2, y2);

            // Result may be NaN or infinity with special inputs
            Assert.True(double.IsNaN(result) || double.IsInfinity(result) || (result >= 0 && result < 360));
        }

        /// <summary>
        /// Tests GetAngleFromPoints with various quadrant combinations.
        /// Input: Points in different quadrants to verify angle calculations.
        /// Expected: Correct angles for diagonal movements between quadrants.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0, 1.0, 1.0, 315.0)] // First quadrant diagonal
        [InlineData(0.0, 0.0, -1.0, 1.0, 225.0)] // Second quadrant diagonal
        [InlineData(0.0, 0.0, -1.0, -1.0, 135.0)] // Third quadrant diagonal
        [InlineData(0.0, 0.0, 1.0, -1.0, 45.0)] // Fourth quadrant diagonal
        [Trait("Category", "auto-generated")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        public void GetAngleFromPoints_QuadrantDiagonals_ReturnsExpectedAngles(double x1, double y1, double x2, double y2, double expectedAngle)
        {
            // Act
            double result = SwipeDirectionHelper.GetAngleFromPoints(x1, y1, x2, y2);

            // Assert
            Assert.Equal(expectedAngle, result, 1);
        }

        /// <summary>
        /// Tests GetAngleFromPoints with very small coordinate differences.
        /// Input: Points with minimal differences to test precision handling.
        /// Expected: Method should handle small differences without precision errors.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetAngleFromPoints_VerySmallDifferences_HandlesCorrectly()
        {
            // Arrange
            double x1 = 1.0, y1 = 1.0;
            double x2 = 1.0 + double.Epsilon, y2 = 1.0 + double.Epsilon;

            // Act
            double result = SwipeDirectionHelper.GetAngleFromPoints(x1, y1, x2, y2);

            // Assert
            Assert.True(result >= 0 && result < 360);
        }

        /// <summary>
        /// Tests GetAngleFromPoints with negative coordinates.
        /// Input: Negative coordinate values to ensure proper handling.
        /// Expected: Correct angle calculations with negative coordinates.
        /// </summary>
        [Theory]
        [InlineData(-5.0, -3.0, -1.0, -1.0)]
        [InlineData(-10.0, 0.0, -5.0, -5.0)]
        [InlineData(0.0, -10.0, 5.0, -5.0)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetAngleFromPoints_NegativeCoordinates_ReturnsValidAngle(double x1, double y1, double x2, double y2)
        {
            // Act
            double result = SwipeDirectionHelper.GetAngleFromPoints(x1, y1, x2, y2);

            // Assert
            Assert.True(result >= 0 && result < 360);
            Assert.False(double.IsNaN(result));
            Assert.False(double.IsInfinity(result));
        }

        /// <summary>
        /// Tests GetAngleFromPoints return value normalization.
        /// Input: Various point combinations.
        /// Expected: All results should be normalized to [0, 360) range.
        /// </summary>
        [Theory]
        [InlineData(100.0, 200.0, 300.0, 400.0)]
        [InlineData(-100.0, -200.0, 50.0, 75.0)]
        [InlineData(1.5, 2.7, 3.14, 2.718)]
        [InlineData(0.001, 0.002, 0.003, 0.004)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetAngleFromPoints_VariousInputs_ReturnsNormalizedAngle(double x1, double y1, double x2, double y2)
        {
            // Act
            double result = SwipeDirectionHelper.GetAngleFromPoints(x1, y1, x2, y2);

            // Assert
            Assert.True(result >= 0.0, $"Angle {result} should be >= 0");
            Assert.True(result < 360.0, $"Angle {result} should be < 360");
            Assert.False(double.IsNaN(result), "Result should not be NaN");
            Assert.False(double.IsInfinity(result), "Result should not be infinity");
        }

        /// <summary>
        /// Tests that GetSwipeDirectionFromAngle returns Up for angles in the range [45, 135).
        /// </summary>
        /// <param name="angle">The angle in degrees to test.</param>
        [Theory]
        [InlineData(45.0)]
        [InlineData(90.0)]
        [InlineData(134.999999)]
        [InlineData(89.5)]
        [InlineData(45.1)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirectionFromAngle_AngleInUpRange_ReturnsUp(double angle)
        {
            // Act
            SwipeDirection result = SwipeDirectionHelper.GetSwipeDirectionFromAngle(angle);

            // Assert
            Assert.Equal(SwipeDirection.Up, result);
        }

        /// <summary>
        /// Tests that GetSwipeDirectionFromAngle returns Right for angles in the ranges [0, 45) or [315, 360).
        /// </summary>
        /// <param name="angle">The angle in degrees to test.</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(22.5)]
        [InlineData(44.999999)]
        [InlineData(315.0)]
        [InlineData(337.5)]
        [InlineData(359.999999)]
        [InlineData(0.1)]
        [InlineData(44.9)]
        [InlineData(315.1)]
        [InlineData(359.9)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirectionFromAngle_AngleInRightRange_ReturnsRight(double angle)
        {
            // Act
            SwipeDirection result = SwipeDirectionHelper.GetSwipeDirectionFromAngle(angle);

            // Assert
            Assert.Equal(SwipeDirection.Right, result);
        }

        /// <summary>
        /// Tests that GetSwipeDirectionFromAngle returns Down for angles in the range [225, 315).
        /// </summary>
        /// <param name="angle">The angle in degrees to test.</param>
        [Theory]
        [InlineData(225.0)]
        [InlineData(270.0)]
        [InlineData(314.999999)]
        [InlineData(247.5)]
        [InlineData(225.1)]
        [InlineData(314.9)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirectionFromAngle_AngleInDownRange_ReturnsDown(double angle)
        {
            // Act
            SwipeDirection result = SwipeDirectionHelper.GetSwipeDirectionFromAngle(angle);

            // Assert
            Assert.Equal(SwipeDirection.Down, result);
        }

        /// <summary>
        /// Tests that GetSwipeDirectionFromAngle returns Left for angles that don't fall into other ranges (default case).
        /// </summary>
        /// <param name="angle">The angle in degrees to test.</param>
        [Theory]
        [InlineData(135.0)]
        [InlineData(180.0)]
        [InlineData(224.999999)]
        [InlineData(150.0)]
        [InlineData(200.0)]
        [InlineData(135.1)]
        [InlineData(224.9)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirectionFromAngle_AngleInLeftRange_ReturnsLeft(double angle)
        {
            // Act
            SwipeDirection result = SwipeDirectionHelper.GetSwipeDirectionFromAngle(angle);

            // Assert
            Assert.Equal(SwipeDirection.Left, result);
        }

        /// <summary>
        /// Tests that GetSwipeDirectionFromAngle handles boundary values correctly at range edges.
        /// </summary>
        /// <param name="angle">The boundary angle to test.</param>
        /// <param name="expectedDirection">The expected swipe direction.</param>
        [Theory]
        [InlineData(45.0, SwipeDirection.Up)]
        [InlineData(135.0, SwipeDirection.Left)]
        [InlineData(225.0, SwipeDirection.Down)]
        [InlineData(315.0, SwipeDirection.Right)]
        [InlineData(360.0, SwipeDirection.Left)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirectionFromAngle_BoundaryValues_ReturnsExpectedDirection(double angle, SwipeDirection expectedDirection)
        {
            // Act
            SwipeDirection result = SwipeDirectionHelper.GetSwipeDirectionFromAngle(angle);

            // Assert
            Assert.Equal(expectedDirection, result);
        }

        /// <summary>
        /// Tests that GetSwipeDirectionFromAngle handles negative angles correctly.
        /// </summary>
        /// <param name="angle">The negative angle to test.</param>
        /// <param name="expectedDirection">The expected swipe direction.</param>
        [Theory]
        [InlineData(-45.0, SwipeDirection.Left)]
        [InlineData(-90.0, SwipeDirection.Left)]
        [InlineData(-180.0, SwipeDirection.Left)]
        [InlineData(-1.0, SwipeDirection.Left)]
        [InlineData(-360.0, SwipeDirection.Left)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirectionFromAngle_NegativeAngles_ReturnsLeft(double angle, SwipeDirection expectedDirection)
        {
            // Act
            SwipeDirection result = SwipeDirectionHelper.GetSwipeDirectionFromAngle(angle);

            // Assert
            Assert.Equal(expectedDirection, result);
        }

        /// <summary>
        /// Tests that GetSwipeDirectionFromAngle handles angles greater than 360 degrees correctly.
        /// </summary>
        /// <param name="angle">The large angle to test.</param>
        /// <param name="expectedDirection">The expected swipe direction.</param>
        [Theory]
        [InlineData(360.0, SwipeDirection.Left)]
        [InlineData(450.0, SwipeDirection.Left)]
        [InlineData(720.0, SwipeDirection.Left)]
        [InlineData(1000.0, SwipeDirection.Left)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirectionFromAngle_LargeAngles_ReturnsLeft(double angle, SwipeDirection expectedDirection)
        {
            // Act
            SwipeDirection result = SwipeDirectionHelper.GetSwipeDirectionFromAngle(angle);

            // Assert
            Assert.Equal(expectedDirection, result);
        }

        /// <summary>
        /// Tests that GetSwipeDirectionFromAngle handles special floating point values.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirectionFromAngle_NaNValue_ReturnsLeft()
        {
            // Act
            SwipeDirection result = SwipeDirectionHelper.GetSwipeDirectionFromAngle(double.NaN);

            // Assert
            Assert.Equal(SwipeDirection.Left, result);
        }

        /// <summary>
        /// Tests that GetSwipeDirectionFromAngle handles positive infinity.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirectionFromAngle_PositiveInfinity_ReturnsLeft()
        {
            // Act
            SwipeDirection result = SwipeDirectionHelper.GetSwipeDirectionFromAngle(double.PositiveInfinity);

            // Assert
            Assert.Equal(SwipeDirection.Left, result);
        }

        /// <summary>
        /// Tests that GetSwipeDirectionFromAngle handles negative infinity.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirectionFromAngle_NegativeInfinity_ReturnsLeft()
        {
            // Act
            SwipeDirection result = SwipeDirectionHelper.GetSwipeDirectionFromAngle(double.NegativeInfinity);

            // Assert
            Assert.Equal(SwipeDirection.Left, result);
        }

        /// <summary>
        /// Tests that GetSwipeDirectionFromAngle handles very small positive values.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirectionFromAngle_VerySmallPositiveValue_ReturnsRight()
        {
            // Act
            SwipeDirection result = SwipeDirectionHelper.GetSwipeDirectionFromAngle(double.Epsilon);

            // Assert
            Assert.Equal(SwipeDirection.Right, result);
        }

        /// <summary>
        /// Tests IsAngleInRange method with various angle values and range boundaries.
        /// Verifies that angles within the range [init, end) return true, and angles outside return false.
        /// </summary>
        /// <param name="angle">The angle to test</param>
        /// <param name="init">The inclusive start of the range</param>
        /// <param name="end">The exclusive end of the range</param>
        /// <param name="expected">Expected result</param>
        [Theory]
        [InlineData(45.0, 0.0f, 90.0f, true)]  // Angle within range
        [InlineData(0.0, 0.0f, 90.0f, true)]   // Angle equals init (inclusive boundary)
        [InlineData(90.0, 0.0f, 90.0f, false)] // Angle equals end (exclusive boundary)
        [InlineData(-10.0, 0.0f, 90.0f, false)] // Angle less than init
        [InlineData(100.0, 0.0f, 90.0f, false)] // Angle greater than end
        [InlineData(180.0, 180.0f, 360.0f, true)] // Angle equals init at different range
        [InlineData(359.999, 180.0f, 360.0f, true)] // Angle just before end
        [InlineData(360.0, 180.0f, 360.0f, false)] // Angle equals end at different range
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsAngleInRange_VariousAnglesAndRanges_ReturnsExpectedResult(double angle, float init, float end, bool expected)
        {
            // Act
            bool result = SwipeDirectionHelper.IsAngleInRange(angle, init, end);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests IsAngleInRange method with edge cases where init equals or is greater than end.
        /// Verifies behavior with empty ranges and reversed ranges.
        /// </summary>
        /// <param name="angle">The angle to test</param>
        /// <param name="init">The inclusive start of the range</param>
        /// <param name="end">The exclusive end of the range</param>
        /// <param name="expected">Expected result</param>
        [Theory]
        [InlineData(45.0, 90.0f, 90.0f, false)] // Empty range (init == end)
        [InlineData(90.0, 90.0f, 90.0f, false)] // Angle equals both init and end in empty range
        [InlineData(45.0, 90.0f, 0.0f, false)]  // Reversed range (init > end)
        [InlineData(45.0, 100.0f, 50.0f, false)] // Angle in reversed range
        [InlineData(0.0, 10.0f, 5.0f, false)]   // Zero angle in reversed range
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsAngleInRange_EdgeCaseRanges_ReturnsExpectedResult(double angle, float init, float end, bool expected)
        {
            // Act
            bool result = SwipeDirectionHelper.IsAngleInRange(angle, init, end);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests IsAngleInRange method with precision-sensitive boundary values.
        /// Verifies correct behavior when dealing with double-float precision differences.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsAngleInRange_PrecisionBoundaryValues_ReturnsExpectedResult()
        {
            // Arrange
            double angle = 90.00000000001; // Slightly greater than 90
            float init = 0.0f;
            float end = 90.0f;

            // Act
            bool result = SwipeDirectionHelper.IsAngleInRange(angle, init, end);

            // Assert
            Assert.False(result); // Should be false as angle > end
        }

        /// <summary>
        /// Tests IsAngleInRange method with very small positive range.
        /// Verifies behavior with minimal range differences.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsAngleInRange_VerySmallRange_ReturnsExpectedResult()
        {
            // Arrange
            double angle = 0.5;
            float init = 0.0f;
            float end = 1.0f;

            // Act
            bool result = SwipeDirectionHelper.IsAngleInRange(angle, init, end);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests IsAngleInRange method with negative range values.
        /// Verifies correct handling of negative init and end values.
        /// </summary>
        [Theory]
        [InlineData(-45.0, -90.0f, 0.0f, true)]   // Negative angle in negative-to-positive range
        [InlineData(-45.0, -30.0f, -60.0f, false)] // Negative angle in reversed negative range
        [InlineData(-90.0, -90.0f, -45.0f, true)]  // Negative angle equals negative init
        [InlineData(-45.0, -90.0f, -45.0f, false)] // Negative angle equals negative end
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsAngleInRange_NegativeRangeValues_ReturnsExpectedResult(double angle, float init, float end, bool expected)
        {
            // Act
            bool result = SwipeDirectionHelper.IsAngleInRange(angle, init, end);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that GetSwipeDirection returns Right for horizontal rightward movement.
        /// Validates movement from left to right along horizontal axis.
        /// Expected result: SwipeDirection.Right.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirection_HorizontalRightwardMovement_ReturnsRight()
        {
            // Arrange
            var initialPoint = new Point(0, 0);
            var endPoint = new Point(10, 0);

            // Act
            var result = SwipeDirectionHelper.GetSwipeDirection(initialPoint, endPoint);

            // Assert
            Assert.Equal(SwipeDirection.Right, result);
        }

        /// <summary>
        /// Tests that GetSwipeDirection returns Left for horizontal leftward movement.
        /// Validates movement from right to left along horizontal axis.
        /// Expected result: SwipeDirection.Left.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirection_HorizontalLeftwardMovement_ReturnsLeft()
        {
            // Arrange
            var initialPoint = new Point(10, 0);
            var endPoint = new Point(0, 0);

            // Act
            var result = SwipeDirectionHelper.GetSwipeDirection(initialPoint, endPoint);

            // Assert
            Assert.Equal(SwipeDirection.Left, result);
        }

        /// <summary>
        /// Tests that GetSwipeDirection returns Up for vertical upward movement.
        /// Validates movement from bottom to top along vertical axis (decreasing Y).
        /// Expected result: SwipeDirection.Up.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirection_VerticalUpwardMovement_ReturnsUp()
        {
            // Arrange
            var initialPoint = new Point(0, 10);
            var endPoint = new Point(0, 0);

            // Act
            var result = SwipeDirectionHelper.GetSwipeDirection(initialPoint, endPoint);

            // Assert
            Assert.Equal(SwipeDirection.Up, result);
        }

        /// <summary>
        /// Tests that GetSwipeDirection returns Down for vertical downward movement.
        /// Validates movement from top to bottom along vertical axis (increasing Y).
        /// Expected result: SwipeDirection.Down.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirection_VerticalDownwardMovement_ReturnsDown()
        {
            // Arrange
            var initialPoint = new Point(0, 0);
            var endPoint = new Point(0, 10);

            // Act
            var result = SwipeDirectionHelper.GetSwipeDirection(initialPoint, endPoint);

            // Assert
            Assert.Equal(SwipeDirection.Down, result);
        }

        /// <summary>
        /// Tests GetSwipeDirection with diagonal movements in all four quadrants.
        /// Validates that diagonal movements are correctly classified into primary directions.
        /// Expected results: Right, Left, Up, Down for respective diagonal movements.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 10, 3, SwipeDirection.Right)]  // Northeast diagonal -> Right
        [InlineData(0, 0, 10, -3, SwipeDirection.Right)] // Southeast diagonal -> Right  
        [InlineData(10, 0, 0, 3, SwipeDirection.Left)]   // Northwest diagonal -> Left
        [InlineData(10, 0, 0, -3, SwipeDirection.Left)]  // Southwest diagonal -> Left
        [InlineData(0, 0, 3, -10, SwipeDirection.Up)]    // Steep upward diagonal -> Up
        [InlineData(0, 0, -3, -10, SwipeDirection.Up)]   // Steep upward diagonal -> Up
        [InlineData(0, 0, 3, 10, SwipeDirection.Down)]   // Steep downward diagonal -> Down
        [InlineData(0, 0, -3, 10, SwipeDirection.Down)]  // Steep downward diagonal -> Down
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirection_DiagonalMovements_ReturnsExpectedDirection(
            double x1, double y1, double x2, double y2, SwipeDirection expected)
        {
            // Arrange
            var initialPoint = new Point(x1, y1);
            var endPoint = new Point(x2, y2);

            // Act
            var result = SwipeDirectionHelper.GetSwipeDirection(initialPoint, endPoint);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests GetSwipeDirection with extreme coordinate values.
        /// Validates behavior with double.MinValue and double.MaxValue coordinates.
        /// Expected result: Method should handle extreme values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue, 0, double.MaxValue, 0, SwipeDirection.Right)]
        [InlineData(double.MaxValue, 0, double.MinValue, 0, SwipeDirection.Left)]
        [InlineData(0, double.MaxValue, 0, double.MinValue, SwipeDirection.Up)]
        [InlineData(0, double.MinValue, 0, double.MaxValue, SwipeDirection.Down)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirection_ExtremeCoordinates_ReturnsExpectedDirection(
            double x1, double y1, double x2, double y2, SwipeDirection expected)
        {
            // Arrange
            var initialPoint = new Point(x1, y1);
            var endPoint = new Point(x2, y2);

            // Act
            var result = SwipeDirectionHelper.GetSwipeDirection(initialPoint, endPoint);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests GetSwipeDirection with special double values including NaN and Infinity.
        /// Validates behavior when coordinates contain special floating-point values.
        /// Expected result: Method should handle special values gracefully.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0, 1, 0)]
        [InlineData(0, double.NaN, 0, 1)]
        [InlineData(1, 0, double.NaN, 0)]
        [InlineData(0, 1, 0, double.NaN)]
        [InlineData(double.PositiveInfinity, 0, 1, 0)]
        [InlineData(0, double.PositiveInfinity, 0, 1)]
        [InlineData(1, 0, double.PositiveInfinity, 0)]
        [InlineData(0, 1, 0, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 0, 1, 0)]
        [InlineData(0, double.NegativeInfinity, 0, 1)]
        [InlineData(1, 0, double.NegativeInfinity, 0)]
        [InlineData(0, 1, 0, double.NegativeInfinity)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirection_SpecialDoubleValues_DoesNotThrow(
            double x1, double y1, double x2, double y2)
        {
            // Arrange
            var initialPoint = new Point(x1, y1);
            var endPoint = new Point(x2, y2);

            // Act & Assert
            var exception = Record.Exception(() => SwipeDirectionHelper.GetSwipeDirection(initialPoint, endPoint));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests GetSwipeDirection with identical start and end points.
        /// Validates behavior when there is no movement (zero-length swipe).
        /// Expected result: Method should handle zero movement without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(5, 5)]
        [InlineData(-10, -10)]
        [InlineData(100.5, 200.7)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirection_IdenticalPoints_DoesNotThrow(double x, double y)
        {
            // Arrange
            var initialPoint = new Point(x, y);
            var endPoint = new Point(x, y);

            // Act & Assert
            var exception = Record.Exception(() => SwipeDirectionHelper.GetSwipeDirection(initialPoint, endPoint));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests GetSwipeDirection with negative coordinate values.
        /// Validates behavior with negative X and Y coordinates in various combinations.
        /// Expected results: Correct directional classification regardless of coordinate signs.
        /// </summary>
        [Theory]
        [InlineData(-10, -5, 0, -5, SwipeDirection.Right)]  // Negative to zero horizontal
        [InlineData(0, -5, -10, -5, SwipeDirection.Left)]   // Zero to negative horizontal
        [InlineData(-5, 0, -5, -10, SwipeDirection.Up)]     // Negative vertical upward
        [InlineData(-5, -10, -5, 0, SwipeDirection.Down)]   // Negative vertical downward
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirection_NegativeCoordinates_ReturnsExpectedDirection(
            double x1, double y1, double x2, double y2, SwipeDirection expected)
        {
            // Arrange
            var initialPoint = new Point(x1, y1);
            var endPoint = new Point(x2, y2);

            // Act
            var result = SwipeDirectionHelper.GetSwipeDirection(initialPoint, endPoint);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests GetSwipeDirection with very small movement differences.
        /// Validates behavior with minimal coordinate changes near floating-point precision limits.
        /// Expected result: Method should handle very small movements correctly.
        /// </summary>
        [Theory]
        [InlineData(0, 0, double.Epsilon, 0, SwipeDirection.Right)]
        [InlineData(double.Epsilon, 0, 0, 0, SwipeDirection.Left)]
        [InlineData(0, double.Epsilon, 0, 0, SwipeDirection.Up)]
        [InlineData(0, 0, 0, double.Epsilon, SwipeDirection.Down)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetSwipeDirection_VerySmallMovements_ReturnsExpectedDirection(
            double x1, double y1, double x2, double y2, SwipeDirection expected)
        {
            // Arrange
            var initialPoint = new Point(x1, y1);
            var endPoint = new Point(x2, y2);

            // Act
            var result = SwipeDirectionHelper.GetSwipeDirection(initialPoint, endPoint);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}