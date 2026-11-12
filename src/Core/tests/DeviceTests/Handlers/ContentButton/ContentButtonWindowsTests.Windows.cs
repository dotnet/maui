#nullable enable
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Xunit;
using WButton = Microsoft.UI.Xaml.Controls.Button;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WRect = Windows.Foundation.Rect;
using WSize = Windows.Foundation.Size;
using WStackPanel = Microsoft.UI.Xaml.Controls.StackPanel;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public class ContentButtonWindowsTests : CoreHandlerTestBase<ContentButtonHandler, ContentButtonStub>
	{
		[Fact]
		public async Task ReplacingVirtualViewUpdatesLayoutAndReleasesOldPress()
		{
			var pressed = false;
			var releases = 0;
			var newReleases = 0;
			var firstContent = new LabelStub { Text = "First" };
			var secondContent = new LabelStub { Text = "Second" };
			var first = new ContentButtonStub
			{
				Content = firstContent,
				Width = 100,
				Height = 50,
				Pressed = () => pressed = true,
				Released = () =>
				{
					pressed = false;
					releases++;
				}
			};
			var second = new ContentButtonStub
			{
				Content = secondContent,
				Width = 180,
				Height = 90,
				Padding = new Thickness(5),
				Released = () => newReleases++
			};
			var handler = await CreateHandlerAsync(first);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformButton = Assert.IsType<MauiContentButton>(handler.PlatformView);
				var panel = platformButton.ContentPanel;
				panel.Measure(new WSize(400, 300));
				Assert.Equal(new WSize(100, 50), panel.DesiredSize);

				PressActivationKey(handler);
				Assert.True(pressed);
				handler.SetVirtualView(first);
				Assert.True(pressed);
				Assert.Equal(0, releases);

				handler.SetVirtualView(second);

				Assert.Same(platformButton, handler.PlatformView);
				Assert.Same(second, panel.CrossPlatformLayout);
				Assert.Same(secondContent.Handler.PlatformView, panel.Content);
				Assert.False(pressed);
				Assert.Equal(1, releases);
				Assert.Equal(0, newReleases);

				panel.Measure(new WSize(400, 300));
				Assert.Equal(new WSize(180, 90), panel.DesiredSize);
				panel.Arrange(new WRect(0, 0, 180, 90));
				Assert.Equal(new Graphics.Rect(5, 5, 170, 80), secondContent.Frame);
			});
		}

		[Fact]
		public async Task ActivationPressIsReleasedWhenFocusMoves()
		{
			var pressed = false;
			var releases = 0;
			var clicks = 0;
			var button = new ContentButtonStub
			{
				Pressed = () => pressed = true,
				Released = () =>
				{
					pressed = false;
					releases++;
				},
				Clicked = () => clicks++
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(async () =>
			{
				var platformButton = handler.PlatformView;
				var otherButton = new WButton { Content = "Other" };
				var root = new WStackPanel { Children = { platformButton, otherButton } };
				await root.AttachAndRun(async () =>
				{
					Assert.True(platformButton.Focus(FocusState.Programmatic));
					await AssertionExtensions.AssertEventually(() => platformButton.FocusState != FocusState.Unfocused);
					PressActivationKey(handler);
					Assert.True(pressed);

					// WinUI raises LostFocus after Focus returns.
					Assert.True(otherButton.Focus(FocusState.Programmatic));
					await AssertionExtensions.AssertEventually(() => !pressed, timeout: 2000);
					Assert.Equal(1, releases);
					Assert.Equal(0, clicks);

					((IElementHandler)handler).DisconnectHandler();
					Assert.Equal(1, releases);
				}, MauiContext);
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task OriginalTabStopsAreRestoredForRemainingAndRemovedDescendants(bool disconnect)
		{
			var handler = await CreateHandlerAsync(new ContentButtonStub());

			await InvokeOnMainThreadAsync(() =>
			{
				var platformButton = Assert.IsType<MauiContentButton>(handler.PlatformView);
				var enabled = new WButton { IsTabStop = true };
				var disabled = new WButton { IsTabStop = false };
				var removed = new WButton { IsTabStop = true };
				var root = new WGrid { Children = { enabled, disabled, removed } };
				platformButton.SetPresentedContent(root);

				Assert.False(enabled.IsTabStop);
				Assert.False(disabled.IsTabStop);
				Assert.False(removed.IsTabStop);
				root.Children.Remove(removed);

				if (disconnect)
					((IElementHandler)handler).DisconnectHandler();
				else
					platformButton.ClearPresentedContent();

				Assert.True(enabled.IsTabStop);
				Assert.False(disabled.IsTabStop);
				Assert.True(removed.IsTabStop);
				Assert.True(root.IsHitTestVisible);
				Assert.Null(platformButton.ContentPanel.Content);
			});
		}

		[Fact]
		public async Task RemovedPresentationDescendantIsNotRetained()
		{
			var handler = await CreateHandlerAsync(new ContentButtonStub());
			await InvokeOnMainThreadAsync(async () =>
			{
				var platformButton = Assert.IsType<MauiContentButton>(handler.PlatformView);
				var root = new WGrid();
				var removed = PresentAndRemoveDescendant(platformButton, root);

				await AssertionExtensions.WaitForGC(removed);

				Assert.Same(root, platformButton.ContentPanel.Content);
				platformButton.ClearPresentedContent();
				GC.KeepAlive(handler);
			});
		}

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
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformButton = Assert.IsType<MauiContentButton>(handler.PlatformView);
				var defaultStroke = platformButton.GetResource<object>("ButtonBorderThemeThickness");

				button.StrokeThickness = 6;
				handler.UpdateValue(nameof(IButtonStroke.StrokeThickness));
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
				Assert.Equal(defaultStroke, platformButton.GetResource<object>("ButtonBorderThemeThickness"));
			});
		}

		static void PressActivationKey(ContentButtonHandler handler)
		{
			// WinUI does not expose a constructor for synthetic activation-key events.
			var keyDown = typeof(ContentButtonHandler).GetMethod("OnKeyDown", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(keyDown);
			keyDown.Invoke(handler, new object?[] { handler.PlatformView, null });
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference PresentAndRemoveDescendant(MauiContentButton platformButton, WGrid root)
		{
			var descendant = new WButton { IsTabStop = true };
			root.Children.Add(descendant);
			platformButton.SetPresentedContent(root);
			Assert.False(descendant.IsTabStop);
			root.Children.Remove(descendant);
			return new WeakReference(descendant);
		}
	}
}
