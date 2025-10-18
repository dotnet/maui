#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ScrollToRequestedEventArgsTests
    {
        [Theory]
        [InlineData(0.0, 0.0, true, 0.0, 0.0, false)]
        [InlineData(0.0, 0.0, false, 0.0, 0.0, true)]
        [InlineData(10.5, 20.3, true, 10.5, 20.3, false)]
        [InlineData(10.5, 20.3, false, 10.5, 20.3, true)]
        [InlineData(-10.0, -20.0, true, -10.0, -20.0, false)]
        [InlineData(-10.0, -20.0, false, -10.0, -20.0, true)]
        [InlineData(double.MaxValue, double.MinValue, true, double.MaxValue, double.MinValue, false)]
        [InlineData(double.MinValue, double.MaxValue, false, double.MinValue, double.MaxValue, true)]
        public void ToRequest_WithPositionBasedConstructor_ReturnsCorrectScrollToRequest(
            double scrollX,
            double scrollY,
            bool shouldAnimate,
            double expectedHorizontalOffset,
            double expectedVerticalOffset,
            bool expectedInstant)
        {
            // Arrange
            var eventArgs = new ScrollToRequestedEventArgs(scrollX, scrollY, shouldAnimate);

            // Act
            var result = eventArgs.ToRequest();

            // Assert
            Assert.Equal(expectedHorizontalOffset, result.HorizontalOffset);
            Assert.Equal(expectedVerticalOffset, result.VerticalOffset);
            Assert.Equal(expectedInstant, result.Instant);
        }

        [Theory]
        [InlineData(double.NaN, 0.0, true)]
        [InlineData(0.0, double.NaN, false)]
        [InlineData(double.NaN, double.NaN, true)]
        [InlineData(double.PositiveInfinity, 0.0, false)]
        [InlineData(0.0, double.PositiveInfinity, true)]
        [InlineData(double.NegativeInfinity, 0.0, false)]
        [InlineData(0.0, double.NegativeInfinity, true)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity, false)]
        [InlineData(double.NegativeInfinity, double.PositiveInfinity, true)]
        public void ToRequest_WithSpecialDoubleValues_ReturnsCorrectScrollToRequest(
            double scrollX,
            double scrollY,
            bool shouldAnimate)
        {
            // Arrange
            var eventArgs = new ScrollToRequestedEventArgs(scrollX, scrollY, shouldAnimate);

            // Act
            var result = eventArgs.ToRequest();

            // Assert
            Assert.Equal(scrollX, result.HorizontalOffset);
            Assert.Equal(scrollY, result.VerticalOffset);
            Assert.Equal(!shouldAnimate, result.Instant);
        }

        [Fact]
        public void ToRequest_WithElementBasedConstructor_ReturnsCorrectScrollToRequest()
        {
            // Arrange
            var element = new Button();
            var position = ScrollToPosition.Start;
            var shouldAnimate = true;
            var eventArgs = new ScrollToRequestedEventArgs(element, position, shouldAnimate);

            // Act
            var result = eventArgs.ToRequest();

            // Assert
            Assert.Equal(eventArgs.ScrollX, result.HorizontalOffset);
            Assert.Equal(eventArgs.ScrollY, result.VerticalOffset);
            Assert.Equal(!shouldAnimate, result.Instant);
        }

        [Fact]
        public void ToRequest_WithItemBasedConstructor_ReturnsCorrectScrollToRequest()
        {
            // Arrange
            var item = new object();
            var position = ScrollToPosition.Center;
            var shouldAnimate = false;
            var eventArgs = new ScrollToRequestedEventArgs(item, position, shouldAnimate);

            // Act
            var result = eventArgs.ToRequest();

            // Assert
            Assert.Equal(eventArgs.ScrollX, result.HorizontalOffset);
            Assert.Equal(eventArgs.ScrollY, result.VerticalOffset);
            Assert.Equal(!shouldAnimate, result.Instant);
        }

        [Fact]
        public void ToRequest_WithItemAndGroupBasedConstructor_ReturnsCorrectScrollToRequest()
        {
            // Arrange
            var item = new object();
            var group = new object();
            var position = ScrollToPosition.End;
            var shouldAnimate = true;
            var eventArgs = new ScrollToRequestedEventArgs(item, group, position, shouldAnimate);

            // Act
            var result = eventArgs.ToRequest();

            // Assert
            Assert.Equal(eventArgs.ScrollX, result.HorizontalOffset);
            Assert.Equal(eventArgs.ScrollY, result.VerticalOffset);
            Assert.Equal(!shouldAnimate, result.Instant);
        }

        /// <summary>
        /// Tests that ToRequest method correctly negates the ShouldAnimate property when creating ScrollToRequest.
        /// Verifies that when ShouldAnimate is true, the resulting Instant property is false, and vice versa.
        /// </summary>
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void ToRequest_ShouldAnimateNegation_CorrectlyMapsToInstantProperty(bool shouldAnimate, bool expectedInstant)
        {
            // Arrange
            var eventArgs = new ScrollToRequestedEventArgs(100.0, 200.0, shouldAnimate);

            // Act
            var result = eventArgs.ToRequest();

            // Assert
            Assert.Equal(expectedInstant, result.Instant);
        }
    }
}
