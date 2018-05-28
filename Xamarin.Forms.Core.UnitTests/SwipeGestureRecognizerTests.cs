using System;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class SwipeGestureRecognizerTests : BaseTestFixture
	{
		[Test]
		public void Constructor()
		{
			var swipe = new SwipeGestureRecognizer();

			Assert.AreEqual(null, swipe.Command);
			Assert.AreEqual(null, swipe.CommandParameter);
			Assert.AreEqual(100, swipe.Threshold);
		}

		[Test]
		public void CallbackPassesParameter()
		{
			var view = new View();
			var swipe = new SwipeGestureRecognizer();
			swipe.CommandParameter = "Hello";

			object result = null;
			swipe.Command = new Command(o => result = o);

			swipe.SendSwiped(view, SwipeDirection.Left);
			Assert.AreEqual(result, swipe.CommandParameter);
		}

		[Test]
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
			Assert.AreEqual(SwipeDirection.Left, direction);
		}

		[Test]
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
			Assert.AreEqual(SwipeDirection.Up, direction);
		}

		[Test]
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
			Assert.IsFalse(detected);
		}
	}
}