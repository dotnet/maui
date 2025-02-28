using System.ComponentModel;
using System.Threading.Tasks;
using Android.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using System.ComponentModel;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SwipeViewTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "SwipeItem Size Initializes Correctly")]
		public async Task SwipeItemSizeInitializesCorrectly()
		{
			SetupBuilder();

			var expectedColor = Colors.Red;

			var content = new VerticalStackLayout
			{
				HeightRequest = 60,
				Background = new SolidColorBrush(Colors.White)
			};

			var swipeItemContent = new Grid
			{
				BackgroundColor = expectedColor,
				WidthRequest = 60,
			};

			var swipeItem = new SwipeItemView
			{
				Content = swipeItemContent
			};

			var swipeItems = new SwipeItems
			{
				swipeItem
			};

			var swipeView = new SwipeView()
			{
				HeightRequest = 60,
				LeftItems = swipeItems,
				Content = content
			};

			await AttachAndRun(swipeView, async (handler) =>
			{
				var platformView = ((SwipeViewHandler)handler).PlatformView;
				var openRequest = new SwipeViewOpenRequest(OpenSwipeItem.LeftItems, false);
				swipeView.Open(OpenSwipeItem.LeftItems, false);

				// The SwipeView add children dynamically opening it.
				await AssertEventually(() => platformView.ChildCount > 1);

				var actionView = platformView.GetChildAt(1) as ViewGroup;
				Assert.NotNull(actionView);

				await AssertEventually(() => actionView.ChildCount > 0);

				var swipeItem = actionView.GetChildAt(0);
				Assert.NotNull(swipeItem);

				await AssertEventually(() => swipeItem.Width > 0);
				Assert.NotEqual(0, swipeItem.Width);
			});
		}

		[Fact]
		[Description("The ScaleX property of a SwipeView should match with native ScaleX")]
        public async Task ScaleXConsistent()
        {
            var swipeView = new SwipeView() { ScaleX = 0.45f };
            var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
            var expected = swipeView.ScaleX;
            var platformScaleX = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleX);
            Assert.Equal(expected, platformScaleX);
        }

		[Fact]
		[Description("The ScaleY property of a SwipeView should match with native ScaleY")]
        public async Task ScaleYConsistent()
        {
            var swipeView = new SwipeView() { ScaleY = 0.45f };
            var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
            var expected = swipeView.ScaleY;
            var platformScaleY = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleY);
            Assert.Equal(expected, platformScaleY);
        }

		MauiSwipeView GetPlatformControl(SwipeViewHandler handler) =>
			handler.PlatformView;

		Task<bool> HasChildren(SwipeViewHandler handler)
		{
			return InvokeOnMainThreadAsync(()
				=> GetPlatformControl(handler).ChildCount != 0);
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
	}
}