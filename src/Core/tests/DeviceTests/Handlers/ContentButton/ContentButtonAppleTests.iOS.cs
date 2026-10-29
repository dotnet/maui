using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
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
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var firstPlatformView = Assert.Single(handler.PlatformView.Subviews, view => view.AccessibilityElementsHidden);
				Assert.Same(first.Handler?.PlatformView, firstPlatformView);
				Assert.False(firstPlatformView.UserInteractionEnabled);
				Assert.False(firstPlatformView.IsAccessibilityElement);
				Assert.True(firstPlatformView.AccessibilityElementsHidden);

				button.Content = second;
				handler.UpdateValue(nameof(IContentView.Content));

				var secondPlatformView = Assert.Single(handler.PlatformView.Subviews, view => view.AccessibilityElementsHidden);
				Assert.Same(second.Handler?.PlatformView, secondPlatformView);
				Assert.Null(firstPlatformView.Superview);
				Assert.True(secondPlatformView.AccessibilityElementsHidden);

				button.Content = null;
				handler.UpdateValue(nameof(IContentView.Content));
				Assert.DoesNotContain(handler.PlatformView.Subviews, view => view.AccessibilityElementsHidden);
			});
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

				handler.PlatformView.SendActionForControlEvents(UIControlEvent.PrimaryActionTriggered);
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
	}
}
