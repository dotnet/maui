#nullable enable annotations

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

		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Picker, PickerHandler>();
				});
			});
		}

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
		public async Task PickerScrollsLongSelectedTextHorizontally()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			SetupBuilder();

			var picker = new Picker
			{
				WidthRequest = 180
			};
			picker.Items.Add("This is a very long selected Picker item that extends beyond its fixed width");
			picker.Items.Add("Another long Picker item");
			picker.SelectedIndex = 0;

			int horizontalScrollDelta = 0;
			await CreateHandlerAndAddToWindow<PickerHandler>(picker, async handler =>
			{
				await handler.PlatformView.WaitForLayoutOrNonZeroSize();
				horizontalScrollDelta = DispatchLeftDrag(handler.PlatformView);
			});

			Assert.True(horizontalScrollDelta > 0, "Picker should horizontally scroll its selected text.");
		}

		static int DispatchLeftDrag(MauiPicker platformView)
		{
			float startX = platformView.Width * 0.8f;
			float endX = platformView.Width * 0.2f;
			float y = platformView.Height * 0.5f;
			long downTime = global::Android.OS.SystemClock.UptimeMillis();

			var down = MotionEvent.Obtain(downTime, downTime, MotionEventActions.Down, startX, y, 0);
			platformView.DispatchTouchEvent(down);
			down.Recycle();
			int initialHorizontalScrollOffset = platformView.ScrollX;

			for (int step = 1; step <= 3; step++)
			{
				float x = startX + ((endX - startX) * step / 3);
				var move = MotionEvent.Obtain(downTime, downTime + (step * 100), MotionEventActions.Move, x, y, 0);
				platformView.DispatchTouchEvent(move);
				move.Recycle();
			}

			int horizontalScrollOffset = platformView.ScrollX;

			var cancel = MotionEvent.Obtain(downTime, downTime + 400, MotionEventActions.Cancel, endX, y, 0);
			platformView.DispatchTouchEvent(cancel);
			cancel.Recycle();

			return horizontalScrollOffset - initialHorizontalScrollOffset;
		}
	}
}
