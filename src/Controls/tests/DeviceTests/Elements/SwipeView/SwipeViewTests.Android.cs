using System.ComponentModel;
using System.Threading.Tasks;
using Android.Views;
using Android.Widget;
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

		[Fact(DisplayName = "Execute Mode Shared Width Fits Unequal Item Content (Issue 37700)")]
		public async Task ExecuteModeSharedWidthFitsUnequalItemContent()
		{
			SetupBuilder();

			var swipeItems = new SwipeItems
			{
				new SwipeItem { Text = "OK" },
				new SwipeItem { Text = "Long action" },
				new SwipeItem
				{
					Text = "This hidden action must not affect visible widths",
					IsVisible = false
				}
			};
			swipeItems.Mode = SwipeMode.Execute;

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = new Grid()
			};

			await AttachAndRun(swipeView, async (handler) =>
			{
				var platformView = ((SwipeViewHandler)handler).PlatformView;

				swipeView.Open(OpenSwipeItem.LeftItems, false);

				await AssertEventually(() => platformView.ChildCount > 1);

				var actionView = platformView.GetChildAt(1) as ViewGroup;
				Assert.NotNull(actionView);

				await AssertEventually(() =>
					actionView.ChildCount == 3 &&
					actionView.GetChildAt(0)?.Width > 0 &&
					actionView.GetChildAt(1)?.Width > 0);

				var firstNativeSwipeItem = actionView.GetChildAt(0);
				var longerNativeSwipeItem = actionView.GetChildAt(1);
				var hiddenNativeSwipeItem = actionView.GetChildAt(2);
				Assert.NotNull(firstNativeSwipeItem);
				Assert.NotNull(longerNativeSwipeItem);
				Assert.NotNull(hiddenNativeSwipeItem);
				Assert.Equal(ViewStates.Gone, hiddenNativeSwipeItem.Visibility);

				Assert.Equal(firstNativeSwipeItem.Width, longerNativeSwipeItem.Width);

				int assignedWidth = longerNativeSwipeItem.Width;
				longerNativeSwipeItem.Measure(
					global::Android.Views.View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified),
					global::Android.Views.View.MeasureSpec.MakeMeasureSpec(longerNativeSwipeItem.Height, MeasureSpecMode.Exactly));

				Assert.True(assignedWidth + 1 >= longerNativeSwipeItem.MeasuredWidth,
					$"Expected the shared width ({assignedWidth}px) to fit the longer item's " +
					$"measured content width ({longerNativeSwipeItem.MeasuredWidth}px).");

				hiddenNativeSwipeItem.Measure(
					global::Android.Views.View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified),
					global::Android.Views.View.MeasureSpec.MakeMeasureSpec(longerNativeSwipeItem.Height, MeasureSpecMode.Exactly));

				Assert.True(assignedWidth < hiddenNativeSwipeItem.MeasuredWidth,
					"Expected hidden item content to be excluded from the shared width.");
			});
		}

		[Fact(DisplayName = "Execute Mode Visibility Change Uses Consistent Item Widths (Issue 37700)")]
		public async Task ExecuteModeVisibilityChangeUsesConsistentItemWidths()
		{
			SetupBuilder();

			var firstSwipeItem = new SwipeItem
			{
				Text = "OK"
			};

			var secondSwipeItem = new SwipeItem
			{
				Text = "A substantially longer swipe action",
				IsVisible = false
			};

			var swipeItems = new SwipeItems
			{
				firstSwipeItem,
				secondSwipeItem
			};
			swipeItems.Mode = SwipeMode.Execute;

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = new Grid()
			};

			await AttachAndRun(swipeView, async (handler) =>
			{
				var platformView = ((SwipeViewHandler)handler).PlatformView;

				swipeView.Open(OpenSwipeItem.LeftItems, false);

				await AssertEventually(() => platformView.ChildCount > 1);

				var actionView = platformView.GetChildAt(1) as ViewGroup;
				Assert.NotNull(actionView);

				await AssertEventually(() => actionView.ChildCount == 2 && actionView.GetChildAt(0)?.Width > 0);

				secondSwipeItem.IsVisible = true;

				var firstNativeSwipeItem = actionView.GetChildAt(0);
				var secondNativeSwipeItem = actionView.GetChildAt(1);
				Assert.NotNull(firstNativeSwipeItem);
				Assert.NotNull(secondNativeSwipeItem);

				await AssertEventually(() =>
					secondNativeSwipeItem.Visibility == ViewStates.Visible &&
					secondNativeSwipeItem.Width > 0);

				Assert.Equal(firstNativeSwipeItem.Width, secondNativeSwipeItem.Width);
			});
		}

		[Fact(DisplayName = "Execute Mode Remeasures Item When Text Changes (Issue 37700)")]
		public async Task ExecuteModeRemeasuresItemWhenTextChanges()
		{
			SetupBuilder();

			var firstSwipeItem = new SwipeItem { Text = "OK" };
			var secondSwipeItem = new SwipeItem { Text = "Go" };
			var swipeItems = new SwipeItems
			{
				firstSwipeItem,
				secondSwipeItem
			};
			swipeItems.Mode = SwipeMode.Execute;

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 500,
				LeftItems = swipeItems,
				Content = new Grid()
			};

			await AttachAndRun(swipeView, async (handler) =>
			{
				var platformView = ((SwipeViewHandler)handler).PlatformView;
				swipeView.Open(OpenSwipeItem.LeftItems, false);

				await AssertEventually(() => platformView.ChildCount > 1);

				var actionView = platformView.GetChildAt(1) as ViewGroup;
				Assert.NotNull(actionView);

				await AssertEventually(() =>
					actionView.ChildCount == 2 &&
					actionView.GetChildAt(1)?.Width > 0);

				var firstNativeSwipeItem = actionView.GetChildAt(0)!;
				var secondNativeSwipeItem = actionView.GetChildAt(1)!;
				int initialWidth = secondNativeSwipeItem.Width;
				secondSwipeItem.Text = "Delete permanently and archive";

				await AssertEventually(() =>
					(secondNativeSwipeItem as TextView)?.Text == secondSwipeItem.Text);

				secondNativeSwipeItem.Measure(
					global::Android.Views.View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified),
					global::Android.Views.View.MeasureSpec.MakeMeasureSpec(secondNativeSwipeItem.Height, MeasureSpecMode.Exactly));

				int expectedWidth = global::System.Math.Min(
					secondNativeSwipeItem.MeasuredWidth,
					platformView.Width / 2);
				Assert.True(expectedWidth > initialWidth);

				await AssertEventually(() =>
					secondNativeSwipeItem.Width == expectedWidth &&
					firstNativeSwipeItem.Width == expectedWidth,
					message: $"Expected both items to resize from {initialWidth}px to {expectedWidth}px; " +
						$"actual widths were {firstNativeSwipeItem.Width}px and {secondNativeSwipeItem.Width}px.");
			});
		}

		[Fact(DisplayName = "Execute Mode Uses Action Extent For Trigger Distance (Issue 37700)")]
		public async Task ExecuteModeUsesActionExtentForTriggerDistance()
		{
			SetupBuilder();

			int invokedCount = 0;
			var swipeItem = new SwipeItem { Text = "OK" };
			swipeItem.Invoked += (_, _) => invokedCount++;

			var swipeItems = new SwipeItems
			{
				swipeItem
			};
			swipeItems.Mode = SwipeMode.Execute;

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = new Grid()
			};

			await AttachAndRun(swipeView, async (handler) =>
			{
				var platformView = ((SwipeViewHandler)handler).PlatformView;

				swipeView.Open(OpenSwipeItem.LeftItems, false);
				await AssertEventually(() => platformView.ChildCount > 1);

				var actionView = platformView.GetChildAt(1) as ViewGroup;
				Assert.NotNull(actionView);
				await AssertEventually(() => actionView.GetChildAt(0)?.Width > 0);

				float actionExtent = actionView.GetChildAt(0)!.Width;
				swipeView.Close(false);
				await AssertEventually(() => platformView.ChildCount == 2);

				SendHorizontalSwipe(platformView, actionExtent * 0.5f);
				Assert.Equal(0, invokedCount);
				await AssertEventually(() => platformView.ChildCount == 2);

				SendHorizontalSwipe(platformView, actionExtent * 0.7f);
				await AssertEventually(() => invokedCount == 1);
			});
		}

		static void SendHorizontalSwipe(MauiSwipeView platformView, float distance)
		{
			float startX = 10;
			float y = platformView.Height / 2f;
			long downTime = global::Android.OS.SystemClock.UptimeMillis();

			var down = MotionEvent.Obtain(downTime, downTime, MotionEventActions.Down, startX, y, 0);
			platformView.OnTouchEvent(down);
			down.Recycle();

			var move = MotionEvent.Obtain(downTime, downTime + 100, MotionEventActions.Move, startX + distance, y, 0);
			platformView.OnTouchEvent(move);
			move.Recycle();

			var up = MotionEvent.Obtain(downTime, downTime + 200, MotionEventActions.Up, startX + distance, y, 0);
			platformView.OnTouchEvent(up);
			up.Recycle();
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