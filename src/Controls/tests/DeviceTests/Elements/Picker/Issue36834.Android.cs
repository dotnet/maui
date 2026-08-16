#nullable enable

using System;
using System.Threading.Tasks;
using Android.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Picker)]
	public class Issue36834 : ControlsHandlerTestBase
	{
		const string IssueNumber = "36834";

		static string? GetReplicationIssue()
		{
#if ANDROID
			return global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
				.MauiTestInstrumentation.Current?.Arguments?.GetString("MAUI_REPRODUCTION_ISSUE");
#elif IOS || MACCATALYST
			return global::Foundation.NSProcessInfo.ProcessInfo.Environment["MAUI_REPRODUCTION_ISSUE"]?.ToString();
#else
			return Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE");
#endif
		}

		[Fact]
		public async Task LongSelectedValueScrollsHorizontally()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Picker, PickerHandler>();
				});
			});

			var picker = new Picker
			{
				ItemsSource = new[] { "This is a very long selected Picker value that should scroll horizontally" },
				SelectedIndex = 0,
				WidthRequest = 180
			};

			await CreateHandlerAndAddToWindow<PickerHandler>(picker, async handler =>
			{
				var platformPicker = handler.PlatformView;
				await platformPicker.WaitForLayoutOrNonZeroSize();

				SwipeLeft(platformPicker);

				Assert.True(
					platformPicker.ScrollX > 0,
					"Picker should horizontally scroll a selected value that is wider than its viewport after a left swipe.");
			});
		}

		static void SwipeLeft(MauiPicker picker)
		{
			float startX = picker.Width * 0.75f;
			float endX = picker.Width * 0.25f;
			float y = picker.Height * 0.5f;
			long downTime = global::Android.OS.SystemClock.UptimeMillis();

			var down = MotionEvent.Obtain(downTime, downTime, MotionEventActions.Down, startX, y, 0);
			picker.DispatchTouchEvent(down);
			down.Recycle();

			var move = MotionEvent.Obtain(downTime, downTime + 16, MotionEventActions.Move, endX, y, 0);
			picker.DispatchTouchEvent(move);
			move.Recycle();

			var up = MotionEvent.Obtain(downTime, downTime + 32, MotionEventActions.Up, endX, y, 0);
			picker.DispatchTouchEvent(up);
			up.Recycle();
		}
	}
}
