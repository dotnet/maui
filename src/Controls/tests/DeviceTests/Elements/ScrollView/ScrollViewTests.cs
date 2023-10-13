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

		// NOTE: this test is slightly different than MemoryTests.HandlerDoesNotLeak
		// It calls CreateHandlerAndAddToWindow(), a valid test case.
		[Fact(DisplayName = "ScrollView Does Not Leak"
#if MACCATALYST
			, Skip = "Fails on Mac Catalyst, fixme"
#endif
			)]
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
			Assert.False(viewReference.IsAlive, "ScrollView should not be alive!");
			Assert.False(handlerReference.IsAlive, "Handler should not be alive!");
			Assert.False(platformReference.IsAlive, "PlatformView should not be alive!");
		}

		[Fact(DisplayName = "ScrollView inside layouts do not grow")]
		public async Task DoesNotGrow()
		{
			var screenWidthConstraint = 600;
			var screenHeightConstraint = 600;

			var label = new Label() { Text = "Text inside a ScrollView" };
			var scrollView = new ScrollView() { MaximumHeightRequest = 500, Content = label };
			var parentLayout = new VerticalStackLayout { scrollView };
			parentLayout.BackgroundColor = Colors.Blue;

			SetupBuilder();

			await CreateHandlerAndAddToWindow(parentLayout, () =>
			{
				var size = (parentLayout as IView).Measure(screenWidthConstraint, screenHeightConstraint);
				var rect = new Rect(0, 0, size.Width, size.Height);
				(parentLayout as IView).Arrange(rect); // Manual layout to prevent device test flakiness on Windows
			});

			Assert.True(parentLayout.Height > 0, "Parent layout should have non-zero height!");
			Assert.True(parentLayout.Height < 500, "ScrollView should not make parent layout grow!");
		}

		[Fact(DisplayName = "ScrollView's viewport fills available space if set to fill"
#if MACCATALYST || IOS
			, Skip = "See: https://github.com/dotnet/maui/issues/17700. If the issue is solved, re-enable the tests"
#endif
		)]
		public async Task ShouldGrow()
		{
			var screenWidthConstraint = 600;
			var screenHeightConstraint = 600;

			var label = new Label() { Text = "Text inside a ScrollView" };
			var childLayout = new VerticalStackLayout { label };
			var scrollView = new ScrollView() { VerticalOptions = LayoutOptions.Fill, Content = childLayout };
			var parentLayout = new Grid { scrollView };

			var expectedHeight = 100;
			parentLayout.HeightRequest = expectedHeight;

			SetupBuilder();

			await CreateHandlerAndAddToWindow(parentLayout, () =>
			{
				var size = (parentLayout as IView).Measure(screenWidthConstraint, screenHeightConstraint);
				var rect = new Rect(0, 0, size.Width, size.Height);
				(parentLayout as IView).Arrange(rect); // Manual layout to prevent device test flakiness on Windows
			});

			// Android is usually off by one or two px. Hence the tolerance
			Assert.Equal(scrollView.Height, childLayout.Height, 2.0);
			Assert.Equal(parentLayout.Height, scrollView.Height, 2.0);
			Assert.Equal(expectedHeight, parentLayout.Height, 2.0);
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
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
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
