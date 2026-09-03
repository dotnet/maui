using System.ComponentModel;
using System.Threading.Tasks;
using Android.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

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

			await AttachAndRun(swipeView, async (handler) =>
			{
				var platformView = ((SwipeViewHandler)handler).PlatformView;

				swipeView.Open(OpenSwipeItem.LeftItems, false);

				// The SwipeView adds children dynamically when opening it.
				await AssertEventually(() => platformView.ChildCount > 1);

				var actionView = platformView.GetChildAt(1) as ViewGroup;
				Assert.NotNull(actionView);

				await AssertEventually(() => actionView.ChildCount > 0);

				var nativeSwipeItem = actionView.GetChildAt(0);
				Assert.NotNull(nativeSwipeItem);

				await AssertEventually(() => nativeSwipeItem.Width > 0);

				int contentWidthPixels = platformView.Width;
				int swipeItemWidthPixels = nativeSwipeItem.Width;

				// Prior to the fix, a single Execute-mode SwipeItem was sized to
				// contentWidth / items.Count, i.e. the entire SwipeView content
				// width, instead of the native menu button's measured content size.
				// With a single short-text item, the measured native width should be
				// noticeably smaller than the full SwipeView content width.
				Assert.True(swipeItemWidthPixels < contentWidthPixels,
					$"Expected the Execute-mode SwipeItem width ({swipeItemWidthPixels}px) to be smaller " +
					$"than the full SwipeView content width ({contentWidthPixels}px), matching the native " +
					$"button's measured content size instead of the entire SwipeView width.");
			});
		}

		[Fact]
		[Description("The ScaleX property of a SwipeView should match with native ScaleX")]
		public async Task ScaleXConsistent()
		{
			var swipeView = new SwipeView() { ScaleX = 0.45f };
			var expected = swipeView.ScaleX;
			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformSwipeView = GetPlatformControl(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => platformSwipeView.ScaleX);
			Assert.Equal(expected, platformScaleX);
		}

		[Fact]
		[Description("The ScaleY property of a SwipeView should match with native ScaleY")]
		public async Task ScaleYConsistent()
		{
			var swipeView = new SwipeView() { ScaleY = 1.23f };
			var expected = swipeView.ScaleY;
			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformSwipeView = GetPlatformControl(handler);
			var platformScaleY = await InvokeOnMainThreadAsync(() => platformSwipeView.ScaleY);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The Scale property of a SwipeView should match with native Scale")]
		public async Task ScaleConsistent()
		{
			var swipeView = new SwipeView() { Scale = 2.0f };
			var expected = swipeView.Scale;
			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformSwipeView = GetPlatformControl(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => platformSwipeView.ScaleX);
			var platformScaleY = await InvokeOnMainThreadAsync(() => platformSwipeView.ScaleY);
			Assert.Equal(expected, platformScaleX);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The RotationX property of a SwipeView should match with native RotationX")]
		public async Task RotationXConsistent()
		{
			var swipeView = new SwipeView() { RotationX = 33.0 };
			var expected = swipeView.RotationX;
			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformSwipeView = GetPlatformControl(handler);
			var platformRotationX = await InvokeOnMainThreadAsync(() => platformSwipeView.RotationX);
			Assert.Equal(expected, platformRotationX);
		}

		[Fact]
		[Description("The RotationY property of a SwipeView should match with native RotationY")]
		public async Task RotationYConsistent()
		{
			var swipeView = new SwipeView() { RotationY = 87.0 };
			var expected = swipeView.RotationY;
			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformSwipeView = GetPlatformControl(handler);
			var platformRotationY = await InvokeOnMainThreadAsync(() => platformSwipeView.RotationY);
			Assert.Equal(expected, platformRotationY);
		}

		[Fact]
		[Description("The Rotation property of a SwipeView should match with native Rotation")]
		public async Task RotationConsistent()
		{
			var swipeView = new SwipeView() { Rotation = 23.0 };
			var expected = swipeView.Rotation;
			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformSwipeView = GetPlatformControl(handler);
			var platformRotation = await InvokeOnMainThreadAsync(() => platformSwipeView.Rotation);
			Assert.Equal(expected, platformRotation);
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
				   var isVisible = nativeView.Visibility == global::Android.Views.ViewStates.Visible;
				   Assert.Equal(expectedValue, isVisible);
			   });
		}

		//src/Compatibility/Core/tests/Android/TranslationTests.cs
		[Fact]
		[Description("The Translation property of a SwipeView should match with native Translation")]
		public async Task SwipeViewTranslationConsistent()
		{
			var swipeView = new SwipeView()
			{
				TranslationX = 50,
				TranslationY = -20
			};

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				AssertTranslationMatches(nativeView, swipeView.TranslationX, swipeView.TranslationY);
			});
		}

		[Fact]
		[Description("The IsEnabled of a SwipeView should match with native IsEnabled")]
		public async Task VerifySwipeViewIsEnabledProperty()
		{
			var swipeView = new SwipeView
			{
				IsEnabled = false
			};
			var expectedValue = swipeView.IsEnabled;

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var isEnabled = nativeView.Enabled;
				Assert.Equal(expectedValue, isEnabled);
			});
		}
	}
}