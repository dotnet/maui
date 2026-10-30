#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public class ContentButtonAppleTests : ControlsHandlerTestBase
	{
		public ContentButtonAppleTests()
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<ContentButton, ContentButtonHandler>();
				handlers.AddHandler<Label, LabelHandler>();
				handlers.AddHandler<Grid, LayoutHandler>();
			}));
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ContentIsArrangedWithinInsetsAfterReplacement(bool nested)
		{
			var label = new Label { Text = "Original" };
			var content = nested ? (View)new Grid { label } : label;
			var button = new ContentButton
			{
				Content = content,
				WidthRequest = 260,
				HeightRequest = 120,
				Padding = new Thickness(12, 8, 20, 16),
				BorderWidth = 2,
			};

			await CreateHandlerAndAddToWindow(button, async () =>
			{
				await AssertionExtensions.AssertEventually(() =>
					content.Width == 224 && content.Height == 92 && label.Width > 0 && label.Height > 0);

				var platformView = (UIKit.UIView)content.Handler!.PlatformView!;
				Assert.Equal(14, platformView.Frame.X);
				Assert.Equal(10, platformView.Frame.Y);
				Assert.Equal(224, platformView.Frame.Width);
				Assert.Equal(92, platformView.Frame.Height);
				Assert.True(platformView.IsFinalMeasureHandledBySuperView());
				var buttonPlatformView = Assert.IsAssignableFrom<UIButton>(button.Handler!.PlatformView);
				Assert.Same(buttonPlatformView, buttonPlatformView.HitTest(new CoreGraphics.CGPoint(40, 40), null));

				var replacement = new Label { Text = "Replacement" };
				button.Content = replacement;
				await AssertionExtensions.AssertEventually(() => replacement.Width == 224 && replacement.Height == 92);

				var replacementPlatformView = (UIKit.UIView)replacement.Handler!.PlatformView!;
				Assert.Equal(14, replacementPlatformView.Frame.X);
				Assert.Equal(10, replacementPlatformView.Frame.Y);
				Assert.Equal(224, replacementPlatformView.Frame.Width);
				Assert.Equal(92, replacementPlatformView.Frame.Height);
				Assert.Null(platformView.Superview);
			});
		}

		[Fact]
		public async Task ContentIsPresentationOnlyAndUpdates()
		{
			var original = new Label { Text = "Original" };
			var replacement = new Label { Text = "Replacement" };
			var button = new ContentButton { Content = original };
			var handler = await CreateHandlerAsync<ContentButtonHandler>(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var originalPlatformView = Assert.IsAssignableFrom<UIView>(original.Handler?.PlatformView);
				Assert.Same(handler.PlatformView, originalPlatformView.Superview);
				Assert.False(originalPlatformView.UserInteractionEnabled);
				Assert.False(originalPlatformView.IsAccessibilityElement);
				Assert.True(originalPlatformView.AccessibilityElementsHidden);

				button.Content = replacement;

				var replacementPlatformView = Assert.IsAssignableFrom<UIView>(replacement.Handler?.PlatformView);
				Assert.Same(handler.PlatformView, replacementPlatformView.Superview);
				Assert.Null(originalPlatformView.Superview);
				Assert.False(replacementPlatformView.UserInteractionEnabled);
				Assert.True(replacementPlatformView.AccessibilityElementsHidden);
			});
		}

		[Fact]
		public async Task NativeCancellationActivationAndDisableReleaseState()
		{
			var pressed = 0;
			var released = 0;
			var clicked = 0;
			var button = new ContentButton();
			button.Pressed += (_, _) => pressed++;
			button.Released += (_, _) => released++;
			button.Clicked += (_, _) => clicked++;

			var handler = await CreateHandlerAsync<ContentButtonHandler>(button);

			await InvokeOnMainThreadAsync(() =>
			{
				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchDown);
				Assert.True(button.IsPressed);
				Assert.Equal(1, pressed);

				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchCancel);
				Assert.False(button.IsPressed);
				Assert.Equal(1, released);

				handler.PlatformView.SendActionForControlEvents(UIControlEvent.PrimaryActionTriggered);
				Assert.Equal(1, clicked);

				Assert.True(handler.PlatformView.AccessibilityActivate());
				Assert.Equal(2, clicked);

				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchDown);
				button.IsEnabled = false;

				Assert.False(button.IsPressed);
				Assert.Equal(2, pressed);
				Assert.Equal(2, released);
				Assert.False(handler.PlatformView.AccessibilityActivate());
				Assert.Equal(2, clicked);
			});
		}

		[Fact]
		public async Task GradientStrokeAndRadiusUpdateToDefaults()
		{
			var button = new ContentButton
			{
				Background = new LinearGradientBrush
				{
					StartPoint = new Point(0, 0),
					EndPoint = new Point(1, 1),
					GradientStops = new GradientStopCollection
					{
						new GradientStop { Color = Colors.Red, Offset = 0 },
						new GradientStop { Color = Colors.Blue, Offset = 1 },
					}
				},
				BorderColor = Colors.Green,
				BorderWidth = 3,
				CornerRadius = 18,
				HeightRequest = 80,
				WidthRequest = 160,
			};

			var handler = await CreateHandlerAsync<ContentButtonHandler>(button);

			await InvokeOnMainThreadAsync(() =>
			{
				handler.PlatformView.Frame = new CoreGraphics.CGRect(0, 0, 160, 80);
				handler.PlatformView.LayoutIfNeeded();

				Assert.Equal(3, handler.PlatformView.Layer.BorderWidth);
				Assert.Equal(18, handler.PlatformView.Layer.CornerRadius);
				Assert.Contains(handler.PlatformView.Layer.Sublayers ?? [], layer => layer.Name == "MauiBackgroundLayer");

				button.BorderColor = null;
				button.BorderWidth = -1;
				button.CornerRadius = -1;
				button.Background = null;

				Assert.Equal(0, handler.PlatformView.Layer.BorderWidth);
				Assert.Equal(0, handler.PlatformView.Layer.CornerRadius);
				Assert.DoesNotContain(handler.PlatformView.Layer.Sublayers ?? [], layer => layer.Name == "MauiBackgroundLayer");
			});
		}

		[Fact]
		public async Task ReusedHandlerUpdatesEventsAndAccessibility()
		{
			var firstClicks = 0;
			var first = new ContentButton { Content = new Label { Text = "First" } };
			first.Clicked += (_, _) => firstClicks++;

			var secondClicks = 0;
			var second = new ContentButton { Content = new Label { Text = "Second" } };
			second.Clicked += (_, _) => secondClicks++;
			SemanticProperties.SetDescription(second, "Second action");

			var handler = await CreateHandlerAsync<ContentButtonHandler>(first);

			await InvokeOnMainThreadAsync(() =>
			{
				var mauiContext = handler.MauiContext ?? throw new InvalidOperationException("The handler must have a MauiContext.");
				((IElementHandler)handler).DisconnectHandler();
				((IElementHandler)handler).SetMauiContext(mauiContext);
				((IElementHandler)handler).SetVirtualView(second);

				handler.PlatformView.SendActionForControlEvents(UIControlEvent.PrimaryActionTriggered);

				Assert.Equal(0, firstClicks);
				Assert.Equal(1, secondClicks);
				Assert.True(handler.PlatformView.IsAccessibilityElement);
				Assert.Equal("Second action", handler.PlatformView.AccessibilityLabel);

				var content = Assert.IsAssignableFrom<UIView>(second.Content.Handler?.PlatformView);
				Assert.Same(handler.PlatformView, content.Superview);
				Assert.True(content.AccessibilityElementsHidden);
				Assert.False(content.IsAccessibilityElement);
			});
		}
	}
}
