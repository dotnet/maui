using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public class ContentButtonControlTests : ControlsHandlerTestBase
	{
		void SetupBuilder() => EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddHandler<ContentButton, ContentButtonHandler>();
			handlers.AddHandler<Label, LabelHandler>();
			handlers.AddHandler<Grid, LayoutHandler>();
			handlers.AddHandler<ContentPresenter, ContentViewHandler>();
		}));

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task NestedContentAndTemplateCanBeReplaced(bool nestedTemplate)
		{
			SetupBuilder();
			await InvokeOnMainThreadAsync(() =>
			{
				var context = new object();
				var child = new Label { Text = "First" };
				var grid = new Grid { child };
				var button = new ContentButton { Content = grid, BindingContext = context };
				var handler = CreateHandler<ContentButtonHandler>(button);
				Assert.NotNull(child.Handler);
				Assert.Same(context, child.BindingContext);

				var replacement = new Label { Text = "Second" };
				button.Content = replacement;
				Assert.Null(grid.Parent);
				Assert.Same(button, replacement.Parent);
				Assert.NotNull(replacement.Handler);
				Assert.Same(context, replacement.BindingContext);

				var presenter = new ContentPresenter();
				View templateRoot = nestedTemplate ? new Grid { presenter } : presenter;
				button.ControlTemplate = new ControlTemplate(() => templateRoot);
				Assert.Same(templateRoot, ((IContentView)button).PresentedContent);
				Assert.Same(replacement, presenter.Content);
				Assert.NotNull(presenter.Handler);
				button.ControlTemplate = null;
				Assert.Null(presenter.Content);
				Assert.Same(replacement, ((IContentView)button).PresentedContent);
				button.Content = null;
				Assert.Null(replacement.Parent);
				Assert.Null(((IContentView)button).PresentedContent);
				((IElementHandler)handler).DisconnectHandler();
			});
		}

#if ANDROID || IOS || MACCATALYST || WINDOWS
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ContentChangedWhileCreatingChildHandlerUsesLatestContent(bool clearContent)
		{
			SetupBuilder();
			await InvokeOnMainThreadAsync(() =>
			{
				var original = new Label { Text = "Original" };
				var replacement = new Label { Text = "Replacement" };
				var button = new ContentButton { Content = original };
				original.HandlerChanged += (_, _) =>
				{
					if (original.Handler is not null)
						button.Content = clearContent ? null : replacement;
				};

				var handler = CreateHandler<ContentButtonHandler>(button);
				Assert.Same(clearContent ? null : replacement, ((IContentView)button).PresentedContent);
				Assert.Same(clearContent ? null : replacement.Handler.PlatformView, GetPresentedPlatformContent(handler.PlatformView));
			});
		}

		[Fact]
		public async Task DisconnectWhileCreatingChildHandlerDoesNotAttachContent()
		{
			SetupBuilder();
			await InvokeOnMainThreadAsync(() =>
			{
				var button = new ContentButton();
				var handler = CreateHandler<ContentButtonHandler>(button);
				var platformButton = handler.PlatformView;
				var content = new Label { Text = "Content" };
				content.HandlerChanged += (_, _) =>
				{
					if (content.Handler is not null)
						((IElementHandler)handler).DisconnectHandler();
				};

				button.Content = content;

				Assert.Null(((IElementHandler)handler).PlatformView);
				Assert.Null(GetPresentedPlatformContent(platformButton));
			});
		}

		[Fact]
		public async Task ReusingHandlerWhileCreatingChildKeepsNewButtonContent()
		{
			SetupBuilder();
			await InvokeOnMainThreadAsync(() =>
			{
				var firstButton = new ContentButton();
				var handler = CreateHandler<ContentButtonHandler>(firstButton);
				var replacement = new Label { Text = "Replacement" };
				var secondButton = new ContentButton { Content = replacement };
				var original = new Label { Text = "Original" };
				original.HandlerChanged += (_, _) =>
				{
					if (original.Handler is not null)
						handler.SetVirtualView(secondButton);
				};

				firstButton.Content = original;

				Assert.Same(secondButton, ((IElementHandler)handler).VirtualView);
				Assert.Same(replacement.Handler.PlatformView, GetPresentedPlatformContent(handler.PlatformView));
			});
		}

		[Fact]
		public async Task ReparentingChildDuringHandlerCreationKeepsItInNewParent()
		{
			SetupBuilder();
			await InvokeOnMainThreadAsync(() =>
			{
				var destination = new ContentView();
				var destinationHandler = CreateHandler<ContentViewHandler>(destination);
				var replacement = new Label { Text = "Replacement" };
				var original = new Label { Text = "Original" };
				var button = new ContentButton { Content = original };
#if WINDOWS
				IElementHandler movedHandler = null;
				object movedPlatformView = null;
				string stateAfterMove = null;
#endif
				original.HandlerChanged += (_, _) =>
				{
					if (original.Handler is not null)
					{
						button.Content = replacement;
						destination.Content = original;
#if WINDOWS
						if (movedHandler is null)
						{
							movedHandler = original.Handler;
							movedPlatformView = original.Handler.PlatformView;
							stateAfterMove = DescribePlatformParents(movedPlatformView, destinationHandler.PlatformView,
								((Microsoft.Maui.Platform.MauiContentButton)button.Handler.PlatformView).ContentPanel);
						}
#endif
					}
				};

				var buttonHandler = CreateHandler<ContentButtonHandler>(button);

				Assert.Same(replacement, button.Content);
				Assert.Same(original, destination.Content);
				Assert.Same(destination, original.Parent);
				Assert.Same(replacement.Handler.PlatformView, GetPresentedPlatformContent(buttonHandler.PlatformView));
#if WINDOWS
				// These views are never added to a window, and WinUI parent queries such as
				// VisualTreeHelper.GetParent do not report parents outside the live tree. Check the
				// panels' children directly; a WinUI element belongs to at most one parent.
				var platformView = original.Handler.PlatformView;
				var destinationPanel = destinationHandler.PlatformView;
				var buttonPanel = ((Microsoft.Maui.Platform.MauiContentButton)buttonHandler.PlatformView).ContentPanel;
				var state = $"After the move: handler={Describe(movedHandler)}, {stateAfterMove}. " +
					$"At the end: handler={Describe(original.Handler)}, {DescribePlatformParents(platformView, destinationPanel, buttonPanel)}";

				Assert.True(ReferenceEquals(movedPlatformView, platformView),
					$"The original label's platform view changed after it moved. {state}");
				Assert.True(HasPlatformChild(destinationPanel, platformView),
					$"The destination ContentPanel does not contain the original label's platform view. {state}");
				Assert.False(HasPlatformChild(buttonPanel, platformView),
					$"The button's ContentPanel still contains the original label's platform view. {state}");
#else
				Assert.Same(destinationHandler.PlatformView, GetPlatformParent(original.Handler.PlatformView));
#endif
			});
		}

		static object GetPresentedPlatformContent(object platformView)
		{
#if ANDROID
			var button = (Google.Android.Material.Card.MaterialCardView)platformView;
			return button.ChildCount == 0 ? null : ((global::Android.Views.ViewGroup)button.GetChildAt(0)).GetChildAt(0);
#elif IOS || MACCATALYST
			var container = ((Microsoft.Maui.Platform.MauiContentButton)platformView).ContentContainerView;
			return container.Subviews.Length == 0 ? null : container.Subviews[0];
#elif WINDOWS
			return ((Microsoft.Maui.Platform.MauiContentButton)platformView).ContentPanel.Content;
#endif
		}

