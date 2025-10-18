#nullable disable

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class PanGestureRecognizerUnitTests : BaseTestFixture
    {
        [Fact]
        public void PanRaisesStartedEventTest()
        {
            var view = new View();
            var pan = new PanGestureRecognizer();

            GestureStatus target = GestureStatus.Canceled;
            pan.PanUpdated += (object sender, PanUpdatedEventArgs e) =>
            {
                target = e.StatusType;
            };

            ((IPanGestureController)pan).SendPanStarted(view, 0);
            Assert.Equal(GestureStatus.Started, target);
        }

        [Fact]
        public void PanRaisesRunningEventTest()
        {
            var view = new View();
            var pan = new PanGestureRecognizer();

            GestureStatus target = GestureStatus.Canceled;
            pan.PanUpdated += (object sender, PanUpdatedEventArgs e) =>
            {
                target = e.StatusType;
            };

            ((IPanGestureController)pan).SendPan(view, gestureId: 0, totalX: 5, totalY: 10);
            Assert.Equal(GestureStatus.Running, target);
        }

        [Fact]
        public void PanRunningEventContainsTotalXTest()
        {
            var view = new View();
            var pan = new PanGestureRecognizer();

            double target = 0;
            pan.PanUpdated += (object sender, PanUpdatedEventArgs e) =>
            {
                target = e.TotalX;
            };

            ((IPanGestureController)pan).SendPan(view, gestureId: 0, totalX: 5, totalY: 10);
            Assert.Equal(5, target);
        }

        [Fact]
        public void PanRunningEventContainsTotalYTest()
        {
            var view = new View();
            var pan = new PanGestureRecognizer();

            double target = 0;
            pan.PanUpdated += (object sender, PanUpdatedEventArgs e) =>
            {
                target = e.TotalY;
            };

            ((IPanGestureController)pan).SendPan(view, gestureId: 0, totalX: 5, totalY: 10);
            Assert.Equal(10, target);
        }

        [Fact]
        public void PanRaisesCompletedEventTest()
        {
            var view = new View();
            var pan = new PanGestureRecognizer();

            GestureStatus target = GestureStatus.Canceled;
            pan.PanUpdated += (object sender, PanUpdatedEventArgs e) =>
            {
                target = e.StatusType;
            };

            ((IPanGestureController)pan).SendPanCompleted(view, 0);
            Assert.Equal(GestureStatus.Completed, target);
        }

        [Fact]
        public void PanRaisesCanceledEventTest()
        {
            var view = new View();
            var pan = new PanGestureRecognizer();

            GestureStatus target = GestureStatus.Started;
            pan.PanUpdated += (object sender, PanUpdatedEventArgs e) =>
            {
                target = e.StatusType;
            };

            ((IPanGestureController)pan).SendPanCanceled(view, 0);
            Assert.Equal(GestureStatus.Canceled, target);
        }
    }

    public partial class PanGestureRecognizerTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that TouchPoints property returns the default value of 1 when not explicitly set.
        /// Input conditions: New PanGestureRecognizer instance.
        /// Expected result: TouchPoints should return 1.
        /// </summary>
        [Fact]
        public void TouchPoints_DefaultValue_ReturnsOne()
        {
            // Arrange
            var panGestureRecognizer = new PanGestureRecognizer();

            // Act
            var result = panGestureRecognizer.TouchPoints;

            // Assert
            Assert.Equal(1, result);
        }

        /// <summary>
        /// Tests that TouchPoints property correctly returns values that were set via the setter.
        /// Input conditions: Various valid integer values including edge cases.
        /// Expected result: Getter should return the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void TouchPoints_SetValue_GetterReturnsSetValue(int value)
        {
            // Arrange
            var panGestureRecognizer = new PanGestureRecognizer();

            // Act
            panGestureRecognizer.TouchPoints = value;
            var result = panGestureRecognizer.TouchPoints;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that TouchPoints property maintains its value after multiple get operations.
        /// Input conditions: Set TouchPoints to a specific value, then call getter multiple times.
        /// Expected result: All getter calls should return the same value.
        /// </summary>
        [Fact]
        public void TouchPoints_MultipleGets_ReturnsConsistentValue()
        {
            // Arrange
            var panGestureRecognizer = new PanGestureRecognizer();
            const int expectedValue = 42;
            panGestureRecognizer.TouchPoints = expectedValue;

            // Act
            var result1 = panGestureRecognizer.TouchPoints;
            var result2 = panGestureRecognizer.TouchPoints;
            var result3 = panGestureRecognizer.TouchPoints;

            // Assert
            Assert.Equal(expectedValue, result1);
            Assert.Equal(expectedValue, result2);
            Assert.Equal(expectedValue, result3);
        }

        /// <summary>
        /// Tests that TouchPoints property correctly handles zero value.
        /// Input conditions: Set TouchPoints to zero.
        /// Expected result: Getter should return zero.
        /// </summary>
        [Fact]
        public void TouchPoints_SetToZero_ReturnsZero()
        {
            // Arrange
            var panGestureRecognizer = new PanGestureRecognizer();

            // Act
            panGestureRecognizer.TouchPoints = 0;
            var result = panGestureRecognizer.TouchPoints;

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that TouchPoints property correctly handles negative values.
        /// Input conditions: Set TouchPoints to various negative values.
        /// Expected result: Getter should return the negative value that was set.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(-999)]
        public void TouchPoints_SetNegativeValue_ReturnsNegativeValue(int negativeValue)
        {
            // Arrange
            var panGestureRecognizer = new PanGestureRecognizer();

            // Act
            panGestureRecognizer.TouchPoints = negativeValue;
            var result = panGestureRecognizer.TouchPoints;

            // Assert
            Assert.Equal(negativeValue, result);
        }

        /// <summary>
        /// Tests that TouchPoints property correctly handles boundary values for integers.
        /// Input conditions: Set TouchPoints to int.MaxValue and int.MinValue.
        /// Expected result: Getter should return the exact boundary values.
        /// </summary>
        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void TouchPoints_SetBoundaryValue_ReturnsBoundaryValue(int boundaryValue)
        {
            // Arrange
            var panGestureRecognizer = new PanGestureRecognizer();

            // Act
            panGestureRecognizer.TouchPoints = boundaryValue;
            var result = panGestureRecognizer.TouchPoints;

            // Assert
            Assert.Equal(boundaryValue, result);
        }
    }
}