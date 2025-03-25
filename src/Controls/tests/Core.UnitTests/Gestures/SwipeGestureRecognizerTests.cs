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
	}
}