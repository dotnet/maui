#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class SwipeGestureRecognizerTests : BaseTestFixture
    {
        [Fact]
        public void Constructor()
        {
            var swipe = new SwipeGestureRecognizer();

            Assert.Null(swipe.Command);
            Assert.Null(swipe.CommandParameter);
            Assert.Equal((uint)100, swipe.Threshold);
        }

        [Fact]
        public void CallbackPassesParameter()
        {
            var view = new View();
            var swipe = new SwipeGestureRecognizer();
            swipe.CommandParameter = "Hello";

            object result = null;
            swipe.Command = new Command(o => result = o);

            swipe.SendSwiped(view, SwipeDirection.Left);
            Assert.Equal(result, swipe.CommandParameter);
        }

        [Fact]
        public void SwipedEventDirectionMatchesTotalXTest()
        {
            var view = new View();
            var swipe = new SwipeGestureRecognizer();

            SwipeDirection direction = SwipeDirection.Up;
            swipe.Swiped += (object sender, SwipedEventArgs e) =>
            {
                direction = e.Direction;
            };

            ((ISwipeGestureController)swipe).SendSwipe(view, totalX: -150, totalY: 10);
            ((ISwipeGestureController)swipe).DetectSwipe(view, SwipeDirection.Left);
            Assert.Equal(SwipeDirection.Left, direction);
        }

        [Fact]
        public void SwipedEventDirectionMatchesTotalYTest()
        {
            var view = new View();
            var swipe = new SwipeGestureRecognizer();

            SwipeDirection direction = SwipeDirection.Left;
            swipe.Swiped += (object sender, SwipedEventArgs e) =>
            {
                direction = e.Direction;
            };

            ((ISwipeGestureController)swipe).SendSwipe(view, totalX: 10, totalY: -150);
            ((ISwipeGestureController)swipe).DetectSwipe(view, SwipeDirection.Up);
            Assert.Equal(SwipeDirection.Up, direction);
        }

        [Fact]
        public void SwipeIgnoredIfBelowThresholdTest()
        {
            var view = new View();
            var swipe = new SwipeGestureRecognizer();

            // Specify a custom threshold for the test.
            swipe.Threshold = 200;

            bool detected = false;
            swipe.Swiped += (object sender, SwipedEventArgs e) =>
            {
                detected = true;
            };

            ((ISwipeGestureController)swipe).SendSwipe(view, totalX: 0, totalY: -175);
            ((ISwipeGestureController)swipe).DetectSwipe(view, SwipeDirection.Up);
            Assert.False(detected);
        }

        [Fact]
        public void SwipedEventDirectionMatchesTotalXTestWithFlags()
        {
            var view = new View();
            var swipe = new SwipeGestureRecognizer();

            SwipeDirection direction = SwipeDirection.Up;
            swipe.Swiped += (object sender, SwipedEventArgs e) =>
            {
                direction = e.Direction;
            };

            ((ISwipeGestureController)swipe).SendSwipe(view, totalX: -150, totalY: 10);
            ((ISwipeGestureController)swipe).DetectSwipe(view, SwipeDirection.Left | SwipeDirection.Right);
            Assert.Equal(SwipeDirection.Left, direction);
        }

        [Fact]
        public void SwipedEventDirectionMatchesTotalYTestWithFlags()
        {
            var view = new View();
            var swipe = new SwipeGestureRecognizer();

            SwipeDirection direction = SwipeDirection.Left;
            swipe.Swiped += (object sender, SwipedEventArgs e) =>
            {
                direction = e.Direction;
            };

            ((ISwipeGestureController)swipe).SendSwipe(view, totalX: 10, totalY: -150);
            ((ISwipeGestureController)swipe).DetectSwipe(view, SwipeDirection.Up | SwipeDirection.Down);
            Assert.Equal(SwipeDirection.Up, direction);
        }

        /// <summary>
        /// Tests that the Direction property returns the default value when not explicitly set.
        /// </summary>
        [Fact]
        public void Direction_DefaultValue_ReturnsDefaultSwipeDirection()
        {
            // Arrange
            var swipeGestureRecognizer = new SwipeGestureRecognizer();

            // Act
            var result = swipeGestureRecognizer.Direction;

            // Assert
            Assert.Equal(default(SwipeDirection), result);
        }

        /// <summary>
        /// Tests that the Direction property correctly stores and retrieves individual SwipeDirection values.
        /// </summary>
        /// <param name="direction">The SwipeDirection value to test</param>
        [Theory]
        [InlineData(SwipeDirection.Right)]
        [InlineData(SwipeDirection.Left)]
        [InlineData(SwipeDirection.Up)]
        [InlineData(SwipeDirection.Down)]
        public void Direction_SetAndGetIndividualValues_ReturnsCorrectValue(SwipeDirection direction)
        {
            // Arrange
            var swipeGestureRecognizer = new SwipeGestureRecognizer();

            // Act
            swipeGestureRecognizer.Direction = direction;
            var result = swipeGestureRecognizer.Direction;

            // Assert
            Assert.Equal(direction, result);
        }

        /// <summary>
        /// Tests that the Direction property correctly stores and retrieves combined SwipeDirection flag values.
        /// </summary>
        /// <param name="direction">The combined SwipeDirection flags to test</param>
        [Theory]
        [InlineData(SwipeDirection.Right | SwipeDirection.Left)]
        [InlineData(SwipeDirection.Up | SwipeDirection.Down)]
        [InlineData(SwipeDirection.Right | SwipeDirection.Up)]
        [InlineData(SwipeDirection.Left | SwipeDirection.Down)]
        [InlineData(SwipeDirection.Right | SwipeDirection.Left | SwipeDirection.Up)]
        [InlineData(SwipeDirection.Right | SwipeDirection.Left | SwipeDirection.Up | SwipeDirection.Down)]
        public void Direction_SetAndGetCombinedValues_ReturnsCorrectValue(SwipeDirection direction)
        {
            // Arrange
            var swipeGestureRecognizer = new SwipeGestureRecognizer();

            // Act
            swipeGestureRecognizer.Direction = direction;
            var result = swipeGestureRecognizer.Direction;

            // Assert
            Assert.Equal(direction, result);
        }

        /// <summary>
        /// Tests that the Direction property can store and retrieve values outside the defined enum range.
        /// </summary>
        /// <param name="invalidValue">The invalid enum value to test</param>
        [Theory]
        [InlineData(16)]
        [InlineData(32)]
        [InlineData(255)]
        [InlineData(-1)]
        public void Direction_SetInvalidEnumValue_StoresAndRetrievesValue(int invalidValue)
        {
            // Arrange
            var swipeGestureRecognizer = new SwipeGestureRecognizer();
            var invalidDirection = (SwipeDirection)invalidValue;

            // Act
            swipeGestureRecognizer.Direction = invalidDirection;
            var result = swipeGestureRecognizer.Direction;

            // Assert
            Assert.Equal(invalidDirection, result);
        }

        /// <summary>
        /// Tests that the Direction property can be set to zero and retrieved correctly.
        /// </summary>
        [Fact]
        public void Direction_SetToZero_ReturnsZero()
        {
            // Arrange
            var swipeGestureRecognizer = new SwipeGestureRecognizer();
            swipeGestureRecognizer.Direction = SwipeDirection.Right; // Set to non-default first

            // Act
            swipeGestureRecognizer.Direction = (SwipeDirection)0;
            var result = swipeGestureRecognizer.Direction;

            // Assert
            Assert.Equal((SwipeDirection)0, result);
        }
    }
}