#if WINDOWS
		static bool HasPlatformChild(Microsoft.Maui.Platform.MauiPanel panel, object platformView)
		{
			foreach (var child in panel.CachedChildren)
			{
				if (ReferenceEquals(child, platformView))
					return true;
			}

			return false;
		}

		static string DescribePlatformParents(object platformView, Microsoft.Maui.Platform.ContentPanel destinationPanel,
			Microsoft.Maui.Platform.ContentPanel buttonPanel)
		{
			var element = (Microsoft.UI.Xaml.FrameworkElement)platformView;
			return $"view={Describe(element)}, Parent={Describe(element.Parent)}, " +
				$"VisualTreeHelper.GetParent={Describe(Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(element))}, " +
				$"destination panel={DescribePanel(destinationPanel, element)}, button panel={DescribePanel(buttonPanel, element)}";
		}

		static string DescribePanel(Microsoft.Maui.Platform.ContentPanel panel, Microsoft.UI.Xaml.UIElement element)
		{
			var children = new System.Collections.Generic.List<string>();
			foreach (var child in panel.CachedChildren)
				children.Add(Describe(child));

			return $"{Describe(panel)} {{ Content={Describe(panel.Content)}, Contains={panel.CachedChildren.Contains(element)}, " +
				$"Children=[{string.Join(", ", children)}] }}";
		}

		static string Describe(object value) =>
			value is null ? "null" : $"{value.GetType().Name}#{System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value)}";
#else
		static object GetPlatformParent(object platformView)
		{
#if ANDROID
			return ((global::Android.Views.View)platformView).Parent;
#elif IOS || MACCATALYST
			return ((UIKit.UIView)platformView).Superview;
#endif
		}
#endif
#endif

		[Fact]
		public async Task ChildTextChangeInvalidatesAndResizesButton()
		{
			SetupBuilder();
			var child = new Label { Text = "A" };
			var button = new ContentButton
			{
				Content = child,
				Padding = 8,
				BorderWidth = 2,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
			};
			await CreateHandlerAndAddToWindow(new Grid { button }, async () =>
			{
				await AssertionExtensions.AssertEventually(() => button.Width > 20 && button.Height > 20);
				var before = button.Width;
				child.Text = "A much longer presentation label";
				await AssertionExtensions.AssertEventually(() => button.Width > before);
				Assert.True(button.Height >= 20);
				child.Text = "A";
				await AssertionExtensions.AssertEventually(() => button.Width <= before + 1);
			});
		}

		[Fact]
		public async Task StylingAndEnabledStateCanUpdateWithAttachedContent()
		{
			SetupBuilder();
			var child = new Label { Text = "Presentation" };
			var button = new ContentButton { Content = child, WidthRequest = 120, HeightRequest = 80 };
			await CreateHandlerAndAddToWindow(button, () =>
			{
				button.Padding = new Thickness(1, 2, 3, 4);
				button.BorderWidth = 3;
				button.BorderColor = Colors.Red;
				button.CornerRadius = 12;
				button.Background = new SolidColorBrush(Colors.Blue);
				button.IsEnabled = false;
				Assert.False(child.IsEnabled);
				button.IsEnabled = true;
				Assert.True(child.IsEnabled);
				var size = ((IView)button).Measure(120, 80);
				Assert.Equal(120, size.Width, 1);
				Assert.Equal(80, size.Height, 1);
			});
		}
	}
}
