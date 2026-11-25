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

		[Fact(DisplayName = "Quick Tap Does Not Raise LongPressed When It Is The Only Gesture")]
		public async Task QuickTapDoesNotRaiseLongPressedWhenItIsTheOnlyGesture()
		{
			// https://github.com/dotnet/maui/issues/40180
			var (layout, longPressedCounts) = CreateLongPressOnlyLayout();

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async handler =>
			{
				var (platformLayout, x, y) = await GetDispatchTarget(layout);

				// Dispatch through the parent so touch target selection works like a real tap:
				// if the child does not consume ACTION_DOWN, it never receives ACTION_UP and the
				// GestureDetector's pending LONG_PRESS message is never cancelled
				long downTime = global::Android.OS.SystemClock.UptimeMillis();
				DispatchTouchEvent(platformLayout, downTime, downTime, MotionEventActions.Down, x, y);
				DispatchTouchEvent(platformLayout, downTime, downTime + 50, MotionEventActions.Up, x, y);

				// Give the pending LONG_PRESS message time to fire if it was not cancelled
				await Task.Delay(ViewConfiguration.LongPressTimeout + 500);

				Assert.Equal(0, longPressedCounts[0]);
			});
		}

		[Fact(DisplayName = "Holding Past The Timeout Raises LongPressed When It Is The Only Gesture")]
		public async Task HoldingPastTimeoutRaisesLongPressedWhenItIsTheOnlyGesture()
		{
			var (layout, longPressedCounts) = CreateLongPressOnlyLayout();

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async handler =>
			{
				var (platformLayout, x, y) = await GetDispatchTarget(layout);

				long downTime = global::Android.OS.SystemClock.UptimeMillis();
				DispatchTouchEvent(platformLayout, downTime, downTime, MotionEventActions.Down, x, y);

				await Task.Delay(ViewConfiguration.LongPressTimeout + 500);

				DispatchTouchEvent(platformLayout, downTime, global::Android.OS.SystemClock.UptimeMillis(), MotionEventActions.Up, x, y);

				Assert.Equal(1, longPressedCounts[0]);
			});
		}

		[Fact(DisplayName = "Quick Tap On A Button Raises Clicked When A Long Press Is The Only Gesture")]
		public async Task QuickTapOnButtonRaisesClickedWhenLongPressIsTheOnlyGesture()
		{
			var (layout, longPressedCounts, clickedCounts) = CreateLongPressOnlyButtonLayout();

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async handler =>
			{
				var (platformLayout, x, y) = await GetDispatchTarget(layout);

				long downTime = global::Android.OS.SystemClock.UptimeMillis();
				DispatchTouchEvent(platformLayout, downTime, downTime, MotionEventActions.Down, x, y);
				DispatchTouchEvent(platformLayout, downTime, downTime + 50, MotionEventActions.Up, x, y);

				// The native click can be posted to the UI thread, and a LONG_PRESS message
				// that was not cancelled would fire within this delay
				await Task.Delay(ViewConfiguration.LongPressTimeout + 500);

				Assert.Equal(1, clickedCounts[0]);
				Assert.Equal(0, longPressedCounts[0]);
			});
		}

		[Fact(DisplayName = "Holding A Button Past The Timeout Raises LongPressed")]
		public async Task HoldingButtonPastTimeoutRaisesLongPressed()
		{
			var (layout, longPressedCounts, _) = CreateLongPressOnlyButtonLayout();

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async handler =>
			{
				var (platformLayout, x, y) = await GetDispatchTarget(layout);

				long downTime = global::Android.OS.SystemClock.UptimeMillis();
				DispatchTouchEvent(platformLayout, downTime, downTime, MotionEventActions.Down, x, y);

				await Task.Delay(ViewConfiguration.LongPressTimeout + 500);

				DispatchTouchEvent(platformLayout, downTime, global::Android.OS.SystemClock.UptimeMillis(), MotionEventActions.Up, x, y);

				Assert.Equal(1, longPressedCounts[0]);
			});
		}

		(Grid Layout, int[] LongPressedCounts, int[] ClickedCounts) CreateLongPressOnlyButtonLayout()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Button, ButtonHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
				});
			});

			var button = new Button { Text = "tap or hold", WidthRequest = 100, HeightRequest = 100 };
			var layout = new Grid { button };
			var longPressedCounts = new int[1];
			var clickedCounts = new int[1];

			var longPress = new LongPressGestureRecognizer();
			longPress.LongPressed += (sender, e) => longPressedCounts[0]++;
			button.GestureRecognizers.Add(longPress);
			button.Clicked += (sender, e) => clickedCounts[0]++;

			return (layout, longPressedCounts, clickedCounts);
		}

		(Grid Layout, int[] LongPressedCounts) CreateLongPressOnlyLayout()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
				});
			});

			var label = new Label { Text = "tap or hold", WidthRequest = 100, HeightRequest = 100 };
			var layout = new Grid { label };
			var longPressedCounts = new int[1];

			var longPress = new LongPressGestureRecognizer();
			longPress.LongPressed += (sender, e) => longPressedCounts[0]++;
			label.GestureRecognizers.Add(longPress);

			return (layout, longPressedCounts);
		}

		static async Task<(global::Android.Views.View PlatformLayout, float X, float Y)> GetDispatchTarget(Grid layout)
		{
			var platformLayout = layout.ToPlatform();
			var platformChild = layout[0].ToPlatform();
			await platformChild.WaitForLayoutOrNonZeroSize();

			var layoutLocation = new int[2];
			platformLayout.GetLocationInWindow(layoutLocation);
			var childLocation = new int[2];
			platformChild.GetLocationInWindow(childLocation);

			float x = childLocation[0] - layoutLocation[0] + (platformChild.Width / 2f);
			float y = childLocation[1] - layoutLocation[1] + (platformChild.Height / 2f);

			return (platformLayout, x, y);
		}

		static void DispatchTouchEvent(global::Android.Views.View view, long downTime, long eventTime, MotionEventActions action, float x, float y)
		{
			var motionEvent = MotionEvent.Obtain(downTime, eventTime, action, x, y, 0);
			view.DispatchTouchEvent(motionEvent);
			motionEvent.Recycle();
		}
	}
}
