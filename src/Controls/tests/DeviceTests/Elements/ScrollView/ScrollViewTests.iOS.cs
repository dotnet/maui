using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ScrollViewTests
	{
		[Fact]
		public async Task ScrollViewContentSizeSet()
		{
			EnsureHandlerCreated(builder => { builder.ConfigureMauiHandlers(handlers => { handlers.AddHandler<Entry, EntryHandler>(); }); });

			var scrollView = new ScrollView();
			var entry = new Entry() { Text = "In a ScrollView", HeightRequest = 10000 };
			scrollView.Content = entry;

			var scrollViewHandler = await InvokeOnMainThreadAsync(() =>
			{
				return CreateHandlerAsync<ScrollViewHandler>(scrollView);
			});

			await InvokeOnMainThreadAsync(async () =>
			{
				await scrollViewHandler.PlatformView.AttachAndRun(() =>
				{
					Assert.Equal(10000, scrollViewHandler.PlatformView.ContentSize.Height);
				});
			});
		}

		[Fact]
		public async Task ContentSizeExpandsToViewport()
		{
			EnsureHandlerCreated(builder => { builder.ConfigureMauiHandlers(handlers => { handlers.AddHandler<Entry, EntryHandler>(); }); });

			var scrollView = new ScrollView();

			var entry = new Entry() { Text = "In a ScrollView", HeightRequest = 10 };


			static CoreGraphics.CGSize getViewportSize(UIScrollView scrollView)
			{
				return scrollView.AdjustedContentInset.InsetRect(scrollView.Bounds).Size;
			}
			;

			var scrollViewHandler = await InvokeOnMainThreadAsync(() =>
			{
				return CreateHandlerAsync<ScrollViewHandler>(scrollView);
			});

			await InvokeOnMainThreadAsync(async () =>
			{
				await scrollViewHandler.PlatformView.AttachAndRun(async () =>
				{
					var uiScrollView = scrollViewHandler.PlatformView;

					uiScrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Always;
					var parent = uiScrollView.Superview;
					uiScrollView.Bounds = parent.Bounds;
					uiScrollView.Center = parent.Center;

					scrollView.Content = entry;

					parent.SetNeedsLayout();
					parent.LayoutIfNeeded();

					await Task.Yield();

					var contentSize = uiScrollView.ContentSize;
					var viewportSize = getViewportSize(uiScrollView);

					Assert.Equal(viewportSize.Height, contentSize.Height);
					Assert.Equal(viewportSize.Width, contentSize.Width);
				});
			});
		}

		internal class TestStackLayout : VerticalStackLayout
		{
			public Rect LastArrangeBounds { get; set; }

			protected override Size ArrangeOverride(Rect bounds)
			{
				LastArrangeBounds = bounds;
				return base.ArrangeOverride(bounds);
			}
		}

		[Fact]
		public async Task ContentChangeDoesNotResetScrollPosition()
		{
			var topLabel = new Label
			{
				WidthRequest = 100,
				HeightRequest = 5000,
				Text = "Hello",
				BackgroundColor = Colors.LightBlue
			};

			var bottomLabel = new Label { Text = "Howdy" };

			var layout = new TestStackLayout
			{
				topLabel,
				bottomLabel
			};

			var scroll = new ScrollView
			{
				Content = layout,
				HeightRequest = 400
			};

			var topLabelHandler = await CreateHandlerAsync<LabelHandler>(topLabel);
			var bottomLabelHandler = await CreateHandlerAsync<LabelHandler>(bottomLabel);
			var layoutHandler = await CreateHandlerAsync<LayoutHandler>(layout);
			var scrollHandler = await CreateHandlerAsync<ScrollViewHandler>(scroll);

			await AttachAndRun(scroll, async (handler) =>
			{
				var platformView = scrollHandler.PlatformView;

				// Scroll down by 5000
				scrollHandler.VirtualView.RequestScrollTo(0, 5000, true);

				// Give it time to update
				await Task.Delay(100);

				// Verify that the content layout didn't pick up any incorrect offsets
				// The arrangement for the actual _content_ should always start at Y=0 because
				// of the ContentView shim. If we ever stop using the ContentView shim for 
				// the iOS ScrollView implementation, this test will likely become invalid.
				Assert.Equal(0, layout.LastArrangeBounds.Top);

				// Change the text of the bottom label; this _should_ have no effect on scrolling
				bottomLabel.Text = "Changed";
				await Task.Delay(100);

				// The content should still be arranged at Y=0
				Assert.Equal(0, layout.LastArrangeBounds.Top);
			});
		}
	}
}
