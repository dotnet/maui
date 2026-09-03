using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SwipeView)]
	public partial class SwipeViewTests : ControlsHandlerTestBase
	{
		MauiSwipeView GetPlatformControl(SwipeViewHandler handler) =>
			handler.PlatformView;

		Task<bool> HasChildren(SwipeViewHandler handler)
		{
			return InvokeOnMainThreadAsync(()
				=> GetPlatformControl(handler).Subviews.Length != 0);
		}

		[Fact(DisplayName = "Execute Mode SwipeItem Measures Native Content Width (Issue 37700)")]
		public async Task ExecuteModeSwipeItemMeasuresNativeContentWidth()
		{
			SetupBuilder();

			var content = new VerticalStackLayout
			{
				HeightRequest = 60,
				Background = new SolidColorBrush(Colors.White)
			};

			var swipeItem = new SwipeItem
			{
				Text = "OK"
			};

			var swipeItems = new SwipeItems
			{
				swipeItem
			};
			swipeItems.Mode = SwipeMode.Execute;

			var swipeView = new SwipeView()
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = content
			};

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformView = GetPlatformControl(handler);

			await InvokeOnMainThreadAsync(async () =>
			{
				await platformView.AttachAndRun(async () =>
				{
					swipeView.Open(OpenSwipeItem.LeftItems, false);

					// The SwipeView adds subviews dynamically when opening it.
					await AssertEventually(() => platformView.Subviews.Length > 1);

					var actionView = platformView.Subviews.OfType<UIStackView>().FirstOrDefault();
					Assert.NotNull(actionView);

					await AssertEventually(() => actionView.Subviews.Length > 0);

					var nativeSwipeItem = actionView.Subviews.FirstOrDefault();
					Assert.NotNull(nativeSwipeItem);

					await AssertEventually(() => nativeSwipeItem.Frame.Width > 0);

					double contentWidth = platformView.Frame.Width;
					double swipeItemWidth = nativeSwipeItem.Frame.Width;

					// Prior to the fix, a single Execute-mode SwipeItem used a fixed
					// SwipeItemWidth (100pt), and Execute-mode threshold/measurement logic
					// did not measure the native UIButton's actual content size. The fix
					// measures the native menu button via SizeThatFits, so a single
					// short-text item should be sized noticeably smaller than the full
					// SwipeView content width.
					Assert.True(swipeItemWidth < contentWidth,
						$"Expected the Execute-mode SwipeItem width ({swipeItemWidth}pt) to be smaller " +
						$"than the full SwipeView content width ({contentWidth}pt), matching the native " +
						$"button's measured content size instead of the entire SwipeView width.");
				});
			});
		}

		[Fact]
		[Description("The Opacity property of a SwipeView should match with native Opacity")]
		public async Task VerifySwipeViewOpacityProperty()
		{
			var swipeView = new SwipeView
			{
				Opacity = 0.35f
			};
			var expectedValue = swipeView.Opacity;

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
   			{
				   var nativeOpacityValue = (float)nativeView.Alpha;
				   Assert.Equal(expectedValue, nativeOpacityValue);
			   });
		}

		[Fact]
		[Description("The IsVisible property of a SwipeView should match with native IsVisible")]
		public async Task VerifySwipeViewIsVisibleProperty()
		{
			var swipeView = new SwipeView
			{
				IsVisible = false
			};
			var expectedValue = swipeView.IsVisible;

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
   			{
				   var isVisible = !nativeView.Hidden;
				   Assert.Equal(expectedValue, isVisible);
			   });
		}
	}
}

