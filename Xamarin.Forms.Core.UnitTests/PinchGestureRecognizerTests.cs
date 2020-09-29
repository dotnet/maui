using System;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	public class PinchGestureRecognizerTests : BaseTestFixture
	{
		[Test]
		public void Constructor()
		{
			var pinch = new PinchGestureRecognizer();

		}

		[Test]
		public void PinchStartedTest()
		{
			var view = new View();
			var pinch = new PinchGestureRecognizer();

			GestureStatus result = GestureStatus.Canceled;
			var point = new Point(10, 10);
			var resultPoint = Point.Zero;
			pinch.PinchUpdated += (object sender, PinchGestureUpdatedEventArgs e) =>
			{
				result = e.Status;
				resultPoint = e.ScaleOrigin;
			};

			((IPinchGestureController)pinch).SendPinchStarted(view, point);
			Assert.AreEqual(GestureStatus.Started, result);
			Assert.AreEqual(point, resultPoint);
		}

		[Test]
		public void PinchCompletedTest()
		{
			var view = new View();
			var pinch = new PinchGestureRecognizer();

			GestureStatus result = GestureStatus.Canceled;
			pinch.PinchUpdated += (object sender, PinchGestureUpdatedEventArgs e) =>
			{
				result = e.Status;
			};

			((IPinchGestureController)pinch).SendPinchEnded(view);
			Assert.AreEqual(GestureStatus.Completed, result);
		}

		[Test]
		public void PinchUpdatedTest()
		{
			var view = new View();
			var pinch = new PinchGestureRecognizer();
			var point = new Point(10, 10);
			var resultPoint = Point.Zero;
			double result = -1;
			pinch.PinchUpdated += (object sender, PinchGestureUpdatedEventArgs e) =>
			{
				result = e.Scale;
				resultPoint = e.ScaleOrigin;
			};

			((IPinchGestureController)pinch).SendPinch(view, 2, point);
			Assert.AreEqual(2, result);
		}

		[Test]
		public void OnlyOnePinchGesturePerViewTest()
		{
			var view = new View();
			view.GestureRecognizers.Add(new PinchGestureRecognizer());
			Assert.Throws<InvalidOperationException>(() => view.GestureRecognizers.Add(new PinchGestureRecognizer()));
		}
	}
}