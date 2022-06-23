using System;
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
}