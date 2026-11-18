using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Views.Accessibility;
using AndroidX.Core.View.Accessibility;
using Google.Android.Material.Card;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using AColor = Android.Graphics.Color;
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
				var container = Assert.IsAssignableFrom<ViewGroup>(handler.PlatformView.GetChildAt(0));
				var firstPlatformView = Assert.IsAssignableFrom<AView>(container.GetChildAt(0));
				var firstImportance = firstPlatformView.ImportantForAccessibility;
				Assert.Equal(ImportantForAccessibility.NoHideDescendants, container.ImportantForAccessibility);

				button.Content = second;
				handler.UpdateValue(nameof(IContentView.Content));

				Assert.Equal(1, handler.PlatformView.ChildCount);
				Assert.Same(container, handler.PlatformView.GetChildAt(0));
				var secondPlatformView = Assert.IsAssignableFrom<AView>(container.GetChildAt(0));
				var secondImportance = secondPlatformView.ImportantForAccessibility;
				Assert.Same(second.Handler?.PlatformView, secondPlatformView);
				Assert.Null(firstPlatformView.Parent);
				Assert.Equal(firstImportance, firstPlatformView.ImportantForAccessibility);
				Assert.Equal(ImportantForAccessibility.NoHideDescendants, container.ImportantForAccessibility);

				button.Content = null;
				handler.UpdateValue(nameof(IContentView.Content));

				Assert.Equal(0, handler.PlatformView.ChildCount);
				Assert.Null(secondPlatformView.Parent);
				Assert.Equal(secondImportance, secondPlatformView.ImportantForAccessibility);
				Assert.Equal(0, container.ChildCount);
			});
		}

		[Fact]
		public async Task PostedThemeChangeIsIgnoredAfterDisconnect()
		{
			await AttachAndRun(new ContentButtonStub(), async handler =>
			{
				var platformView = handler.PlatformView;
				using var configuration = new Configuration(platformView.Resources!.Configuration!);
				var nightMode = configuration.UiMode & UiMode.NightMask;
				configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) |
					(nightMode == UiMode.NightYes ? UiMode.NightNo : UiMode.NightYes);

				platformView.DispatchConfigurationChanged(configuration);
				((IElementHandler)handler).DisconnectHandler();
				Assert.Null(((IElementHandler)handler).PlatformView);
				Assert.Null(((IElementHandler)handler).VirtualView);

				var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
				platformView.Post(() => completed.SetResult());
				await completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
			});
		}

		[Fact]
		public async Task PostedThemeChangeRefreshesConnectedNativeDefaultsAndStyling()
		{
			var button = new ContentButtonStub
			{
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Blue),
				StrokeColor = Colors.Green,
				StrokeThickness = 3,
				CornerRadius = -1,
			};

			await AttachAndRun(button, async handler =>
			{
				var platformView = Assert.IsType<MauiContentButton>(handler.PlatformView);
				using var themedCard = new MaterialCardView(platformView.Context!);
				var expectedRadius = themedCard.Radius;
				var expectedRipple = themedCard.RippleColor!.DefaultColor;
				using var replacementBackground = new ColorDrawable(AColor.Magenta);
				platformView.Background = replacementBackground;
				platformView.Radius = 99;
				platformView.UpdateRippleColor(Colors.Yellow);

				using var configuration = new Configuration(platformView.Resources!.Configuration!);
				var nightMode = configuration.UiMode & UiMode.NightMask;
				configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) |
					(nightMode == UiMode.NightYes ? UiMode.NightNo : UiMode.NightYes);

				platformView.DispatchConfigurationChanged(configuration);
				await DrainPostedWork(platformView);

				Assert.True(platformView.TryGetMauiBackground(
					out _,
					out _,
					out var background,
					out var stroke,
					out var mask));
				Assert.Equal(expectedRadius, background.CornerRadius, 1);
				Assert.Equal(expectedRadius, stroke.CornerRadius, 1);
				Assert.Equal(expectedRadius, mask.CornerRadius, 1);
				Assert.Equal(expectedRipple, platformView.RippleColor!.DefaultColor);
				Assert.IsType<RippleDrawable>(platformView.Background);
			});
		}

		[Fact]
		public async Task HandlerProvidedNativeRadiusSurvivesAttachmentAndThemeRefresh()
		{
			var button = new ContentButtonStub { CornerRadius = -1 };
			var handler = await InvokeOnMainThreadAsync(() =>
				CreateHandler<ContentButtonHandler, StyledContentButtonHandler>(button, MauiContext));

			await InvokeOnMainThreadAsync(async () =>
			{
				await handler.PlatformView.AttachAndRun(async () =>
				{
					await DrainPostedWork(handler.PlatformView);
					Assert.Equal(StyledContentButtonHandler.NativeRadius, handler.PlatformView.Radius);

					using var configuration = new Configuration(handler.PlatformView.Resources!.Configuration!);
					var nightMode = configuration.UiMode & UiMode.NightMask;
					configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) |
						(nightMode == UiMode.NightYes ? UiMode.NightNo : UiMode.NightYes);

					handler.PlatformView.DispatchConfigurationChanged(configuration);
					await DrainPostedWork(handler.PlatformView);
					Assert.Equal(StyledContentButtonHandler.NativeRadius, handler.PlatformView.Radius);
				});
			});
		}

		[Fact]
		public async Task DisableDuringPressCanDisconnectDuringRelease()
		{
			var pressed = 0;
			var released = 0;
			var button = new ContentButtonStub();

			await AttachAndRun(button, handler =>
			{
				button.Pressed = () =>
				{
					pressed++;
					button.IsEnabled = false;
					handler.UpdateValue(nameof(IView.IsEnabled));
				};
				button.Released = () =>
				{
					released++;
					((IElementHandler)handler).DisconnectHandler();
				};

				var exception = Record.Exception(() =>
					DispatchTouch(handler.PlatformView, MotionEventActions.Down, 1, 1));

				Assert.Null(exception);
				Assert.Equal(1, pressed);
				Assert.Equal(1, released);
				Assert.Null(((IElementHandler)handler).PlatformView);
				Assert.Null(((IElementHandler)handler).VirtualView);
				return Task.CompletedTask;
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task RebindingReleasesOnlyThePreviouslyPressedView(bool replaceView)
		{
			var firstReleased = 0;
			var secondReleased = 0;
			var first = new ContentButtonStub { Released = () => firstReleased++ };
			var second = new ContentButtonStub { Released = () => secondReleased++ };

			await AttachAndRun(first, handler =>
			{
				var platformView = handler.PlatformView;
				DispatchTouch(platformView, MotionEventActions.Down);

				handler.SetVirtualView(replaceView ? second : first);
				Assert.Equal(replaceView ? 1 : 0, firstReleased);
				Assert.Equal(0, secondReleased);

				DispatchTouch(platformView, MotionEventActions.Cancel);
				Assert.Equal(1, firstReleased);
				Assert.Equal(0, secondReleased);

				DispatchTouch(platformView, MotionEventActions.Down);
				DispatchTouch(platformView, MotionEventActions.Up);
				Assert.Equal(replaceView ? 1 : 2, firstReleased);
				Assert.Equal(replaceView ? 1 : 0, secondReleased);

				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task PositionOnlyLayoutKeepsBackgroundWhileResizingRefreshesIt()
		{
			var button = new ContentButtonStub
			{
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Blue),
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = handler.PlatformView;
				platformView.Layout(0, 0, 160, 80);
				Assert.True(platformView.TryGetMauiBackground(out _, out _, out var background, out _, out _));

				platformView.Layout(20, 30, 180, 110);
				Assert.True(platformView.TryGetMauiBackground(out _, out _, out var movedBackground, out _, out _));
				Assert.Same(background, movedBackground);

				platformView.Layout(20, 30, 200, 130);
				Assert.True(platformView.TryGetMauiBackground(out _, out _, out var resizedBackground, out _, out _));
				Assert.NotSame(background, resizedBackground);
			});
		}

		[Fact]
		public async Task ResizeRerunsBackgroundMapperCustomization()
		{
			var button = new ContentButtonStub
			{
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Blue),
			};
			var customBackground = new ColorDrawable(AColor.Magenta);
			var mapper = new PropertyMapper<IContentButton, IContentButtonHandler>(ContentButtonHandler.Mapper);
			mapper.AppendToMapping(nameof(IView.Background), (handler, _) =>
				handler.PlatformView.Background = customBackground);
			var handler = new ContentButtonHandler(mapper);

			await InvokeOnMainThreadAsync(() =>
			{
				InitializeViewHandler(button, handler);
				var platformView = handler.PlatformView;
				Assert.Same(customBackground, platformView.Background);

				platformView.Layout(0, 0, 160, 80);
				platformView.Layout(0, 0, 180, 90);

				Assert.Same(customBackground, platformView.Background);
			});
		}

		[Fact]
		public async Task ResizeDoesNotRebuildSolidBackground()
		{
			var button = new ContentButtonStub
			{
				Background = new SolidPaint(Colors.Red),
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = handler.PlatformView;
				platformView.Layout(0, 0, 160, 80);
				var background = platformView.Background;

				platformView.Layout(0, 0, 180, 90);

				Assert.Same(background, platformView.Background);
			});
		}

		[Fact]
		public async Task ReleaseOutsideBoundsCancelsClickWhileInsideReleaseActivates()
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

			await AttachAndRun(button, async handler =>
			{
				var platformView = handler.PlatformView;
				var downTime = SystemClock.UptimeMillis();
				var insideX = platformView.Width - 1;
				var y = platformView.Height / 2f;
				DispatchTouch(platformView, MotionEventActions.Down, insideX, y, downTime);
				DispatchTouch(platformView, MotionEventActions.Move, platformView.Width + 1, y, downTime);
				DispatchTouch(platformView, MotionEventActions.Up, platformView.Width + 1, y, downTime);
				await Task.Delay(250);

				Assert.Equal(1, pressed);
				Assert.Equal(1, released);
				Assert.Equal(0, clicked);

				downTime = SystemClock.UptimeMillis();
				DispatchTouch(platformView, MotionEventActions.Down, platformView.Width / 2f, y, downTime);
				DispatchTouch(platformView, MotionEventActions.Up, platformView.Width / 2f, y, downTime);
				await Task.Delay(250);

				Assert.Equal(2, pressed);
				Assert.Equal(2, released);
				Assert.Equal(1, clicked);
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

		static Task DrainPostedWork(AView view)
		{
			var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			view.Post(() => completed.SetResult());
			return completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
		}

		static void DispatchTouch(AView view, MotionEventActions action, float x = 1, float y = 1, long? downTime = null)
		{
			var now = SystemClock.UptimeMillis();
			using var motionEvent = MotionEvent.Obtain(downTime ?? now, now, action, x, y, 0);
			view.DispatchTouchEvent(motionEvent);
		}

		sealed class StyledContentButtonHandler : ContentButtonHandler
		{
			internal const float NativeRadius = 12;

			protected override MaterialCardView CreatePlatformView()
			{
				var button = base.CreatePlatformView();
				button.Radius = NativeRadius;
				return button;
			}
		}
	}
}
