#nullable disable

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Foldable;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class IFoldableServiceTests
    {
        /// <summary>
        /// Tests that GetLocationOnScreen throws ArgumentNullException when visualElement parameter is null.
        /// This verifies proper null parameter validation in the interface contract.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void GetLocationOnScreen_NullVisualElement_ThrowsArgumentNullException()
        {
            // Arrange
            var foldableService = Substitute.For<IFoldableService>();
            foldableService.GetLocationOnScreen(null).Returns<Point?>(x => throw new ArgumentNullException(nameof(x)));

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => foldableService.GetLocationOnScreen(null));
        }

        /// <summary>
        /// Tests that GetLocationOnScreen returns null when the visual element is not visible on screen.
        /// This simulates scenarios where elements are off-screen or hidden.
        /// Expected result: null Point should be returned.
        /// </summary>
        [Fact]
        public void GetLocationOnScreen_ValidVisualElementNotOnScreen_ReturnsNull()
        {
            // Arrange
            var foldableService = Substitute.For<IFoldableService>();
            var visualElement = Substitute.For<VisualElement>();
            foldableService.GetLocationOnScreen(visualElement).Returns((Point?)null);

            // Act
            var result = foldableService.GetLocationOnScreen(visualElement);

            // Assert
            Assert.Null(result);
            foldableService.Received(1).GetLocationOnScreen(visualElement);
        }

        /// <summary>
        /// Tests GetLocationOnScreen with various coordinate values including edge cases.
        /// This verifies the method handles different coordinate scenarios properly.
        /// Expected result: The method should return the configured Point values.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(100.5, 200.7)]
        [InlineData(-50.0, -75.0)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(1.23456789, 9.87654321)]
        public void GetLocationOnScreen_ValidVisualElement_ReturnsExpectedPoint(double expectedX, double expectedY)
        {
            // Arrange
            var foldableService = Substitute.For<IFoldableService>();
            var visualElement = Substitute.For<VisualElement>();
            var expectedPoint = new Point(expectedX, expectedY);
            foldableService.GetLocationOnScreen(visualElement).Returns(expectedPoint);

            // Act
            var result = foldableService.GetLocationOnScreen(visualElement);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedX, result.Value.X);
            Assert.Equal(expectedY, result.Value.Y);
            foldableService.Received(1).GetLocationOnScreen(visualElement);
        }

        /// <summary>
        /// Tests GetLocationOnScreen with special double values (NaN, Infinity).
        /// This verifies the method handles edge cases for floating point coordinates.
        /// Expected result: The method should return Points with the special double values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 100.0)]
        [InlineData(100.0, double.NaN)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, 100.0)]
        [InlineData(100.0, double.PositiveInfinity)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 100.0)]
        [InlineData(100.0, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        public void GetLocationOnScreen_SpecialDoubleValues_ReturnsExpectedPoint(double expectedX, double expectedY)
        {
            // Arrange
            var foldableService = Substitute.For<IFoldableService>();
            var visualElement = Substitute.For<VisualElement>();
            var expectedPoint = new Point(expectedX, expectedY);
            foldableService.GetLocationOnScreen(visualElement).Returns(expectedPoint);

            // Act
            var result = foldableService.GetLocationOnScreen(visualElement);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedX, result.Value.X);
            Assert.Equal(expectedY, result.Value.Y);
            foldableService.Received(1).GetLocationOnScreen(visualElement);
        }

        /// <summary>
        /// Tests that GetLocationOnScreen returns Point.Zero when visual element is at origin.
        /// This verifies the method works correctly with the Point.Zero static field.
        /// Expected result: Point with X=0, Y=0 should be returned.
        /// </summary>
        [Fact]
        public void GetLocationOnScreen_VisualElementAtOrigin_ReturnsPointZero()
        {
            // Arrange
            var foldableService = Substitute.For<IFoldableService>();
            var visualElement = Substitute.For<VisualElement>();
            foldableService.GetLocationOnScreen(visualElement).Returns(Point.Zero);

            // Act
            var result = foldableService.GetLocationOnScreen(visualElement);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.0, result.Value.X);
            Assert.Equal(0.0, result.Value.Y);
            Assert.Equal(Point.Zero, result.Value);
            foldableService.Received(1).GetLocationOnScreen(visualElement);
        }

        /// <summary>
        /// Tests that GetLocationOnScreen is called with the exact same VisualElement instance.
        /// This verifies proper parameter passing and method invocation.
        /// Expected result: The method should be called with the exact instance provided.
        /// </summary>
        [Fact]
        public void GetLocationOnScreen_ValidVisualElement_CallsWithSameInstance()
        {
            // Arrange
            var foldableService = Substitute.For<IFoldableService>();
            var visualElement = Substitute.For<VisualElement>();
            var expectedPoint = new Point(10, 20);
            foldableService.GetLocationOnScreen(visualElement).Returns(expectedPoint);

            // Act
            var result = foldableService.GetLocationOnScreen(visualElement);

            // Assert
            foldableService.Received(1).GetLocationOnScreen(Arg.Is<VisualElement>(ve => ve == visualElement));
        }

        /// <summary>
        /// Tests that GetLocationOnScreen handles multiple calls with different VisualElements.
        /// This verifies the method can be called multiple times with different parameters.
        /// Expected result: Each call should return the configured result for that specific element.
        /// </summary>
        [Fact]
        public void GetLocationOnScreen_MultipleVisualElements_ReturnsCorrectResults()
        {
            // Arrange
            var foldableService = Substitute.For<IFoldableService>();
            var visualElement1 = Substitute.For<VisualElement>();
            var visualElement2 = Substitute.For<VisualElement>();
            var point1 = new Point(100, 200);
            var point2 = new Point(300, 400);

            foldableService.GetLocationOnScreen(visualElement1).Returns(point1);
            foldableService.GetLocationOnScreen(visualElement2).Returns(point2);

            // Act
            var result1 = foldableService.GetLocationOnScreen(visualElement1);
            var result2 = foldableService.GetLocationOnScreen(visualElement2);

            // Assert
            Assert.NotNull(result1);
            Assert.Equal(point1, result1.Value);
            Assert.NotNull(result2);
            Assert.Equal(point2, result2.Value);
            foldableService.Received(1).GetLocationOnScreen(visualElement1);
            foldableService.Received(1).GetLocationOnScreen(visualElement2);
        }

        /// <summary>
        /// Tests that GetHinge returns the expected Rect when the service is mocked with a standard rectangle.
        /// Verifies the basic functionality of the GetHinge method with normal positive coordinate and dimension values.
        /// </summary>
        [Fact]
        public void GetHinge_WithNormalRectangle_ReturnsExpectedRect()
        {
            // Arrange
            var expectedRect = new Rect(10, 20, 100, 50);
            var foldableService = Substitute.For<IFoldableService>();
            foldableService.GetHinge().Returns(expectedRect);

            // Act
            var result = foldableService.GetHinge();

            // Assert
            Assert.Equal(expectedRect, result);
        }

        /// <summary>
        /// Tests that GetHinge returns Rect.Zero when the service is configured to return a zero rectangle.
        /// Verifies handling of the zero rectangle case which represents no hinge area.
        /// </summary>
        [Fact]
        public void GetHinge_WithZeroRectangle_ReturnsZeroRect()
        {
            // Arrange
            var expectedRect = Rect.Zero;
            var foldableService = Substitute.For<IFoldableService>();
            foldableService.GetHinge().Returns(expectedRect);

            // Act
            var result = foldableService.GetHinge();

            // Assert
            Assert.Equal(Rect.Zero, result);
        }

        /// <summary>
        /// Tests GetHinge with various edge case rectangle configurations including negative coordinates,
        /// negative dimensions, maximum values, and special double values.
        /// Verifies that the method correctly handles and returns extreme coordinate and dimension values.
        /// </summary>
        /// <param name="x">The X coordinate of the rectangle</param>
        /// <param name="y">The Y coordinate of the rectangle</param>
        /// <param name="width">The width of the rectangle</param>
        /// <param name="height">The height of the rectangle</param>
        [Theory]
        [InlineData(-100, -50, 200, 100)] // Negative coordinates, positive dimensions
        [InlineData(0, 0, -10, -20)] // Zero coordinates, negative dimensions
        [InlineData(double.MaxValue, double.MaxValue, 1, 1)] // Maximum coordinates
        [InlineData(double.MinValue, double.MinValue, 1, 1)] // Minimum coordinates
        [InlineData(0, 0, double.MaxValue, double.MaxValue)] // Maximum dimensions
        [InlineData(-500.5, 300.7, 150.3, 75.9)] // Decimal coordinates and dimensions
        public void GetHinge_WithEdgeCaseRectangles_ReturnsExpectedRect(double x, double y, double width, double height)
        {
            // Arrange
            var expectedRect = new Rect(x, y, width, height);
            var foldableService = Substitute.For<IFoldableService>();
            foldableService.GetHinge().Returns(expectedRect);

            // Act
            var result = foldableService.GetHinge();

            // Assert
            Assert.Equal(expectedRect, result);
        }

        /// <summary>
        /// Tests GetHinge with special double values like NaN and infinity values.
        /// Verifies that the method can handle and return rectangles with special floating-point values
        /// that might occur in edge cases or error conditions.
        /// </summary>
        /// <param name="x">The X coordinate of the rectangle</param>
        /// <param name="y">The Y coordinate of the rectangle</param>
        /// <param name="width">The width of the rectangle</param>
        /// <param name="height">The height of the rectangle</param>
        [Theory]
        [InlineData(double.NaN, 0, 100, 50)] // NaN X coordinate
        [InlineData(0, double.NaN, 100, 50)] // NaN Y coordinate
        [InlineData(0, 0, double.NaN, 50)] // NaN width
        [InlineData(0, 0, 100, double.NaN)] // NaN height
        [InlineData(double.PositiveInfinity, 0, 100, 50)] // Positive infinity X
        [InlineData(0, double.PositiveInfinity, 100, 50)] // Positive infinity Y
        [InlineData(0, 0, double.PositiveInfinity, 50)] // Positive infinity width
        [InlineData(0, 0, 100, double.PositiveInfinity)] // Positive infinity height
        [InlineData(double.NegativeInfinity, 0, 100, 50)] // Negative infinity X
        [InlineData(0, double.NegativeInfinity, 100, 50)] // Negative infinity Y
        [InlineData(0, 0, double.NegativeInfinity, 50)] // Negative infinity width
        [InlineData(0, 0, 100, double.NegativeInfinity)] // Negative infinity height
        public void GetHinge_WithSpecialDoubleValues_ReturnsExpectedRect(double x, double y, double width, double height)
        {
            // Arrange
            var expectedRect = new Rect(x, y, width, height);
            var foldableService = Substitute.For<IFoldableService>();
            foldableService.GetHinge().Returns(expectedRect);

            // Act
            var result = foldableService.GetHinge();

            // Assert
            Assert.Equal(expectedRect, result);
        }

        /// <summary>
        /// Tests that multiple calls to GetHinge return consistent results when the mock is configured
        /// with a specific rectangle. Verifies the stability and repeatability of the GetHinge method.
        /// </summary>
        [Fact]
        public void GetHinge_MultipleCalls_ReturnsConsistentResult()
        {
            // Arrange
            var expectedRect = new Rect(25, 30, 150, 80);
            var foldableService = Substitute.For<IFoldableService>();
            foldableService.GetHinge().Returns(expectedRect);

            // Act
            var result1 = foldableService.GetHinge();
            var result2 = foldableService.GetHinge();
            var result3 = foldableService.GetHinge();

            // Assert
            Assert.Equal(expectedRect, result1);
            Assert.Equal(expectedRect, result2);
            Assert.Equal(expectedRect, result3);
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }
    }

    public partial class FoldEventArgsTests
    {
        /// <summary>
        /// Tests the ToString method with default property values to ensure proper string formatting.
        /// Verifies that the method returns the expected format with default boolean and Rect values.
        /// </summary>
        [Fact]
        public void ToString_WithDefaultValues_ReturnsExpectedFormat()
        {
            // Arrange
            var args = new FoldEventArgs();

            // Act
            var result = args.ToString();

            // Assert
            var expected = $"FoldEventArgs:: isSeparating: {args.isSeparating} FoldingFeatureBounds: {args.FoldingFeatureBounds} WindowBounds: {args.WindowBounds}";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the ToString method with various combinations of property values.
        /// Verifies that the method correctly formats different boolean and Rect values into the expected string format.
        /// </summary>
        [Theory]
        [InlineData(true, 0, 0, 100, 200, 10, 20, 300, 400)]
        [InlineData(false, 50, 75, 150, 250, 0, 0, 500, 600)]
        [InlineData(true, -10, -20, 80, 90, 100, 200, 300, 400)]
        [InlineData(false, 0, 0, 0, 0, 0, 0, 0, 0)]
        public void ToString_WithVariousPropertyValues_ReturnsCorrectFormat(
            bool isSeparating,
            double foldX, double foldY, double foldWidth, double foldHeight,
            double windowX, double windowY, double windowWidth, double windowHeight)
        {
            // Arrange
            var foldingFeatureBounds = new Rect(foldX, foldY, foldWidth, foldHeight);
            var windowBounds = new Rect(windowX, windowY, windowWidth, windowHeight);
            var args = new FoldEventArgs
            {
                isSeparating = isSeparating,
                FoldingFeatureBounds = foldingFeatureBounds,
                WindowBounds = windowBounds
            };

            // Act
            var result = args.ToString();

            // Assert
            var expected = $"FoldEventArgs:: isSeparating: {isSeparating} FoldingFeatureBounds: {foldingFeatureBounds} WindowBounds: {windowBounds}";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the ToString method with extreme Rect values including large numbers and negative coordinates.
        /// Verifies that the method handles edge case values correctly without throwing exceptions.
        /// </summary>
        [Fact]
        public void ToString_WithExtremeRectValues_ReturnsExpectedFormat()
        {
            // Arrange
            var foldingFeatureBounds = new Rect(double.MaxValue, double.MinValue, double.MaxValue, double.MaxValue);
            var windowBounds = new Rect(-1000.5, -2000.75, 5000.25, 6000.125);
            var args = new FoldEventArgs
            {
                isSeparating = true,
                FoldingFeatureBounds = foldingFeatureBounds,
                WindowBounds = windowBounds
            };

            // Act
            var result = args.ToString();

            // Assert
            var expected = $"FoldEventArgs:: isSeparating: True FoldingFeatureBounds: {foldingFeatureBounds} WindowBounds: {windowBounds}";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the ToString method with special double values like NaN and Infinity in Rect coordinates.
        /// Verifies that the method handles special floating-point values correctly.
        /// </summary>
        [Fact]
        public void ToString_WithSpecialDoubleValues_ReturnsExpectedFormat()
        {
            // Arrange
            var foldingFeatureBounds = new Rect(double.NaN, double.PositiveInfinity, double.NegativeInfinity, 100);
            var windowBounds = new Rect(0, 0, double.NaN, double.PositiveInfinity);
            var args = new FoldEventArgs
            {
                isSeparating = false,
                FoldingFeatureBounds = foldingFeatureBounds,
                WindowBounds = windowBounds
            };

            // Act
            var result = args.ToString();

            // Assert
            var expected = $"FoldEventArgs:: isSeparating: False FoldingFeatureBounds: {foldingFeatureBounds} WindowBounds: {windowBounds}";
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the ToString method to ensure it always returns a non-null string.
        /// Verifies that the method never returns null regardless of property values.
        /// </summary>
        [Fact]
        public void ToString_AlwaysReturnsNonNullString_NeverReturnsNull()
        {
            // Arrange
            var args = new FoldEventArgs
            {
                isSeparating = true,
                FoldingFeatureBounds = new Rect(10, 20, 30, 40),
                WindowBounds = new Rect(0, 0, 100, 100)
            };

            // Act
            var result = args.ToString();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        /// <summary>
        /// Tests the ToString method contains all expected components in the output string.
        /// Verifies that the method includes the class name prefix and all property names.
        /// </summary>
        [Fact]
        public void ToString_ContainsAllExpectedComponents_IncludesClassNameAndPropertyNames()
        {
            // Arrange
            var args = new FoldEventArgs
            {
                isSeparating = true,
                FoldingFeatureBounds = new Rect(1, 2, 3, 4),
                WindowBounds = new Rect(5, 6, 7, 8)
            };

            // Act
            var result = args.ToString();

            // Assert
            Assert.Contains("FoldEventArgs::", result);
            Assert.Contains("isSeparating:", result);
            Assert.Contains("FoldingFeatureBounds:", result);
            Assert.Contains("WindowBounds:", result);
        }
    }
}