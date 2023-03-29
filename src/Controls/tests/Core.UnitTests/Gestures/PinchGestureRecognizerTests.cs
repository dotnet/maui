using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PinchGestureRecognizerTests : BaseTestFixture
	{
		[Fact]
		public void Constructor()
		{
			var pinch = new PinchGestureRecognizer();

		}

		[Fact]
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
			Assert.Equal(GestureStatus.Started, result);
			Assert.Equal(point, resultPoint);
		}

		[Fact]
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
			Assert.Equal(GestureStatus.Completed, result);
		}

		[Fact]
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
			Assert.Equal(2, result);
		}

		[Fact]
		public void OnlyOnePinchGesturePerViewTest()
		{
			var view = new View();
			view.GestureRecognizers.Add(new PinchGestureRecognizer());
			Assert.Throws<InvalidOperationException>(() => view.GestureRecognizers.Add(new PinchGestureRecognizer()));
		}
	}
}

