using System;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{

	public class PanGestureRecognizerUnitTests : BaseTestFixture
	{
		[Test]
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
			Assert.AreEqual(GestureStatus.Started, target);
		}

		[Test]
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
			Assert.AreEqual(GestureStatus.Running, target);
		}

		[Test]
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
			Assert.AreEqual(5, target);
		}

		[Test]
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
			Assert.AreEqual(10, target);
		}

		[Test]
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
			Assert.AreEqual(GestureStatus.Completed, target);
		}

		[Test]
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
			Assert.AreEqual(GestureStatus.Canceled, target);
		}
	}
}