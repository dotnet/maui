using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
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
		// Issue #23478: TalkBack's "double tap to activate" invokes View.PerformAccessibilityAction(ACTION_CLICK)
		// on the focused SwipeItem, a code path distinct from the manual touch hit-testing SwipeView normally
		// relies on to execute commands. This verifies the accessibility action reaches the bound Command.
		[Fact(DisplayName = "SwipeItem Command Executes Via Accessibility ACTION_CLICK")]
		public async Task SwipeItemCommandExecutesViaAccessibilityActionClick()
		{
			SetupBuilder();

			bool commandExecuted = false;
			ICommand command = new Command(() => commandExecuted = true);

			var content = new Grid
			{
				HeightRequest = 60,
				Background = new SolidPaint(Colors.White)
			};

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				Command = command
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

				swipeView.Open(OpenSwipeItem.LeftItems, false);

				// The SwipeView adds the action-item container as a child dynamically when opened.
				await AssertEventually(() => platformView.ChildCount > 1);

				var actionView = platformView.GetChildAt(1) as ViewGroup;
				Assert.NotNull(actionView);

				await AssertEventually(() => actionView.ChildCount > 0);

				var swipeItemView = actionView.GetChildAt(0);
				Assert.NotNull(swipeItemView);

				await InvokeOnMainThreadAsync(() =>
				{
					// A real accessibility service (TalkBack) only dispatches ACTION_CLICK to nodes
					// that actually advertise the action. Assert discoverability, not just dispatch,
					// so this test would fail if the node never exposed ACTION_CLICK in the first place.
					var nodeInfo = AccessibilityNodeInfoCompat.Wrap(swipeItemView.CreateAccessibilityNodeInfo());
					Assert.Contains(nodeInfo.ActionList, action => action.Id == AccessibilityNodeInfoCompat.AccessibilityActionCompat.ActionClick.Id);

					var actionClickId = AccessibilityNodeInfoCompat.AccessibilityActionCompat.ActionClick.Id;
#pragma warning disable CS0618 // ViewCompat.PerformAccessibilityAction is obsolete but is the correct API for simulating an accessibility-service action in a test
					ViewCompat.PerformAccessibilityAction(swipeItemView, actionClickId, null);
#pragma warning restore CS0618
				});

				await AssertEventually(() => commandExecuted);
				Assert.True(commandExecuted);
			});
		}

		// Issue #23478 (SwipeItemView variant): SwipeItemView's platform view is a plain ContentViewGroup,
		// which — unlike SwipeItem's AppCompatButton — is not inherently clickable. This verifies the
		// accessibility action still reaches the bound Command for custom-content swipe items.
		[Fact(DisplayName = "SwipeItemView Command Executes Via Accessibility ACTION_CLICK")]
		public async Task SwipeItemViewCommandExecutesViaAccessibilityActionClick()
		{
			SetupBuilder();

			bool commandExecuted = false;
			ICommand command = new Command(() => commandExecuted = true);

			var content = new Grid
			{
				HeightRequest = 60,
				Background = new SolidPaint(Colors.White)
			};

			var swipeItemContent = new Grid
			{
				BackgroundColor = Colors.Red,
				WidthRequest = 60,
			};

			var swipeItemView = new SwipeItemView
			{
				Content = swipeItemContent,
				Command = command
			};

			var swipeItems = new SwipeItems
			{
				swipeItemView
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

				swipeView.Open(OpenSwipeItem.LeftItems, false);

				// The SwipeView adds the action-item container as a child dynamically when opened.
				await AssertEventually(() => platformView.ChildCount > 1);

				var actionView = platformView.GetChildAt(1) as ViewGroup;
				Assert.NotNull(actionView);

				await AssertEventually(() => actionView.ChildCount > 0);

				var swipeItemPlatformView = actionView.GetChildAt(0);
				Assert.NotNull(swipeItemPlatformView);

				await InvokeOnMainThreadAsync(() =>
				{
					var nodeInfo = AccessibilityNodeInfoCompat.Wrap(swipeItemPlatformView.CreateAccessibilityNodeInfo());
					Assert.Contains(nodeInfo.ActionList, action => action.Id == AccessibilityNodeInfoCompat.AccessibilityActionCompat.ActionClick.Id);

					var actionClickId = AccessibilityNodeInfoCompat.AccessibilityActionCompat.ActionClick.Id;
#pragma warning disable CS0618 // ViewCompat.PerformAccessibilityAction is obsolete but is the correct API for simulating an accessibility-service action in a test
					ViewCompat.PerformAccessibilityAction(swipeItemPlatformView, actionClickId, null);
#pragma warning restore CS0618
				});

				await AssertEventually(() => commandExecuted);
				Assert.True(commandExecuted);
			});
		}

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