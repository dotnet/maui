using System;
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

		[Theory]
		[InlineData(100.0, 0.0, 90.0, 0.0, 100.0)]
		[InlineData(100.0, 100.0, 90.0, -100.0, 100.0)]
		[InlineData(100.0, 0.0, 180.0, -100.0, 0.0)]
		[InlineData(100.0, 100.0, 270.0, 100.0, -100.0)]
		public void TransformSwipeCoordinatesWithRotation_ReturnsTransformedCoordinates(
			double inputX, double inputY, double rotation, double expectedX, double expectedY)
		{
			var result = Internals.SwipeGestureExtensions.TransformSwipeCoordinatesWithRotation(inputX, inputY, rotation);

			Assert.Equal(expectedX, result.x, 1);
			Assert.Equal(expectedY, result.y, 1);
		}

		[Theory]
		[InlineData(100.0, 50.0, double.NaN, 100.0, 50.0)]
		[InlineData(75.0, 25.0, double.PositiveInfinity, 75.0, 25.0)]
		[InlineData(200.0, 150.0, double.NegativeInfinity, 200.0, 150.0)]
		public void TransformSwipeCoordinatesWithRotation_InvalidRotation_ReturnsOriginalCoordinates(
			double inputX, double inputY, double rotation, double expectedX, double expectedY)
		{
			var result = Internals.SwipeGestureExtensions.TransformSwipeCoordinatesWithRotation(inputX, inputY, rotation);

			Assert.Equal(expectedX, result.x, 1);
			Assert.Equal(expectedY, result.y, 1);
		}

		[Theory]
		[InlineData(SwipeDirection.Up, 90.0, SwipeDirection.Right)]
		[InlineData(SwipeDirection.Right, 90.0, SwipeDirection.Down)]
		[InlineData(SwipeDirection.Down, 90.0, SwipeDirection.Left)]
		[InlineData(SwipeDirection.Left, 90.0, SwipeDirection.Up)]
		public void TransformSwipeDirectionForRotation_90DegreeClockwise_ReturnsCorrectDirection(
			SwipeDirection direction, double rotation, SwipeDirection expected)
		{
			var result = Internals.SwipeGestureExtensions.TransformSwipeDirectionForRotation(direction, rotation);

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData(SwipeDirection.Up, 180.0, SwipeDirection.Down)]
		[InlineData(SwipeDirection.Right, 180.0, SwipeDirection.Left)]
		[InlineData(SwipeDirection.Down, 180.0, SwipeDirection.Up)]
		[InlineData(SwipeDirection.Left, 180.0, SwipeDirection.Right)]
		public void TransformSwipeDirectionForRotation_180Degree_ReturnsCorrectDirection(
			SwipeDirection direction, double rotation, SwipeDirection expected)
		{
			var result = Internals.SwipeGestureExtensions.TransformSwipeDirectionForRotation(direction, rotation);

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData(SwipeDirection.Up, double.NaN, SwipeDirection.Up)]
		[InlineData(SwipeDirection.Right, double.PositiveInfinity, SwipeDirection.Right)]
		[InlineData(SwipeDirection.Down, double.NegativeInfinity, SwipeDirection.Down)]
		public void TransformSwipeDirectionForRotation_InvalidRotation_ReturnsOriginalDirection(
			SwipeDirection direction, double rotation, SwipeDirection expected)
		{
			var result = Internals.SwipeGestureExtensions.TransformSwipeDirectionForRotation(direction, rotation);

			Assert.Equal(expected, result);
		}
	}
}