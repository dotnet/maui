#nullable enable
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Xunit;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public class ContentButtonWindowsTests : CoreHandlerTestBase<ContentButtonHandler, ContentButtonStub>
	{
		[Fact]
		public async Task NativeContentIsPresentationOnlyAndUpdates()
		{
			var first = new ButtonStub { Text = "First" };
			var second = new ButtonStub { Text = "Second" };
			var button = new ContentButtonStub { Content = first };
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformButton = Assert.IsType<MauiContentButton>(handler.PlatformView);
				var contentPanel = platformButton.ContentPanel;
				var firstPlatformView = Assert.IsAssignableFrom<FrameworkElement>(first.Handler?.PlatformView);

				Assert.Same(button, contentPanel.CrossPlatformLayout);
				Assert.Same(firstPlatformView, contentPanel.Content);
				Assert.False(contentPanel.IsHitTestVisible);
				Assert.False(firstPlatformView.IsHitTestVisible);
				Assert.False(Assert.IsAssignableFrom<Microsoft.UI.Xaml.Controls.Control>(firstPlatformView).IsTabStop);
				Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(contentPanel));

				button.Content = second;
				handler.UpdateValue(nameof(IContentView.Content));

				var secondPlatformView = Assert.IsAssignableFrom<FrameworkElement>(second.Handler?.PlatformView);
				Assert.Same(secondPlatformView, contentPanel.Content);
				Assert.Null(firstPlatformView.Parent);
				Assert.True(firstPlatformView.IsHitTestVisible);
				Assert.True(Assert.IsAssignableFrom<Microsoft.UI.Xaml.Controls.Control>(firstPlatformView).IsTabStop);
				Assert.False(secondPlatformView.IsHitTestVisible);

				button.Content = null!;
				handler.UpdateValue(nameof(IContentView.Content));
				Assert.Null(contentPanel.Content);
				Assert.True(secondPlatformView.IsHitTestVisible);
			});
		}

		[Fact]
		public async Task AutomationInvokeClicksExactlyOnceAndReconnects()
		{
			var clicks = 0;
			var button = new ContentButtonStub
			{
				Clicked = () => clicks++,
				Semantics = new Semantics { Description = "Content action" }
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var firstPlatformView = handler.PlatformView;
				var firstPeer = new MauiButtonAutomationPeer(firstPlatformView);
				var invokeProvider = Assert.IsAssignableFrom<IInvokeProvider>(firstPeer.GetPattern(PatternInterface.Invoke));

				Assert.Equal(AutomationControlType.Button, firstPeer.GetAutomationControlType());
				Assert.Equal("Content action", firstPeer.GetName());
				Assert.Null(firstPeer.GetChildren());

				invokeProvider.Invoke();
				Assert.Equal(1, clicks);

				((IElementHandler)handler).DisconnectHandler();
				invokeProvider.Invoke();
				Assert.Equal(1, clicks);

				handler.SetVirtualView(button);
				var secondPeer = new MauiButtonAutomationPeer(handler.PlatformView);
				var secondInvokeProvider = Assert.IsAssignableFrom<IInvokeProvider>(secondPeer.GetPattern(PatternInterface.Invoke));
				secondInvokeProvider.Invoke();
				Assert.Equal(2, clicks);
			});
		}

		[Fact]
		public async Task NativePaddingAndStrokeReserveInsetsOnce()
		{
			var button = new ContentButtonStub
			{
				Content = new LabelStub { Text = "Content" },
				Padding = new Thickness(1, 2, 3, 4),
				StrokeThickness = 6
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformButton = Assert.IsType<MauiContentButton>(handler.PlatformView);
				var nativeStroke = Assert.IsType<WThickness>(platformButton.Resources["ButtonBorderThemeThickness"]);

				Assert.Equal(new WThickness(0), platformButton.Padding);
				Assert.Equal(new WThickness(-6), platformButton.ContentPanel.Margin);
				Assert.Equal(new WThickness(6), nativeStroke);

				button.Padding = new Thickness(9, 8, 7, 6);
				handler.UpdateValue(nameof(IPadding.Padding));
				Assert.Equal(new WThickness(0), platformButton.Padding);

				button.StrokeThickness = -1;
				handler.UpdateValue(nameof(IButtonStroke.StrokeThickness));
				Assert.Equal(new WThickness(0), platformButton.ContentPanel.Margin);
				Assert.False(platformButton.Resources.Keys.Contains("ButtonBorderThemeThickness"));
			});
		}
	}
}
