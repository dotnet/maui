using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Gesture)]
	public class GestureTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "Removing The View When A Pan Starts Does Not Throw")]
		public async Task RemovingViewWhenPanStartsDoesNotThrow()
		{
			// https://github.com/dotnet/maui/issues/38065
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<BoxView, BoxViewHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
				});
			});

			var boxView = new BoxView { WidthRequest = 100, HeightRequest = 100, Color = Colors.Red };
			var layout = new Grid { boxView };
			var statuses = new List<GestureStatus>();

			var panGesture = new PanGestureRecognizer();
			panGesture.PanUpdated += (sender, e) =>
			{
				statuses.Add(e.StatusType);

				// Removing the view disconnects its gestures while the pan is being handled
				if (e.StatusType == GestureStatus.Started)
					layout.Remove(boxView);
			};
			boxView.GestureRecognizers.Add(panGesture);

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async handler =>
			{
				var platformView = boxView.ToPlatform();
				await platformView.WaitForLayoutOrNonZeroSize();

				// Drag further than the touch slop so the pan starts
				long downTime = global::Android.OS.SystemClock.UptimeMillis();
				DispatchTouchEvent(platformView, downTime, downTime, MotionEventActions.Down, 50, 50);
				DispatchTouchEvent(platformView, downTime, downTime + 16, MotionEventActions.Move, 50, 250);
				DispatchTouchEvent(platformView, downTime, downTime + 32, MotionEventActions.Up, 50, 250);

				Assert.Contains(GestureStatus.Started, statuses);
				Assert.Null(boxView.Parent);
			});
		}

		static void DispatchTouchEvent(global::Android.Views.View view, long downTime, long eventTime, MotionEventActions action, float x, float y)
		{
			var motionEvent = MotionEvent.Obtain(downTime, eventTime, action, x, y, 0);
			view.DispatchTouchEvent(motionEvent);
			motionEvent.Recycle();
		}
	}
}
