using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ScrollView)]
	public partial class ScrollViewTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(ScrollOrientation.Vertical)]
		[InlineData(ScrollOrientation.Both)]
		public async Task TestContentSizeChangedHorizontal(ScrollOrientation orientation)
		{
			var handler = await SetUpScrollView(orientation);
			var scroll = handler.VirtualView as ScrollView;
			var changed = WatchContentSizeChanged(scroll);

			await AttachAndRun(scroll, async (handler) =>
			{
				var expectedSize = new Size(100, 100);
				await AssertContentSize(() => scroll.ContentSize, expectedSize);

				scroll.Content.WidthRequest = 200;
				await AssertContentSizeChanged(changed);

				expectedSize = new Size(200, 100);
				await AssertContentSize(() => scroll.ContentSize, expectedSize);
			});
		}

		[Theory]
		[InlineData(ScrollOrientation.Vertical)]
		[InlineData(ScrollOrientation.Both)]
		public async Task TestContentSizeChangedVertical(ScrollOrientation orientation)
		{
			var handler = await SetUpScrollView(orientation);
			var scroll = handler.VirtualView as ScrollView;
			var changed = WatchContentSizeChanged(scroll);

			await AttachAndRun(scroll, async (handler) =>
			{
				var expectedSize = new Size(100, 100);
				await AssertContentSize(() => scroll.ContentSize, expectedSize);

				scroll.Content.HeightRequest = 200;
				await AssertContentSizeChanged(changed);

				expectedSize = new Size(100, 200);
				await AssertContentSize(() => scroll.ContentSize, expectedSize);
			});
		}

		[Theory]
		[InlineData(ScrollOrientation.Vertical, 100, 300, 0, 100)]
		[InlineData(ScrollOrientation.Horizontal, 0, 100, 100, 300)]
		[InlineData(ScrollOrientation.Both, 100, 300, 100, 300)]
		public async Task TestScrollContentMargin(ScrollOrientation orientation, int verticalMargin,
			int expectedHeight, int horizontalMargin, int expectedWidth)
		{
			var handler = await SetUpScrollView(orientation, verticalMargin: verticalMargin, horizontalMargin: horizontalMargin);
			var scroll = handler.VirtualView as ScrollView;


			await AttachAndRun(scroll, async (handler) =>
			{
				var expectedSize = new Size(expectedWidth, expectedHeight);
				await AssertContentSize(() => scroll.ContentSize, expectedSize);
			});
		}

		[Fact(DisplayName = "ScrollView Does Not Leak")]
		public async Task DoesNotLeak()
		{
			SetupBuilder();
			WeakReference viewReference = null;
			WeakReference handlerReference = null;
			WeakReference platformReference = null;

			{
				var view = new Microsoft.Maui.Controls.ScrollView();
				var page = new ContentPage { Content = view };
				await CreateHandlerAndAddToWindow(page, () =>
				{
					viewReference = new(view);
					handlerReference = new(view.Handler);
					platformReference = new(view.Handler.PlatformView);
					page.Content = null;
				});
			}

			await AssertionExtensions.WaitForGC(viewReference, handlerReference, platformReference);
			Assert.False(viewReference.IsAlive, "View should not be alive!");
			Assert.False(handlerReference.IsAlive, "Handler should not be alive!");
			Assert.False(platformReference.IsAlive, "PlatformView should not be alive!");
		}

		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<IScrollView, ScrollViewHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
				});
			});
		}

		static async Task AssertContentSizeChanged(Task<bool> changed)
		{
			await WaitAssert(() => changed.IsCompleted && changed.Result, timeout: 5000, message: "PropertyChanged event with PropertyName 'ContentSize' did not fire").ConfigureAwait(false);
		}

		static async Task AssertContentSize(Func<Size> actual, Size expected)
		{
			await WaitAssert(() => CloseEnough(actual(), expected, 0.2), timeout: 5000, message: $"ContentSize was {actual()}, expected {expected}");
		}

		static bool CloseEnough(Size a, Size b, double tolerance)
		{
			if (System.Math.Abs(a.Width - b.Width) > tolerance)
			{
				return false;
			}

			if (System.Math.Abs(a.Height - b.Height) > tolerance)
			{
				return false;
			}

			return true;
		}

		static Task<bool> WatchContentSizeChanged(ScrollView scrollView)
		{
			var tcs = new TaskCompletionSource<bool>();

			void handler(object sender, PropertyChangedEventArgs args)
			{
				if (args.PropertyName == "ContentSize")
				{
					scrollView.PropertyChanged -= handler;
					tcs.SetResult(true);
				}
			}

			scrollView.PropertyChanged += handler;
			return tcs.Task;
		}

		async Task<ScrollViewHandler> SetUpScrollView(ScrollOrientation orientation, int horizontalMargin = 0, int verticalMargin = 0)
		{
			var view = new Label
			{
				WidthRequest = 100,
				HeightRequest = 100,
				Text = "Hello",
				BackgroundColor = Colors.LightBlue,
				Margin = new Thickness(horizontalMargin, verticalMargin)
			};

			var scroll = new ScrollView
			{
				Orientation = orientation,
				Content = view
			};

			var labelHandler = await CreateHandlerAsync<LabelHandler>(view);
			return await CreateHandlerAsync<ScrollViewHandler>(scroll);
		}

		static async Task WaitAssert(Func<bool> predicate, int interval = 100, int timeout = 1000, string message = "")
		{
			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Start();

			bool success = predicate();

			while (!success && watch.ElapsedMilliseconds < timeout)
			{
				await Task.Delay(interval).ConfigureAwait(false);
				success = predicate();
			}

			if (!success)
			{
				Assert.Fail(message);
			}
		}
	}
}
