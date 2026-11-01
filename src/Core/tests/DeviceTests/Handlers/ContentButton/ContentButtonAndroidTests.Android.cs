using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Views.Accessibility;
using AndroidX.Core.View.Accessibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public class ContentButtonAndroidTests : CoreHandlerTestBase<ContentButtonHandler, ContentButtonStub>
	{
		[Fact]
		public async Task ContentIsPresentationOnlyAndReplacementRestoresNativeState()
		{
			var first = new ButtonStub { Text = "First" };
			var second = new ButtonStub { Text = "Second" };
			var button = new ContentButtonStub { Content = first };
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal(1, handler.PlatformView.ChildCount);
				var firstPlatformView = Assert.IsAssignableFrom<AView>(handler.PlatformView.GetChildAt(0));
				Assert.Equal(ImportantForAccessibility.NoHideDescendants, firstPlatformView.ImportantForAccessibility);

				button.Content = second;
				handler.UpdateValue(nameof(IContentView.Content));

				Assert.Equal(1, handler.PlatformView.ChildCount);
				var secondPlatformView = Assert.IsAssignableFrom<AView>(handler.PlatformView.GetChildAt(0));
				Assert.Same(second.Handler?.PlatformView, secondPlatformView);
				Assert.Null(firstPlatformView.Parent);
				Assert.NotEqual(ImportantForAccessibility.NoHideDescendants, firstPlatformView.ImportantForAccessibility);
				Assert.Equal(ImportantForAccessibility.NoHideDescendants, secondPlatformView.ImportantForAccessibility);

				button.Content = null;
				handler.UpdateValue(nameof(IContentView.Content));

				Assert.Equal(0, handler.PlatformView.ChildCount);
				Assert.Null(secondPlatformView.Parent);
				Assert.NotEqual(ImportantForAccessibility.NoHideDescendants, secondPlatformView.ImportantForAccessibility);
			});
		}

		[Fact]
		public async Task NativeCancelDisableDetachAndReattachReleasePressedState()
		{
			var pressed = 0;
			var released = 0;
			var clicked = 0;
			var button = new ContentButtonStub
			{
				Pressed = () => pressed++,
				Released = () => released++,
				Clicked = () => clicked++,
			};

			await AttachAndRun(button, handler =>
			{
				var platformView = handler.PlatformView;

				DispatchTouch(platformView, MotionEventActions.Down);
				DispatchTouch(platformView, MotionEventActions.Cancel);
				Assert.Equal(1, pressed);
				Assert.Equal(1, released);

				Assert.True(platformView.PerformClick());
				Assert.Equal(1, clicked);

				DispatchTouch(platformView, MotionEventActions.Down);
				button.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));
				Assert.Equal(2, pressed);
				Assert.Equal(2, released);
				Assert.False(platformView.Enabled);

				DispatchTouch(platformView, MotionEventActions.Down);
				Assert.Equal(2, pressed);
				platformView.PerformClick();
				Assert.Equal(1, clicked);

				button.IsEnabled = true;
				handler.UpdateValue(nameof(IView.IsEnabled));
				DispatchTouch(platformView, MotionEventActions.Down);

				var parent = Assert.IsAssignableFrom<ViewGroup>(platformView.Parent);
				var layoutParameters = platformView.LayoutParameters;
				parent.RemoveView(platformView);
				Assert.Equal(3, pressed);
				Assert.Equal(3, released);

				parent.AddView(platformView, layoutParameters);
				DispatchTouch(platformView, MotionEventActions.Down);
				DispatchTouch(platformView, MotionEventActions.Up);
				Assert.Equal(4, pressed);
				Assert.Equal(4, released);

				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task AccessibilityExposesSingleNamedButtonAction()
		{
			var child = new ButtonStub { Text = "Presentation" };
			var button = new ContentButtonStub { Content = child };
			button.Semantics.Description = "Open details";
			await AttachAndRun(button, handler =>
			{
				using var info = handler.PlatformView.CreateAccessibilityNodeInfo();
				var compat = AccessibilityNodeInfoCompat.Wrap(info);

				Assert.Equal("android.widget.Button", info.ClassName);
				Assert.Equal("Open details", info.ContentDescription);
				Assert.True(info.Enabled);
				Assert.Contains(
					compat.ActionList,
					action => action.Id == AccessibilityNodeInfoCompat.AccessibilityActionCompat.ActionClick.Id);

				Assert.Equal(1, handler.PlatformView.ChildCount);
				var platformChild = Assert.IsAssignableFrom<AView>(handler.PlatformView.GetChildAt(0));
				Assert.Equal(ImportantForAccessibility.NoHideDescendants, platformChild.ImportantForAccessibility);

				button.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));

				using var disabledInfo = handler.PlatformView.CreateAccessibilityNodeInfo();
				Assert.False(disabledInfo.Enabled);

				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task GradientBackgroundStrokeAndRadiusCanReturnToNativeDefaults()
		{
			var button = new ContentButtonStub
			{
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Blue),
				StrokeColor = Colors.Green,
				StrokeThickness = 3,
				CornerRadius = 18,
				Width = 160,
				Height = 80,
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = Assert.IsType<MauiContentButton>(handler.PlatformView);
				Assert.True(platformView.TryGetMauiBackground(
					out var ripple,
					out _,
					out var background,
					out var stroke,
					out var mask));
				Assert.NotNull(ripple);

				var expectedRadius = button.GetStrokeProperties(platformView.Context!, false).Radius;
				Assert.Equal(expectedRadius, background.CornerRadius, 1);
				Assert.Equal(expectedRadius, stroke.CornerRadius, 1);
				Assert.Equal(expectedRadius, mask.CornerRadius, 1);

				button.Background = null;
				button.StrokeColor = null;
				button.StrokeThickness = -1;
				button.CornerRadius = -1;
				handler.UpdateValue(nameof(IView.Background));
				handler.UpdateValue(nameof(IButtonStroke.StrokeColor));
				handler.UpdateValue(nameof(IButtonStroke.StrokeThickness));
				handler.UpdateValue(nameof(IButtonStroke.CornerRadius));

				Assert.False(platformView.TryGetMauiBackground(out _, out _, out _, out _, out _));
			});
		}

		static void DispatchTouch(AView view, MotionEventActions action)
		{
			var now = SystemClock.UptimeMillis();
			using var motionEvent = MotionEvent.Obtain(now, now, action, 1, 1, 0);
			view.DispatchTouchEvent(motionEvent);
		}
	}
}
