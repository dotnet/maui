using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Primitives;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public class ContentButtonAppleTests : CoreHandlerTestBase<ContentButtonHandler, ContentButtonStub>
	{
		[Fact]
		public async Task NativeContentIsPresentationOnlyAndUpdates()
		{
			var first = new LabelStub { Text = "First" };
			var second = new LabelStub { Text = "Second" };
			var button = new ContentButtonStub { Content = first };

			var original = await InvokeOnMainThreadAsync(() =>
			{
				var view = first.ToPlatform(MauiContext);
				return (view.UserInteractionEnabled, view.IsAccessibilityElement, view.AccessibilityElementsHidden);
			});

			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = Assert.IsType<MauiContentButton>(handler.PlatformView);
				var container = platformView.ContentContainerView;
				Assert.Same(platformView, container.Superview);
				Assert.False(container.UserInteractionEnabled);
				Assert.False(container.IsAccessibilityElement);
				Assert.True(container.AccessibilityElementsHidden);

				var firstPlatformView = Assert.IsAssignableFrom<UIView>(first.Handler?.PlatformView);
				Assert.Same(container, firstPlatformView.Superview);
				AssertOriginalState(firstPlatformView);

				button.Content = second;
				handler.UpdateValue(nameof(IContentView.Content));

				var secondPlatformView = Assert.IsAssignableFrom<UIView>(second.Handler?.PlatformView);
				Assert.Same(container, secondPlatformView.Superview);
				Assert.Null(firstPlatformView.Superview);
				AssertOriginalState(firstPlatformView);

				button.Content = null;
				handler.UpdateValue(nameof(IContentView.Content));
				Assert.Null(secondPlatformView.Superview);
			});

			void AssertOriginalState(UIView view)
			{
				Assert.Equal(original.UserInteractionEnabled, view.UserInteractionEnabled);
				Assert.Equal(original.IsAccessibilityElement, view.IsAccessibilityElement);
				Assert.Equal(original.AccessibilityElementsHidden, view.AccessibilityElementsHidden);
			}
		}

		[Fact]
		public async Task LiveVirtualViewReplacementReleasesPreviousPress()
		{
			var firstPressed = 0;
			var firstReleased = 0;
			var secondPressed = 0;
			var secondReleased = 0;
			var first = new ContentButtonStub { Pressed = () => firstPressed++, Released = () => firstReleased++ };
			var second = new ContentButtonStub { Pressed = () => secondPressed++, Released = () => secondReleased++ };
			var handler = await CreateHandlerAsync(first);

			await InvokeOnMainThreadAsync(() =>
			{
				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchDown);
				handler.SetVirtualView(first);
				Assert.Equal(1, firstPressed);
				Assert.Equal(0, firstReleased);

				handler.SetVirtualView(second);
				Assert.Equal(1, firstReleased);

				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchUpInside);
				Assert.Equal(0, secondReleased);

				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchDown);
				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchUpInside);
				Assert.Equal(1, secondPressed);
				Assert.Equal(1, secondReleased);
				Assert.Equal(1, firstPressed);
			});
		}

		[Fact]
		public async Task NativeButtonRaisesWindowLifecycleEvents()
		{
			var handler = await CreateHandlerAsync(new ContentButtonStub());

			await InvokeOnMainThreadAsync(() =>
			{
				var events = 0;
				var lifecycle = Assert.IsAssignableFrom<IUIViewLifeCycleEvents>(handler.PlatformView);
				void OnMovedToWindow(object sender, System.EventArgs e) => events++;
				lifecycle.MovedToWindow += OnMovedToWindow;

#pragma warning disable CA1422 // A detached test window is sufficient to observe MovedToWindow.
				var window = new UIWindow(new CoreGraphics.CGRect(0, 0, 100, 100));
#pragma warning restore CA1422
				window.AddSubview(handler.PlatformView);
				handler.PlatformView.RemoveFromSuperview();
				lifecycle.MovedToWindow -= OnMovedToWindow;

				Assert.Equal(2, events);
			});
		}

		[Fact]
		public async Task HandlerProvidedNativeStylingIsRestoredAsDefault()
		{
			var button = new ContentButtonStub
			{
				Background = new SolidPaint(Colors.Blue),
				StrokeColor = Colors.Green,
				StrokeThickness = 6,
				CornerRadius = 9,
			};

			var handler = await InvokeOnMainThreadAsync(() =>
				CreateHandler<ContentButtonHandler, StyledContentButtonHandler>(button, MauiContext));

			await InvokeOnMainThreadAsync(() =>
			{
				button.Background = null;
				button.StrokeColor = null;
				button.StrokeThickness = -1;
				button.CornerRadius = -1;
				handler.UpdateValue(nameof(IView.Background));
				handler.UpdateValue(nameof(IButtonStroke.StrokeColor));
				handler.UpdateValue(nameof(IButtonStroke.StrokeThickness));
				handler.UpdateValue(nameof(IButtonStroke.CornerRadius));

				var layer = handler.PlatformView.Layer;
				Assert.Equal(StyledContentButtonHandler.Background, handler.PlatformView.BackgroundColor);
				Assert.Equal(StyledContentButtonHandler.Border.CGColor, layer.BorderColor);
				Assert.Equal(3, layer.BorderWidth);
				Assert.Equal(5, layer.CornerRadius);
			});
		}

		sealed class StyledContentButtonHandler : ContentButtonHandler
		{
			internal static readonly UIColor Background = UIColor.Red;
			internal static readonly UIColor Border = UIColor.Purple;

			protected override UIButton CreatePlatformView()
			{
				var button = base.CreatePlatformView();
				button.BackgroundColor = Background;
				button.Layer.BorderColor = Border.CGColor;
				button.Layer.BorderWidth = 3;
				button.Layer.CornerRadius = 5;
				return button;
			}
		}

		[Fact]
		public async Task NativeEventsHandleCancellationActivationDisableAndDisconnect()
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

			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = handler.PlatformView;
				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchDown);
				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchUpOutside);
				Assert.Equal(1, pressed);
				Assert.Equal(1, released);

				handler.PlatformView.SendActionForControlEvents(UIControlEvent.PrimaryActionTriggered);
				Assert.Equal(1, clicked);

				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchDown);
				button.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));
				Assert.Equal(2, pressed);
				Assert.Equal(2, released);
				Assert.False(handler.PlatformView.Enabled);

				button.IsEnabled = true;
				handler.UpdateValue(nameof(IView.IsEnabled));
				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchDown);
				((IElementHandler)handler).DisconnectHandler();
				Assert.Equal(3, pressed);
				Assert.Equal(3, released);

				platformView.SendActionForControlEvents(UIControlEvent.PrimaryActionTriggered);
				Assert.Equal(1, clicked);
			});
		}

		[Theory]
		[InlineData(0, 0)]
		[InlineData(1, 1)]
		[InlineData(100, 60)]
		[InlineData(double.PositiveInfinity, double.PositiveInfinity)]
		public async Task NativeMeasureHandlesTinyFiniteAndInfiniteConstraints(double width, double height)
		{
			var button = new ContentButtonStub
			{
				Width = Dimension.Unset,
				Height = Dimension.Unset,
				Content = new LabelStub { Text = "Content" },
				Padding = new Thickness(8),
				StrokeThickness = 2,
			};
			var handler = await CreateHandlerAsync(button);

			var size = await InvokeOnMainThreadAsync(() =>
				handler.PlatformView.SizeThatFits(new CoreGraphics.CGSize(width, height)));

			Assert.False(double.IsNaN(size.Width));
			Assert.False(double.IsNaN(size.Height));
			Assert.True(size.Width >= 0);
			Assert.True(size.Height >= 0);
		}

		[Fact]
		public async Task NativeMeasureIncludesContentAndInsets()
		{
			var button = new ContentButtonStub
			{
				Width = Dimension.Unset,
				Height = Dimension.Unset,
				Content = new LabelStub { Text = "Content", Width = 50, Height = 30 },
				Padding = new Thickness(8),
				StrokeThickness = 2,
			};
			var handler = await CreateHandlerAsync(button);

			var size = await InvokeOnMainThreadAsync(() =>
				handler.PlatformView.SizeThatFits(new CoreGraphics.CGSize(100, 80)));

			Assert.Equal(70, size.Width);
			Assert.Equal(50, size.Height);
		}
	}
}
