#nullable enable
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Xunit;
using WButton = Microsoft.UI.Xaml.Controls.Button;
using WCornerRadius = Microsoft.UI.Xaml.CornerRadius;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;
using WRect = Windows.Foundation.Rect;
using WSize = Windows.Foundation.Size;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WStackPanel = Microsoft.UI.Xaml.Controls.StackPanel;
using WThickness = Microsoft.UI.Xaml.Thickness;
using WWriteableBitmap = Microsoft.UI.Xaml.Media.Imaging.WriteableBitmap;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public class ContentButtonWindowsTests : CoreHandlerTestBase<ContentButtonHandler, ContentButtonStub>
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task BackgroundUpdateSupersedesPendingImage(bool clear)
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var button = new ContentButtonStub();
				var handler = CreateHandler(button);
				var completion = new TaskCompletionSource<IImageSourceServiceResult<WImageSource>>();
				var provider = new ControlledImageSourceService { Result = completion.Task };
				var pending = handler.PlatformView.UpdateBackgroundImageSourceAsync(new FontImageSourceStub(), provider);
				button.Background = clear ? null : new SolidPaint(Colors.Blue);
				handler.UpdateValue(nameof(IView.Background));
				var current = handler.PlatformView.Background;
				var result = new ImageSourceServiceResult(new WWriteableBitmap(12, 18));
				completion.SetResult(result);
				await pending;
				Assert.Same(current, handler.PlatformView.Background);
				Assert.True(result.IsDisposed);
			});
		}

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
				// WinUI skips Arrange when the rect matches an earlier arrange that wasn't invalidated.
				panel.InvalidateArrange();
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

		[Fact]
		public async Task ReplacingFocusedVirtualViewTransfersFocus()
		{
			var first = new ContentButtonStub();
			var second = new ContentButtonStub();
			var handler = await CreateHandlerAsync(first);

			await InvokeOnMainThreadAsync(async () =>
			{
				var platformView = handler.PlatformView;
				var root = new WStackPanel { Children = { platformView } };
				await root.AttachAndRun(async () =>
				{
					Assert.True(platformView.Focus(FocusState.Programmatic));
					await AssertionExtensions.AssertEventually(() => first.IsFocused);

					handler.SetVirtualView(second);

					Assert.NotEqual(FocusState.Unfocused, platformView.FocusState);
					Assert.False(first.IsFocused);
					Assert.True(second.IsFocused);
				}, MauiContext);
			});
		}

		[Fact]
		public async Task ChangingCornerRadiusRefreshesShadowMapping()
		{
			var button = new ContentButtonStub { Shadow = new ShadowStub { Paint = new SolidPaint(Colors.Black) } };
			var shadowUpdates = 0;
			var mapper = new PropertyMapper<IContentButton, IContentButtonHandler>(ContentButtonHandler.Mapper);
			mapper.AppendToMapping(nameof(IView.Shadow), (_, _) => shadowUpdates++);
			var handler = new ContentButtonHandler(mapper);

			await InvokeOnMainThreadAsync(() =>
			{
				InitializeViewHandler(button, handler);
				var previousUpdates = shadowUpdates;
				button.CornerRadius = 30;
				handler.UpdateValue(nameof(IButtonStroke.CornerRadius));

				Assert.Equal(previousUpdates + 1, shadowUpdates);
			});
		}

		[Fact]
		public async Task KeyActivationDoesNotClickReplacementSetByReleasedCallback()
		{
			var originalClicks = 0;
			var replacementClicks = 0;
			ContentButtonHandler? handler = null;
			var replacement = new ContentButtonStub { Clicked = () => replacementClicks++ };
			var original = new ContentButtonStub
			{
				Released = () => handler!.SetVirtualView(replacement),
				Clicked = () => originalClicks++
			};
			handler = await CreateHandlerAsync(original);

			await InvokeOnMainThreadAsync(() =>
			{
				PressActivationKey(handler);
				ReleaseActivationKey(handler);
				InvokeClick(handler);

				Assert.Same(replacement, ((IElementHandler)handler).VirtualView);
				Assert.Equal(0, originalClicks);
				Assert.Equal(0, replacementClicks);
			});
		}

		[Fact]
		public async Task CollectedKeyTargetDoesNotClickReplacementView()
		{
			var clicks = 0;
			var replacement = new ContentButtonStub { Clicked = () => clicks++ };
			var (original, handler) = await PressAndReplace(replacement);

			await AssertionExtensions.WaitForGC(original);
			await InvokeOnMainThreadAsync(() =>
			{
				InvokeClick(handler);
				Assert.Equal(0, clicks);

				InvokeClick(handler);
				Assert.Equal(1, clicks);
			});
			GC.KeepAlive(handler);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task CanceledPointerAllowsReplacementAutomation(bool replaceOnRelease)
		{
			var clicks = 0;
			ContentButtonHandler? handler = null;
			var replacement = new ContentButtonStub { Clicked = () => clicks++ };
			var original = new ContentButtonStub
			{
				Released = () =>
				{
					if (replaceOnRelease)
						handler!.SetVirtualView(replacement);
				}
			};
			var connectedHandler = await CreateHandlerAsync(original);
			handler = connectedHandler;

			await InvokeOnMainThreadAsync(() =>
			{
				var press = typeof(ContentButtonHandler).GetMethod("Press", BindingFlags.Instance | BindingFlags.NonPublic);
				var cancel = typeof(ContentButtonHandler).GetMethod("OnPointerReleased", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.NotNull(press);
				Assert.NotNull(cancel);
				press.Invoke(connectedHandler, null);
				cancel.Invoke(connectedHandler, new object?[] { connectedHandler.PlatformView, null });

				if (!replaceOnRelease)
					connectedHandler.SetVirtualView(replacement);

				var peer = new MauiButtonAutomationPeer(connectedHandler.PlatformView);
				var invokeProvider = Assert.IsAssignableFrom<IInvokeProvider>(peer.GetPattern(PatternInterface.Invoke));
				invokeProvider.Invoke();
				Assert.Equal(1, clicks);
			});
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		async Task<(WeakReference Original, ContentButtonHandler Handler)> PressAndReplace(ContentButtonStub replacement)
		{
			var original = new ContentButtonStub();
			var handler = await CreateHandlerAsync(original);
			var reference = new WeakReference(original);

			await InvokeOnMainThreadAsync(() =>
			{
				PressActivationKey(handler);
				handler.SetVirtualView(replacement);
			});

			return (reference, handler);
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
				Assert.True(firstPlatformView.IsHitTestVisible);
				Assert.False(Assert.IsAssignableFrom<Microsoft.UI.Xaml.Controls.Control>(firstPlatformView).IsTabStop);
				Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(contentPanel));

				button.Content = second;
				handler.UpdateValue(nameof(IContentView.Content));

				var secondPlatformView = Assert.IsAssignableFrom<FrameworkElement>(second.Handler?.PlatformView);
				Assert.Same(secondPlatformView, contentPanel.Content);
				Assert.Null(firstPlatformView.Parent);
				Assert.True(firstPlatformView.IsHitTestVisible);
				Assert.True(Assert.IsAssignableFrom<Microsoft.UI.Xaml.Controls.Control>(firstPlatformView).IsTabStop);
				Assert.False(contentPanel.IsHitTestVisible);

				button.Content = null!;
				handler.UpdateValue(nameof(IContentView.Content));
				Assert.Null(contentPanel.Content);
				Assert.True(secondPlatformView.IsHitTestVisible);
			});
		}

		[Fact]
		public async Task ContentInputTransparencyChangedWhileHostedSurvivesRemoval()
		{
			var content = new LabelStub { Text = "Content", InputTransparent = true };
			var button = new ContentButtonStub { Content = content };
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformButton = Assert.IsType<MauiContentButton>(handler.PlatformView);
				var platformContent = Assert.IsAssignableFrom<FrameworkElement>(content.Handler?.PlatformView);
				Assert.False(platformContent.IsHitTestVisible);

				content.InputTransparent = false;
				content.Handler!.UpdateValue(nameof(IView.InputTransparent));
				Assert.False(platformButton.ContentPanel.IsHitTestVisible);

				button.Content = null!;
				handler.UpdateValue(nameof(IContentView.Content));

				Assert.Null(platformContent.Parent);
				Assert.True(platformContent.IsHitTestVisible);
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
		public async Task AutomationInvokeAbandonsClickWhenReleasedDisconnectsHandler()
		{
			var presses = 0;
			var releases = 0;
			var clicks = 0;
			ContentButtonHandler? handler = null;
			var button = new ContentButtonStub
			{
				Pressed = () => presses++,
				Released = () =>
				{
					releases++;
					((IElementHandler)handler!).DisconnectHandler();
				},
				Clicked = () => clicks++
			};
			var connectedHandler = await CreateHandlerAsync(button);
			handler = connectedHandler;

			await InvokeOnMainThreadAsync(async () =>
			{
				PressActivationKey(connectedHandler);
				Assert.Equal(1, presses);

				var peer = new ButtonAutomationPeer(connectedHandler.PlatformView);
				var invokeProvider = Assert.IsAssignableFrom<IInvokeProvider>(peer.GetPattern(PatternInterface.Invoke));
				invokeProvider.Invoke();

				await AssertionExtensions.AssertEventually(() => releases == 1);
				Assert.Equal(1, presses);
				Assert.Equal(1, releases);
				Assert.Equal(0, clicks);
				Assert.Null(((IElementHandler)connectedHandler).VirtualView);
			});
		}

		[Fact]
		public async Task AutomationInvokeAbandonsClickWhenReleasedReplacesVirtualView()
		{
			var presses = 0;
			var releases = 0;
			var originalClicks = 0;
			var replacementClicks = 0;
			ContentButtonHandler? handler = null;
			var replacement = new ContentButtonStub
			{
				Clicked = () => replacementClicks++
			};
			var original = new ContentButtonStub
			{
				Pressed = () => presses++,
				Released = () =>
				{
					releases++;
					handler!.SetVirtualView(replacement);
				},
				Clicked = () => originalClicks++
			};
			var connectedHandler = await CreateHandlerAsync(original);
			handler = connectedHandler;

			await InvokeOnMainThreadAsync(async () =>
			{
				PressActivationKey(connectedHandler);
				Assert.Equal(1, presses);

				var peer = new ButtonAutomationPeer(connectedHandler.PlatformView);
				var invokeProvider = Assert.IsAssignableFrom<IInvokeProvider>(peer.GetPattern(PatternInterface.Invoke));
				invokeProvider.Invoke();

				await AssertionExtensions.AssertEventually(
					() => releases == 1 && ReferenceEquals(((IElementHandler)connectedHandler).VirtualView, replacement));
				Assert.Equal(1, presses);
				Assert.Equal(1, releases);
				Assert.Equal(0, originalClicks);
				Assert.Equal(0, replacementClicks);
			});
		}

		[Fact]
		public async Task HandlerProvidedNativeResourcesAreRestoredAsDefaults()
		{
			var button = new ContentButtonStub();
			var handler = await InvokeOnMainThreadAsync(() =>
				CreateHandler<ContentButtonHandler, StyledContentButtonHandler>(button, MauiContext));

			await InvokeOnMainThreadAsync(() =>
			{
				var resources = handler.PlatformView.Resources;

				Assert.Same(StyledContentButtonHandler.Background, resources["ButtonBackground"]);
				Assert.Same(StyledContentButtonHandler.Border, resources["ButtonBorderBrushPressed"]);
				Assert.Equal(StyledContentButtonHandler.BorderThickness, resources["ButtonBorderThemeThickness"]);
				Assert.Equal(StyledContentButtonHandler.CornerRadius, resources["ControlCornerRadius"]);
				// Mapping null styling must not add local resources beyond those the native view started with.
				Assert.Equal(handler.InitialLocalKeys, LocalStylingKeys(resources));

				button.Background = new SolidPaintStub(Colors.Blue);
				button.StrokeColor = Colors.Green;
				button.StrokeThickness = 7;
				button.CornerRadius = 11;
				handler.UpdateValue(nameof(IView.Background));
				handler.UpdateValue(nameof(IButtonStroke.StrokeColor));
				handler.UpdateValue(nameof(IButtonStroke.StrokeThickness));
				handler.UpdateValue(nameof(IButtonStroke.CornerRadius));

				Assert.NotSame(StyledContentButtonHandler.Background, resources["ButtonBackground"]);
				Assert.NotSame(StyledContentButtonHandler.Border, resources["ButtonBorderBrushPressed"]);
				Assert.Equal(new WThickness(7), Assert.IsType<WThickness>(resources["ButtonBorderThemeThickness"]));
				Assert.Equal(WinUIHelpers.CreateCornerRadius(11), Assert.IsType<WCornerRadius>(resources["ControlCornerRadius"]));

				button.Background = null!;
				button.StrokeColor = null!;
				button.StrokeThickness = -1;
				button.CornerRadius = -1;
				handler.UpdateValue(nameof(IView.Background));
				handler.UpdateValue(nameof(IButtonStroke.StrokeColor));
				handler.UpdateValue(nameof(IButtonStroke.StrokeThickness));
				handler.UpdateValue(nameof(IButtonStroke.CornerRadius));

				Assert.Same(StyledContentButtonHandler.Background, resources["ButtonBackground"]);
				Assert.Same(StyledContentButtonHandler.Border, resources["ButtonBorderBrushPressed"]);
				Assert.Equal(StyledContentButtonHandler.BorderThickness, resources["ButtonBorderThemeThickness"]);
				Assert.Equal(StyledContentButtonHandler.CornerRadius, resources["ControlCornerRadius"]);
				// Every resource MAUI wrote for the explicit styling is removed again.
				Assert.Equal(handler.InitialLocalKeys, LocalStylingKeys(resources));
			});
		}

		static readonly string[] StylingResourceKeys =
		{
			"ButtonBackground",
			"ButtonBackgroundPointerOver",
			"ButtonBackgroundPressed",
			"ButtonBackgroundDisabled",
			"ButtonBorderBrush",
			"ButtonBorderBrushPointerOver",
			"ButtonBorderBrushPressed",
			"ButtonBorderBrushDisabled",
			"ButtonBorderThemeThickness",
			"ControlCornerRadius",
		};

		static string[] LocalStylingKeys(Microsoft.UI.Xaml.ResourceDictionary resources) =>
			StylingResourceKeys.Where(key => resources.Keys.Contains(key)).ToArray();

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

		static void ReleaseActivationKey(ContentButtonHandler handler)
		{
			var keyUp = typeof(ContentButtonHandler).GetMethod("OnKeyUp", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(keyUp);
			keyUp.Invoke(handler, new object?[] { handler.PlatformView, null });
		}

		static void InvokeClick(ContentButtonHandler handler)
		{
			var onClick = typeof(ContentButtonHandler).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(onClick);
			onClick.Invoke(handler, new object?[] { handler.PlatformView, new RoutedEventArgs() });
		}

		sealed class StyledContentButtonHandler : ContentButtonHandler
		{
			internal static readonly WSolidColorBrush Background = new(Colors.Red.ToWindowsColor());
			internal static readonly WSolidColorBrush Border = new(Colors.Purple.ToWindowsColor());
			internal static readonly WThickness BorderThickness = new(3);
			internal static readonly WCornerRadius CornerRadius = WinUIHelpers.CreateCornerRadius(5);

			internal string[] InitialLocalKeys { get; private set; } = Array.Empty<string>();

			protected override WButton CreatePlatformView()
			{
				var button = base.CreatePlatformView();
				button.Resources["ButtonBackground"] = Background;
				button.Resources["ButtonBorderBrushPressed"] = Border;
				button.Resources["ButtonBorderThemeThickness"] = BorderThickness;
				button.Resources["ControlCornerRadius"] = CornerRadius;
				InitialLocalKeys = LocalStylingKeys(button.Resources);
				return button;
			}
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
